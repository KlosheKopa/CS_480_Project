using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI Elements to Hide on Death")]
    public GameObject crosshairDot;
    public GameObject healthBarObject;
    public GameObject staminaBarObject;
    public GameObject clawArm;
    public GameObject levelText;
    public GameObject expIcon;
    public GameObject expBar;           // ← Drag your top-level ExpBar here

    [Header("Health Bar Slider")]
    public Slider healthBar;

    [Header("Shader References")]
    public Image healthBarFillImage;

    [Header("Death UI")]
    public TextMeshProUGUI deathText;
    public Image fadeImage;
    public GameObject deathScreen;

    [Header("Death Sequence")]
    public float deathFadeTime = 3f;

    private PlayerStats stats;
    public bool isDead = false;
    private float invincibilityTimer = 0f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (stats != null) stats.CurrentHealth = stats.maxHealth;
        RefreshHealthBar();

        if (deathScreen != null)
            deathScreen.SetActive(false);
    }

    void Update()
    {
        if (invincibilityTimer > 0)
            invincibilityTimer -= Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || invincibilityTimer > 0) return;

        float actualDamage = damage * (1f - stats.defense);
        stats.CurrentHealth -= actualDamage;

        invincibilityTimer = stats.invincibilityTime;

        RefreshHealthBar();

        if (stats.CurrentHealth <= 0)
            Die();
    }

    public void RefreshHealthBar()
    {
        if (stats != null)
        {
            float fillRatio = stats.CurrentHealth / stats.maxHealth;

            // 1. Keep the standard slider logic if you're using it for positioning
            if (healthBar != null) healthBar.value = fillRatio;

            // 2. SEND VALUE TO SHADER
            // Make sure "_FillAmount" matches the Reference name in your Shader Blackboard
            if (healthBarFillImage != null && healthBarFillImage.material != null)
            {
                healthBarFillImage.material.SetFloat("_FillAmount", fillRatio);
            }
        }
    }

    void Die()
    {
        isDead = true;

        // Disable controls
        var pc = GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        var cs = GetComponent<ClawShooter>();
        if (cs != null) cs.enabled = false;

        var ps = GetComponent<PlayerStamina>();
        if (ps != null) ps.enabled = false;

        // Hide all gameplay UI
        if (crosshairDot != null) crosshairDot.SetActive(false);
        if (healthBarObject != null) healthBarObject.SetActive(false);
        if (staminaBarObject != null) staminaBarObject.SetActive(false);
        if (clawArm != null) clawArm.SetActive(false);
        if (levelText != null) levelText.SetActive(false);
        if (expIcon != null) expIcon.SetActive(false);

        // Hide entire EXP Bar + the stubborn Fill dot
        if (expBar != null)
        {
            expBar.SetActive(false);
        }

        // Show death screen
        if (deathScreen != null)
            deathScreen.SetActive(true);

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        float timer = 0f;
        while (timer < deathFadeTime)
        {
            timer += Time.deltaTime;
            float fadeT = Mathf.Clamp01(timer / deathFadeTime);

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = fadeT * 0.98f;
                fadeImage.color = c;
            }
            if (deathText != null)
            {
                Color c = deathText.color;
                c.a = fadeT;
                deathText.color = c;
            }
            yield return null;
        }

        Time.timeScale = 0f;
    }
}