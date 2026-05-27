using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class AbilityUnlockPopup : MonoBehaviour
{
    private const string DoubleJumpPopupPath = "UI/AbilityUnlock/double_jump_popup";
    private const string WallClimbPopupPath = "UI/AbilityUnlock/wall_climb_popup";
    private const float ReferencePopupWidth = 900f;

    private struct PopupRequest
    {
        public Texture2D Texture;
    }

    private static AbilityUnlockPopup instance;

    private readonly Queue<PopupRequest> queuedPopups = new Queue<PopupRequest>();

    private GameObject root;
    private RectTransform popupRect;
    private RawImage popupImage;
    private Button okButton;

    private bool isShowing;
    private bool hasPausedGame;
    private float previousTimeScale = 1f;
    private bool previousAudioPause;
    private CursorLockMode previousCursorLockMode;
    private bool previousCursorVisible;
    private float canCloseAt;

    public static void ShowDoubleJump()
    {
        Show(DoubleJumpPopupPath);
    }

    public static void ShowWallClimb()
    {
        Show(WallClimbPopupPath);
    }

    private static void Show(string texturePath)
    {
        EnsureInstance();

        Texture2D texture = Resources.Load<Texture2D>(texturePath);
        if (texture == null)
        {
            Debug.LogWarning("Ability unlock popup texture missing: Resources/" + texturePath);
            return;
        }

        instance.ShowOrQueue(new PopupRequest { Texture = texture });
    }

    private static void EnsureInstance()
    {
        if (instance != null) return;

        GameObject popupObject = new GameObject("AbilityUnlockPopup");
        instance = popupObject.AddComponent<AbilityUnlockPopup>();
        DontDestroyOnLoad(popupObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildUI();
        EnsureEventSystem();
    }

    private void Update()
    {
        if (!isShowing || Time.unscaledTime < canCloseAt) return;

        bool closePressed =
            (Keyboard.current != null &&
             (Keyboard.current.enterKey.wasPressedThisFrame ||
              Keyboard.current.spaceKey.wasPressedThisFrame ||
              Keyboard.current.escapeKey.wasPressedThisFrame)) ||
            (Gamepad.current != null &&
             (Gamepad.current.buttonSouth.wasPressedThisFrame ||
              Gamepad.current.startButton.wasPressedThisFrame));

        if (closePressed)
        {
            CloseCurrentPopup();
        }
    }

    private void ShowOrQueue(PopupRequest request)
    {
        if (isShowing)
        {
            queuedPopups.Enqueue(request);
            return;
        }

        ShowNow(request);
    }

    private void ShowNow(PopupRequest request)
    {
        PauseGame();

        popupImage.texture = request.Texture;
        ResizePopup(request.Texture);

        root.SetActive(true);
        okButton.Select();
        isShowing = true;
        canCloseAt = Time.unscaledTime + 0.25f;
    }

    private void CloseCurrentPopup()
    {
        root.SetActive(false);
        isShowing = false;

        if (queuedPopups.Count > 0)
        {
            ShowNow(queuedPopups.Dequeue());
            return;
        }

        ResumeGame();
    }

    private void PauseGame()
    {
        if (hasPausedGame) return;

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousCursorLockMode = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        hasPausedGame = true;
    }

    private void ResumeGame()
    {
        if (!hasPausedGame) return;

        Time.timeScale = previousTimeScale;
        AudioListener.pause = previousAudioPause;
        Cursor.lockState = previousCursorLockMode;
        Cursor.visible = previousCursorVisible;

        hasPausedGame = false;
    }

    private void BuildUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        root = CreateChild("Root", transform);
        Stretch(root.GetComponent<RectTransform>());

        Image dimmer = root.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.78f);

        GameObject popup = CreateChild("ApprovedPopupImage", root.transform);
        popupRect = popup.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = Vector2.zero;
        popupImage = popup.AddComponent<RawImage>();
        popupImage.raycastTarget = false;

        okButton = CreateInvisibleOkButton(popup.transform);

        root.SetActive(false);
    }

    private void ResizePopup(Texture2D texture)
    {
        float aspect = (float)texture.height / texture.width;
        popupRect.sizeDelta = new Vector2(ReferencePopupWidth, ReferencePopupWidth * aspect);
    }

    private Button CreateInvisibleOkButton(Transform parent)
    {
        GameObject buttonObject = CreateChild("OKClickTarget", parent);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.sizeDelta = new Vector2(170f, 92f);
        buttonRect.anchoredPosition = new Vector2(-26f, 16f);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(CloseCurrentPopup);

        return button;
    }

    private static GameObject CreateChild(string name, Transform parent)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }
}
