using UnityEngine;
using TMPro;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    [HideInInspector]
    public bool isFrozen = false;

    public string kataYangDibawa; 
    public TextMeshProUGUI textMeshComponent; 
    
    private Transform playerTarget;


    public void SetTarget(Transform target)
    {
        playerTarget = target;

        if (textMeshComponent != null)
        {
            textMeshComponent.text = kataYangDibawa;
        }
    }

    void Update()
    {
        if (isFrozen || playerTarget == null) return;

        // Musuh selalu bergerak mengejar posisi Kucing saat ini
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, speed * Time.deltaTime);

        // Selalu menghadap ke arah Kucing
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        if (textMeshComponent != null)
{
            // Ambil transform milik canvas (induk dari objek teks)
            Transform canvasTransform = textMeshComponent.canvas.transform;

            string namaSceneAktif = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (namaSceneAktif == "Scene_Medium")
            {
                canvasTransform.rotation = Quaternion.Euler(0f, 0f, 0f); 
            }
            else if (namaSceneAktif == "Scene_Easy")
            {
                canvasTransform.rotation = Quaternion.Euler(0f, 90f, 0f); 
            }
            else
            {
                canvasTransform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }
        if (Vector3.Distance(transform.position, playerTarget.position) < 1.0f)
        {

            Debug.Log("Kucing Ditabrak Jamur! Darah Berkurang!"); // Ganti dengan logika pengurangan HP atau efek lain sesuai kebutuhan
        }
    }
}