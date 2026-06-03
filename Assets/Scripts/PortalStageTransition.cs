using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalStageTransition : MonoBehaviour
{
    [Header("Transition Target")]
    [Tooltip("The build index ID number of the next scene.")]
    public int nextSceneIndex = 1;

    private ParticleSystem[] portalParticles;
    private bool hasTriggered = false;

    void Awake()
    {
        portalParticles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void EnablePortalVFX()
    {
        gameObject.SetActive(true);

        // Force all nested child gameobjects to activate their visual existence
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        if (portalParticles == null) portalParticles = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in portalParticles)
        {
            if (ps != null)
            {
                ps.gameObject.SetActive(true);
                var emission = ps.emission;
                emission.enabled = true;
                ps.Clear();
                ps.Play();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Character Controller fail-safe verification check
        bool isPlayerTag = other.CompareTag("Player") || (other.transform.root != null && other.transform.root.CompareTag("Player"));
        bool isPlayerController = other.GetComponent<CharacterController>() != null || other.GetComponentInParent<CharacterController>() != null;

        if (isPlayerTag || isPlayerController)
        {
            TriggerStageTransition();
        }
    }

    private void TriggerStageTransition()
    {
        hasTriggered = true;
        Time.timeScale = 1f; // Force normal timescale matrix right before loading

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Debug.Log($"[Portal Success] Loading Scene Build Index: {nextSceneIndex}");
        SceneManager.LoadScene(nextSceneIndex);
    }
}
