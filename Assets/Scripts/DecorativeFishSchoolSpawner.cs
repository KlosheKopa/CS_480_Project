using UnityEngine;

[System.Serializable]
public class DecorativeFishSpawnGroup
{
    public string groupName;
    public GameObject prefab;
    public int count = 1;
    public Vector2 yRange = new Vector2(40f, 80f);
    public Vector2 scaleRange = new Vector2(0.25f, 0.6f);
    public Vector2 swimSpeedRange = new Vector2(1f, 2f);
    public Vector2 turnSpeedRange = new Vector2(1.8f, 3.5f);
    public Vector3 modelRotationOffset;
}

public class DecorativeFishSchoolSpawner : MonoBehaviour
{
    [Header("Fish Prefabs")]
    public GameObject[] fishPrefabs;
    public DecorativeFishSpawnGroup[] fishGroups;
    public int fishCount = 12;

    [Header("Scene Bounds")]
    public Collider[] sideWallColliders;
    public Collider invisibleCeilingCollider;
    public Vector3 swimAreaSize = new Vector3(35f, 8f, 35f);
    public float boundaryPadding = 8f;
    public float boundaryTurnDistance = 10f;
    public bool spawnOnStart = true;
    public bool parentFishToSpawner = true;
    public bool clearExistingBeforeSpawn = true;

