using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    public float maxStamina = 3f;
    public float staminaCostPerDash = 1f;
    public float staminaRegenPerSecond = 1.5f;
    public float staminaRegenPauseAfterDash = 1.5f;
    public float healPercentage = 0.25f;

    [Header("Bubble Stats")]
    public float bubbleSpeed = 10f;
    public float bubbleFireRate = 0.8f;
    public float bubbleDamage = 10f;

    [Header("New Upgrades")]
    public float defense = 0f;
    public float invincibilityTime = 1f;

    [Header("Dash")]
    public float dashDistanceMultiplier = 1f;

    [Header("Level & EXP")]
    public int currentLevel = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 10;
    public int maxLevel = 56;               //11 upgrades = 55 level + 1

    public int healthUpgrades = 0;
    public int staminaUpgrades = 0;
    public int regenPauseUpgrades = 0;
    public int regenSpeedUpgrades = 0;
    public int dashDistanceUpgrades = 0;
    public int bubbleDamageUpgrades = 0;
    public int bubbleFireRateUpgrades = 0;
    public int bubbleSpeedUpgrades = 0;
    public int defenseUpgrades = 0;
    public int invincibilityUpgrades = 0;
    public int healUpgrades = 0;

    public float CurrentHealth { get; set; }
    public float CurrentStamina { get; set; }

    public float CurrentEXPProgress => (IsMaxLevel || expToNextLevel <= 0) ? 1f : (float)currentEXP / expToNextLevel;

    public bool isAutoFireUnlocked => bubbleFireRateUpgrades >= 5;
    public bool IsMaxLevel => currentLevel >= maxLevel;

    private int pendingLevelUps = 0;

    // This runs every time Unity shows the Inspector or the script is recompiled
    private void OnValidate()
    {
        maxLevel = 56;
    }

    private void Awake()
    {
        maxLevel = 56;   // Force it at runtime too
        RecalculateAllStats();
        if (CurrentHealth <= 0) CurrentHealth = maxHealth;
    }


    public void RecalculateAllStats()
    {
        maxHealth = 100f + healthUpgrades * 50f;
        maxStamina = 3f + staminaUpgrades * 1f;
        staminaRegenPauseAfterDash = 1.5f - regenPauseUpgrades * 0.2f;
        staminaRegenPerSecond = 1.5f + regenSpeedUpgrades * 0.3f;
        dashDistanceMultiplier = 1f + dashDistanceUpgrades * 0.2f;

        bubbleDamage = 10f * Mathf.Pow(1.5f, bubbleDamageUpgrades);
        bubbleFireRate = 0.8f - bubbleFireRateUpgrades * 0.15f;
        bubbleSpeed = 10f + bubbleSpeedUpgrades * 4f;

        defense = defenseUpgrades * 0.04f;
        invincibilityTime = 1f + invincibilityUpgrades * 0.2f;
        healPercentage = 0.25f + healUpgrades * 0.03f;
    }

    public void AddEXP(int amount)
    {
        if (IsMaxLevel)
        {
            currentEXP = expToNextLevel;
            return;
        }

        currentEXP += amount;

        while (currentEXP >= expToNextLevel && !IsMaxLevel)
        {
            currentEXP -= expToNextLevel;
            currentLevel++;
            expToNextLevel += 5;
            pendingLevelUps++;
        }

        if (pendingLevelUps > 0)
        {
            LevelUpManager manager = FindFirstObjectByType<LevelUpManager>();
            if (manager != null)
                manager.ShowLevelUpScreen();
        }
    }

    public void LevelUpCompleted()
    {
        pendingLevelUps = Mathf.Max(0, pendingLevelUps - 1);

        if (pendingLevelUps > 0)
        {
            LevelUpManager manager = FindFirstObjectByType<LevelUpManager>();
            if (manager != null)
                manager.ShowLevelUpScreen();
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var cs = GetComponentInChildren<ClawShooter>(true);
            if (cs != null) cs.enabled = true;

            Debug.Log("=== ALL LEVEL UPS COMPLETE - Game fully resumed ===");
        }
    }

    public void UpgradeMaxHealth() { healthUpgrades = Mathf.Min(healthUpgrades + 1, 5); RecalculateAllStats(); CurrentHealth = maxHealth; }
    public void UpgradeMaxStamina() { staminaUpgrades = Mathf.Min(staminaUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeStaminaRegenPause() { regenPauseUpgrades = Mathf.Min(regenPauseUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeStaminaRegenSpeed() { regenSpeedUpgrades = Mathf.Min(regenSpeedUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeDashDistance() { dashDistanceUpgrades = Mathf.Min(dashDistanceUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeBubbleDamage() { bubbleDamageUpgrades = Mathf.Min(bubbleDamageUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeBubbleFireRate() { bubbleFireRateUpgrades = Mathf.Min(bubbleFireRateUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeBubbleSpeed() { bubbleSpeedUpgrades = Mathf.Min(bubbleSpeedUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeDefense() { defenseUpgrades = Mathf.Min(defenseUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeInvincibilityTime() { invincibilityUpgrades = Mathf.Min(invincibilityUpgrades + 1, 5); RecalculateAllStats(); }
    public void UpgradeHealPercentage() { healUpgrades = Mathf.Min(healUpgrades + 1, 5); RecalculateAllStats(); }
}