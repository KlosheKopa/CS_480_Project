using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Static instance allows other scripts to call GameManager.Instance
    public static GameManager Instance;

    [Header("UI Panels")]
    public GameObject victoryPanel;
    public GameObject deathScreen;

    void Awake()
    {
        // Setup Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by BossUI when boss reaches 0 HP
    public void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        EndGame();
    }

    // Called by PlayerHealth when player reaches 0 HP
    public void ShowGameOver()
    {
        if (deathScreen != null) deathScreen.SetActive(true);
        EndGame();
    }

    private void EndGame()
    {
        Time.timeScale = 0f; // Freeze game logic
        Cursor.lockState = CursorLockMode.None; // Release mouse
        Cursor.visible = true; // Show mouse
    }

    // Link this to your "Restart" buttons in the Inspector
    public void ResetGame()
    {
        Time.timeScale = 1f; // IMPORTANT: Resume time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
