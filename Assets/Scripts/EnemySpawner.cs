using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    [Header("Area Settings")]
    public Vector3 minSpawnRange = new Vector3(-10, 0, -10);
    public Vector3 maxSpawnRange = new Vector3(10, 0, 10);

    [Tooltip("Radius batas dalam. Musuh TIDAK AKAN spawn di dalam radius ini dari pusat (0,0,0)")]
    public float safeZoneRadius = 3f;

    [Header("Enemy Movement")]
    public float enemySpeed = 2f;

    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool validPosition = false;
        int maxAttempts = 10;
        int attempts = 0;

        while (!validPosition && attempts < maxAttempts)
        {
            attempts++;

            float randomX = Random.Range(minSpawnRange.x, maxSpawnRange.x);
            float randomZ = Random.Range(minSpawnRange.z, maxSpawnRange.z);
            spawnPosition = new Vector3(randomX, 0f, randomZ);

            if (Vector3.Distance(spawnPosition, Vector3.zero) >= safeZoneRadius)
            {
                validPosition = true;
            }
        }

        if (validPosition)
        {
            GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            EnemyMovement movement = spawnedEnemy.AddComponent<EnemyMovement>();
            movement.speed = enemySpeed;
        }
    }
}

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    [HideInInspector]
    public bool isFrozen = false;
    private Vector3 targetPosition = Vector3.zero;

    void Update()
    {
        if (isFrozen) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}