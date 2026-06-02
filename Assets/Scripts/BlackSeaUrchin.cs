using UnityEngine;

public class BlackSeaUrchin : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 3f;
    public float damageInterval = 0.1f;

    private float lastDamageTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }
}