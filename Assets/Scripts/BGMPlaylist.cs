using UnityEngine;

public class BGMPlaylist : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] tracks;
    [Range(0f, 1f)] public float volume = 0.2f;

    private int currentTrackIndex = 0;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        PlayCurrentTrack();
    }

    private void Update()
    {
        if (tracks == null || tracks.Length == 0 || audioSource.isPlaying) return;

        currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;
        PlayCurrentTrack();
    }

    private void PlayCurrentTrack()
    {
        if (tracks == null || tracks.Length == 0) return;

        AudioClip clip = tracks[currentTrackIndex];
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
    }
}
