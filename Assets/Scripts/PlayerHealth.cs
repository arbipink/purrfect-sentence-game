using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Heart UI")]
    public Image[] hearts;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthUI();

        Debug.Log("Player took damage! Remaining Health: " + currentHealth);

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
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentHealth;
        }
    }

    void OnPlayerDeath()
    {
        Debug.Log("Player died! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}