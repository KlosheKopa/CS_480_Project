using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatData", menuName = "ScriptableObjects/PlayerStatData")]
public class PlayerStatData : ScriptableObject
{
    [Header("Level & EXP")]
    public int currentLevel = 1;
    public int currentEXP = 0;
    public int expToNextLevel = 10;

    [Header("Upgrade Levels")]
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

    // Call this upon death or when clicking "New Game" from your Main Menu
    public void ResetToDefaults()
    {
        currentLevel = 1;
        currentEXP = 0;
        expToNextLevel = 10;

        healthUpgrades = 0;
        staminaUpgrades = 0;
        regenPauseUpgrades = 0;
        regenSpeedUpgrades = 0;
        dashDistanceUpgrades = 0;
        bubbleDamageUpgrades = 0;
        bubbleFireRateUpgrades = 0;
        bubbleSpeedUpgrades = 0;
        defenseUpgrades = 0;
        invincibilityUpgrades = 0;
        healUpgrades = 0;

        Debug.Log("=== ScriptableObject Run Data Successfully Reset to Baseline Defaults ===");
    }
}