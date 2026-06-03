using System.Collections;
using UnityEngine;

public class Leviathan : MonoBehaviour, IBoss
{
    [Header("References")]
    public Transform player;
    private Animator animator;

    [Header("Movement Settings")]
    public float normalSwimSpeed = 3f;
    public float fastSwimSpeed = 14f;
    public float rotationSpeed = 4f;

    [Header("AI Ranges")]
    public float detectionRange = 25f;
    public float attackRange = 4f;

    [Header("Combat Settings")]
    public float maxHealth = 500;
    private float currentHealth;
    public float firstAttackDuration = 1.5f;
    public float secondAttackDuration = 1.5f;

    [Header("Slashing Wave Layout Customization")]
    public GameObject slashPrefab;
    [Tooltip("How many meters out in front of the monster the slash should appear")]
    public float slashSpawnForwardOffset = 6f;

    [Header("Diagonal Slashing Adjustments")]
    [Tooltip("How many degrees to tilt the slashes diagonally (Z-axis roll)")]
    public float diagonalTiltAngle = 30f;
    [Tooltip("How many meters to drop the 1st slash down relative to the boss's chest")]
    public float firstAttackHeightDrop = 2.0f;
    [Tooltip("How many degrees to pitch the 1st slash downward to fix the high aim")]
    public float firstAttackPitchAngle = 15f;
    private bool isSecondAttack = false;

    [Header("Anchor & Depth Bounds")]
    public Transform anchorObject;
    public float maxAnchorRadius = 25f;
    public float minimumDepthFloor = 0f;

    [Header("Audio")]
    public AudioClip patrolSwimClip;
    public AudioClip fastSwimClip;
    public AudioClip attackClip;
    public AudioClip deathClip;
    public AudioClip bubbleHitClip;
    [Range(0f, 1f)] public float loopVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Vector3 wanderTarget;
    private float minWanderDistance = 3f;
    private bool isBusy = false;
    private AudioSource loopAudioSource;
    private AudioSource sfxAudioSource;
    private int lastObservedStateHash;
    private bool deathSoundPlayed;

    private static readonly int PatrolSwimState = Animator.StringToHash("patrol_swimF");
    private static readonly int FastSwimState = Animator.StringToHash("fast_swimF");
    private static readonly int AttackState = Animator.StringToHash("attack");
    private static readonly int DeathState = Animator.StringToHash("ded");

    private enum BossState { Patrol, Dead }
    private BossState currentState = BossState.Patrol;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentState == BossState.Dead;

#if UNITY_EDITOR
    private const string PatrolSoundPath = "Assets/Audio/enemy_sounds/Leviathan_sounds/patrol_swimF.mp3";
    private const string FastSwimSoundPath = "Assets/Audio/enemy_sounds/Leviathan_sounds/fast_swimF.mp3";
    private const string AttackSoundPath = "Assets/Audio/enemy_sounds/Leviathan_sounds/levi_attack.mp3";
    private const string DeathSoundPath = "Assets/Audio/enemy_sounds/Leviathan_sounds/Levi_die.mp3";
    private const string HitSoundPath = "Assets/Audio/enemy_sounds/Leviathan_sounds/levi_get_hit.mp3";

    private void Reset()
    {
        AssignDefaultSounds();
    }

    private void OnValidate()
    {
        AssignDefaultSounds();
    }

    private void AssignDefaultSounds()
    {
        if (patrolSwimClip == null)
            patrolSwimClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(PatrolSoundPath);
        if (fastSwimClip == null)
            fastSwimClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(FastSwimSoundPath);
        if (attackClip == null)
            attackClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(AttackSoundPath);
        if (deathClip == null)
            deathClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DeathSoundPath);
        if (bubbleHitClip == null)
            bubbleHitClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(HitSoundPath);
    }
