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

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        spawner = FindAnyObjectByType<EnemySpawner>();

        SetupPauseToggle();
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
        }
    }

    void HandleMouseDrag()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        GameObject enemyNearMouse = FindEnemyAtMousePosition(mousePosition);

        if (enemyNearMouse != null && !connectedDots.Contains(enemyNearMouse))
        {
            GameObject currentDot = enemyNearMouse;

            EnemyMovement movement = currentDot.GetComponent<EnemyMovement>();
            if (movement != null) movement.isFrozen = true;

            Vector3 dotPos = currentDot.transform.position;
            dotPos.y += lineHoverOffset;

            lastSelectedDot = currentDot;
            connectedDots.Add(currentDot);

            SpawnSmokeVFX(dotPos);
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
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null)
                        {
                            SpawnSmokeVFX(dot.transform.position);
                            Destroy(dot);
                        }
                    }
                    
                    // Notify spawner to proceed to the next sentence queue if available
                    if (spawner != null) spawner.ProceedToNextSentence();
                }
                else
                {
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

    private void OnPauseToggleChanged(bool isPaused)
    {
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void ResumeGame()
    {
        if (pauseToggle != null)
        {
            pauseToggle.isOn = false;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void GoToMainMenu()
    {
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
