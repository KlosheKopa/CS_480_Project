using System.Collections;
using System.Collections.Generic; // Needed for Lists
using UnityEngine;

public class UltimateEnemySpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int maxEnemies = 10;
    public float spawnDelay = 2.5f;

    [Header("Spawner Radius Settings")]
    public float maxSpawnRadius = 25f; // Flat radius around THIS spawner object
    public float playerActivationDistance = 40f; // How close player must be to turn on spawner

    private Transform playerTransform;

    // FIX: Track alive enemies using a List instead of a simple number counter
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        StartCoroutine(CombinedSpawnRoutine());
    }

    IEnumerator CombinedSpawnRoutine()
    {
        while (true)
        {
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                if (distance <= playerActivationDistance)
                {
                    // FIX: Remove any dead (destroyed) enemies from our list before checking the count
                    aliveEnemies.RemoveAll(enemy => enemy == null);

                    if (aliveEnemies.Count < maxEnemies)
                    {
                        SpawnEnemy();
                    }
                }
            }
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * maxSpawnRadius;
        float targetX = transform.position.x + randomCircle.x;
        float targetZ = transform.position.z + randomCircle.y;
        Vector3 spawnPosition = new Vector3(targetX, transform.position.y, targetZ);

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        // FIX: Add the new living enemy to our tracking list
        aliveEnemies.Add(spawnedEnemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxSpawnRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerActivationDistance);
    }
}
