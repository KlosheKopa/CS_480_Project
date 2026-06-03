using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [Header("Drop Settings")]
    [Tooltip("The item prefab to spawn.")]
    public GameObject itemPrefab;

    [Tooltip("Percentage chance to drop (0 to 100).")]
    [Range(0f, 100f)]
    public float dropChance = 30f;

    // This is the public method ANY unique script can trigger
    public void DropLoot()
    {
        if (itemPrefab == null) return;

        // Roll the dice
        float randomRoll = Random.Range(0f, 100f);

        if (randomRoll <= dropChance)
        {
            // Spawn at the current position before this object disappears
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
    }
}
