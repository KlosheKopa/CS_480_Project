using UnityEngine;
using System.Collections;

public class LockedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public float openSpeed = 0.25f;      // Adjust this number (lower = slower)
    public float openDistance = 12f;    // How far down it sinks

    [Header("Audio")]
    public AudioClip unlockSound;
    [Range(0f, 1f)] public float unlockVolume = 1f;

    [HideInInspector] public bool isOpen = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private AudioSource unlockSource;

#if UNITY_EDITOR
    private const string DefaultUnlockSoundPath = "Assets/Audio/item_PickUp_sounds/door_unlock_sound.mp3";

    private void Reset()
    {
        AssignDefaultUnlockSound();
    }

    private void OnValidate()
    {
        if (unlockSound == null)
            AssignDefaultUnlockSound();
    }

    private void AssignDefaultUnlockSound()
    {
        unlockSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultUnlockSoundPath);
    }
#endif

    private void Awake()
    {
        unlockSource = GetComponent<AudioSource>();
        if (unlockSource == null)
            unlockSource = gameObject.AddComponent<AudioSource>();

        unlockSource.playOnAwake = false;
        unlockSource.loop = false;
        unlockSource.spatialBlend = 1f;
    }

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition - Vector3.up * openDistance;
    }

    public void OpenTheDoor()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        float elapsed = 0f;

        PlayUnlockSound();

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            transform.position = Vector3.Lerp(closedPosition, openPosition, elapsed);
            yield return null;
        }

        transform.position = openPosition;
        StopUnlockSound();
    }

    private void OnDisable()
    {
        StopUnlockSound();
    }

    private void PlayUnlockSound()
    {
        if (unlockSound == null || unlockSource == null) return;

        unlockSource.clip = unlockSound;
        unlockSource.volume = unlockVolume;
        unlockSource.Play();
    }

    private void StopUnlockSound()
    {
        if (unlockSource != null && unlockSource.isPlaying)
            unlockSource.Stop();
    }
}
