using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CreateMainMenuScene
{
    private const string PosterPath = "Assets/UI_Sprites/game_poster.png";
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Create Main Menu Scene")]
    public static void Run()
    {
        ConfigurePosterImport();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;

        Sprite posterSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PosterPath);

        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1672f, 941f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backgroundObject = new GameObject("PosterBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        StretchToParent(backgroundRect);

        Image backgroundImage = backgroundObject.GetComponent<Image>();
        backgroundImage.sprite = posterSprite;
        backgroundImage.preserveAspect = false;
        backgroundImage.raycastTarget = false;

        GameObject buttonObject = new GameObject("StartGameButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(MainMenuStartButton));
        buttonObject.transform.SetParent(canvasObject.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0f);
        buttonRect.anchorMax = new Vector2(0.5f, 0f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, 118f);
        buttonRect.sizeDelta = new Vector2(540f, 104f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0.05f, 0.9f, 1f, 0f);
        buttonImage.raycastTarget = true;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.9f, 1f, 0.65f);
        outline.effectDistance = new Vector2(3f, -3f);

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.05f, 0.9f, 1f, 0f);
        colors.highlightedColor = new Color(0.05f, 0.9f, 1f, 0.28f);
        colors.pressedColor = new Color(1f, 1f, 1f, 0.38f);
        colors.selectedColor = new Color(0.05f, 0.9f, 1f, 0.18f);
        colors.disabledColor = new Color(0f, 0f, 0f, 0.25f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        MainMenuStartButton startButton = buttonObject.GetComponent<MainMenuStartButton>();
        UnityEventTools.AddPersistentListener(button.onClick, startButton.StartGame);

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.transform.SetParent(null);

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        EditorSceneManager.SaveScene(scene, ScenePath);
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigurePosterImport()
    {
        AssetDatabase.ImportAsset(PosterPath);

        TextureImporter importer = AssetImporter.GetAtPath(PosterPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaSource = TextureImporterAlphaSource.None;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
    }

    private static void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
}
