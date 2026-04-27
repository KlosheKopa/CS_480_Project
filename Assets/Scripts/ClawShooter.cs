using UnityEngine;
using UnityEngine.InputSystem;

public class ClawShooter : MonoBehaviour
{
    [Header("Bubble Shooting")]
    public GameObject bubblePrefab;
    public float shootForce = 12f;

    [Header("Fire Rate")]
    public float fireRate = 0.8f;

    public Transform shootPoint;   // Tip of the ClawPincer

    private InputAction shootAction;
    private Transform cameraTransform;
    private float nextFireTime = 0f;

    void Awake()
    {
        shootAction = GetComponentInParent<PlayerInput>().actions["Shoot"];
        cameraTransform = GetComponentInParent<Camera>().transform;
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
        if (bubblePrefab == null || shootPoint == null) return;

        // Spawn at the exact tip of the pincer
        Vector3 spawnPos = shootPoint.position + shootPoint.forward * 0.3f;

        GameObject bubble = Instantiate(bubblePrefab, spawnPos, cameraTransform.rotation);

        Rigidbody rb = bubble.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // This line is the key: shoot where the CAMERA is looking
            rb.linearVelocity = cameraTransform.forward * shootForce;
        }
    }
}