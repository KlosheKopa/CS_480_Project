using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [Header("Key Settings")]
    public GameObject keyUI;           // Should be assigned to your KeyIcon

    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1f;

#if UNITY_EDITOR
    private const string DefaultPickupSoundPath = "Assets/Audio/item_PickUp_sounds/key_pickup.mp3";

    private void Reset()
    {
        AssignDefaultPickupSound();
    }

    private void OnValidate()
    {
        if (pickupSound == null)
            AssignDefaultPickupSound();
    }

    private void AssignDefaultPickupSound()
    {
        pickupSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultPickupSoundPath);
    }
#endif

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

                PlayPickupSound();
                Destroy(gameObject);
            }
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound == null) return;

        AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
    }
}
