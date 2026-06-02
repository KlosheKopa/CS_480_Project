using UnityEngine;
using System.Collections;

public class Starfish : MonoBehaviour
{
    public enum StarfishState { Flat, Active, ReturningToFlat }

    [Header("Starfish Settings")]
    public float detectionRange = 20f;
    public float standUpTime = 2.2f;
    public float facePlayerTime = 1.5f;
    public float windUpTime = 1.5f;
    public float chargeDistance = 33.6f;
    public float chargeDuration = 7.0f;

    [Header("Ground Contact")]
    public float groundOffset = 0.25f;

    [Header("Health")]
    public float maxHealth = 50f;

    [Header("Damage to Player")]
    public float damageToPlayer = 15f;

    [Header("Death Settings")]
    public float deathSpinTime = 3f;
    public float deathFallTime = 3f;
    public float despawnDelay = 8f;
    public int expOnDeath = 3;

    private Transform player;
    private StarfishState currentState = StarfishState.Flat;
    private bool isDead = false;
    private float currentHealth;
    private float groundY;
    private float initialGroundY;
    private bool isFalling = false;
    private bool hasActivated = false;

    private Vector3 phase2Position;
    private Vector3 lockedChargeDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = maxHealth;
        currentState = StarfishState.Flat;

        FindGroundImmediately();
        initialGroundY = groundY;

        if (transform.position.y > groundY + groundOffset + 0.3f)
        {
            transform.position = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }
        else
        {
            isFalling = true;
            StartCoroutine(FallToFlat());
        }
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!hasActivated && !isFalling && currentState == StarfishState.Flat && distance <= detectionRange)
        {
            hasActivated = true;
            currentState = StarfishState.Active;
            StartCoroutine(PerformAttackSequence());
        }
        else if (!isFalling && currentState == StarfishState.Active && distance > detectionRange)
        {
            currentState = StarfishState.ReturningToFlat;
            StartCoroutine(FallToFlat());
        }
    }

    private void FindGroundImmediately()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position + Vector3.up * 100f, Vector3.down, 200f);
        float highestY = float.MinValue;

        foreach (RaycastHit h in hits)
        {
            if (h.collider.gameObject == gameObject) continue;
            if (h.point.y > highestY && h.point.y < transform.position.y)
            {
                highestY = h.point.y;
            }
        }

        groundY = (highestY > float.MinValue) ? highestY : transform.position.y - 10f;
    }

    private IEnumerator FallToFlat()
    {
        isFalling = true;
        FindGroundImmediately();

        RaycastHit hit;
        float targetY = groundY + groundOffset;
        if (Physics.Raycast(transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f))
        {
            groundY = hit.point.y;
            targetY = hit.point.y + groundOffset;
        }

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        float elapsed = 0f;
        float duration = 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.rotation = Quaternion.Lerp(startRot, targetRot, smoothT);
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        transform.rotation = targetRot;
        transform.position = targetPos;

        isFalling = false;
        currentState = StarfishState.Flat;

        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator PerformAttackSequence()
    {
        currentState = StarfishState.Active;

        FindGroundImmediately();
        transform.position = new Vector3(transform.position.x, groundY + groundOffset, transform.position.z);

        Quaternion flatRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.rotation = flatRot;

        float elapsed = 0f;
        Quaternion uprightRot = Quaternion.Euler(0f, transform.eulerAngles.y, 90f);

        while (elapsed < standUpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / standUpTime;

            float lift = Mathf.Lerp(0f, 1.8f, t);
            transform.position = new Vector3(transform.position.x, groundY + lift, transform.position.z);
            transform.rotation = Quaternion.Lerp(flatRot, uprightRot, t);

            yield return null;
        }

        while (true)
        {
            elapsed = 0f;
            while (elapsed < facePlayerTime)
            {
                elapsed += Time.deltaTime;
                Vector3 direction = (player.position - transform.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(direction);
                targetRot = Quaternion.Euler(0f, targetRot.eulerAngles.y, 90f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, elapsed / facePlayerTime);
                yield return null;
            }

            phase2Position = transform.position;
            lockedChargeDirection = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up).normalized;

            float actualWindUp = windUpTime > 0.1f ? windUpTime : 2.5f;
            elapsed = 0f;
            while (elapsed < actualWindUp)
            {
                elapsed += Time.deltaTime;
                float spinSpeed = Mathf.Lerp(300f, 1200f, elapsed / actualWindUp);
                transform.Rotate(spinSpeed * Time.deltaTime, 0f, 0f);
                yield return null;
            }

            Vector3 chargeDirection = lockedChargeDirection;
            float chargeStartGroundY = initialGroundY;
            elapsed = 0f;

            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 90f);

            while (elapsed < chargeDuration)
            {
                elapsed += Time.deltaTime;

                Vector3 horizontalDir = new Vector3(chargeDirection.x, 0f, chargeDirection.z).normalized;

                RaycastHit wallHit;
                bool hitWall = Physics.Raycast(transform.position, horizontalDir, out wallHit, 0.6f)
                               && !wallHit.collider.CompareTag("Player");

                if (!hitWall)
                {
                    Vector3 move = horizontalDir * (chargeDistance / chargeDuration) * Time.deltaTime;
                    transform.position += move;
                }

                float currentGroundY = chargeStartGroundY;
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position + Vector3.up * 4f, Vector3.down, out groundHit, 8f))
                {
                    if (groundHit.point.y < currentGroundY)
                        currentGroundY = groundHit.point.y;
                }

                transform.position = new Vector3(transform.position.x, currentGroundY + groundOffset, transform.position.z);
                transform.Rotate(-1600f * Time.deltaTime, 0f, 0f);
                yield return null;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damageToPlayer);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"⭐ Starfish took {damage} damage. HP left: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            isDead = true;

            PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddEXP(expOnDeath);
            }

            StopAllCoroutines();
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        FindGroundImmediately();

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * 0.65f;
        Vector3 finalPos = new Vector3(startPos.x, groundY + groundOffset, startPos.z);

        float elapsed = 0f;

        while (elapsed < deathFallTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathFallTime;

            // Reverse flip (negative direction)
            float spin = Mathf.Lerp(0f, 720f, t);
            transform.rotation = startRot * Quaternion.Euler(-spin, 0f, 0f);   // ← Reversed direction

            if (t < 0.35f)
            {
                float upT = t / 0.35f;
                transform.position = Vector3.Lerp(startPos, peakPos, upT);
            }
            else
            {
                float downT = (t - 0.35f) / 0.65f;
                transform.position = Vector3.Lerp(peakPos, finalPos, downT);
            }

            yield return null;
        }

        transform.rotation = targetRot;
        transform.position = finalPos;

        yield return new WaitForSeconds(despawnDelay - deathFallTime);
        Destroy(gameObject);
    }
}