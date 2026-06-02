using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    public GameObject keyUI;           // Should be assigned to your KeyIcon

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.hasKey)
            {
                player.hasKey = true;

                if (keyUI != null)
                    keyUI.SetActive(true);

                Destroy(gameObject);
            }
        }
    }
}