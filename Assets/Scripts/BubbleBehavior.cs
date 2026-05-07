using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float lifeTime = 3f;
    public float upwardForce = 3f;
    public float floatDelay = 1f;
    public float damage = 10f;

    private Rigidbody rb;
    private float timer = 0f;
    private float ignoreCollisionTime = 0.15f;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null) rb.useGravity = false;
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer > floatDelay && rb != null)
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore ANY part of the player (root + ClawArm + any future children)
        if (other.transform.root.CompareTag("Player"))
        {
            hasHit = false;   // reset so it can still hit enemies later
            return;
        }

        // Normal collision logic (walls, jellyfish, etc.)
        if (timer < ignoreCollisionTime || hasHit) return;

        hasHit = true;

        if (other.CompareTag("Jellyfish"))
        {
            Jellyfish jelly = other.GetComponent<Jellyfish>();
            if (jelly != null)
                jelly.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // Destroy on walls/environment
        Destroy(gameObject);
    }
}