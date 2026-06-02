using UnityEngine;
using System.Collections;
using System;

public class JellyBoss : MonoBehaviour
{
    public enum State { Idle, Preparing, Shooting, Cooldown }

    [Header("Enemy Settings")]
    public float maxHealth = 60f;
    public float moveSpeed = 4f;
    public float detectionRange = 35f;
    public float minDistanceToPlayer = 12f; // The minimum distance to preserve
    public int experience = 15;

    [Header("Timings")]
    public float prepareTime = 1.2f;
    public float cooldownTime = 2.5f;
    public float timeBetweenBullets = 0.4f;

    [Header("Shooting")]
    public GameObject inkProjectilePrefab;
    public float projectileSpeed = 20f;
    public int minBullets = 3;
    public int maxBullets = 5;

    [Header("References")]
    public Transform shootPoint;
    public Transform player;

    [Header("Death Effects")]
    public GameObject bubblePopParticles; // Drag your particle prefab into this slot in the Unity Inspector

    [Header("Audio Clips")]
    public AudioClip floatingSound;
    public AudioClip detectedSound;
    public AudioClip shootSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Drops")]
    public GameObject greenFishPrefab;
    public int dropEveryXItems = 3;

    private static int totalKills = 0;
    private Rigidbody rb;
    private Collider col;
    private AudioSource audioSource;
    private PlayerStats playerStats;

    private float currentHealth;
    public float CurrentHealth => currentHealth;
    public bool isDead = false;
    public event Action OnDeath;

    private State currentState = State.Idle;
    private bool isChasingAudio = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false; // Kept false for physical jelly movement forces
        rb.useGravity = false;

        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (floatingSound != null)
        {
            audioSource.clip = floatingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerStats = playerObj.GetComponent<PlayerStats>();
            }
        }
        else
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // State Machine Integration
        switch (currentState)
        {
            case State.Idle:
                HandleMovementAndState(distance);
                break;
            case State.Preparing:
                TrackPlayer();
                break;
            case State.Shooting:
                TrackPlayer();
                break;
            case State.Cooldown:
                HandleMovementAndState(distance);
                break;
        }
    }

    private void HandleMovementAndState(float distance)
    {
        // 1. Maintain Spacing & Distance Vector Logic
        if (distance <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            if (distance < minDistanceToPlayer)
            {
                // Back away from player to enforce minimum distance limit
                Vector3 backAwayDirection = -directionToPlayer;
                rb.linearVelocity = backAwayDirection * moveSpeed;
            }
            else if (distance > minDistanceToPlayer + 2f)
            {
                // Advance closer if too far out
                rb.linearVelocity = directionToPlayer * moveSpeed;
            }
            else
            {
                // Hover calmly in place if within sweet spot range
                rb.linearVelocity = new Vector3(0, Mathf.Sin(Time.time * 2f) * 0.5f, 0);
            }

            // 2. Trigger Shooting Loop if in Idle state
            if (currentState == State.Idle)
            {
                StartCoroutine(PrepareToShootSequence());
            }
        }
        else
        {
            // Standard Idle Bobbing Behavior when Player is outside range
            rb.linearVelocity = new Vector3(0, Mathf.Sin(Time.time) * 1f, 0);
        }
    }

    private void TrackPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
        // Maintain a subtle floating bounce during attack preparations
        rb.linearVelocity = new Vector3(0, Mathf.Sin(Time.time * 3f) * 0.2f, 0);
    }

    private IEnumerator PrepareToShootSequence()
    {
        currentState = State.Preparing;
        PlaySound(detectedSound);

        float timer = 0f;
        while (timer < prepareTime)
        {
            TrackPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(ShootPhaseSequence());
    }

    private IEnumerator ShootPhaseSequence()
    {
        currentState = State.Shooting;
        rb.linearVelocity = Vector3.zero; // Hold steady while spraying bullets

        int bulletCount = UnityEngine.Random.Range(minBullets, maxBullets + 1);
        for (int i = 0; i < bulletCount; i++)
        {
            if (isDead || player == null) yield break;

            ShootProjectile();
            yield return new WaitForSeconds(timeBetweenBullets);
        }

        StartCoroutine(CooldownSequence());
    }

    private void ShootProjectile()
    {
        if (inkProjectilePrefab == null || player == null) return;

        Transform spawnLocation = shootPoint != null ? shootPoint : transform;
        Vector3 directionToPlayer = (player.position - spawnLocation.position).normalized;

        // Spawn slightly forward to prevent self-collision clips
        Vector3 spawnPos = spawnLocation.position + directionToPlayer * 0.8f;
        GameObject projectile = Instantiate(inkProjectilePrefab, spawnPos, Quaternion.LookRotation(directionToPlayer));

        PlaySound(shootSound);

        // Pass velocity directions if the bullet script requires explicit initializers
        var bulletScript = projectile.GetComponent<InkBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToPlayer);
        }

        // Standard safety collision cleanups
        Collider bulletCollider = projectile.GetComponent<Collider>();
        if (bulletCollider != null && col != null)
        {
            Physics.IgnoreCollision(bulletCollider, col, true);
        }
    }

    private IEnumerator CooldownSequence()
    {
        currentState = State.Cooldown;
        yield return new WaitForSeconds(cooldownTime);
        currentState = State.Idle;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        PlaySound(hurtSound);

        // .CompareTo(0f) returns 1 if health is greater than 0.
        // If it does NOT return 1, health is 0 or negative.
        if (currentHealth.CompareTo(0f) != 1)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;

        if (audioSource != null)
        {
            audioSource.Stop(); // Stop idle/chase sounds
            if (deathSound != null) audioSource.PlayOneShot(deathSound);
        }

        if (bubblePopParticles != null)
        {
            GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);
            ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                // Calculate how long it takes to play the system completely
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(spawnedParticles, totalDuration);
            }
            else
            {
                // Fallback safety deletion if no particle system component is found
                Destroy(spawnedParticles, 3.0f);
            }

        }

            OnDeath?.Invoke();

        // === NEW: Award 1 EXP to the player ===
        PlayerStats playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddEXP(experience);
            Debug.Log("Jellyfish killed - Awarded 1 EXP to player");
        }

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        if (col != null) col.enabled = false;

        // Disable TopDamageTrigger too
        TopDamageTrigger topTrigger = GetComponentInChildren<TopDamageTrigger>();
        if (topTrigger != null)
        {
            topTrigger.enabled = false;
            Collider tCol = topTrigger.GetComponent<Collider>();
            if (tCol != null) tCol.enabled = false;
        }

        Debug.Log("Jellyfish died - collider and physics disabled");

        // Instantly despawn the enemy object from the scene
        Destroy(gameObject);
        yield break;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, soundVolume);
    }
}
