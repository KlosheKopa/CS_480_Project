using UnityEngine;

public class TopDamageTrigger : MonoBehaviour
{
    public float damage = 10f;
    public float bounceForce = 22f;

    private Jellyfish parentJellyfish;

    void Awake()
    {
        parentJellyfish = GetComponentInParent<Jellyfish>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (parentJellyfish != null && parentJellyfish.isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerController playerController = other.GetComponent<PlayerController>();

            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            if (playerController != null)
            {
                Vector3 bounceDir = (other.transform.position - transform.parent.position).normalized;
                bounceDir.y = 0f;
                playerController.BounceBack(bounceDir * bounceForce);
            }
        }
    }
}