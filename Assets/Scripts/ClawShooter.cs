using UnityEngine;
using UnityEngine.InputSystem;

public class ClawShooter : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject bubblePrefab;
    public Transform shootPoint;
    public Camera playerCamera;

    private PlayerInput playerInput;
    private InputAction shootAction;
    private PlayerStats stats;

    private float nextFireTime = 0f;

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
        stats = GetComponentInParent<PlayerStats>();
    }

    void OnEnable() => shootAction.Enable();
    void OnDisable() => shootAction.Disable();

    void Update()
    {
        if (stats == null) return;

        bool canShoot = Time.time >= nextFireTime;

        if (stats.isAutoFireUnlocked)
        {
            if (shootAction.IsPressed() && canShoot)
            {
                ShootBubble();
                nextFireTime = Time.time + stats.bubbleFireRate;
            }
        }
        else
        {
            if (shootAction.WasPressedThisFrame() && canShoot)
            {
                ShootBubble();
                nextFireTime = Time.time + stats.bubbleFireRate;
            }
        }
    }

    private void ShootBubble()
    {
        if (bubblePrefab == null || shootPoint == null || playerCamera == null)
        {
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        Vector3 spawnPos = shootPoint.position;
        spawnPos += playerCamera.transform.right * -0.15f;

        GameObject bubble = Instantiate(bubblePrefab, spawnPos, Quaternion.identity);

        // Pass live stats to the bubble
        BubbleBehavior bb = bubble.GetComponent<BubbleBehavior>();
        if (bb != null)
            bb.damage = stats.bubbleDamage;          

        Rigidbody rb = bubble.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = playerCamera.transform.forward * stats.bubbleSpeed;
        }
    }
}