using System.Collections;
using System.Collections.Generic; // BIANG KEROK ERROR 1 SUDAH DIADAKAN DI SINI
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    [Header("Area Settings")]
    public int maxEnemies = 6;
    public float minSpawnDistance = 15f; 
    public float maxSpawnDistance = 25f;

    [Tooltip("Radius batas dalam. Musuh TIDAK AKAN spawn di dalam radius ini dari pusat (0,0,0)")]
    public float safeZoneRadius = 3f;

    [Header("Enemy Movement")]
    public float enemySpeed = 2f;
    public Transform playerTransform;

    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
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
        if (playerTransform == null) return;

        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Tentukan arah acak melingkar
        Vector2 randomDirectionInternal = Random.insideUnitCircle.normalized; 
        Vector3 spawnOffset = new Vector3(randomDirectionInternal.x, 0, randomDirectionInternal.y) * randomDistance;

        // Gabungkan dengan posisi kucing
        Vector3 spawnPosition = playerTransform.position + spawnOffset;
        spawnPosition.y = playerTransform.position.y;

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);
        
        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = enemySpeed;
            movement.SetTarget(playerTransform);
        }
    }
}