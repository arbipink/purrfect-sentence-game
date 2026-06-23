using System.Collections;
using System.Collections.Generic;
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

    [Header("Enemy Movement")]
    public float enemySpeed = 2f;
    public Transform playerTransform;
    public LevelData dataLevelIni; 
    private int indexKalimatAktif = 0; 

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
        if (playerTransform == null || dataLevelIni == null) return;
        if (indexKalimatAktif >= dataLevelIni.daftarKalimat.Count) return;

        // 1. Ambil semua potongan kata benar untuk kalimat saat ini
        List<string> semuaKataBenar = dataLevelIni.daftarKalimat[indexKalimatAktif].potonganKataBenar;
        if (semuaKataBenar == null || semuaKataBenar.Count == 0) return;

        // 2. Buat daftar kata yang SAAT INI SUDAH ADA di layar game
        List<string> kataYangSudahAdaDiLayar = new List<string>();
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyMovement em = enemy.GetComponent<EnemyMovement>();
                if (em != null && !string.IsNullOrEmpty(em.kataYangDibawa))
                {
                    kataYangSudahAdaDiLayar.Add(em.kataYangDibawa);
                }
            }
        }

        // 3. Cari kata apa saja yang belum ada di layar (Kata Tersedia)
        List<string> kataYangBelumSpawn = new List<string>();
        foreach (string kata in semuaKataBenar)
        {
            if (!kataYangSudahAdaDiLayar.Contains(kata))
            {
                kataYangBelumSpawn.Add(kata);
            }
        }

        // 4. Jika SEMUA KATA sudah lahir di layar, kita stop spawn biar tidak ada duplikasi!
        if (kataYangBelumSpawn.Count == 0)
        {
            // Debug.Log("Semua kata dari kalimat ini sudah ada di layar. Menunggu player menarik garis...");
            return; 
        }

        // 5. Pilih satu kata secara acak dari daftar kata yang BELUM SPAWN tadi
        string kataUntukJamur = kataYangBelumSpawn[Random.Range(0, kataYangBelumSpawn.Count)];


        // --- Logika kalkulasi posisi spawn di samping/belakang (Tetap Aman) ---
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnDirection = Vector3.zero;
        float randomChance = Random.Range(0f, 100f);

        if (randomChance < 40f)
        {
            spawnDirection = new Vector3(Random.Range(-1f, 1f), 0f, -1f).normalized;
        }
        else if (randomChance < 70f)
        {
            spawnDirection = new Vector3(-1f, 0f, Random.Range(-0.5f, 0.5f)).normalized;
        }
        else
        {
            spawnDirection = new Vector3(1f, 0f, Random.Range(-0.5f, 0.5f)).normalized;
        }

        Vector3 spawnOffset = spawnDirection * randomDistance;
        Vector3 spawnPosition = playerTransform.position + spawnOffset;
        spawnPosition.y = playerTransform.position.y;

        // 6. Lahirkan musuhnya!
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);
        
        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = enemySpeed;
            movement.kataYangDibawa = kataUntukJamur; // Kirim kata yang unik
            movement.SetTarget(playerTransform);
        }
    }

    public int GetCurrentKalimatIndex()
    {
        return indexKalimatAktif;
    }

    public void LanjutKalimatBerikutnya()
    {
        indexKalimatAktif++;
        
        if (indexKalimatAktif >= dataLevelIni.daftarKalimat.Count)
        {
            Debug.Log("SEMUA KALIMAT SELESAI! LEVEL CLEAR!");
        }
        else
        {
            Debug.Log("Kalimat sukses! Bersihkan sisa musuh dan lanjut ke kalimat berikutnya...");
            foreach (var enemy in activeEnemies) 
            { 
                if (enemy != null) Destroy(enemy); 
            }
            activeEnemies.Clear();
        }
    }
}