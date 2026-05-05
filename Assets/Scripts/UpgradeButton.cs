using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI upgradeText;

    [Header("Upgrade Info - Type here")]
    public string upgradeKey;                 // Internal name (e.g. "MaxHealth")
    [TextArea(2, 4)]
    public string upgradeDisplayText;         // ← Type the text you want to show here

    private Button button;
    private LevelUpManager levelUpManager;

    void Awake()
    {
        button = GetComponent<Button>();
        levelUpManager = FindFirstObjectByType<LevelUpManager>();

        // Auto-find the text if not dragged
        if (upgradeText == null)
            upgradeText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    // Called by LevelUpManager when the panel appears
    public void Setup(string key, string displayText)
    {
        upgradeKey = key;
        upgradeDisplayText = displayText;
        if (upgradeText != null)
            upgradeText.text = displayText;
    }

    private void OnButtonClicked()
    {
        if (levelUpManager != null && !string.IsNullOrEmpty(upgradeKey))
        {
            levelUpManager.ChooseUpgrade(upgradeKey);
        }
    }
}