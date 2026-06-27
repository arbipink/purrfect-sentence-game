using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject creditsPanel;

    private Button startButton;
    private Button creditButton;
    private Button exitButton;

    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button backButton;

    private void Start()
    {
        // Force cursor state for main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false); 
        }

        // Auto-wire buttons by name
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.name == "Play Game")
            {
                startButton = btn;
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(ShowLevelSelection);
            }
            else if (btn.gameObject.name == "Credit")
            {
                creditButton = btn;
                creditButton.onClick.RemoveAllListeners();
                creditButton.onClick.AddListener(ShowCredits);
            }
            else if (btn.gameObject.name == "Exit")
            {
                exitButton = btn;
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
            }
            else if (btn.gameObject.name == "Easy")
            {
                easyButton = btn;
                easyButton.onClick.RemoveAllListeners();
                easyButton.onClick.AddListener(() => LoadLevel("Scene_Easy"));
                easyButton.gameObject.SetActive(false);
            }
            else if (btn.gameObject.name == "Medium")
            {
                mediumButton = btn;
                mediumButton.onClick.RemoveAllListeners();
                mediumButton.onClick.AddListener(() => LoadLevel("Scene_Medium"));
                mediumButton.gameObject.SetActive(false);
            }
            else if (btn.gameObject.name == "Hard")
            {
                hardButton = btn;
                hardButton.onClick.RemoveAllListeners();
                hardButton.onClick.AddListener(() => LoadLevel("Scene_Hard"));
                hardButton.gameObject.SetActive(false); 
            }
            else if (btn.gameObject.name == "BackButton")
            {
                backButton = btn;
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBackToMainMenu);
                backButton.gameObject.SetActive(false); 
            }
        }
    }

    private void LoadLevel(string sceneName)
    {
        PlayButtonClick();
        Debug.Log("Loading level: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void ShowLevelSelection()
    {
        PlayButtonClick();

        // Hide Main Menu buttons
        if (startButton != null) startButton.gameObject.SetActive(false);
        if (creditButton != null) creditButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        // Show Difficulty buttons
        if (easyButton != null) easyButton.gameObject.SetActive(true);
        if (mediumButton != null) mediumButton.gameObject.SetActive(true);
        if (hardButton != null) hardButton.gameObject.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
    }

    public void GoBackToMainMenu()
    {
        PlayButtonClick();

        // Show Main Menu buttons
        if (startButton != null) startButton.gameObject.SetActive(true);
        if (creditButton != null) creditButton.gameObject.SetActive(true);
        if (exitButton != null) exitButton.gameObject.SetActive(true);

        // Hide Difficulty buttons
        if (easyButton != null) easyButton.gameObject.SetActive(false);
        if (mediumButton != null) mediumButton.gameObject.SetActive(false);
        if (hardButton != null) hardButton.gameObject.SetActive(false);

        // Hide Credit Text GameObject
        // if (creditTextObj != null) creditTextObj.SetActive(false);

        // Destroy dynamic panel if it exists
        if (creditsPanel != null)
        {
            // Destroy(creditsPanel)
            creditsPanel.SetActive(false);
        }

        // Hide Back Button
        if (backButton != null) backButton.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        PlayButtonClick();
        Debug.Log("Exiting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowCredits()
    {
        PlayButtonClick();

        // Hide Main Menu buttons
        if (startButton != null) startButton.gameObject.SetActive(false);
        if (creditButton != null) creditButton.gameObject.SetActive(false);
        if (exitButton != null) exitButton.gameObject.SetActive(false);

        // Show Back Button
        if (backButton != null) backButton.gameObject.SetActive(true);

        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    private void PlayButtonClick()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }
}
