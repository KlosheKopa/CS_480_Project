using System;
using System.Collections;
using UnityEngine;

public class CrabMonsterEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 120f;
    public int expOnDeath = 25;

    [Header("Movement")]
    public float detectionRange = 10f;
    public float attackRange = 2.4f;
    public float moveSpeed = 3.2f;
    public float turnSpeed = 8f;
    public float groundRayDistance = 8f;
    public float groundOffset = 0.05f;

    [Header("Damage")]
    public float touchDamage = 18f;
    public float touchDamageCooldown = 1f;

    [Header("Collider")]
    public Vector3 colliderCenter = new Vector3(0f, 1f, 0f);
    public float colliderRadius = 1.3f;
    public float colliderHeight = 2.2f;

    [Header("Animation Triggers")]
    public string idleTrigger = "Fight_Idle_1";
    public string walkTrigger = "Walk_Cycle_1";
    public string deathTrigger = "Die";
    public float intimidateDuration = 1.6f;
    public float takeDamageAnimationCooldown = 0.25f;

    [Header("Audio")]
    public AudioClip intimidateSound;
    public AudioClip walkSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    public event Action OnDeath;

    [HideInInspector] public bool isDead = false;

    private Animator animator;
    private CapsuleCollider hitCollider;
    private Rigidbody rb;
    private AudioSource audioSource;
    private Transform player;
    private float currentHealth;
    private float lastDamageTime;
    private float lastTakeAnimationTime;
    private string currentLoopTrigger;
    private bool hasIntimidatedThisChase;
    private bool isIntimidating;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        hitCollider = GetComponent<CapsuleCollider>();
        if (hitCollider == null) hitCollider = gameObject.AddComponent<CapsuleCollider>();
        hitCollider.isTrigger = true;
        hitCollider.center = colliderCenter;
        hitCollider.radius = colliderRadius;
        hitCollider.height = colliderHeight;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        PlayLoop(idleTrigger);
        SnapToGround();
    }

    private void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange)
        {
            hasIntimidatedThisChase = false;
            StopWalkSound();
            PlayLoop(idleTrigger);
            return;
        }

        FacePlayer();

        if (!hasIntimidatedThisChase)
        {
            StartCoroutine(IntimidateBeforeChase());
            return;
        }

        if (isIntimidating) return;

        if (distance > attackRange)
        {
            MoveTowardPlayer();
            PlayLoop(walkTrigger);
            StartWalkSound();
        }
        else
        {
            StopWalkSound();
            TryDamagePlayer(player.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead || isIntimidating || !hasIntimidatedThisChase || !other.CompareTag("Player")) return;
        TryDamagePlayer(other.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Crab Monster took {damage} damage. HP left: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            StartCoroutine(DeathSequence());
            return;
        }

        if (Time.time >= lastTakeAnimationTime + takeDamageAnimationCooldown)
        {
            PlayTrigger(RandomTakeDamageTrigger());
            PlaySound(hurtSound);
            lastTakeAnimationTime = Time.time;
        }
    }

    private void MoveTowardPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        SnapToGround();
    }

    private void FacePlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    private void SnapToGround()
    {
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, groundRayDistance);
        float closestDistance = float.MaxValue;
        bool foundGround = false;
        Vector3 groundPoint = transform.position;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform)) continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                groundPoint = hit.point;
                foundGround = true;
            }
        }

        if (foundGround)
        {
            transform.position = new Vector3(transform.position.x, groundPoint.y + groundOffset, transform.position.z);
        }
    }

    private void TryDamagePlayer(GameObject playerObject)
    {
        if (Time.time < lastDamageTime + touchDamageCooldown) return;

        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth == null || playerHealth.isDead) return;

        PlayTrigger(RandomAttackTrigger());
        PlaySound(attackSound);
        playerHealth.TakeDamage(touchDamage);
        lastDamageTime = Time.time;
    }

    private IEnumerator IntimidateBeforeChase()
    {
        if (isIntimidating) yield break;

        hasIntimidatedThisChase = true;
        isIntimidating = true;
        currentLoopTrigger = null;

        StopWalkSound();
        PlayTrigger(RandomIntimidateTrigger());
        PlaySound(intimidateSound);

        yield return new WaitForSeconds(intimidateDuration);
        isIntimidating = false;
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        OnDeath?.Invoke();

        StopWalkSound();
        if (hitCollider != null) hitCollider.enabled = false;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.AddEXP(expOnDeath);
        }

        PlayTrigger(deathTrigger);
        PlaySound(deathSound);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    private void PlayLoop(string triggerName)
    {
        if (currentLoopTrigger == triggerName) return;
        PlayTrigger(triggerName);
        currentLoopTrigger = triggerName;
    }

    private void PlayTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName)) return;
        animator.SetTrigger(triggerName);
    }

    private void StartWalkSound()
    {
        if (audioSource == null || walkSound == null) return;
        if (audioSource.isPlaying && audioSource.clip == walkSound) return;

        audioSource.clip = walkSound;
        audioSource.loop = true;
        audioSource.volume = soundVolume;
        audioSource.Play();
    }

    private void StopWalkSound()
    {
        if (audioSource == null || audioSource.clip != walkSound) return;

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip, soundVolume);
    }

    private string RandomAttackTrigger()
    {
        return $"Attack_{UnityEngine.Random.Range(1, 6)}";
    }

    private string RandomIntimidateTrigger()
    {
        return $"Intimidate_{UnityEngine.Random.Range(1, 4)}";
    }

    private string RandomTakeDamageTrigger()
    {
        return $"Take_Damage_{UnityEngine.Random.Range(1, 4)}";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
