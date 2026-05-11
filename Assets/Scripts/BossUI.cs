using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    public Slider healthSlider;
    private Jellyfish bossScript;
    private bool victoryTriggered = false;

    public void SetupBoss(Jellyfish targetBoss)
    {
        bossScript = targetBoss;
        healthSlider.maxValue = targetBoss.maxHealth;
        healthSlider.value = targetBoss.maxHealth;

        // Reset state for new boss
        victoryTriggered = false;
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (bossScript != null)
        {
            // Smoothly update the health bar visual
            // Using CurrentHealth (uppercase) as discussed for the public property
            healthSlider.value = Mathf.Lerp(healthSlider.value, bossScript.CurrentHealth, Time.deltaTime * 5f);

            // Check for death and trigger victory only once
            if (bossScript.isDead && !victoryTriggered)
            {
                victoryTriggered = true;
                Invoke("TriggerVictory", 1.5f); // Slight delay for the death animation
            }
        }
    }

    void TriggerVictory()
    {
        gameObject.SetActive(false);
        GameManager.Instance.ShowVictory();
    }
}
