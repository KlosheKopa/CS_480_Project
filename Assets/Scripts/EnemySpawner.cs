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

    [Header("Spawner Radius Settings")]
    public Transform playerTransform; // Needed to check distance to player
    public float maxSpawnRadius = 25f; // Flat radius around THIS spawner object
    public float playerActivationDistance = 40f; // How close player must be to turn on spawner

    [Header("NavMesh Validation")]
    public float navMeshSearchRadius = 5f;

    private int currentEnemyCount = 0;

    void Start()
    {
        // Auto-assign terrain if left empty
        if (targetTerrain == null) targetTerrain = Terrain.activeTerrain;

        StartCoroutine(CombinedSpawnRoutine());
    }

    IEnumerator CombinedSpawnRoutine()
    {
        // Main continuous spawner loop (starts spawning immediately)
        while (true)
        {
            // Only spawn if player is close enough AND we are under the limit
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= playerActivationDistance)
            {
                if (currentEnemyCount < maxEnemies)
                {
                    SpawnEnemy();
                }
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || targetTerrain == null) return;

        // Get terrain dimensions
        Vector3 terrainPos = targetTerrain.transform.position;
        Vector3 terrainSize = targetTerrain.terrainData.size;

        // Calculate a random point within the spawner's maximum radius
        Vector2 randomCircle = Random.insideUnitCircle * maxSpawnRadius;
        float targetX = transform.position.x + randomCircle.x;
        float targetZ = transform.position.z + randomCircle.y;

        // Clamp inside terrain bounds with edge buffer
        targetX = Mathf.Clamp(targetX, terrainPos.x + edgeBuffer, terrainPos.x + terrainSize.x - edgeBuffer);
        targetZ = Mathf.Clamp(targetZ, terrainPos.z + edgeBuffer, terrainPos.z + terrainSize.z - edgeBuffer);

        // Sample terrain height
        float targetY = targetTerrain.SampleHeight(new Vector3(targetX, 0f, targetZ)) + terrainPos.y + heightOffset;
        Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

        // Validate on NavMesh
        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
            currentEnemyCount++;
        }
    }

    // Draw a visual wire circle in the Scene View to easily adjust sizes
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerActivationDistance);
    }
}
