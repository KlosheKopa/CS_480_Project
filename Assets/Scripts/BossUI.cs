using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;

    [Header("Dynamic Portal Activation Setup")]
    [Tooltip("Drag the child vfx_Portal_01 GameObject directly here.")]
    public PortalStageTransition stageExitPortal;

    private IBoss bossScript;
    private bool victoryTriggered = false;

    public void SetupBoss(IBoss targetBoss)
    {
        bossScript = targetBoss;
        healthSlider.maxValue = targetBoss.MaxHealth;
        healthSlider.value = targetBoss.MaxHealth;

        victoryTriggered = false;
        gameObject.SetActive(true);

        if (stageExitPortal != null)
        {
            // Safely locate and disable the trigger box collider through nesting matrices
            Collider portalCollider = stageExitPortal.GetComponentInChildren<Collider>(true);
            if (portalCollider == null) portalCollider = stageExitPortal.GetComponentInParent<Collider>(true);
            if (portalCollider != null) portalCollider.enabled = false;

            // Turn off the portal structure object last
            stageExitPortal.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (bossScript != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, bossScript.CurrentHealth, Time.deltaTime * 5f);

            if (bossScript.IsDead && !victoryTriggered)
            {
                victoryTriggered = true;
                Invoke("TriggerVictory", 1.5f);
            }
        }
    }

    void TriggerVictory()
    {
        gameObject.SetActive(false);

        bool hasNextStageScene = stageExitPortal != null && stageExitPortal.nextSceneIndex > 0;

        if (hasNextStageScene)
        {
            Debug.Log("[Boss UI] Victory detected! Spawning and activating portal gateway trigger structure.");

            stageExitPortal.gameObject.SetActive(true);
            stageExitPortal.EnablePortalVFX();

            Collider portalCollider = stageExitPortal.GetComponentInChildren<Collider>(true);
            if (portalCollider == null) portalCollider = stageExitPortal.GetComponentInParent<Collider>(true);

            if (portalCollider != null)
            {
                portalCollider.enabled = true;
                Debug.Log("[Boss UI] Success: Gate Box Collider enabled.");
            }
        }
        else
        {
            Debug.Log("[Boss UI] Victory! No next scene build index configured, showing GameManager panel overlay.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShowVictory();
            }
        }
    }
}
