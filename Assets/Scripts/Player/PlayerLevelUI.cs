using UnityEngine;
using TMPro;

public class PlayerLevelUI : MonoBehaviour
{
    public TextMeshProUGUI levelText;

    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (levelText != null && stats != null)
        {
            if (stats.IsMaxLevel)
            {
                levelText.text = "\n\nM\nA\nX";   // Vertical MAX at level 56
            }
            else
            {
                levelText.text = stats.currentLevel.ToString();
            }
        }
    }
}