#endif

    void Awake()
    {
        animator = GetComponent<Animator>();
        ConfigureAudioSources();
    }

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        if (player == null) player = GameObject.FindWithTag("Player")?.transform;

        if (anchorObject == null)
        {
            GameObject anchorFallback = new GameObject("Leviathan_Anchor_Fallback");
            anchorFallback.transform.position = transform.position;
            anchorObject = anchorFallback.transform;
        }

        SetNewWanderTarget();
        SyncAudioWithAnimatorState();
    }

    void Update()
    {
        if (currentState == BossState.Dead) return;
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        SyncAudioWithAnimatorState();

        if (isBusy) return;

        float currentDistanceToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        if (currentDistanceToPlayer <= detectionRange && player != null)
        {
            StartCoroutine(ExecuteSequenceLoop());
        }
        else
        {
            LookAtTarget(wanderTarget);
            MoveForward(normalSwimSpeed);

            if (Vector3.Distance(transform.position, wanderTarget) < minWanderDistance) SetNewWanderTarget();
        }
    }

    IEnumerator ExecuteSequenceLoop()
    {
        isBusy = true;
        isSecondAttack = false;

        // 1. SNAPSHOT & PREPARE CHARGE
        Vector3 initialTargetSpot = player.position;
        if (initialTargetSpot.y < minimumDepthFloor) initialTargetSpot.y = minimumDepthFloor;

        Vector3 targetDirection = (initialTargetSpot - transform.position).normalized;
        if (targetDirection != Vector3.zero) transform.rotation = Quaternion.LookRotation(targetDirection);

        animator.ResetTrigger("AttackDone");
        animator.SetTrigger("Charge");

        // 2. THE STRAIGHT CHARGE PHASE
        float timeoutTimer = 0f;
        while (Vector3.Distance(transform.position, initialTargetSpot) > attackRange && timeoutTimer < 3.0f)
        {
            timeoutTimer += Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, initialTargetSpot, fastSwimSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. FIRST ATTACK 
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(firstAttackDuration);
        animator.SetTrigger("AttackDone");

        // 4. THE TRACKING TURN (Stationary Pivot)
        if (player != null)
        {
            float turnTimer = 0f;
            while (turnTimer < 0.5f)
            {
                turnTimer += Time.deltaTime;
                Vector3 lookDir = (player.position - transform.position).normalized;
                if (lookDir != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * 2f);
                }
                yield return null;
            }
        }
        isSecondAttack = true;

        // 5. SECOND ATTACK
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("AttackDone");

        // Using crossfade ensures the animation never goes missing due to transition lag
        animator.CrossFadeInFixedTime("attack", 0.05f);

        yield return new WaitForSeconds(secondAttackDuration);
        animator.SetTrigger("AttackDone");

        // 6. FORCE IDLE SWIM DISENGAGEMENT
        SetNewWanderTarget();
        float breakTimer = 0f;
        while (breakTimer < 4.0f)
        {
            breakTimer += Time.deltaTime;
            LookAtTarget(wanderTarget);
            MoveForward(normalSwimSpeed);
            yield return null;
        }

        isBusy = false;
    }

    // FIXED: This is now an Animation Event callback function.
    // It will be triggered directly by the animation timeline at a pixel-perfect frame!
    public void TriggerClawSlashVFX()
    {
        if (slashPrefab == null) return;

        // 1. Establish the baseline position forward down the monster's line of sight
        Vector3 spawnPosition = transform.position + (transform.forward * slashSpawnForwardOffset);

        // 2. Extract the base rotation Euler angles from the monster's heading
        Vector3 currentEuler = transform.rotation.eulerAngles;

        if (!isSecondAttack)
        {
            // === FIRST ATTACK CONFIGURATION ===
            // Apply a pitch correction (X-axis) to point it lower, and tilt it diagonally down-left (Z-axis)
            spawnPosition += (-transform.up * firstAttackHeightDrop);
            currentEuler.x += firstAttackPitchAngle;
            currentEuler.z += diagonalTiltAngle;
        }
        else
        {
            // === SECOND ATTACK CONFIGURATION ===
            // Keep pitch standard, but tilt the Z-axis in the opposite direction (down-right) to form an "X" combo pattern
            currentEuler.z -= diagonalTiltAngle;
        }

        // 3. Convert the modified Euler values back into a clean Quaternion rotation matrix
        Quaternion finalSlashRotation = Quaternion.Euler(currentEuler);

        // 4. Instantiate the angled, corrected prefab instance into the world space grid
        Instantiate(slashPrefab, spawnPosition, finalSlashRotation);
    }

    void SetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * maxAnchorRadius;
        Vector3 potentialTarget = anchorObject.position + randomDirection;
        if (potentialTarget.y < minimumDepthFloor) potentialTarget.y = minimumDepthFloor;
        wanderTarget = potentialTarget;
    }

    void MoveForward(float speed)
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        if (transform.position.y < minimumDepthFloor)
        {
            Vector3 clampedPos = transform.position;
            clampedPos.y = minimumDepthFloor;
            transform.position = clampedPos;
        }
    }

    void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    public void TakeBubbleDamage(float damage)
    {
        TakeDamage(damage);

        if (currentHealth > 0)
            PlayOneShot(bubbleHitClip);
    }

    private void ConfigureAudioSources()
    {
        loopAudioSource = gameObject.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;
        loopAudioSource.loop = true;
        loopAudioSource.spatialBlend = 1f;

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.loop = false;
        sfxAudioSource.spatialBlend = 1f;
    }

    private void SyncAudioWithAnimatorState()
    {
        if (animator == null) return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        int stateHash = stateInfo.shortNameHash;
        if (stateHash == lastObservedStateHash) return;

        lastObservedStateHash = stateHash;

        if (stateHash == PatrolSwimState)
        {
            PlayLoop(patrolSwimClip);
        }
        else if (stateHash == FastSwimState)
        {
            PlayLoop(fastSwimClip);
        }
        else if (stateHash == AttackState)
        {
            StopLoop();
            PlayOneShot(attackClip);
        }
        else if (stateHash == DeathState)
        {
            StopLoop();
            PlayDeathSound();
        }
        else
        {
            StopLoop();
        }
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null || loopAudioSource == null) return;
        if (loopAudioSource.clip == clip && loopAudioSource.isPlaying) return;

        loopAudioSource.clip = clip;
        loopAudioSource.volume = loopVolume;
        loopAudioSource.Play();
    }

    private void StopLoop()
    {
        if (loopAudioSource != null && loopAudioSource.isPlaying)
            loopAudioSource.Stop();
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null || sfxAudioSource == null) return;

        sfxAudioSource.volume = sfxVolume;
        sfxAudioSource.PlayOneShot(clip);
    }

    private void PlayDeathSound()
    {
        if (deathSoundPlayed) return;

        deathSoundPlayed = true;
        PlayOneShot(deathClip);
    }

    private void Die()
    {
        currentState = BossState.Dead;
        StopLoop();
        PlayDeathSound();
        animator.SetTrigger("Death");
        Destroy(gameObject, deathClip != null ? Mathf.Max(3f, deathClip.length) : 3f);
    }
}
