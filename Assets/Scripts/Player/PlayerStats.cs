using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Persistent Data Reference")]
    [Tooltip("Drag the ActivePlayerRunData ScriptableObject asset here")]
    public PlayerStatData runData;

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
    public float invincibilityTime = 0.5f;

    [Header("Dash")]
    public float dashDistanceMultiplier = 1f;

    // REDIRECTS: Pull values dynamically directly out of the ScriptableObject container asset
    public int currentLevel { get => runData.currentLevel; set => runData.currentLevel = value; }
    public int currentEXP { get => runData.currentEXP; set => runData.currentEXP = value; }
    public int expToNextLevel { get => runData.expToNextLevel; set => runData.expToNextLevel = value; }

    public int healthUpgrades { get => runData.healthUpgrades; set => runData.healthUpgrades = value; }
    public int staminaUpgrades { get => runData.staminaUpgrades; set => runData.staminaUpgrades = value; }
    public int regenPauseUpgrades { get => runData.regenPauseUpgrades; set => runData.regenPauseUpgrades = value; }
    public int regenSpeedUpgrades { get => runData.regenSpeedUpgrades; set => runData.regenSpeedUpgrades = value; }
    public int dashDistanceUpgrades { get => runData.dashDistanceUpgrades; set => runData.dashDistanceUpgrades = value; }
    public int bubbleDamageUpgrades { get => runData.bubbleDamageUpgrades; set => runData.bubbleDamageUpgrades = value; }
    public int bubbleFireRateUpgrades { get => runData.bubbleFireRateUpgrades; set => runData.bubbleFireRateUpgrades = value; }
    public int bubbleSpeedUpgrades { get => runData.bubbleSpeedUpgrades; set => runData.bubbleSpeedUpgrades = value; }
    public int defenseUpgrades { get => runData.defenseUpgrades; set => runData.defenseUpgrades = value; }
    public int invincibilityUpgrades { get => runData.invincibilityUpgrades; set => runData.invincibilityUpgrades = value; }
    public int healUpgrades { get => runData.healUpgrades; set => runData.healUpgrades = value; }

    [HideInInspector] public int maxLevel = 56;

    public float CurrentHealth { get; set; }
    public float CurrentStamina { get; set; }

    public float CurrentEXPProgress => (IsMaxLevel || expToNextLevel <= 0) ? 1f : (float)currentEXP / expToNextLevel;

    public bool isAutoFireUnlocked => bubbleFireRateUpgrades >= 5;
    public bool IsMaxLevel => currentLevel >= maxLevel;

    private int pendingLevelUps = 0;

    private void OnValidate()
    {
        maxLevel = 56;
    }

    private void Awake()
    {
        maxLevel = 56;
        RecalculateAllStats();
        if (CurrentHealth <= 0) CurrentHealth = maxHealth;
    }

    public void RecalculateAllStats()
    {
        // Safe check if you forgot to link the asset file
        if (runData == null) return;

        maxHealth = 100f + healthUpgrades * 50f;
        maxStamina = 3f + staminaUpgrades * 1f;
        staminaRegenPauseAfterDash = 1.5f - regenPauseUpgrades * 0.2f;
        staminaRegenPerSecond = 1.5f + regenSpeedUpgrades * 0.3f;
        dashDistanceMultiplier = 1f + dashDistanceUpgrades * 0.2f;

        bubbleDamage = 10f * Mathf.Pow(1.5f, bubbleDamageUpgrades);
        bubbleFireRate = 0.8f - bubbleFireRateUpgrades * 0.15f;
        bubbleSpeed = 10f + bubbleSpeedUpgrades * 4f;

        defense = defenseUpgrades * 0.04f;
        invincibilityTime = 0.5f + invincibilityUpgrades * 0.2f;
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

    // This method handles player death cleanly
    public void HandleDeath()
    {
        if (runData != null) runData.ResetToDefaults();
        // Trigger scene reload or Game Over screen sequence here
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