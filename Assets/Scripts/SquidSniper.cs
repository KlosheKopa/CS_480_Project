using UnityEngine;
using System.Collections;

public class SquidSniper : MonoBehaviour
{
    public enum State { Idle, Preparing, Shooting, Cooldown, Searching }

    [Header("Stats")]
    public float maxHealth = 80f;
    public int expOnDeath = 40;
    public float detectionRange = 50f;
    public float touchDamage = 15f;
    public float touchDamageCooldown = 0.8f;

    [Header("Timings")]
    public float prepareTime = 1.5f;
    public float cooldownTime = 2f;
    public float searchDuration = 4f;
    public float timeBetweenBullets = 0.8f;

    [Header("Shooting")]
    public GameObject inkProjectilePrefab;
    public float projectileSpeed = 18f;
    public int minBullets = 5;
    public int maxBullets = 6;
    public float postShotHoldTime = 1.5f;

    [Header("Turning")]
    public float bodyTurnSpeed = 120f;
    public float flipDuration = 0.06f;
    public float returnDuration = 1.5f;

    [Header("References")]
    public Transform siphon;
    public Transform shootPoint;
    public Transform forwardReference;

    [HideInInspector] public bool isDead = false;

    private Transform player;
    private State currentState = State.Idle;
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 lastKnownPlayerPosition;
    private float currentHealth;
    private float lastTouchDamageTime = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        currentHealth = maxHealth;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (player == null || isDead) return;

        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Searching:
                HandleSearching();
                break;
            case State.Preparing:
                TrackPlayer();
                break;
            case State.Shooting:
                TrackPlayerWhileFlipped();
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Player") && Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(touchDamage);
                lastTouchDamageTime = Time.time;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isDead = true;
            currentState = State.Cooldown;
            StopAllCoroutines();
            StartCoroutine(DeathSequence());
        }
        else
        {
            if (currentState == State.Idle || currentState == State.Searching)
            {
                AlertFromBubble();
            }
        }
    }

    private IEnumerator DeathSequence()
    {
        PlayerStats stats = FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.AddEXP(expOnDeath);
        }

        // Disable ALL colliders on this object and all children (including TopTrigger)
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider c in allColliders)
        {
            c.enabled = false;
        }

        float spinTime = 1.5f;
        float descendTime = 4f;

        float elapsed = 0f;
        Quaternion startRot = transform.rotation;
        Quaternion upsideRot = transform.rotation * Quaternion.Euler(180f, 0f, 0f);

        while (elapsed < spinTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spinTime;
            Quaternion targetRot = Quaternion.Slerp(startRot, upsideRot, t);
            transform.rotation = targetRot * Quaternion.Euler(0f, 360f * t * 1.5f, 0f);
            yield return null;
        }

        elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos;
        targetPos.y -= 1.6f;

        while (elapsed < descendTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / descendTime;
            transform.Rotate(0f, 90f * Time.deltaTime, 0f);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void HandleIdle()
    {
        if (CanSeePlayerFromFront())
        {
            lastKnownPlayerPosition = player.position;
            StartCoroutine(PrepareToShoot());
        }
    }

    private void HandleSearching()
    {
        if (CanSeePlayerFromFront())
        {
            lastKnownPlayerPosition = player.position;
            StartCoroutine(PrepareToShoot());
        }
    }

    private bool CanSeePlayerFromFront()
    {
        if (forwardReference == null || player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange) return false;

        Vector3 dirToPlayer = (player.position - forwardReference.position).normalized;
        float dot = Vector3.Dot(forwardReference.forward, dirToPlayer);

        if (dot < 0.6f) return false;

        if (Physics.Raycast(forwardReference.position, dirToPlayer, out RaycastHit hit, detectionRange))
        {
            return hit.transform.CompareTag("Player");
        }

        return false;
    }

    private IEnumerator PrepareToShoot()
    {
        currentState = State.Preparing;

        float timer = 0f;
        while (timer < prepareTime)
        {
            TrackPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(ShootPhase());
    }

    private IEnumerator ShootPhase()
    {
        currentState = State.Shooting;

        Quaternion startRot = transform.rotation;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Quaternion targetFlip = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(-90f, 0f, 0f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos;
        targetPos.y -= 0.6f;

        float flipT = 0f;
        while (flipT < flipDuration)
        {
            flipT += Time.deltaTime;
            float progress = Mathf.Clamp01(flipT / flipDuration);
            transform.rotation = Quaternion.Slerp(startRot, targetFlip, progress);
            transform.position = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }

        int bulletCount = Random.Range(minBullets, maxBullets + 1);
        for (int i = 0; i < bulletCount; i++)
        {
            ShootInkTowardPlayer();
            yield return new WaitForSeconds(timeBetweenBullets);
        }

        yield return new WaitForSeconds(postShotHoldTime);

        Quaternion currentRot = transform.rotation;
        Quaternion targetReturnRot = Quaternion.Euler(0f, currentRot.eulerAngles.y, currentRot.eulerAngles.z);

        float returnT = 0f;
        while (returnT < returnDuration)
        {
            returnT += Time.deltaTime;
            float progress = returnT / returnDuration;
            transform.rotation = Quaternion.Slerp(currentRot, targetReturnRot, progress);
            transform.position = Vector3.Lerp(transform.position, originalPosition, progress);
            yield return null;
        }

        StartCoroutine(ReturnAndCooldown());
    }

    private void ShootInkTowardPlayer()
    {
        if (inkProjectilePrefab == null || shootPoint == null || player == null || isDead) return;

        Vector3 directionToPlayer = (player.position - shootPoint.position).normalized;
        Vector3 spawnPos = shootPoint.position + directionToPlayer * 0.8f;

        GameObject ink = Instantiate(inkProjectilePrefab, spawnPos, Quaternion.LookRotation(directionToPlayer));

        InkBullet bulletScript = ink.GetComponent<InkBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToPlayer);
        }

        Collider bulletCollider = ink.GetComponent<Collider>();
        Collider squidCollider = GetComponent<Collider>();
        if (bulletCollider != null && squidCollider != null)
        {
            Physics.IgnoreCollision(bulletCollider, squidCollider, true);
        }
    }

    private IEnumerator ReturnAndCooldown()
    {
        currentState = State.Cooldown;

        yield return new WaitForSeconds(cooldownTime);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange && !CanSeePlayerFromFront())
        {
            currentState = State.Searching;
            yield return new WaitForSeconds(searchDuration);

            if (!CanSeePlayerFromFront())
            {
                currentState = State.Idle;
            }
        }
        else
        {
            currentState = State.Idle;
        }
    }

    public void AlertFromBubble()
    {
        if (currentState == State.Shooting || currentState == State.Preparing || isDead) return;

        StartCoroutine(ReactToDamage());
    }

    private IEnumerator ReactToDamage()
    {
        lastKnownPlayerPosition = player.position;
        yield return new WaitForSeconds(1.5f);

        if (currentState == State.Idle || currentState == State.Searching)
        {
            StartCoroutine(PrepareToShoot());
        }
    }

    private void TrackPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * (bodyTurnSpeed / 90f));
        }
    }

    private void TrackPlayerWhileFlipped()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            Quaternion currentYRot = Quaternion.Euler(transform.eulerAngles.x, targetRot.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, currentYRot, Time.deltaTime * (bodyTurnSpeed / 90f));
        }
    }
}