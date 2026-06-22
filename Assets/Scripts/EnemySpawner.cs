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

    [Tooltip("Radius batas dalam. Musuh TIDAK AKAN spawn di dalam radius ini dari pusat (0,0,0)")]
    public float safeZoneRadius = 3f;

    [Header("Enemy Movement")]
    public float enemySpeed = 2f;
    public Transform playerTransform;
    public LevelData dataLevelIni; // Variabel penampung ScriptableObject
    private int indexKalimatAktif = 0; // Variabel pencatat antrean kalimat saat ini

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

        string kataUntukJamur = "";

        // 1. Ambil data kata benar dan kata pengecoh
        List<string> kataBenar = dataLevelIni.daftarKalimat[indexKalimatAktif].potonganKataBenar;
        List<string> kataSalah = dataLevelIni.kataPengecoh;


        bool kataPertamaSudahAda = false;
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                EnemyMovement em = enemy.GetComponent<EnemyMovement>();
                if (em != null && kataBenar.Count > 0 && em.kataYangDibawa == kataBenar[0])
                {
                    kataPertamaSudahAda = true;
                    break;
                }
            }
        }

        // Jika kata pertama di kalimat BELUM ADA sama sekali di layar, paksa jamur ini bawa kata pertama itu!
        if (!kataPertamaSudahAda && kataBenar.Count > 0)
        {
            kataUntukJamur = kataBenar[0]; // Kunci ke kata pertama (misal: "Saya")
        }
        else
        {
            // Jika kata pertama sudah ada, baru kita gacha sisanya agar ada tantangan
            float chance = Random.Range(0f, 100f);

            if (kataSalah != null && kataSalah.Count > 0 && chance < 40f) // 40% peluang pengecoh
            {
                kataUntukJamur = kataSalah[Random.Range(0, kataSalah.Count)];
            }
            else
            {
                // Ambil kata benar sisanya secara acak agar layar ramai kata kunci
                kataUntukJamur = kataBenar[Random.Range(0, kataBenar.Count)];
            }
        }

        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Menggunakan probabilitas sederhana agar musuh muncul dari belakang atau samping kiri/kanan
        Vector3 spawnDirection = Vector3.zero;
        float randomChance = Random.Range(0f, 100f);

        if (randomChance < 40f)
        {
            // 40% muncul dari arah belakang kamera (Sumbu Z negatif relatif terhadap kucing)
            spawnDirection = new Vector3(Random.Range(-1f, 1f), 0f, -1f).normalized;
        }
        else if (randomChance < 70f)
        {
            // 30% muncul dari arah kiri (Sumbu X negatif)
            spawnDirection = new Vector3(-1f, 0f, Random.Range(-0.5f, 0.5f)).normalized;
        }
        else
        {
            // 30% muncul dari arah kanan (Sumbu X positif)
            spawnDirection = new Vector3(1f, 0f, Random.Range(-0.5f, 0.5f)).normalized;
        }

        Vector3 spawnOffset = spawnDirection * randomDistance;
        // Gabungkan dengan posisi kucing
        Vector3 spawnPosition = playerTransform.position + spawnOffset;
        spawnPosition.y = playerTransform.position.y;

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);
        
        EnemyMovement movement = spawnedEnemy.GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = enemySpeed;
            movement.kataYangDibawa = kataUntukJamur;
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
            // Nanti di sini kamu bisa panggil fungsi ganti scene UI Menang atau LoadScene level berikutnya
        }
        else
        {
            Debug.Log("Kalimat sukses! Bersihkan sisa musuh dan lanjut ke kalimat berikutnya...");
            
            // Hancurkan semua musuh sisa dari kalimat sebelumnya agar tidak memenuhi layar
            foreach (var enemy in activeEnemies) 
            { 
                if (enemy != null) Destroy(enemy); 
            }
            activeEnemies.Clear();
        }
    }
}