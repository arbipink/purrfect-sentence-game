using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DotConnectManager : MonoBehaviour
{
    private LineRenderer currentLine;
    private List<Vector3> linePoints = new List<Vector3>();
    private GameObject lastSelectedDot;

    private List<GameObject> connectedDots = new List<GameObject>();

    [Header("Line Settings")]
    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.yellow;

    [Tooltip("Sedikit angkat garis di atas tanah agar tidak z-fighting / kedip-kedip")]
    public float lineHoverOffset = 0.05f;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    private EnemySpawner spawner;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        spawner = FindFirstObjectByType<EnemySpawner>();
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
        
        var pointer = Pointer.current;
        if (pointer == null) return;

        var press = pointer.press;

        if (press != null)
        {
            if (press.wasPressedThisFrame)
            {
                HandleMouseClick();
            }
            else if (press.isPressed && currentLine != null)
            {
                HandleMouseDrag();
            }
            else if (press.wasReleasedThisFrame)
            {
                StopDrawing();
            }
        }
    }

    void HandleMouseClick()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        
        // 1. Proyeksikan posisi mouse langsung ke ruang 3D sejajar jarak kedalaman kamera
        // Kita hitung jarak antara kamera dan target jamur (menggunakan estimasi jarak konstan)
        float targetZDepth = 15f; // Estimasi jarak horizontal dari Main Camera ke jalanan kucing
        
        if (lastSelectedDot != null)
        {
            targetZDepth = Mathf.Abs(Camera.main.transform.position.z - lastSelectedDot.transform.position.z);
        }
        else
        {
            // Jika belum ada dot yang dipilih, cari jamur pertama di scene untuk patokan jarak Z
            EnemyMovement sampleEnemy = FindFirstObjectByType<EnemyMovement>();
            if (sampleEnemy != null)
            {
                targetZDepth = Mathf.Abs(Camera.main.transform.position.z - sampleEnemy.transform.position.z);
            }
        }

        Vector3 screenToWorldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, targetZDepth));

        // 2. Gunakan OverlapSphere di titik proyeksi tersebut untuk menangkap Collider musuh
        // Radius 2.0f agar area klik mouse sedikit lebih luas dan toleran terhadap sentuhan player
        Collider[] hitColliders = Physics.OverlapSphere(screenToWorldPoint, 2.0f, enemyLayer.value);

        if (hitColliders.Length > 0)
        {
            Collider enemyCollider = hitColliders[0];

            if (enemyCollider.CompareTag("Enemy"))
            {
                lastSelectedDot = enemyCollider.gameObject;

                connectedDots.Clear();
                connectedDots.Add(lastSelectedDot);

                EnemyMovement movement = lastSelectedDot.GetComponent<EnemyMovement>();
                if (movement != null)
                {
                    movement.isFrozen = true;
                }

                Vector3 startPos = lastSelectedDot.transform.position;
                startPos.y += lineHoverOffset;

                CreateNewLine(startPos);
            }
        }
    }

    void HandleMouseDrag()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        
        float targetZDepth = 15f;
        if (lastSelectedDot != null)
        {
            targetZDepth = Mathf.Abs(Camera.main.transform.position.z - lastSelectedDot.transform.position.z);
        }

        Vector3 screenToWorldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, targetZDepth));

        Collider[] hitColliders = Physics.OverlapSphere(screenToWorldPoint, 2.0f, enemyLayer.value);

        if (hitColliders.Length > 0)
        {
            Collider enemyCollider = hitColliders[0];

            if (enemyCollider.CompareTag("Enemy") && !connectedDots.Contains(enemyCollider.gameObject))
            {
                GameObject currentDot = enemyCollider.gameObject;

                EnemyMovement movement = currentDot.GetComponent<EnemyMovement>();
                if (movement != null)
                {
                    movement.isFrozen = true;
                }

                Vector3 dotPos = currentDot.transform.position;
                dotPos.y += lineHoverOffset;

                linePoints.Add(dotPos);
                currentLine.positionCount = linePoints.Count;
                currentLine.SetPosition(linePoints.Count - 1, dotPos);

                lastSelectedDot = currentDot;
                connectedDots.Add(currentDot);
            }
        }

        if (currentLine == null) return;

        // Untuk visual penarikan garis di tanah tetap gunakan ground raycast standar agar menempel rapi
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 mouseWorldPos;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer.value))
        {
            mouseWorldPos = hit.point + (Vector3.up * lineHoverOffset);
        }
        else
        {
            mouseWorldPos = screenToWorldPoint + (Vector3.up * lineHoverOffset);
        }

        currentLine.positionCount = linePoints.Count + 1;
        currentLine.SetPosition(linePoints.Count, mouseWorldPos);
    }

    void StopDrawing()
    {
        if (currentLine != null)
        {
            if (connectedDots.Count > 1)
            {
                // --- LOGIKA VALIDASI KALIMAT ---
                if (CekApakahKalimatBenar())
                {
                    // Jika Benar, Lenyapkan Jamur!
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null) Destroy(dot);
                    }
                    
                    // Beritahu spawner untuk lanjut ke antrean kalimat berikutnya jika ada
                    if (spawner != null) spawner.LanjutKalimatBerikutnya();
                }
                else
                {
                    // Jika Salah Susunan, Lepas Freeze! Mereka bakal menyerang lagi
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null)
                        {
                            EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                            if (movement != null) movement.isFrozen = false;
                        }
                    }
                    Debug.Log("Susunan Kalimat Salah! Coba Lagi!");
                }
            }
            else
            {
                // Jika cuma klik 1 dot lalu dilepas
                foreach (GameObject dot in connectedDots)
                {
                    if (dot != null)
                    {
                        EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                        if (movement != null) movement.isFrozen = false;
                    }
                }
            }

            Destroy(currentLine.gameObject);
            currentLine = null;
            lastSelectedDot = null;
            connectedDots.Clear();
        }
    }

    bool CekApakahKalimatBenar()
    {
        if (spawner == null || spawner.dataLevelIni == null) return false;
        
        // Ambil kunci jawaban kalimat aktif dari ScriptableObject via Spawner
        int kalimatIndex = spawner.GetCurrentKalimatIndex(); // Kita butuh fungsi helper ini di Spawner nanti
        List<string> kunciJawaban = spawner.dataLevelIni.daftarKalimat[kalimatIndex].potonganKataBenar;

        // Jika jumlah kata yang ditarik player tidak sama dengan jumlah kata kunci jawaban, otomatis salah!
        if (connectedDots.Count != kunciJawaban.Count) return false;

        // Cek urutan katanya satu per satu
        for (int i = 0; i < connectedDots.Count; i++)
        {
            EnemyMovement moveComponent = connectedDots[i].GetComponent<EnemyMovement>();
            if (moveComponent == null || moveComponent.kataYangDibawa != kunciJawaban[i])
            {
                return false; // Ada satu urutan kata yang salah / membawa kata pengecoh
            }
        }

        return true; // Urutan 100% Sesuai Kunci Jawaban!
    }

    void CreateNewLine(Vector3 startPosition)
    {
        linePoints.Clear();
        linePoints.Add(startPosition);

        GameObject lineObj = new GameObject("Line_" + System.DateTime.Now.Ticks);
        currentLine = lineObj.AddComponent<LineRenderer>();

        currentLine.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.startColor = lineColor;
        currentLine.endColor = lineColor;
        currentLine.useWorldSpace = true;

        currentLine.positionCount = 1;
        currentLine.SetPosition(0, startPosition);
    }
}