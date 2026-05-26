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

    private CharacterController controller;
    private PlayerController playerController;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private float lastWallContactTime;

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

        // Do NOT re-enable PlayerController here
    }
}