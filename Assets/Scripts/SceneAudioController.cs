using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneAudioController : MonoBehaviour
{
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (playOnStart)
        {
            PlayBGMForActiveScene();
        }
    }

    public void PlayBGMForActiveScene()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager == null)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();

        switch (sceneName)
        {
            case "mainmenu":
            case "credit":
                audioManager.PlayMainMenuBGM();
                break;
            case "leveleasy":
            case "scene_easy":
                audioManager.PlayEasyBGM();
                break;
            case "levelmedium":
            case "scene_medium":
                audioManager.PlayMediumBGM();
                break;
            case "levelhard":
            case "scene_hard":
                audioManager.PlayHardBGM();
                break;
        }
    }
}
