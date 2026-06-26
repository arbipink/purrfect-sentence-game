using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DotConnectManager : MonoBehaviour
{
    private GameObject lastSelectedDot;

    private List<GameObject> connectedDots = new List<GameObject>();

    [Tooltip("Slightly lift the smoke effect above the ground to avoid clipping")]
    public float lineHoverOffset = 0.25f;

    [Header("VFX Settings")]
    public GameObject[] smokePrefabs;
    [Tooltip("Distance in world space between smoke spawns along the line")]
    public float smokeSpawnIntervalDistance = 0.5f;
    private Vector3 lastSmokeSpawnPos;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public float clickRadiusPixels = 70f;
    private EnemySpawner spawner;

    private Toggle pauseToggle;
    private bool suppressPauseToggleAudio;
    private bool pauseSlidersReady;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        spawner = FindAnyObjectByType<EnemySpawner>();

        SetupPauseToggle();
        SetupPauseSliders();
    }

    void Update()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }

        if (Time.timeScale == 0f) return;
        
        var pointer = Pointer.current;
        if (pointer == null) return;

        var press = pointer.press;

        if (press != null)
        {
            if (press.wasPressedThisFrame)
            {
                HandleMouseClick();
            }
            else if (press.isPressed && lastSelectedDot != null)
            {
                HandleMouseDrag();
            }
            else if (press.wasReleasedThisFrame)
            {
                StopDrawing();
            }
        }
    }

    GameObject FindEnemyAtMousePosition(Vector2 mousePos)
    {
        EnemyMovement[] allEnemies = FindObjectsByType<EnemyMovement>(FindObjectsInactive.Exclude);
        GameObject selectedEnemy = null;
        float closestDistance = clickRadiusPixels;

        foreach (EnemyMovement enemy in allEnemies)
        {
            if (enemy == null) continue;

            // Convert enemy's 3D position to 2D screen coordinates
            Vector2 enemyOnScreen = Camera.main.WorldToScreenPoint(enemy.transform.position);
            float distance = Vector2.Distance(mousePos, enemyOnScreen);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                selectedEnemy = enemy.gameObject;
            }
        }
        return selectedEnemy;
    }

    void HandleMouseClick()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        GameObject enemyNearMouse = FindEnemyAtMousePosition(mousePosition);
        
        // 1. Project mouse position directly into 3D space relative to camera depth
        // Estimate constant distance from camera to target
        
        if (enemyNearMouse != null)
        {
            lastSelectedDot = enemyNearMouse;
            connectedDots.Clear();
            connectedDots.Add(lastSelectedDot);

            EnemyMovement movement = lastSelectedDot.GetComponent<EnemyMovement>();
            if (movement != null) movement.isFrozen = true;

            Vector3 startPos = lastSelectedDot.transform.position;
            startPos.y += lineHoverOffset;

            lastSmokeSpawnPos = startPos;
            SpawnSmokeVFX(startPos);
            PlaySlashFeedback(startPos);
        }
    }

    void HandleMouseDrag()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        GameObject enemyNearMouse = FindEnemyAtMousePosition(mousePosition);

        if (enemyNearMouse != null && !connectedDots.Contains(enemyNearMouse))
        {
            GameObject currentDot = enemyNearMouse;
            Vector3 previousDotPos = lastSelectedDot != null ? lastSelectedDot.transform.position : currentDot.transform.position;
            previousDotPos.y += lineHoverOffset;

            EnemyMovement movement = currentDot.GetComponent<EnemyMovement>();
            if (movement != null) movement.isFrozen = true;

            Vector3 dotPos = currentDot.transform.position;
            dotPos.y += lineHoverOffset;

            lastSelectedDot = currentDot;
            connectedDots.Add(currentDot);

            SpawnSmokeVFX(dotPos);
            PlaySlashFeedback(previousDotPos, dotPos);
            lastSmokeSpawnPos = dotPos;
        }

        // Visualize dynamic line pointing to mouse cursor over the ground
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 mouseWorldPos;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer.value))
        {
            mouseWorldPos = hit.point + (Vector3.up * lineHoverOffset);
        }
        else
        {
            float targetZDepth = Mathf.Abs(Camera.main.transform.position.z - lastSelectedDot.transform.position.z);
            mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, targetZDepth)) + (Vector3.up * lineHoverOffset);
        }

        // Check if we should spawn smoke along the line based on the separate interval distance
        float distSinceLastSmoke = Vector3.Distance(mouseWorldPos, lastSmokeSpawnPos);
        if (distSinceLastSmoke >= smokeSpawnIntervalDistance)
        {
            SpawnSmokeVFX(mouseWorldPos);
            lastSmokeSpawnPos = mouseWorldPos;
        }
    }

    void StopDrawing()
    {
        if (lastSelectedDot != null)
        {
            if (connectedDots.Count > 1)
            {
                // --- SENTENCE VALIDATION LOGIC ---
                if (IsSentenceCorrect())
                {
                    Vector3 feedbackPosition = GetConnectedDotsCenter();
                    AudioManager audioManager = AudioManager.Instance;
                    if (audioManager != null)
                    {
                        audioManager.PlayCorrectAnswer();
                    }

                    SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
                    if (vfxManager != null)
                    {
                        vfxManager.PlayCorrectAnswer(feedbackPosition);
                    }

                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null)
                        {
                            SpawnSmokeVFX(dot.transform.position);
                            EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                            if (movement != null)
                            {
                                movement.Die();
                            }
                            else
                            {
                                Destroy(dot);
                            }
                        }
                    }
                    
                    // Notify spawner to proceed to the next sentence queue if available
                    if (spawner != null) spawner.ProceedToNextSentence();
                }
                else
                {
                    AudioManager audioManager = AudioManager.Instance;
                    if (audioManager != null)
                    {
                        audioManager.PlayWrongAnswer();
                    }

                    SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
                    if (vfxManager != null)
                    {
                        vfxManager.PlayWrongAnswer(Camera.main != null ? Camera.main.transform : null);
                    }

                    // If incorrect, unfreeze enemies so they resume attacking
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null)
                        {
                            EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                            if (movement != null) movement.isFrozen = false;
                        }
                    }
                    Debug.Log("Incorrect sentence structure! Try again!");
                }
            }
            else
            {
                foreach (GameObject dot in connectedDots)
                {
                    if (dot != null)
                    {
                        EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                        if (movement != null) movement.isFrozen = false;
                    }
                }
            }

            lastSelectedDot = null;
            connectedDots.Clear();
        }
    }

    bool IsSentenceCorrect()
    {
        if (spawner == null || spawner.levelData == null) return false;
        
        // Get the active sentence answer key from ScriptableObject via Spawner
        int sentenceIndex = spawner.GetCurrentSentenceIndex();
        List<string> answerKey = spawner.levelData.sentences[sentenceIndex].correctWordFragments;

        // If word count doesn't match the answer key, it's incorrect
        if (connectedDots.Count != answerKey.Count) return false;

        // Check word sequence one by one
        for (int i = 0; i < connectedDots.Count; i++)
        {
            EnemyMovement moveComponent = connectedDots[i].GetComponent<EnemyMovement>();
            if (moveComponent == null || moveComponent.wordCarried != answerKey[i])
            {
                return false;
            }
        }

        return true; // Sequence perfectly matches the answer key
    }

    private void SetupPauseToggle()
    {
        pauseToggle = FindAnyObjectByType<Toggle>();
        if (pauseToggle != null)
        {
            pauseToggle.isOn = false; // Start unpaused
            pauseToggle.onValueChanged.AddListener(OnPauseToggleChanged);
        }

        // Auto-wire buttons by name
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button btn in buttons)
        {
            if (btn.gameObject.name == "Resume")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(ResumeGame);
            }
            else if (btn.gameObject.name == "Restart")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartGame);
            }
            else if (btn.gameObject.name == "BackToMenu")
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(GoToMainMenu);
            }
        }
    }

    private void SetupPauseSliders()
    {
        if (pauseSlidersReady)
        {
            return;
        }

        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Slider slider in sliders)
        {
            if (slider == null)
            {
                continue;
            }

            string normalizedName = NormalizeUIName(slider.gameObject.name);

            if (normalizedName.Contains("sfx"))
            {
                WireSlider(slider, GetSFXVolume, SetSFXVolume);
            }
            else if (normalizedName.Contains("bgm") || normalizedName.Contains("music"))
            {
                WireSlider(slider, GetBGMVolume, SetBGMVolume);
            }
            else if (normalizedName.Contains("vfx") || normalizedName.Contains("visualeffect"))
            {
                WireSlider(slider, GetVFXIntensity, SetVFXIntensity);
            }
        }

        pauseSlidersReady = true;
    }

    private void WireSlider(Slider slider, System.Func<float> getter, UnityEngine.Events.UnityAction<float> setter)
    {
        if (slider == null || getter == null || setter == null)
        {
            return;
        }

        slider.onValueChanged.RemoveListener(setter);
        slider.SetValueWithoutNotify(Mathf.Clamp(getter(), slider.minValue, slider.maxValue));
        slider.onValueChanged.AddListener(setter);
    }

    private float GetBGMVolume()
    {
        AudioManager audioManager = AudioManager.Instance;
        return audioManager != null ? audioManager.GetBGMVolume() : 1f;
    }

    private float GetSFXVolume()
    {
        AudioManager audioManager = AudioManager.Instance;
        return audioManager != null ? audioManager.GetSFXVolume() : 1f;
    }

    private float GetVFXIntensity()
    {
        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        return vfxManager != null ? vfxManager.GetVFXIntensity() : 1f;
    }

    private void SetBGMVolume(float value)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.SetBGMVolume(NormalizeSliderValue(value));
        }
    }

    private void SetSFXVolume(float value)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(NormalizeSliderValue(value));
        }
    }

    private void SetVFXIntensity(float value)
    {
        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        if (vfxManager != null)
        {
            vfxManager.SetVFXIntensity(NormalizeSliderValue(value));
        }
    }

    private float NormalizeSliderValue(float value)
    {
        return Mathf.Clamp01(value);
    }

    private string NormalizeUIName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        value = value.ToLowerInvariant();
        System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    private void OnPauseToggleChanged(bool isPaused)
    {
        if (!suppressPauseToggleAudio)
        {
            AudioManager audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                audioManager.PlayButtonClick();
                if (isPaused)
                {
                    audioManager.PlayPause();
                }
            }
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void ResumeGame()
    {
        PlayButtonClick();

        if (pauseToggle != null)
        {
            suppressPauseToggleAudio = true;
            pauseToggle.isOn = false;
            suppressPauseToggleAudio = false;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void RestartGame()
    {
        PlayButtonClick();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
        PlayButtonClick();
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void SpawnSmokeVFX(Vector3 position)
    {
        if (smokePrefabs == null || smokePrefabs.Length == 0) return;

        // Choose a random smoke prefab
        GameObject prefab = smokePrefabs[Random.Range(0, smokePrefabs.Length)];
        if (prefab != null)
        {
            GameObject smokeInstance = Instantiate(prefab, position, Quaternion.identity);
            
            // Clean up the instance after its duration to avoid memory leaks
            ParticleSystem ps = smokeInstance.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                if (!main.loop)
                {
                    Destroy(smokeInstance, main.duration + main.startLifetime.constantMax);
                }
                else
                {
                    Destroy(smokeInstance, 2.0f);
                }
            }
            else
            {
                Destroy(smokeInstance, 2.0f);
            }
        }
    }

    private void PlaySlashFeedback(Vector3 position)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlaySlash();
        }

        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        if (vfxManager != null)
        {
            vfxManager.PlaySlash(position);
        }
    }

    private void PlaySlashFeedback(Vector3 startPosition, Vector3 endPosition)
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlaySlash();
        }

        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        if (vfxManager != null)
        {
            vfxManager.PlaySlash(startPosition, endPosition);
        }
    }

    private void PlayButtonClick()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayButtonClick();
        }
    }

    private Vector3 GetConnectedDotsCenter()
    {
        Vector3 totalPosition = Vector3.zero;
        int validDotCount = 0;

        foreach (GameObject dot in connectedDots)
        {
            if (dot == null)
            {
                continue;
            }

            totalPosition += dot.transform.position;
            validDotCount++;
        }

        if (validDotCount == 0)
        {
            return Vector3.zero;
        }

        return totalPosition / validDotCount + Vector3.up * lineHoverOffset;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (smokePrefabs == null || smokePrefabs.Length == 0)
        {
            List<GameObject> foundPrefabs = new List<GameObject>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Smokes" });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    foundPrefabs.Add(prefab);
                }
            }
            if (foundPrefabs.Count > 0)
            {
                smokePrefabs = foundPrefabs.ToArray();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif
}
