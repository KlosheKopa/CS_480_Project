using UnityEngine;

public class ShortRangeSlash : MonoBehaviour
{
    [Header("Combat Settings")]
    public int slashDamage = 40;
    public float hitboxActiveDuration = 0.5f;

    private Collider[] localColliders;
    private bool trackingActive = true;

    void Start()
    {
        // Finds all Box Colliders attached right here on this same parent object
        localColliders = GetComponents<Collider>();

        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        float totalLifetime = ps != null ? ps.main.duration : 1.5f;

        // Destroy the whole wave projectile instance when the VFX finishes
        Destroy(gameObject, totalLifetime);
    }

    void Update()
    {
        if (hitboxActiveDuration > 0)
        {
            hitboxActiveDuration -= Time.deltaTime;
            if (hitboxActiveDuration <= 0)
            {
                trackingActive = false;
                ToggleAllColliders(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!trackingActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(slashDamage);
            }

            // Turn off everything instantly to prevent multiple hit registration 
            // from the separate overlapping box parameters
            trackingActive = false;
            ToggleAllColliders(false);
        }
    }

    void ToggleAllColliders(bool state)
    {
        if (localColliders == null) return;
        foreach (Collider col in localColliders)
        {
            if (col != null) col.enabled = state;
        }
    }
}