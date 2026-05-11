using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Needed for NavMesh

public class UltimateEnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public Terrain targetTerrain;
    public GameObject controlHintUI;

    [Header("Spawn Settings")]
    public int maxEnemies = 10;
    public float spawnDelay = 2.5f;
    public float initialStartDelay = 10f;
    public float edgeBuffer = 10f;
    public float heightOffset = 0.2f;

    [Header("Player Spawning")]
    public Transform playerTransform; // Drag player here or find via Tag
    public float minSpawnDist = 15f;  // Minimum distance from player
    public float maxSpawnDist = 30f;  // Maximum distance from player

    [Header("NavMesh Validation")]
    public float navMeshSearchRadius = 5f;

    private int currentEnemyCount = 0;
    private bool canSpawn = false;

    void Start()
    {
        // Auto-assign terrain if left empty
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;

        StartCoroutine(CombinedSpawnRoutine());
    }

    IEnumerator CombinedSpawnRoutine()
    {
        // 1. Show the UI
        if (controlHintUI != null) controlHintUI.SetActive(true);

        float timer = 0f;
        bool skipWait = false;

        // 2. Wait for 10 seconds OR until 'P' is pressed
        while (timer < 10f)
        {
            timer += Time.unscaledDeltaTime;

            // Check for 'P' key press using the New Input System
            if (UnityEngine.InputSystem.Keyboard.current.pKey.wasPressedThisFrame)
            {
                skipWait = true;
                break;
            }

            yield return null;
        }

        // 3. Hide UI and start the infinite spawning loop
        if (controlHintUI != null) controlHintUI.SetActive(false);
        Debug.Log(skipWait ? "Started early via P key" : "Started after 10s timer");

        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemyNearPlayer();
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnEnemyNearPlayer()
    {
        if (playerTransform == null) return;

        // 1. Generate a random circle direction
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized;

        // 2. Pick a distance between min and max
        float randomDist = UnityEngine.Random.Range(minSpawnDist, maxSpawnDist);

        // 3. Create a candidate X and Z around the player
        float spawnX = playerTransform.position.x + (randomCircle.x * randomDist);
        float spawnZ = playerTransform.position.z + (randomCircle.y * randomDist);

        // 4. Find the ground height at that spot
        float yVal = targetTerrain.SampleHeight(new Vector3(spawnX, 0, spawnZ)) + targetTerrain.transform.position.y;
        Vector3 candidatePos = new Vector3(spawnX, yVal, spawnZ);

        // 5. Final NavMesh Validation
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePos, out hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            Vector3 finalPos = hit.position + Vector3.up * heightOffset;
            GameObject enemy = Instantiate(enemyPrefab, finalPos, Quaternion.identity);
            currentEnemyCount++;

            Jellyfish jelly = enemy.GetComponent<Jellyfish>();
            if (jelly != null) jelly.OnDeath += () => currentEnemyCount--;
        }
    }
}
