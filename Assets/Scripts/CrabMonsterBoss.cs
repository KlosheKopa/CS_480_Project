using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrabMonsterBoss : MonoBehaviour, IBoss
{
    [Header("Boss Health")]
    public float phaseOneHealth = 120f;
    public float phaseTwoHealth = 120f;
    public int expOnDeath = 75;
    public float phaseTransitionInvulnerability = 1.8f;

    [Header("Movement")]
    public float detectionRange = 25f;
    public float attackRange = 2.4f;
    public float moveSpeed = 3.2f;
    public float turnSpeed = 8f;
    public float groundRayDistance = 8f;
    public float groundOffset = 0.05f;
    public float groundSnapDistance = 0.75f;
    public float minGroundNormalY = 0.3f;
    public float gravity = -25f;
    public float maxFallSpeed = -45f;

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
    public string phaseTransitionTrigger = "Intimidate_1";
    public float intimidateDuration = 1.6f;
    public float takeDamageAnimationCooldown = 0.25f;

    [Header("Phase Materials")]
    public Renderer bodyRenderer;
    public Renderer eyesRenderer;
    public Material phaseOneBodyMaterial;
    public Material phaseOneEyesMaterial;
    public Material phaseTwoBodyMaterial;
    public Material phaseTwoEyesMaterial;

    [Header("Overhead Health Bars")]
    public bool showOverheadHealthBars = true;
    public float healthBarVerticalOffset = 0.8f;
    public float healthBarWidth = 220f;
    public float healthBarHeight = 14f;
    public float healthBarSpacing = 4f;
    public float healthBarWorldScale = 0.02f;
    public Color phaseOneBarColor = new Color(0.9f, 0.12f, 0.08f, 1f);
    public Color phaseTwoBarColor = new Color(0.62f, 0.06f, 0.9f, 1f);
    public Color healthBarBackColor = new Color(0.05f, 0.03f, 0.06f, 0.85f);

    [Header("Audio")]
    public AudioClip intimidateSound;
    public AudioClip walkSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 0.9f;

    public event Action OnDeath;

    [HideInInspector] public bool isDead = false;

    private Animator animator;
    private CapsuleCollider hitCollider;
    private Rigidbody rb;
    private AudioSource audioSource;
    private Transform player;
    private float phaseOneCurrentHealth;
    private float phaseTwoCurrentHealth;
    private float lastDamageTime;
    private float lastTakeAnimationTime;
    private string currentLoopTrigger;
    private bool hasIntimidatedThisChase;
    private bool isIntimidating;
    private bool isTransitioningPhase;
    private bool phaseTwoActive;
    private float verticalVelocity;
    private GameObject healthBarObject;
    private RectTransform phaseOneFill;
    private RectTransform phaseTwoFill;

    public float MaxHealth => phaseOneHealth + phaseTwoHealth;
    public float CurrentHealth => phaseTwoActive ? Mathf.Max(phaseTwoCurrentHealth, 0f) : Mathf.Max(phaseOneCurrentHealth, 0f) + phaseTwoHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ConfigureCollider();
        ConfigureRigidbody();
        ConfigureAudio();
        CacheRenderers();

        phaseOneCurrentHealth = phaseOneHealth;
        phaseTwoCurrentHealth = phaseTwoHealth;

        ApplyPhaseOneMaterials();
        CreateOverheadHealthBars();
        UpdateOverheadHealthBars();
    }

    private void Start()
    {
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
        if (isDead) return;
        if (player == null || isTransitioningPhase)
        {
            ApplyVerticalMotion();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > detectionRange)
        {
            hasIntimidatedThisChase = false;
            StopWalkSound();
            PlayLoop(idleTrigger);
            ApplyVerticalMotion();
            return;
        }

        FacePlayer();

        if (!hasIntimidatedThisChase)
        {
            StartCoroutine(IntimidateBeforeChase());
            ApplyVerticalMotion();
            return;
        }

        if (isIntimidating)
        {
            ApplyVerticalMotion();
            return;
        }

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

        ApplyVerticalMotion();
    }

    private void LateUpdate()
    {
        PositionOverheadHealthBars();
    }

    private void OnDestroy()
    {
        if (healthBarObject != null)
        {
            Destroy(healthBarObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead || isTransitioningPhase || isIntimidating || !hasIntimidatedThisChase || !other.CompareTag("Player")) return;
        TryDamagePlayer(other.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isTransitioningPhase) return;

        if (!phaseTwoActive)
        {
            phaseOneCurrentHealth -= damage;
            Debug.Log($"Crab Monster Boss phase 1 took {damage} damage. HP left: {phaseOneCurrentHealth}/{phaseOneHealth}");

            if (phaseOneCurrentHealth <= 0f)
            {
                phaseOneCurrentHealth = 0f;
                UpdateOverheadHealthBars();
                StartCoroutine(EnterPhaseTwo());
                return;
            }
        }
        else
        {
            phaseTwoCurrentHealth -= damage;
            Debug.Log($"Crab Monster Boss phase 2 took {damage} damage. HP left: {phaseTwoCurrentHealth}/{phaseTwoHealth}");

            if (phaseTwoCurrentHealth <= 0f)
            {
                phaseTwoCurrentHealth = 0f;
                UpdateOverheadHealthBars();
                StartCoroutine(DeathSequence());
                return;
            }
        }

        UpdateOverheadHealthBars();
        PlayHurtFeedback();
    }

    private IEnumerator EnterPhaseTwo()
    {
        if (phaseTwoActive) yield break;

        isTransitioningPhase = true;
        phaseTwoActive = true;
        currentLoopTrigger = null;

        StopWalkSound();
        PlayTrigger(phaseTransitionTrigger);
        PlaySound(intimidateSound);

        yield return new WaitForSeconds(phaseTransitionInvulnerability);

        ApplyPhaseTwoMaterials();
        isTransitioningPhase = false;
        PlayLoop(idleTrigger);
    }

    private void ConfigureCollider()
    {
        hitCollider = GetComponent<CapsuleCollider>();
        if (hitCollider == null) hitCollider = gameObject.AddComponent<CapsuleCollider>();
        hitCollider.isTrigger = true;
        hitCollider.center = colliderCenter;
        hitCollider.radius = colliderRadius;
        hitCollider.height = colliderHeight;
    }

    private void ConfigureRigidbody()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void ConfigureAudio()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void CacheRenderers()
    {
        if (bodyRenderer == null) bodyRenderer = FindRendererByName("Rikayon");
        if (eyesRenderer == null) eyesRenderer = FindRendererByName("Eyes");
    }

    private Renderer FindRendererByName(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name != childName) continue;

            Renderer childRenderer = child.GetComponent<Renderer>();
            if (childRenderer != null) return childRenderer;
        }

        return null;
    }

    private void ApplyPhaseOneMaterials()
    {
        ApplyMaterial(bodyRenderer, phaseOneBodyMaterial);
        ApplyMaterial(eyesRenderer, phaseOneEyesMaterial);
    }

    private void ApplyPhaseTwoMaterials()
    {
        ApplyMaterial(bodyRenderer, phaseTwoBodyMaterial);
        ApplyMaterial(eyesRenderer, phaseTwoEyesMaterial);
    }

    private void ApplyMaterial(Renderer targetRenderer, Material material)
    {
        if (targetRenderer == null || material == null) return;
        targetRenderer.sharedMaterial = material;
    }

    private void CreateOverheadHealthBars()
    {
        if (!showOverheadHealthBars || healthBarObject != null) return;

        healthBarObject = new GameObject($"{name}_OverheadHealthBars", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = healthBarObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rootRect = healthBarObject.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight * 2f + healthBarSpacing);

        phaseOneFill = CreateHealthBar(rootRect, "Phase1Health", 0f, phaseOneBarColor);
        phaseTwoFill = CreateHealthBar(rootRect, "Phase2Health", -(healthBarHeight + healthBarSpacing), phaseTwoBarColor);
    }

    private RectTransform CreateHealthBar(RectTransform parent, string barName, float yOffset, Color fillColor)
    {
        GameObject backgroundObject = new GameObject(barName, typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(parent, false);

        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(0f, yOffset);
        backgroundRect.sizeDelta = new Vector2(healthBarWidth, healthBarHeight);

        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.color = healthBarBackColor;

        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(backgroundObject.transform, false);

        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(0f, 1f);
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = new Vector2(healthBarWidth, 0f);

        Image fillImage = fillObject.GetComponent<Image>();
        fillImage.color = fillColor;

        return fillRect;
    }

    private void PositionOverheadHealthBars()
    {
        if (healthBarObject == null) return;

        if (TryGetRendererBounds(out Bounds renderBounds))
        {
            healthBarObject.transform.position = renderBounds.center + Vector3.up * (renderBounds.extents.y + healthBarVerticalOffset);
        }
        else
        {
            healthBarObject.transform.position = transform.position + Vector3.up * 4f;
        }

        Camera targetCamera = Camera.main;
        if (targetCamera != null)
        {
            healthBarObject.transform.rotation = Quaternion.LookRotation(healthBarObject.transform.position - targetCamera.transform.position);
        }

        healthBarObject.transform.localScale = Vector3.one * healthBarWorldScale;
    }

    private bool TryGetRendererBounds(out Bounds bounds)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bounds = new Bounds(transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer childRenderer in renderers)
        {
            if (childRenderer == null || !childRenderer.enabled) continue;

            if (!hasBounds)
            {
                bounds = childRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(childRenderer.bounds);
            }
        }

        return hasBounds;
    }

    private void UpdateOverheadHealthBars()
    {
        SetFillWidth(phaseOneFill, phaseOneCurrentHealth, phaseOneHealth);
        SetFillWidth(phaseTwoFill, phaseTwoCurrentHealth, phaseTwoHealth);
    }

    private void SetFillWidth(RectTransform fillRect, float current, float max)
    {
        if (fillRect == null) return;

        float normalized = max <= 0f ? 0f : Mathf.Clamp01(current / max);
        fillRect.sizeDelta = new Vector2(healthBarWidth * normalized, 0f);
    }

    private void MoveTowardPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
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
        if (!TryFindGround(out Vector3 groundPoint)) return;

        transform.position = new Vector3(transform.position.x, groundPoint.y + groundOffset, transform.position.z);
        verticalVelocity = 0f;
    }

    private void ApplyVerticalMotion()
    {
        if (TryFindGround(out Vector3 groundPoint))
        {
            float targetY = groundPoint.y + groundOffset;
            float yDifference = transform.position.y - targetY;

            if (yDifference <= groundSnapDistance)
            {
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                verticalVelocity = 0f;
                return;
            }
        }

        verticalVelocity = Mathf.Max(verticalVelocity + gravity * Time.deltaTime, maxFallSpeed);
        transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
    }

    private bool TryFindGround(out Vector3 groundPoint)
    {
        Vector3 rayStart = transform.position + Vector3.up * 2f;
        RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, groundRayDistance);
        float closestDistance = float.MaxValue;
        groundPoint = transform.position;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.normal.y < minGroundNormalY) continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                groundPoint = hit.point;
            }
        }

        return closestDistance < float.MaxValue;
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
        if (healthBarObject != null) healthBarObject.SetActive(false);

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

    private void PlayHurtFeedback()
    {
        if (Time.time < lastTakeAnimationTime + takeDamageAnimationCooldown) return;

        PlayTrigger(RandomTakeDamageTrigger());
        PlaySound(hurtSound);
        lastTakeAnimationTime = Time.time;
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
