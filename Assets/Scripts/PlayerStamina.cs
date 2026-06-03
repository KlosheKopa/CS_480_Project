using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerStamina : MonoBehaviour
{
    [Header("UI")]
    public Slider staminaBar;

    private PlayerStats stats;
    private InputAction dashAction;
    private float regenPauseTimer = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        dashAction = GetComponentInParent<PlayerInput>().actions["Dash"];
    }

    void OnEnable() => dashAction.Enable();
    void OnDisable() => dashAction.Disable();

    void Start()
    {
        if (stats != null)
            stats.CurrentStamina = stats.maxStamina;
        RefreshStaminaBar();
    }

    void Update()
    {
        if (regenPauseTimer > 0)
            regenPauseTimer -= Time.deltaTime;

        if (regenPauseTimer <= 0 && stats != null && stats.CurrentStamina < stats.maxStamina)
        {
            float regenAmount = stats.staminaRegenPerSecond * Time.deltaTime;
            stats.CurrentStamina += regenAmount;

            if (stats.CurrentStamina > stats.maxStamina)
                stats.CurrentStamina = stats.maxStamina;

            RefreshStaminaBar();
        }

        if (dashAction.WasPressedThisFrame())
        {
            TryDash();
        }
    }

    void TryDash()
    {
        if (stats == null || stats.CurrentStamina < stats.staminaCostPerDash)
            return;

        // Only consume stamina if the dash actually happens
        bool dashExecuted = GetComponent<PlayerController>().TryPerformDash();

        if (dashExecuted)
        {
            stats.CurrentStamina -= stats.staminaCostPerDash;
            RefreshStaminaBar();
            regenPauseTimer = stats.staminaRegenPauseAfterDash;
        }
    }

    public void RefreshStaminaBar()
    {
        if (staminaBar != null && stats != null)
            staminaBar.value = stats.CurrentStamina / stats.maxStamina;
    }
}