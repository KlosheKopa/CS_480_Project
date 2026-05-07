using UnityEngine;

public class HealingFish : MonoBehaviour
{
    [Header("Movement")]
    public float swimSpeed = 2f;
    public float circleRadius = 1.5f;
    public float circleHeight = 0.75f;

    private Vector3 centerPoint;
    private float angle = 0f;

    void Start()
    {
        centerPoint = transform.position;
    }

    void Update()
    {
        angle += swimSpeed * Time.deltaTime;

        float x = Mathf.Sin(angle) * circleRadius;
        float z = Mathf.Cos(angle) * circleRadius;
        float y = Mathf.Sin(angle * 1.5f) * circleHeight;

        Vector3 targetPos = centerPoint + new Vector3(x, y, z);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 8f);

        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (stats == null || health == null) return;

        if (stats.CurrentHealth < stats.maxHealth)
        {
            float healAmount = stats.maxHealth * stats.healPercentage;
            stats.CurrentHealth = Mathf.Min(stats.CurrentHealth + healAmount, stats.maxHealth);

            if (health.healthBar != null)
                health.healthBar.value = stats.CurrentHealth / stats.maxHealth;

            Debug.Log($"✅ Green Fish healed {stats.healPercentage * 100:F0}% HP");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Fish ignored (player at full health)");
        }
    }
}