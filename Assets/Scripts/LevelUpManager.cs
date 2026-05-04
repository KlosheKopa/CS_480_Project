using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance;

    [Header("UI")]
    public GameObject levelUpPanel;
    public UpgradeButton[] upgradeButtons;

    private PlayerStats stats;
    private ClawShooter clawShooter;

    void Awake()
    {
        Instance = this;
        levelUpPanel.SetActive(false);
    }

    void Start()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        clawShooter = FindFirstObjectByType<ClawShooter>();
    }

    public void ShowLevelUpScreen()
    {
        Time.timeScale = 0f;
        if (clawShooter != null) clawShooter.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        levelUpPanel.SetActive(true);

        List<string> available = new List<string>();

        if (IsBubbleUpgradeUnlocked("BubbleDamage") && stats.bubbleDamageUpgrades < 5)
            available.Add("BubbleDamage");

        if (IsBubbleUpgradeUnlocked("BubbleFireRate") && stats.bubbleFireRateUpgrades < 5)
            available.Add("BubbleFireRate");
        if (IsBubbleUpgradeUnlocked("BubbleSpeed") && stats.bubbleSpeedUpgrades < 5)
            available.Add("BubbleSpeed");

        if (stats.healthUpgrades < 5) available.Add("MaxHealth");
        if (stats.staminaUpgrades < 5) available.Add("MaxStamina");
        if (stats.regenPauseUpgrades < 5) available.Add("StaminaRegenPause");
        if (stats.regenSpeedUpgrades < 5) available.Add("StaminaRegenSpeed");
        if (stats.dashDistanceUpgrades < 5) available.Add("DashDistance");
        if (stats.defenseUpgrades < 5) available.Add("Defense");
        if (stats.invincibilityUpgrades < 5) available.Add("InvincibilityTime");
        if (stats.healUpgrades < 5) available.Add("HealPercentage");

        if (available.Count == 0)
            available.Add("MaxHealth");

        available.Shuffle();

        for (int i = 0; i < 4; i++)
        {
            string key = available[i % available.Count];
            string displayText = GetDisplayName(key);
            upgradeButtons[i].Setup(key, displayText);
        }
    }

    private bool IsBubbleUpgradeUnlocked(string key)
    {
        int upgradesDone = 0;
        switch (key)
        {
            case "BubbleDamage": upgradesDone = stats.bubbleDamageUpgrades; break;
            case "BubbleFireRate": upgradesDone = stats.bubbleFireRateUpgrades; break;
            case "BubbleSpeed": upgradesDone = stats.bubbleSpeedUpgrades; break;
        }
        int requiredLevel = 5 + (upgradesDone * 5);
        return stats.currentLevel >= requiredLevel;
    }

    private string GetDisplayName(string key)
    {
        switch (key)
        {
            case "MaxHealth": return "Max Health (Lv. " + stats.healthUpgrades + "/5)\nCurrent: +" + (100 + stats.healthUpgrades * 50) + " HP\nUpgrade: +50 HP";
            case "MaxStamina": return "Max Stamina (Lv. " + stats.staminaUpgrades + "/5)\nCurrent: +" + (3 + stats.staminaUpgrades) + "\nUpgrade: +1";
            case "StaminaRegenPause": return "Stamina Regen Delay (Lv. " + stats.regenPauseUpgrades + "/5)\nCurrent: " + (1.5f - stats.regenPauseUpgrades * 0.2f).ToString("F1") + "s\nUpgrade: -0.2s";
            case "StaminaRegenSpeed": return "Stamina Regen Speed (Lv. " + stats.regenSpeedUpgrades + "/5)\nCurrent: +" + (1.5f + stats.regenSpeedUpgrades * 0.3f) + "/s\nUpgrade: +0.3/s";
            case "DashDistance": return "Dash Distance (Lv. " + stats.dashDistanceUpgrades + "/5)\nCurrent: +" + (stats.dashDistanceUpgrades * 20) + "%\nUpgrade: +20%";
            case "BubbleDamage": return "Bubble Damage (Lv. " + stats.bubbleDamageUpgrades + "/5)\nCurrent: ×" + Mathf.Pow(1.5f, stats.bubbleDamageUpgrades).ToString("F1") + "\nUpgrade: ×1.5";
            case "BubbleFireRate": return "Fire Rate (Lv. " + stats.bubbleFireRateUpgrades + "/5)\nCurrent: " + (0.8f - stats.bubbleFireRateUpgrades * 0.15f).ToString("F2") + "s\nUpgrade: -0.15s";
            case "BubbleSpeed": return "Bubble Speed (Lv. " + stats.bubbleSpeedUpgrades + "/5)\nCurrent: +" + (10 + stats.bubbleSpeedUpgrades * 4) + "\nUpgrade: +4";
            case "Defense": return "Damage Reduction (Lv. " + stats.defenseUpgrades + "/5)\nCurrent: +" + (stats.defenseUpgrades * 4) + "%\nUpgrade: +4%";
            case "InvincibilityTime": return "Invincibility (Lv. " + stats.invincibilityUpgrades + "/5)\nCurrent: +" + (1f + stats.invincibilityUpgrades * 0.2f) + "s\nUpgrade: +0.2s";
            case "HealPercentage": return "Green Fish Healing (Lv. " + stats.healUpgrades + "/5)\nCurrent: +" + (stats.healPercentage * 100f).ToString("F0") + "%\nUpgrade: +3%";
            default: return key;
        }
    }

    public void ChooseUpgrade(string upgradeKey)
    {
        switch (upgradeKey)
        {
            case "MaxHealth": stats.UpgradeMaxHealth(); break;
            case "MaxStamina": stats.UpgradeMaxStamina(); break;
            case "StaminaRegenPause": stats.UpgradeStaminaRegenPause(); break;
            case "StaminaRegenSpeed": stats.UpgradeStaminaRegenSpeed(); break;
            case "DashDistance": stats.UpgradeDashDistance(); break;
            case "BubbleDamage": stats.UpgradeBubbleDamage(); break;
            case "BubbleFireRate": stats.UpgradeBubbleFireRate(); break;
            case "BubbleSpeed": stats.UpgradeBubbleSpeed(); break;
            case "Defense": stats.UpgradeDefense(); break;
            case "InvincibilityTime": stats.UpgradeInvincibilityTime(); break;
            case "HealPercentage": stats.UpgradeHealPercentage(); break;
        }

        levelUpPanel.SetActive(false);

        // Re-enable shooting immediately (safety layer)
        if (clawShooter != null) clawShooter.enabled = true;

        if (stats != null)
            stats.LevelUpCompleted();
    }
}

// Helper for shuffling
static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}