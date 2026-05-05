using UnityEngine;
using UnityEngine.UI;

public class PlayerEXP : MonoBehaviour
{
    [Header("UI")]
    public Slider expBar;          

    private PlayerStats stats;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (expBar != null && stats != null)
        {
            expBar.value = stats.CurrentEXPProgress;   // 0.0 to 1.0
        }
    }
}