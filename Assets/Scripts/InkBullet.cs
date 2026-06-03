using UnityEngine;

public class InkBullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 25f;
    public float lifetime = 5f;
    public int damage = 15;

    [Header("Tags")]
    public string playerTag = "Player";
    public string squidSniperTag = "SquidSniper";

    private Rigidbody rb;
    private bool canCollide = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
        Invoke(nameof(EnableCollision), 0.1f);
    }

    public void Initialize(Vector3 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }
    }

    private void EnableCollision()
    {
        canCollide = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canCollide) return;

        // === Pass through player's bubbles (do not despawn) ===
        if (other.GetComponent<BubbleBehavior>() != null)
        {
            return;
        }

        // === Hit Player ===
        if (other.CompareTag(playerTag))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // === Hit Squid Sniper → pass through ===
        if (other.CompareTag(squidSniperTag))
        {
            return;
        }

        // === Hit anything else (floor, walls, props, etc.) → despawn ===
        Destroy(gameObject);
    }
}