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

    private bool gameOverHandled;

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

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayDamage();
        }

        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        if (vfxManager != null)
        {
            vfxManager.PlayDamage(transform.position, gameObject);
        }

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
        if (gameOverHandled)
        {
            return;
        }

        gameOverHandled = true;

        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayGameOver();
        }

        Debug.Log("Player died! Restarting level...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
