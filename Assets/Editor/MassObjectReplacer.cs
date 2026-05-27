using UnityEditor;
using UnityEngine;

public class MassObjectReplacer : EditorWindow
{
    private GameObject replacementObject;

    [MenuItem("Tools/Mass Object Replacer")]
    public static void ShowWindow()
        => GetWindow<MassObjectReplacer>("Object Replacer");

    private void OnGUI()
    {
        GUILayout.Label("Mass Replace Selected Objects", EditorStyles.boldLabel);

        replacementObject = (GameObject)EditorGUILayout.ObjectField(
            "New Object / Prefab",
            replacementObject,
            typeof(GameObject),
            false
        );

        if (GUILayout.Button("Replace Selected Objects"))
        {
            ReplaceObjects();
        }
    }

    private void ReplaceObjects()
    {
        if (replacementObject == null)
        {
            Debug.LogError("Please assign a New Object/Prefab first!");
            return;
        }

        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogError("No objects selected in the Scene!");
            return;
        }

        // Register the action so you can press Ctrl+Z to undo
        Undo.RegisterCompleteObjectUndo(selectedObjects, "Mass Replace");

        foreach (GameObject oldObj in selectedObjects)
        {
            // Create the new object at the old object's position and rotation
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementObject);
            if (newObj == null)
            {
                newObj = Instantiate(replacementObject);
            }

            newObj.transform.SetParent(oldObj.transform.parent);
            newObj.transform.position = oldObj.transform.position;
            newObj.transform.rotation = oldObj.transform.rotation;
            newObj.transform.localScale = oldObj.transform.localScale;

            Undo.RegisterCreatedObjectUndo(newObj, "Create Replacement");
            Undo.DestroyObjectImmediate(oldObj);
        }
    }
}
