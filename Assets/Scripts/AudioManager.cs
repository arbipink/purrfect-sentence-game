using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip easyBGM;
    [SerializeField] private AudioClip mediumBGM;
    [SerializeField] private AudioClip hardBGM;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip slashSFX;
    [SerializeField] private AudioClip correctAnswerSFX;
    [SerializeField] private AudioClip wrongAnswerSFX;
    [SerializeField] private AudioClip enemyDefeatedSFX;
    [SerializeField] private AudioClip damageSFX;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private AudioClip levelCompleteSFX;
    [SerializeField] private AudioClip pauseSFX;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private float lastButtonClickTime = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Reset()
    {
        EnsureAudioSources();
        ApplyVolumes();
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmSource == null)
        {
            bgmSource = sources.Length > 0 ? sources[0] : gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sources = GetComponents<AudioSource>();
            sfxSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        }

        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        EnsureAudioSources();

        if (clip == null || bgmSource == null)
        {
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    private void PlaySFX(AudioClip clip)
    {
        EnsureAudioSources();

        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    private void ApplyVolumes()
    {
        masterVolume = Mathf.Clamp01(masterVolume);
        bgmVolume = Mathf.Clamp01(bgmVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume * bgmVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = masterVolume * sfxVolume;
        }
    }

    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    public void PlayEasyBGM()
    {
        PlayBGM(easyBGM);
    }

    public void PlayMediumBGM()
    {
        PlayBGM(mediumBGM);
    }

    public void PlayHardBGM()
    {
        PlayBGM(hardBGM);
    }

    public void PlayButtonClick()
    {
        if (Time.unscaledTime - lastButtonClickTime < 0.05f)
        {
            return;
        }

        lastButtonClickTime = Time.unscaledTime;
        PlaySFX(buttonClickSFX);
    }

    public void PlaySlash()
    {
        PlaySFX(slashSFX);
    }

    public void PlayCorrectAnswer()
    {
        PlaySFX(correctAnswerSFX);
    }

    public void PlayWrongAnswer()
    {
        PlaySFX(wrongAnswerSFX);
    }

    public void PlayEnemyDefeated()
    {
        PlaySFX(enemyDefeatedSFX);
    }

    public void PlayDamage()
    {
        PlaySFX(damageSFX);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverSFX);
    }

    public void PlayLevelComplete()
    {
        PlaySFX(levelCompleteSFX);
    }

    public void PlayPause()
    {
        PlaySFX(pauseSFX);
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        ApplyVolumes();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        ApplyVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        ApplyVolumes();
    }
}
