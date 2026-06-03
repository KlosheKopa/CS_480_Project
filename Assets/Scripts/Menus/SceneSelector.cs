using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Data Reset")]
    [Tooltip("Drag your ActivePlayerRunData ScriptableObject file here to wipe it on a new run")]
    public PlayerStatData runDataToClear;

    public void LoadScene(int sceneID)
    {
        Time.timeScale = 1f;

        // FIXED: Wipes the upgrades and level/EXP data right before loading the scene
        if (runDataToClear != null)
        {
            runDataToClear.ResetToDefaults();
        }

        SceneManager.LoadScene(sceneID);
    }
}