    [Header("Randomization")]
    public Vector2 scaleRange = new Vector2(0.75f, 1.25f);
    public Vector2 swimSpeedRange = new Vector2(1.2f, 3f);
    public Vector2 turnSpeedRange = new Vector2(1.8f, 4f);
    public Vector3 modelRotationOffset;

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnFish();
        }
    }

    public void SpawnFish()
    {
        bool hasGroups = fishGroups != null && fishGroups.Length > 0;
        bool hasLegacyPrefabs = fishPrefabs != null && fishPrefabs.Length > 0;

        if (!hasGroups && !hasLegacyPrefabs)
        {
            Debug.LogWarning("[Decorative Fish] No fish prefabs assigned.", this);
            return;
        }

        if (clearExistingBeforeSpawn)
        {
            ClearSpawnedFish();
        }

        if (hasGroups)
        {
            for (int groupIndex = 0; groupIndex < fishGroups.Length; groupIndex++)
            {
                SpawnGroup(fishGroups[groupIndex], groupIndex);
            }

            return;
        }

        for (int i = 0; i < fishCount; i++)
        {
            GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
            if (prefab == null) continue;

            Vector3 spawnPosition = RandomPointInBounds(new Vector2(transform.position.y - swimAreaSize.y * 0.5f, transform.position.y + swimAreaSize.y * 0.5f));
            Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Transform parent = parentFishToSpawner ? transform : null;
            GameObject fish = Instantiate(prefab, spawnPosition, spawnRotation, parent);
            fish.name = $"DecorativeFish_{i + 1:00}_{prefab.name}";

            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            fish.transform.localScale *= randomScale;

            DecorativeFishWander wander = fish.GetComponent<DecorativeFishWander>();
            if (wander == null)
            {
                wander = fish.AddComponent<DecorativeFishWander>();
            }

            wander.boundsCenter = transform;
            wander.swimAreaSize = swimAreaSize;
            wander.swimSpeed = Random.Range(swimSpeedRange.x, swimSpeedRange.y);
            wander.turnSpeed = Random.Range(turnSpeedRange.x, turnSpeedRange.y);
            wander.modelRotationOffset = modelRotationOffset;
            wander.boundaryTurnDistance = boundaryTurnDistance;
            ApplyBoundsToWander(wander, new Vector2(transform.position.y - swimAreaSize.y * 0.5f, transform.position.y + swimAreaSize.y * 0.5f));
            DisableGameplayPhysics(fish);
        }
    }

    [ContextMenu("Respawn Preview Fish")]
    public void RespawnPreviewFish()
    {
        SpawnFish();
    }

    [ContextMenu("Clear Preview Fish")]
    public void ClearSpawnedFish()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith("DecorativeFish_")) continue;

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    private void SpawnGroup(DecorativeFishSpawnGroup group, int groupIndex)
    {
        if (group == null || group.prefab == null || group.count <= 0) return;

        string groupName = string.IsNullOrWhiteSpace(group.groupName) ? group.prefab.name : group.groupName;

        for (int i = 0; i < group.count; i++)
        {
            Vector3 spawnPosition = RandomPointInBounds(group.yRange);
            Quaternion spawnRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Transform parent = parentFishToSpawner ? transform : null;
            GameObject fish = Instantiate(group.prefab, spawnPosition, spawnRotation, parent);
            fish.name = $"DecorativeFish_{groupName}_{i + 1:00}";

            float randomScale = Random.Range(group.scaleRange.x, group.scaleRange.y);
            fish.transform.localScale *= randomScale;

            DecorativeFishWander wander = fish.GetComponent<DecorativeFishWander>();
            if (wander == null)
            {
                wander = fish.AddComponent<DecorativeFishWander>();
            }

            wander.boundsCenter = transform;
            wander.swimAreaSize = swimAreaSize;
            wander.swimSpeed = Random.Range(group.swimSpeedRange.x, group.swimSpeedRange.y);
            wander.turnSpeed = Random.Range(group.turnSpeedRange.x, group.turnSpeedRange.y);
            wander.modelRotationOffset = group.modelRotationOffset;
            wander.boundaryTurnDistance = boundaryTurnDistance;
            ApplyBoundsToWander(wander, group.yRange);
            DisableGameplayPhysics(fish);
        }
    }

    private Vector3 RandomPointInBounds(Vector2 yRange)
    {
        GetHorizontalBounds(out float minX, out float maxX, out float minZ, out float maxZ);
        float minY = Mathf.Min(yRange.x, yRange.y);
        float maxY = Mathf.Max(yRange.x, yRange.y);

        if (invisibleCeilingCollider != null)
        {
            maxY = Mathf.Min(maxY, invisibleCeilingCollider.bounds.min.y);
        }

        return new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            Random.Range(minZ, maxZ)
        );
    }

    private void ApplyBoundsToWander(DecorativeFishWander wander, Vector2 yRange)
    {
        GetHorizontalBounds(out float minX, out float maxX, out float minZ, out float maxZ);
        float minY = Mathf.Min(yRange.x, yRange.y);
        float maxY = Mathf.Max(yRange.x, yRange.y);

        if (invisibleCeilingCollider != null)
        {
            maxY = Mathf.Min(maxY, invisibleCeilingCollider.bounds.min.y);
        }

        wander.SetWorldBounds(
            new Vector3(minX, minY, minZ),
            new Vector3(maxX, maxY, maxZ)
        );
    }

    private void GetHorizontalBounds(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        bool hasBounds = false;
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;
        minZ = float.PositiveInfinity;
        maxZ = float.NegativeInfinity;

        if (sideWallColliders != null)
        {
            foreach (Collider wallCollider in sideWallColliders)
            {
                if (wallCollider == null) continue;

                Bounds bounds = wallCollider.bounds;
                minX = Mathf.Min(minX, bounds.min.x);
                maxX = Mathf.Max(maxX, bounds.max.x);
                minZ = Mathf.Min(minZ, bounds.min.z);
                maxZ = Mathf.Max(maxZ, bounds.max.z);
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            Vector3 halfSize = swimAreaSize * 0.5f;
            minX = transform.position.x - halfSize.x;
            maxX = transform.position.x + halfSize.x;
            minZ = transform.position.z - halfSize.z;
            maxZ = transform.position.z + halfSize.z;
        }

        minX += boundaryPadding;
        maxX -= boundaryPadding;
        minZ += boundaryPadding;
        maxZ -= boundaryPadding;
    }

    private void DisableGameplayPhysics(GameObject fish)
    {
        foreach (Collider fishCollider in fish.GetComponentsInChildren<Collider>(true))
        {
            fishCollider.enabled = false;
        }

        foreach (Rigidbody body in fish.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
            body.useGravity = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        GetHorizontalBounds(out float minX, out float maxX, out float minZ, out float maxZ);
        float minY = transform.position.y - swimAreaSize.y * 0.5f;
        float maxY = transform.position.y + swimAreaSize.y * 0.5f;
        Vector3 min = new Vector3(minX, minY, minZ);
        Vector3 max = new Vector3(maxX, maxY, maxZ);
        Gizmos.DrawWireCube((min + max) * 0.5f, max - min);
    }
}
