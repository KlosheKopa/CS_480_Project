using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AlphaDecorativeFishSetupAutoRunner
{
    private const string PendingFlagPath = "Library/CodexAlphaDecorativeFishSetup.pending";

    static AlphaDecorativeFishSetupAutoRunner()
    {
        EditorApplication.delayCall += RunIfPending;
    }

    [MenuItem("Tools/Setup/Request Alpha Decorative Fish Autorun")]
    public static void RequestRun()
    {
        File.WriteAllText(PendingFlagPath, "pending");
        AssetDatabase.Refresh();
    }

    private static void RunIfPending()
    {
        if (!File.Exists(PendingFlagPath)) return;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += RunIfPending;
            return;
        }

        File.Delete(PendingFlagPath);

        try
        {
            AlphaDecorativeFishSetup.Run();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}

public static class AlphaDecorativeFishSetup
{
    private const string AlphaScenePath = "Assets/Scenes/Alpha.unity";
    private const string FishPrefabFolder = "Assets/Prefabs/Fish";
    private const string SpawnerName = "Decorative Fish School - Alpha";

    private struct FishModelConfig
    {
        public string GroupName;
        public string SourcePath;
        public string PrefabPath;
        public int Count;
        public Vector2 YRange;
        public float TargetLongestDimension;
        public Vector2 ScaleRange;
        public Vector2 SwimSpeedRange;
        public Vector2 TurnSpeedRange;

        public FishModelConfig(
            string groupName,
            string sourcePath,
            string prefabPath,
            int count,
            Vector2 yRange,
            float targetLongestDimension,
            Vector2 scaleRange,
            Vector2 swimSpeedRange,
            Vector2 turnSpeedRange)
        {
            GroupName = groupName;
            SourcePath = sourcePath;
            PrefabPath = prefabPath;
            Count = count;
            YRange = yRange;
            TargetLongestDimension = targetLongestDimension;
            ScaleRange = scaleRange;
            SwimSpeedRange = swimSpeedRange;
            TurnSpeedRange = turnSpeedRange;
        }
    }

    private static readonly FishModelConfig[] FishConfigs =
    {
        new FishModelConfig(
            "Swimming Shark",
            "Assets/Models/fishes/animated-swimming-great-white-shark-loop/source/Swimming shark.glb",
            $"{FishPrefabFolder}/SwimmingShark_Decorative.prefab",
            2,
            new Vector2(60f, 80f),
            7f,
            new Vector2(0.85f, 1.15f),
            new Vector2(1.2f, 2f),
            new Vector2(1.5f, 2.8f)),
        new FishModelConfig(
            "Hammerhead Shark",
            "Assets/Models/fishes/model_73a_-_great_hammerhead_shark.glb",
            $"{FishPrefabFolder}/HammerheadShark_Decorative.prefab",
            2,
            new Vector2(60f, 80f),
            6f,
            new Vector2(0.85f, 1.2f),
            new Vector2(1.1f, 1.9f),
            new Vector2(1.5f, 2.8f)),
        new FishModelConfig(
            "Manta Ray",
            "Assets/Models/fishes/model_84b_-_manta_ray_swimming.glb",
            $"{FishPrefabFolder}/MantaRay_Decorative.prefab",
            2,
            new Vector2(60f, 80f),
            5.5f,
            new Vector2(0.9f, 1.25f),
            new Vector2(1f, 1.7f),
            new Vector2(1.4f, 2.5f)),
        new FishModelConfig(
            "Tuna Fish",
            "Assets/Models/fishes/tuna_fish.glb",
            $"{FishPrefabFolder}/TunaFish_Decorative.prefab",
            8,
            new Vector2(40f, 80f),
            3f,
            new Vector2(0.8f, 1.25f),
            new Vector2(1.4f, 2.5f),
            new Vector2(1.8f, 3.5f)),
        new FishModelConfig(
            "Fishe",
            "Assets/Models/fishes/fishe/source/fishe.glb",
            $"{FishPrefabFolder}/Fishe_Decorative.prefab",
            8,
            new Vector2(40f, 80f),
            2.4f,
            new Vector2(0.75f, 1.2f),
            new Vector2(1.3f, 2.4f),
            new Vector2(1.8f, 3.5f)),
    };

    [MenuItem("Tools/Setup/Alpha Decorative Fish")]
    public static void Run()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(FishPrefabFolder);

        AssetDatabase.Refresh();

        List<GameObject> prefabs = new List<GameObject>();
        foreach (FishModelConfig config in FishConfigs)
        {
            AssetDatabase.ImportAsset(config.SourcePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            GameObject prefab = CreateDecorativePrefab(config);
            prefabs.Add(prefab);
        }

        SetupAlphaScene(prefabs);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Alpha Decorative Fish] Setup complete for Alpha.");
    }

    private static GameObject CreateDecorativePrefab(FishModelConfig config)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(config.SourcePath);
        if (source == null)
        {
            throw new System.InvalidOperationException($"Could not load fish model as GameObject: {config.SourcePath}");
        }

        GameObject root = new GameObject(config.GroupName.Replace(" ", "") + "_Decorative");
        GameObject model = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (model == null)
        {
            model = Object.Instantiate(source);
        }

        model.name = config.GroupName + "_Model";
        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        RemoveGameplayPhysics(root);
        NormalizeSize(root, config.TargetLongestDimension);
        PlayImportedAnimationIfAvailable(root);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, config.PrefabPath);
        Object.DestroyImmediate(root);

        if (prefab == null)
        {
            throw new System.InvalidOperationException($"Could not save decorative fish prefab: {config.PrefabPath}");
        }

        return prefab;
    }

    private static void SetupAlphaScene(List<GameObject> prefabs)
    {
        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene scene = FindLoadedScene(AlphaScenePath);
        bool openedSceneForSetup = !scene.IsValid();

        if (openedSceneForSetup)
        {
            scene = EditorSceneManager.OpenScene(AlphaScenePath, OpenSceneMode.Additive);
        }

        GameObject spawnerObject = FindGameObjectInScene(scene, SpawnerName);
        if (spawnerObject == null)
        {
            spawnerObject = new GameObject(SpawnerName);
            SceneManager.MoveGameObjectToScene(spawnerObject, scene);
        }

        spawnerObject.transform.position = new Vector3(150f, 60f, 150f);

        DecorativeFishSchoolSpawner spawner = spawnerObject.GetComponent<DecorativeFishSchoolSpawner>();
        if (spawner == null)
        {
            spawner = spawnerObject.AddComponent<DecorativeFishSchoolSpawner>();
        }

        spawner.sideWallColliders = FindColliders(scene, "Side Wall 1", "Side Wall 2", "Side Wall 3", "Side Wall 4");
        GameObject ceiling = FindGameObjectInScene(scene, "Invisible Ceiling");
        spawner.invisibleCeilingCollider = ceiling != null ? ceiling.GetComponent<Collider>() : null;

        spawner.swimAreaSize = new Vector3(285f, 40f, 285f);
        spawner.boundaryPadding = 10f;
        spawner.boundaryTurnDistance = 14f;
        spawner.spawnOnStart = true;
        spawner.parentFishToSpawner = true;
        spawner.clearExistingBeforeSpawn = true;

        spawner.fishPrefabs = new GameObject[0];
        spawner.fishGroups = new DecorativeFishSpawnGroup[FishConfigs.Length];
        for (int i = 0; i < FishConfigs.Length; i++)
        {
            FishModelConfig config = FishConfigs[i];
            spawner.fishGroups[i] = new DecorativeFishSpawnGroup
            {
                groupName = config.GroupName,
                prefab = prefabs[i],
                count = config.Count,
                yRange = config.YRange,
                scaleRange = config.ScaleRange,
                swimSpeedRange = config.SwimSpeedRange,
                turnSpeedRange = config.TurnSpeedRange,
                modelRotationOffset = Vector3.zero
            };
        }

        spawner.ClearSpawnedFish();
        spawner.RespawnPreviewFish();

        EditorUtility.SetDirty(spawner);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        if (openedSceneForSetup)
        {
            EditorSceneManager.CloseScene(scene, true);
            if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousActiveScene);
            }
        }
    }

    private static Scene FindLoadedScene(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                return scene;
            }
        }

        return default;
    }

    private static GameObject FindGameObjectInScene(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform match = FindChildRecursive(root.transform, objectName);
            if (match != null)
            {
                return match.gameObject;
            }
        }

        return null;
    }

    private static Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root.name == objectName) return root;

        foreach (Transform child in root)
        {
            Transform match = FindChildRecursive(child, objectName);
            if (match != null) return match;
        }

        return null;
    }

    private static Collider[] FindColliders(Scene scene, params string[] names)
    {
        List<Collider> colliders = new List<Collider>();
        foreach (string objectName in names)
        {
            GameObject sceneObject = FindGameObjectInScene(scene, objectName);
            if (sceneObject == null) continue;

            Collider sceneCollider = sceneObject.GetComponent<Collider>();
            if (sceneCollider != null)
            {
                colliders.Add(sceneCollider);
            }
        }

        return colliders.ToArray();
    }

    private static void RemoveGameplayPhysics(GameObject root)
    {
        foreach (Collider fishCollider in root.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(fishCollider);
        }

        foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
        {
            Object.DestroyImmediate(body);
        }
    }

    private static void NormalizeSize(GameObject root, float targetLongestDimension)
    {
        Bounds bounds = CalculateRendererBounds(root);
        float longestDimension = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (longestDimension <= 0.001f) return;

        root.transform.localScale = Vector3.one * (targetLongestDimension / longestDimension);
    }

    private static Bounds CalculateRendererBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void PlayImportedAnimationIfAvailable(GameObject root)
    {
        foreach (Animation animation in root.GetComponentsInChildren<Animation>(true))
        {
            foreach (AnimationState state in animation)
            {
                state.wrapMode = WrapMode.Loop;
                animation.clip = state.clip;
                animation.playAutomatically = true;
                break;
            }
        }

        foreach (Animator animator in root.GetComponentsInChildren<Animator>(true))
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = System.IO.Path.GetFileName(folderPath);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
