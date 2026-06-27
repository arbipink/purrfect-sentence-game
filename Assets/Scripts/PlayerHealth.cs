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

    [Header("UI Game Over Settings")]
    private bool gameOverHandled;
    public GameObject gameOverPanel;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
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

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Debug.Log("Player died! Restarting level...");
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 0f;
    }

    public void RestartLevelButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenuButton()
    {
        Time.timeScale = 1f;
        
        SceneManager.LoadScene("MainMenu"); 
    }
}
