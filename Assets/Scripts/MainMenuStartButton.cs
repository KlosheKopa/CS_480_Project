using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuStartButton : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "SampleScene";

    public void StartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(gameSceneName);
    }
}
