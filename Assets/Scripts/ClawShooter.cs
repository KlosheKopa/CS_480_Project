using UnityEngine;
using UnityEngine.InputSystem;

public class ClawShooter : MonoBehaviour
{
    [Header("Bubble Shooting")]
    public GameObject bubblePrefab;
    public float shootForce = 12f;
    public float spawnOffset = 0.2f;

    [Header("Fire Rate")]
    public float fireRate = 0.8f;

    public Transform shootPoint;   // Tip of the ClawPincer

    private InputAction shootAction;
    private Transform cameraTransform;
    private Collider[] playerColliders;
    private float nextFireTime = 0f;

    void Awake()
    {
        shootAction = GetComponentInParent<PlayerInput>().actions["Shoot"];

        Camera playerCamera = GetComponentInParent<Camera>();
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }

        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null)
        {
            playerColliders = playerInput.GetComponentsInChildren<Collider>(true);
        }
    }

    void OnEnable() => shootAction.Enable();
    void OnDisable() => shootAction.Disable();

    void Update()
    {
        if (shootAction.WasPressedThisFrame() && Time.time >= nextFireTime)
        {
            ShootBubble();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootBubble()
    {
        if (bubblePrefab == null || shootPoint == null || cameraTransform == null) return;

        Vector3 aimDirection = cameraTransform.forward.normalized;

        // Spawn from the claw tip area, but use the camera aim direction for travel.
        Vector3 spawnPos = shootPoint.position + aimDirection * spawnOffset;

        GameObject bubble = Instantiate(
            bubblePrefab,
            spawnPos,
            Quaternion.LookRotation(aimDirection)
        );

        BubbleBehavior bubbleBehavior = bubble.GetComponent<BubbleBehavior>();
        if (bubbleBehavior != null)
        {
            bubbleBehavior.forwardSpeed = shootForce;
            bubbleBehavior.Initialize(aimDirection, playerColliders);
        }
    }
}
