using System.Collections;
using UnityEngine;
using UnityEngine.AI; // Needed for NavMesh

public class UltimateEnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;
    public Terrain targetTerrain;

    [Header("Spawn Settings")]
    public int maxEnemies = 10;
    public float spawnDelay = 2.5f;
    public float edgeBuffer = 10f;
    public float heightOffset = 0.2f;

    [Header("Player Spawning")]
    public Transform playerTransform; // Drag player here or find via Tag
    public float minSpawnDist = 15f;  // Minimum distance from player
    public float maxSpawnDist = 30f;  // Maximum distance from player

    [Header("NavMesh Validation")]
    public float navMeshSearchRadius = 5f;

    private int currentEnemyCount = 0;

    void Start()
    {
        // Auto-assign terrain if left empty
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Only try to spawn if we are under the limit
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemyNearPlayer();
            }
            // Wait for the delay before checking again
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
