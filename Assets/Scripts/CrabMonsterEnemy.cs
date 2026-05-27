using System;
using System.Collections;
using UnityEngine;

public class CrabMonsterEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 120f;
    public int expOnDeath = 25;

    [Header("Movement")]
    public float detectionRange = 25f;
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

    public event Action OnDeath;

    [HideInInspector] public bool isDead = false;

    private Animator animator;
    private CapsuleCollider hitCollider;
    private Rigidbody rb;
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
        }
        else
        {
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
        playerHealth.TakeDamage(touchDamage);
        lastDamageTime = Time.time;
    }

    private IEnumerator IntimidateBeforeChase()
    {
        if (isIntimidating) yield break;

        hasIntimidatedThisChase = true;
        isIntimidating = true;
        currentLoopTrigger = null;

        PlayTrigger(RandomIntimidateTrigger());

        yield return new WaitForSeconds(intimidateDuration);
        isIntimidating = false;
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (hitCollider != null) hitCollider.enabled = false;

        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.AddEXP(expOnDeath);
        }

        PlayTrigger(deathTrigger);

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
}
