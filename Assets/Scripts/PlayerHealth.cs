using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("UI Settings")]
    public TextMeshProUGUI healthText;
    private Canvas healthCanvas;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHealthUI();

        // Play damage feedback/shake or other reactions here if needed
        Debug.Log($"Player took damage! Remaining Health: {currentHealth}");

        // Clear active enemies and proceed to the next grammar wave
        EnemySpawner spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.HandlePlayerHit();
        }

        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}";
        }
    }

    void OnPlayerDeath()
    {
        Debug.Log("Player died! Restarting level...");
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
