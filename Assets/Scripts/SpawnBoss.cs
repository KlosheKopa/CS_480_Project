using UnityEngine;
using UnityEngine.InputSystem;

public class BossInteractTrigger : MonoBehaviour
{
    [Header("Settings")]
    public GameObject bossPrefab;
    public float spawnHeightOffset = 5f;

    [Header("UI Reference")]
    public BossUI bossHealthUI;
    public GameObject promptUI;

    private bool playerInRange = false;
    private bool hasSpawned = false;

    // Explicit cache to store the player's transform when they step into the trigger zone
    private Transform playerTransform;

    void Update()
    {
        if (playerInRange && !hasSpawned && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        hasSpawned = true;
        Vector3 spawnPos = transform.position + (Vector3.up * spawnHeightOffset);
        GameObject bossInstance = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

        if (promptUI != null) promptUI.SetActive(false);

        // === FIX: Explicitly pass the player reference directly to the spawned boss ===
        if (playerTransform != null)
        {
            // Look for your script component on the newly instantiated instance
            JellyBoss jellyBossScript = bossInstance.GetComponentInChildren<JellyBoss>();
            if (jellyBossScript != null)
            {
                jellyBossScript.player = playerTransform;
                Debug.Log($"[Spawn System] Successfully injected Player reference into spawned {bossInstance.name}");
            }
        }
        else
        {
            Debug.LogError("[Spawn System] Failed to spawn boss correctly because playerTransform was null!");
        }

        // Dynamically search the spawned prefab for ANY script that uses IBoss for UI setup
        if (bossHealthUI != null)
        {
            IBoss bossInterface = bossInstance.GetComponentInChildren<IBoss>();

            if (bossInterface != null)
            {
                bossHealthUI.SetupBoss(bossInterface);
            }
            else
            {
                Debug.LogError($"The spawned prefab '{bossPrefab.name}' does not have a script that implements the IBoss interface!");
            }
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            playerInRange = true;

            // Capture the exact moving player transform entering this exact zone
            playerTransform = other.transform;

            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // Clear the reference safely when walking away
            playerTransform = null;

            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}
