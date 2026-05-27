using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void LoadScene(int sceneID)
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneID);
    }
}