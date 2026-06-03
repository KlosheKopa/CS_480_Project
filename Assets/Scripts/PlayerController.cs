using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;

    [Header("Dash")]
    public float dashSpeed = 84f;
    public float dashDuration = 0.35f;

    [Header("Bounce")]
    public float bounceDuration = 0.25f;

    [Header("Pause Menu")]
    public GameObject pausePanel;

    [Header("Stats Screen")]
    public ShowPlayerStatsUI statsUI;

    [Header("Double Jump")]
    public bool hasDoubleJump = false;
    private bool canDoubleJump = false;

    [Header("Key System")]
    public bool hasKey = false;
    public GameObject keyUI;

    private CharacterController controller;
    [HideInInspector] public Vector3 velocity;
    private bool isGrounded;

    public Transform playerCamera;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    private InputAction giveEXPAction;
    private InputAction showStatsAction;

    private PlayerHealth playerHealth;
    private PlayerStats stats;

    private float xRotation = 0f;
    private float dashTimeRemaining = 0f;
    private Vector3 currentDashVelocity;
    private float bounceTimeRemaining = 0f;
    private Vector3 currentBounceVelocity;

    private bool isPaused = false;
    private Vector3 startPosition;
    private bool cameraInitialized = false;
    private bool hasAirDashed = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerHealth = GetComponent<PlayerHealth>();
        stats = GetComponent<PlayerStats>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        pauseAction = playerInput.actions["Pause"];
        giveEXPAction = playerInput.actions["GiveEXP"];
        showStatsAction = playerInput.actions["ShowStats"];
    }

    void OnEnable() => showStatsAction.Enable();
    void OnDisable() => showStatsAction.Disable();

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        startPosition = transform.position;

        if (keyUI != null) keyUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!cameraInitialized && playerCamera != null)
        {
            cameraInitialized = true;
            xRotation = 0f;
            playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    void Update()
    {
        if (playerHealth != null && playerHealth.isDead) return;

        WallClimb wallClimb = GetComponent<WallClimb>();
        if (wallClimb != null && wallClimb.isClimbing)
        {
            goto MouseLookOnly;
        }

        if (transform.position.y <= -10f)
        {
            transform.position = startPosition;
            velocity = Vector3.zero;
            return;
        }

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            canDoubleJump = true;
            hasAirDashed = false;
        }

        if (pauseAction.WasPressedThisFrame())
        {
            if (LevelUpManager.Instance != null && LevelUpManager.Instance.levelUpPanel.activeSelf)
                return;

            if (playerHealth != null && playerHealth.isDead) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            if (pausePanel != null) pausePanel.SetActive(isPaused);
        }

        if (isPaused) return;

        if (giveEXPAction.WasPressedThisFrame())
        {
            if (stats != null) stats.AddEXP(100);
        }

        if (showStatsAction.IsPressed())
        {
            if (statsUI != null) statsUI.ShowStatsPanel();
        }
        else
        {
            if (statsUI != null) statsUI.HideStatsPanel();
        }

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (jumpAction.triggered)
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (hasDoubleJump && canDoubleJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                canDoubleJump = false;
            }
        }

        velocity.y += gravity * Time.deltaTime;

        if (dashTimeRemaining > 0)
        {
            dashTimeRemaining -= Time.deltaTime;
            controller.Move(currentDashVelocity * Time.deltaTime);
            currentDashVelocity = Vector3.Lerp(currentDashVelocity, Vector3.zero, Time.deltaTime * 8f);
        }

        if (bounceTimeRemaining > 0)
        {
            bounceTimeRemaining -= Time.deltaTime;
            controller.Move(currentBounceVelocity * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);

    MouseLookOnly:

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ====================== DASH SYSTEM ======================
    public bool TryPerformDash()
    {
        if (!isGrounded && hasAirDashed)
            return false; // Already used air dash

        if (!isGrounded)
            hasAirDashed = true;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 dashDirection = (moveInput.sqrMagnitude < 0.1f)
            ? -transform.forward
            : (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;

        float finalDashSpeed = dashSpeed * (stats != null ? stats.dashDistanceMultiplier : 1f);
        currentDashVelocity = dashDirection * finalDashSpeed;
        dashTimeRemaining = dashDuration;

        return true; // Dash executed successfully
    }

    public void PerformDash()
    {
        TryPerformDash();
    }

    public void BounceBack(Vector3 bounceVelocity)
    {
        currentBounceVelocity = bounceVelocity;
        currentBounceVelocity.y = 4f;
        bounceTimeRemaining = bounceDuration;
    }

    public void UnlockDoubleJump()
    {
        hasDoubleJump = true;
    }

    // ====================== KEY + DOOR DETECTION ======================
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("LockedDoor"))
        {
            LockedDoor door = hit.gameObject.GetComponent<LockedDoor>();
            if (door != null && !door.isOpen)
            {
                if (hasKey)
                {
                    // Has key → open the door (no text)
                    hasKey = false;

                    if (keyUI != null)
                        keyUI.SetActive(false);

                    door.OpenTheDoor();
                }
                else
                {
                    // No key → show locked message
                    DoorLockedPrompt prompt = FindFirstObjectByType<DoorLockedPrompt>();
                    if (prompt != null)
                        prompt.ShowLockedMessage();
                }
            }
        }
    }
}