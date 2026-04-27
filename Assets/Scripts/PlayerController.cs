using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Audio")]
    public AudioSource footstepAudioSource;
    public AudioSource jumpAudioSource;
    public AudioClip jumpClip;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public Transform playerCamera;   // Drag your Main Camera (child) here

    // New Input System references
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Link the actions from your PlayerControls asset
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
    }

    void Update()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // === MOVEMENT (Vector3 from Chapter 3) ===
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        controller.Move(move * walkSpeed * Time.deltaTime);

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        bool shouldPlayFootsteps = isGrounded && isMoving;

        if (footstepAudioSource != null)
        {
            if (shouldPlayFootsteps)
            {
                if (!footstepAudioSource.isPlaying)
                {
                    footstepAudioSource.Play();
                }
            }
            else if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
        }

        // === JUMP ===
        if (jumpAction.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (jumpAudioSource != null && jumpClip != null)
            {
                jumpAudioSource.PlayOneShot(jumpClip);
            }
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // === MOUSE LOOK ===
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
