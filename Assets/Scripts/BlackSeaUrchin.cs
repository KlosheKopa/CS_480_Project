using UnityEngine;

public class BlackSeaUrchin : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 3f;
    public float damageInterval = 0.1f;

    [Header("Audio")]
    public AudioClip hitPlayerSound;
    public AudioClip bubbleHitSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private float lastDamageTime = 0f;
    private AudioSource hitPlayerAudioSource;
    private AudioSource bubbleHitAudioSource;

    private void Awake()
    {
        hitPlayerAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(hitPlayerAudioSource);

        bubbleHitAudioSource = gameObject.AddComponent<AudioSource>();
        SetupAudioSource(bubbleHitAudioSource);
    }

    private void OnDisable()
    {
        StopHitPlayerSound();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    PlayerStats playerStats = other.GetComponent<PlayerStats>();
                    float healthBeforeHit = playerStats != null ? playerStats.CurrentHealth : 0f;

                    playerHealth.TakeDamage(damage);
                    if (playerStats == null || playerStats.CurrentHealth < healthBeforeHit)
                    {
                        PlayHitPlayerSound();
                    }

                    lastDamageTime = Time.time;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopHitPlayerSound();
        }
    }

    public void PlayBubbleHitSound()
    {
        PlaySound(bubbleHitAudioSource, bubbleHitSound);
    }

    private void PlayHitPlayerSound()
    {
        if (hitPlayerSound == null || hitPlayerAudioSource == null) return;
        if (hitPlayerAudioSource.isPlaying) return;

        hitPlayerAudioSource.clip = hitPlayerSound;
        hitPlayerAudioSource.volume = soundVolume;
        hitPlayerAudioSource.Play();
    }

    private void StopHitPlayerSound()
    {
        if (hitPlayerAudioSource != null)
        {
            hitPlayerAudioSource.Stop();
        }
    }

    private void PlaySound(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null) return;

        source.PlayOneShot(clip, soundVolume);
    }

    private void SetupAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 1f;
    }
}
