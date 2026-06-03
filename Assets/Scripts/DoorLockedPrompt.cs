using UnityEngine;
using TMPro;
using System.Collections;

public class DoorLockedPrompt : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI promptText;

    private Coroutine currentRoutine;
    private Color originalColor;
    private bool isShowing = false;

    void Awake()
    {
        if (promptText != null)
        {
            originalColor = promptText.color;
            originalColor.a = 0f;
            promptText.color = originalColor;
        }
    }

    public void ShowLockedMessage()
    {
        if (promptText == null) return;
        if (isShowing) return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowMessageRoutine());
    }

    private IEnumerator ShowMessageRoutine()
    {
        isShowing = true;

        // === Support both active and inactive at start ===
        bool wasInactive = !gameObject.activeSelf;
        if (wasInactive)
            gameObject.SetActive(true);
        // ================================================

        promptText.text = "Door is Locked. Needs a Key to Open";

        // Fade in (1 second)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            originalColor.a = Mathf.Lerp(0f, 1f, t);
            promptText.color = originalColor;
            yield return null;
        }

        // Stay visible (2 seconds)
        yield return new WaitForSeconds(2f);

        // Fade out (2 seconds)
        t = 0f;
        while (t < 2f)
        {
            t += Time.deltaTime;
            originalColor.a = Mathf.Lerp(1f, 0f, t / 2f);
            promptText.color = originalColor;
            yield return null;
        }

        // Final cleanup
        originalColor.a = 0f;
        promptText.color = originalColor;

        // Turn it back off only if it was inactive at the start
        if (wasInactive)
            gameObject.SetActive(false);

        isShowing = false;
    }
}