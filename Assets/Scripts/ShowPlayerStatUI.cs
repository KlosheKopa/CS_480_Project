using UnityEngine;
using TMPro;

public class ShowPlayerStatsUI : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    private PlayerStats stats;

    void Awake()
    {
        stats = FindFirstObjectByType<PlayerStats>();
        gameObject.SetActive(false);   // hidden by default
    }

    public void ShowStatsPanel()
    {
        gameObject.SetActive(true);
        UpdateStatsText();
    }

    public void HideStatsPanel()
    {
        gameObject.SetActive(false);
    }

    private void UpdateStatsText()
    {
        string text = "<b>PLAYER UPGRADES</b>\n\n";

        text += $"Max Health (Lv. {stats.healthUpgrades}/5)      Current: +{100 + stats.healthUpgrades * 50} HP\n";
        text += $"Max Stamina (Lv. {stats.staminaUpgrades}/5)     Current: +{3 + stats.staminaUpgrades}\n";
        text += $"Stamina Regen Delay (Lv. {stats.regenPauseUpgrades}/5)   Current: {(1.5f - stats.regenPauseUpgrades * 0.2f):F1}s\n";
        text += $"Stamina Regen Speed (Lv. {stats.regenSpeedUpgrades}/5)   Current: +{(1.5f + stats.regenSpeedUpgrades * 0.3f):F1}/s\n";
        text += $"Dash Distance (Lv. {stats.dashDistanceUpgrades}/5)      Current: +{stats.dashDistanceUpgrades * 20}%\n";
        text += $"Bubble Damage (Lv. {stats.bubbleDamageUpgrades}/5)     Current: ×{Mathf.Pow(1.5f, stats.bubbleDamageUpgrades):F1}\n";
        text += $"Fire Rate (Lv. {stats.bubbleFireRateUpgrades}/5)        Current: {(0.8f - stats.bubbleFireRateUpgrades * 0.15f):F2}s\n";
        text += $"Bubble Speed (Lv. {stats.bubbleSpeedUpgrades}/5)       Current: +{10 + stats.bubbleSpeedUpgrades * 4}\n";
        text += $"Damage Reduction (Lv. {stats.defenseUpgrades}/5)      Current: +{stats.defenseUpgrades * 4}%\n";
        text += $"Invincibility (Lv. {stats.invincibilityUpgrades}/5)    Current: +{(1f + stats.invincibilityUpgrades * 0.2f):F1}s\n";
        text += $"Green Fish Healing (Lv. {stats.healUpgrades}/5)      Current: +{(stats.healPercentage * 100):F0}%\n";

        statsText.text = text;
    }
}