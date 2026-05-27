using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float lifeTime = 3f;
    public float upwardForce = 3f;
    public float floatDelay = 1f;
    public float damage = 10f;

    [Header("Audio")]
    public AudioClip bubblePopClip;
    [Range(0f, 1f)] public float bubblePopVolume = 1f;

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
        {
            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore player
        if (other.transform.root.CompareTag("Player"))
        {
            hasHit = false;
            return;
        }

        if (timer < ignoreCollisionTime || hasHit) return;

        hasHit = true;

        // === JELLYFISH ===
        if (other.CompareTag("Jellyfish"))
        {
            Jellyfish jelly = other.GetComponent<Jellyfish>();
            if (jelly != null)
                jelly.TakeDamage(damage);


            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
            return;

        }
        // === STARFISH ===
        else if (other.CompareTag("Starfish"))
        {
            Starfish star = other.GetComponent<Starfish>();
            if (star != null)
                star.TakeDamage(damage);

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
            return;
        }

        // === SQUID SNIPER (NEW) ===
        else if (other.GetComponent<SquidSniper>() != null || other.GetComponentInParent<SquidSniper>() != null)
        {
            SquidSniper squid = other.GetComponentInParent<SquidSniper>();
            if (squid != null)
                squid.TakeDamage(damage);

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
            return;
        }

        // Destroy on walls / environment
        Destroy(gameObject);
        if (bubblePopClip != null)
        {
            AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
        }
    }
}