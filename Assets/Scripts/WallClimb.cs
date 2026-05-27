using UnityEngine;
using UnityEngine.InputSystem;

public class WallClimb : MonoBehaviour
{
    [Header("Wall Climb Settings")]
    public float climbSpeed = 5f;
    public string climbableTag = "Climbable";
    public float wallCheckDistance = 0.6f;
    public float climbOffGraceTime = 0.5f;

    [Header("Ability")]
    public bool hasAbility = false;

    [Header("State")]
    [HideInInspector] public bool isClimbing = false;

    [Header("Audio")]
    public AudioClip wallClimbClip;
    [Range(0f, 1f)] public float wallClimbVolume = 1f;

    private CharacterController controller;
    private PlayerController playerController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private float lastWallContactTime;
    private AudioSource wallClimbSource;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }

        wallClimbSource = gameObject.AddComponent<AudioSource>();
        wallClimbSource.playOnAwake = false;
        wallClimbSource.loop = true;
        wallClimbSource.spatialBlend = 0f;
    }

    private void OnDisable()
    {
        StopWallClimbSound();
    }

    void Update()
    {
        if (isClimbing)
        {
            HandleClimbing();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hasAbility) return;

        if (hit.gameObject.CompareTag(climbableTag) && !isClimbing)
        {
            StartClimbing();
        }
    }

    private void HandleClimbing()
    {
        // Wall check + grace period
        Vector3 checkOrigin = transform.position + Vector3.up * 0.5f;
        bool touchingWall = Physics.Raycast(checkOrigin, transform.forward, wallCheckDistance);

        if (touchingWall)
        {
            lastWallContactTime = Time.time;
        }
        else
        {
            if (Time.time - lastWallContactTime > climbOffGraceTime)
            {
                StopClimbing();
                return;
            }
        }

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (playerController != null)
            playerController.velocity.y = 0f;

        // Full movement on wall (W/S + A/D)
        Vector3 move = (transform.right * moveInput.x + Vector3.up * moveInput.y) * climbSpeed;
        controller.Move(move * Time.deltaTime);

        if (moveInput.sqrMagnitude > 0.01f)
        {
            PlayWallClimbSound();
        }
        else
        {
            StopWallClimbSound();
        }

        // Space = jump off
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            StopClimbing();
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        lastWallContactTime = Time.time;

        // Do NOT disable PlayerController here
    }

    private void StopClimbing()
    {
        isClimbing = false;
        StopWallClimbSound();

        // Do NOT re-enable PlayerController here
    }

    private void PlayWallClimbSound()
    {
        if (wallClimbClip == null || wallClimbSource == null) return;
        if (wallClimbSource.isPlaying) return;

        wallClimbSource.clip = wallClimbClip;
        wallClimbSource.volume = wallClimbVolume;
        wallClimbSource.Play();
    }

    private void StopWallClimbSound()
    {
        if (wallClimbSource != null)
        {
            wallClimbSource.Stop();
        }
    }
}
