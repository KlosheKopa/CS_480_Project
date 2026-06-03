using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DecorativeFishSchoolSpawner))]
public class DecorativeFishSchoolSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DecorativeFishSchoolSpawner spawner = (DecorativeFishSchoolSpawner)target;

        GUILayout.Space(8f);

        if (GUILayout.Button("Respawn Preview Fish"))
        {
            spawner.RespawnPreviewFish();
            EditorUtility.SetDirty(spawner);
        }

        if (GUILayout.Button("Clear Preview Fish"))
        {
            spawner.ClearSpawnedFish();
            EditorUtility.SetDirty(spawner);
        }
    }
}
