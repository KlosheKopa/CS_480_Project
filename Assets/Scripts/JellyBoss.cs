using System.Collections;
using UnityEngine;
using System;

public class JellyBoss : MonoBehaviour, IBoss
{
    [Header("Explicit Target Setup")]
    [Tooltip("Drag the active Player GameObject from the Hierarchy directly into this slot.")]
    public Transform player;
    public Transform shootPoint;

    [Header("Enemy Settings")]
    public float maxHealth = 80f;
    public float moveSpeed = 4f;
    public float detectionRange = 35f;
    public float minDistanceToPlayer = 12f;
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

    [Header("Death Effects & Audio")]
    public GameObject bubblePopParticles;
    public AudioClip floatingSound;
    public AudioClip detectedSound;
    public AudioClip shootSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private float currentHealth;
    private bool isDead = false;
    private bool isBusy = false;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public event Action OnDeath;

    private Collider col;
    private AudioSource audioSource;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (floatingSound != null)
        {
            audioSource.clip = floatingSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // NO AUTOMATIC SEARCHES: Rely completely on the explicit reference passed in the inspector
        if (player == null)
        {
            Debug.LogError("[JellyBoss] Explicit Player Reference is missing! Please drag your live Player from the Hierarchy into the Inspector slot.", gameObject);
        }
    }

    void Update()
    {
        // Absolute safety guard against unassigned explicit references
        if (player == null || isDead || isBusy) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            StartCoroutine(ExecuteSequenceLoop());
        }
        else
        {
            transform.position += new Vector3(0, Mathf.Sin(Time.time) * 1f, 0) * Time.deltaTime;
        }
    }

    IEnumerator ExecuteSequenceLoop()
    {
        isBusy = true;
        PlaySound(detectedSound);

        // 1. PREPARE & TRACK TARGET
        float timer = 0f;
        while (timer < prepareTime)
        {
            if (isDead || player == null) yield break;

            Vector3 dir = (player.position - transform.position).normalized;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }

            transform.position += new Vector3(0, Mathf.Sin(Time.time * 3f) * 0.2f, 0) * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. SPRAY PROJECTILES
        if (player != null && !isDead)
        {
            int bulletCount = UnityEngine.Random.Range(minBullets, maxBullets + 1);
            for (int i = 0; i < bulletCount; i++)
            {
                if (isDead || player == null) yield break;

                ShootProjectile();
                yield return new WaitForSeconds(timeBetweenBullets);
            }
        }

        // 3. SPACING MOVEMENT & COOLDOWN
        float cooldownTimer = 0f;
        while (cooldownTimer < cooldownTime)
        {
            if (isDead || player == null) yield break;

            float currentDistance = Vector3.Distance(transform.position, player.position);
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            }

            if (currentDistance < minDistanceToPlayer)
            {
                Vector3 backAwayDirection = -directionToPlayer;
                transform.position = Vector3.MoveTowards(transform.position, transform.position + backAwayDirection, moveSpeed * Time.deltaTime);
            }
            else if (currentDistance > minDistanceToPlayer + 2f)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += new Vector3(0, Mathf.Sin(Time.time * 2f) * 0.5f, 0) * Time.deltaTime;
            }

            cooldownTimer += Time.deltaTime;
            yield return null;
        }

        isBusy = false;
    }

    private void ShootProjectile()
    {
        if (inkProjectilePrefab == null || player == null) return;

        Transform spawnLocation = shootPoint != null ? shootPoint : transform;
        Vector3 directionToPlayer = (player.position - spawnLocation.position).normalized;

        Vector3 spawnPos = spawnLocation.position + directionToPlayer * 7f;
        GameObject projectile = Instantiate(inkProjectilePrefab, spawnPos, Quaternion.LookRotation(directionToPlayer));

        PlaySound(shootSound);

        var bulletScript = projectile.GetComponent<InkBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToPlayer);
        }

        Collider bulletCollider = projectile.GetComponent<Collider>();
        if (bulletCollider != null && col != null)
        {
            Physics.IgnoreCollision(bulletCollider, col, true);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        PlaySound(hurtSound);

        if (currentHealth <= 0f)
        {
            isDead = true;
            StopAllCoroutines();
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            if (deathSound != null) audioSource.PlayOneShot(deathSound);
        }

        if (bubblePopParticles != null)
        {
            GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);
            ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
                Destroy(spawnedParticles, totalDuration);
            }
            else
            {
                Destroy(spawnedParticles, 3.0f);
            }
        }

        OnDeath?.Invoke();

        PlayerStats playerStats = GameObject.FindWithTag("Player")?.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.AddEXP(experience);
            Debug.Log($"Jellyfish killed - Awarded {experience} EXP to player");
        }

        if (col != null) col.enabled = false;

        TopDamageTrigger topTrigger = GetComponentInChildren<TopDamageTrigger>();
        if (topTrigger != null)
        {
            topTrigger.enabled = false;
            Collider tCol = topTrigger.GetComponent<Collider>();
            if (tCol != null) tCol.enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        if (deathSound != null)
        {
            yield return new WaitForSeconds(deathSound.length);
        }
        else
        {
            yield return null;
        }

        Destroy(gameObject);
    }


    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, soundVolume);
    }
}
