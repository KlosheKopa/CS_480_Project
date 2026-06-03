using UnityEngine;

public class WallClimbPickup : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1f;

#if UNITY_EDITOR
    private const string DefaultPickupSoundPath = "Assets/Audio/item_PickUp_sounds/skill_orbs_pickup.mp3";

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
            WallClimb wallClimb = other.GetComponent<WallClimb>();
            if (wallClimb != null)
            {
                wallClimb.hasAbility = true;   // Give the ability
                AbilityUnlockPopup.ShowWallClimb();
            }

            PlayPickupSound();
            Destroy(gameObject);
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound == null) return;

        GameObject audioObject = new GameObject("WallClimbPickupSound");
        audioObject.transform.position = transform.position;

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = pickupSound;
        audioSource.volume = pickupVolume;
        audioSource.ignoreListenerPause = true;
        audioSource.Play();

        Destroy(audioObject, pickupSound.length);
    }
}
