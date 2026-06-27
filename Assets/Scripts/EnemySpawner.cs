using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    [Header("Area Settings")]
    public int maxEnemies = 6;
    public float minSpawnDistance = 15f; 
    public float maxSpawnDistance = 25f;
    public float minDistanceBetweenEnemies = 8f;

    [Header("Enemy Movement")]
    public Transform playerTransform;
    
    [FormerlySerializedAs("dataLevelIni")]
    public LevelData levelData; 
    private int activeSentenceIndex = 0; 

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool levelClearHandled;

    [Header("UI Successful Settings")]
    public GameObject successPanel;

    void Start()
    {
        if (playerTransform != null)
        {
            PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
            if (health == null)
            {
                health = playerTransform.gameObject.AddComponent<PlayerHealth>();
            }
        }
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            activeEnemies.RemoveAll(item => item == null);
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        if (playerTransform == null || levelData == null) return;
        if (activeSentenceIndex >= levelData.sentences.Count) return;

        // Retrieve all correct word fragments for the current sentence
        List<string> allCorrectWords = levelData.sentences[activeSentenceIndex].correctWordFragments;
        if (allCorrectWords == null || allCorrectWords.Count == 0) return;

        // Load list of words currently present on screen
        List<string> wordsAlreadyOnScreen = new List<string>();
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyMovement em = enemy.GetComponent<EnemyMovement>();
                if (em != null && !string.IsNullOrEmpty(em.wordCarried))
                {
                    wordsAlreadyOnScreen.Add(em.wordCarried);
                }
            }
        }

        // Find which words have not been spawned yet (Available Words)
        List<string> wordsNotYetSpawned = new List<string>();
        foreach (string word in allCorrectWords)
        {
            if (!wordsAlreadyOnScreen.Contains(word))
            {
                wordsNotYetSpawned.Add(word);
            }
        }

        // If all words are already spawned on screen, stop spawning to avoid duplicates
        if (wordsNotYetSpawned.Count == 0)
        {
            return; 
        }

        // Pick a random word from the list of words that have not yet spawned
        string wordForMushroom = wordsNotYetSpawned[Random.Range(0, wordsNotYetSpawned.Count)];

        // ## Logic to calculate spawn position on sides or behind the player with collision/proximity check
        Vector3 spawnPosition = Vector3.zero;
        int maxAttempts = 30;
        bool foundPosition = false;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 spawnDirection = Vector3.zero;
            float randomChance = Random.Range(0f, 100f);

            if (randomChance < 50f)
            {
                Vector3 backwardDirection = -playerTransform.forward;
                Vector3 sideOffset = playerTransform.right * Random.Range(-0.5f, 0.5f);
                spawnDirection = (backwardDirection + sideOffset).normalized;
            }
            else if (randomChance < 70f)
            {
                spawnDirection = -playerTransform.right;
            }
            else
            {
                spawnDirection = playerTransform.right;
            }

            Vector3 spawnOffset = spawnDirection * randomDistance;
            Vector3 candidatePosition = playerTransform.position + spawnOffset;

            // Raycast down to find ground height at candidate position
            float raycastStartHeight = candidatePosition.y + 10f;
            Vector3 rayOrigin = new Vector3(candidatePosition.x, raycastStartHeight, candidatePosition.z);

            LayerMask mask = LayerMask.GetMask("Ground");
            DotConnectManager dcm = FindAnyObjectByType<DotConnectManager>();
            if (dcm != null)
            {
                mask = dcm.groundLayer;
            }

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 30f, mask.value))
            {
                candidatePosition.y = hit.point.y;
            }
            else
            {
                candidatePosition.y = playerTransform.position.y;
            }

            // Verify if candidatePosition is far enough from all existing active enemies in X or Z axis
            bool tooCloseToAny = false;
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy == null) continue;

                float diffX = Mathf.Abs(candidatePosition.x - enemy.transform.position.x);
                float diffZ = Mathf.Abs(candidatePosition.z - enemy.transform.position.z);

                // If close in both X and Z, they are too close to each other
                if (diffX < minDistanceBetweenEnemies && diffZ < minDistanceBetweenEnemies)
                {
                    tooCloseToAny = true;
                    break;
                }
            }

            spawnPosition = candidatePosition;

            if (!tooCloseToAny)
            {
                foundPosition = true;
                break;
            }
        }

        if (!foundPosition)
        {
            Debug.LogWarning("Could not find a spawn position far enough from other enemies. Spawning anyway.");
        }

        // Spawn Enemy
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);
        
        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.wordCarried = wordForMushroom;
            movement.SetTarget(playerTransform);
        }
    }

    public int GetCurrentSentenceIndex()
    {
        return activeSentenceIndex;
    }

    public void HandlePlayerHit()
    {
        // Disable enemy movement scripts to prevent duplicate triggers in the same frame
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyMovement em = enemy.GetComponent<EnemyMovement>();
                if (em != null) em.enabled = false;
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();

        // Proceed to next sentence
        ProceedToNextSentence();
    }

    public void ProceedToNextSentence()
    {
        activeSentenceIndex++;
        
        if (activeSentenceIndex >= levelData.sentences.Count)
        {
            if (levelClearHandled)
            {
                return;
            }

            levelClearHandled = true;
            Debug.Log("ALL SENTENCES COMPLETED! LEVEL CLEAR!");
            PlayLevelCompleteFeedback();

            Invoke("OnLevelClear", 2f);
        }
        else
        {
            Debug.Log("Sentence successful! Clear remaining enemies and proceed to the next sentence...");
            foreach (var enemy in activeEnemies) 
            { 
                if (enemy != null) Destroy(enemy); 
            }
            activeEnemies.Clear();
        }
    }

    void OnLevelClear()
    {
        if (SceneManager.GetActiveScene().name == "Scene_Hard")
        {
            Debug.Log("Congratulations! you have completed the hardest level!");
        } else
        {
            if (successPanel != null)
            {
                successPanel.SetActive(true);
            }
            Invoke("ChangeNextScene", 2.5f);
            
        }
    }

    void ChangeNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void PlayLevelCompleteFeedback()
    {
        AudioManager audioManager = AudioManager.Instance;
        if (audioManager != null)
        {
            audioManager.PlayLevelComplete();
        }

        SimpleVFXManager vfxManager = SimpleVFXManager.Instance;
        if (vfxManager != null)
        {
            Vector3 effectPosition = playerTransform != null ? playerTransform.position : Vector3.zero;
            vfxManager.PlayLevelComplete(effectPosition + Vector3.up * 1.5f);
        }
    }
}
