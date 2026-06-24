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
        InitializeHealthUI();
        UpdateHealthUI();
    }

    void InitializeHealthUI()
    {
        // Try to find an existing canvas or text in the scene
        if (healthText == null)
        {
            healthText = FindAnyObjectByType<TextMeshProUGUI>();
            if (healthText != null && !healthText.name.Contains("Health"))
            {
                healthText = null;
            }
        }

        // If still null, dynamically create a clean screen overlay Canvas and Health Text
        if (healthText == null)
        {
            GameObject canvasObj = new GameObject("HealthCanvas");
            healthCanvas = canvasObj.AddComponent<Canvas>();
            healthCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Keep UI on top of other canvas elements
            healthCanvas.sortingOrder = 99;

            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create TextMeshPro UI Element
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(canvasObj.transform, false);
            healthText = textObj.AddComponent<TextMeshProUGUI>();

            // Position at top-left of the screen with margin
            RectTransform rect = healthText.rectTransform;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(25, -25);
            rect.sizeDelta = new Vector2(400, 100);

            // Premium Styling
            healthText.fontSize = 32;
            healthText.fontWeight = FontWeight.Bold;
            healthText.color = new Color(0.9f, 0.2f, 0.2f); // Vibrant rose red
            
            // Add subtle shadow/outline for readability against any background
            healthText.outlineColor = Color.black;
            healthText.outlineWidth = 0.2f;
        }
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
