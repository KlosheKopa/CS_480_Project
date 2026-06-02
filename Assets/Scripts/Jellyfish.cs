using UnityEngine;
using System.Collections;

public class Jellyfish : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float maxHealth = 40f;
    public float moveSpeed = 3f;
    public float detectionRange = 12f;
    public float maxChaseRange = 20f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("Height Limit")]
    public float maxHeight = 15f;
    public float returnToGroundY = 1.25f;
    public float highAndFarTimeRequired = 2f;

    private Transform player;
    private Rigidbody rb;
    private Collider col;
    private float currentHealth;
    private float lastAttackTime = 0f;
    public bool isDead = false;

    private float highAndFarTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        player = GameObject.FindWithTag("Player").transform;
        currentHealth = maxHealth;
        Debug.Log($"Jellyfish spawned with {currentHealth} HP");
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, Mathf.Sin(Time.time) * 1f, 0);
        }

        // Height limit logic
        bool isHigh = transform.position.y >= 5f && transform.position.y <= maxHeight;
        bool isFar = distance > 2f;

        if (isHigh && isFar)
        {
            highAndFarTimer += Time.deltaTime;
            if (highAndFarTimer >= highAndFarTimeRequired)
            {
                Vector3 descendTarget = new Vector3(transform.position.x, returnToGroundY, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, descendTarget, moveSpeed * Time.deltaTime * 2f);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -moveSpeed * 1.5f, rb.linearVelocity.z);

                if (transform.position.y <= returnToGroundY + 0.1f)
                    highAndFarTimer = 0f;
            }
        }
        else
        {
            highAndFarTimer = 0f;
        }

        if (transform.position.y > maxHeight)
        {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float oldHealth = currentHealth;
        currentHealth -= damage;

        Debug.Log($"[JELLYFISH DAMAGE] Took {damage} damage | Health: {oldHealth} → {currentHealth} | Frame: {Time.frameCount}");

        if (currentHealth <= 0)
        {
            Debug.Log("Jellyfish reached 0 HP - starting death sequence");
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        // === NEW: Award 1 EXP to the player ===
        PlayerStats playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddEXP(1);
            Debug.Log("Jellyfish killed - Awarded 1 EXP to player");
        }

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (col != null) col.enabled = false;

        // Disable TopDamageTrigger too
        TopDamageTrigger topTrigger = GetComponentInChildren<TopDamageTrigger>();
        if (topTrigger != null)
        {
            topTrigger.enabled = false;
            Collider tCol = topTrigger.GetComponent<Collider>();
            if (tCol != null) tCol.enabled = false;
        }

        Debug.Log("Jellyfish died - collider and physics disabled");

        // Drop to ground
        Vector3 targetPos = new Vector3(transform.position.x, 0.1f, transform.position.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        // Fade out
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            Color color = mat.color;
            float timer = 0f;
            while (timer < 3f)
            {
                timer += Time.deltaTime;
                color.a = Mathf.Lerp(color.a, 0f, timer / 3f);
                mat.color = color;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth == null || playerHealth.isDead) return;

        float playerY = collision.transform.position.y;
        float jellyTopY = transform.position.y + 0.6f;

        if (playerY > jellyTopY)
        {
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);

            PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                Vector3 bounceDir = (collision.transform.position - transform.position).normalized;
                bounceDir.y = 0f;
                pc.BounceBack(bounceDir * 12f);
            }
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                lastAttackTime = Time.time;
            }
        }
    }
}