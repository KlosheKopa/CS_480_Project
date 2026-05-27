using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BossInteractTrigger : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject bossPrefab;
    public float spawnHeightOffset = 5f; // Adjust this to spawn higher/lower
    
    [Header("UI Reference")]
    public BossUI bossHealthUI;
    public GameObject promptUI;

    private bool playerInRange = false;
    private bool hasSpawned = false;

    void Update()
    {
        // Checks if the specific key you chose is pressed using the New System
        if (playerInRange && !hasSpawned && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        hasSpawned = true;
        Vector3 spawnPos = transform.position + (Vector3.up * spawnHeightOffset);
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        if (promptUI != null) promptUI.SetActive(false);

        // Link the boss to the UI
        if (bossHealthUI != null)
        {
            bossHealthUI.SetupBoss(boss.GetComponent<Jellyfish>());
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasSpawned)
        {
            playerInRange = true;
            if (promptUI != null) promptUI.SetActive(true); // Show the prompt
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false); // Hide the prompt
        }
    }
}
