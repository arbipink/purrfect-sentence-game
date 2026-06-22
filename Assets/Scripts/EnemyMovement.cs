using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    [HideInInspector]
    public bool isFrozen = false;
    
    private Transform playerTarget;

    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    void Update()
    {
        if (isFrozen || playerTarget == null) return;

        // Musuh selalu bergerak mengejar posisi Kucing saat ini
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, speed * Time.deltaTime);

        // Selalu menghadap ke arah Kucing
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        if (Vector3.Distance(transform.position, playerTarget.position) < 1.0f)
        {
            
            Debug.Log("Kucing Ditabrak Jamur! Darah Berkurang!"); // Ganti dengan logika pengurangan darah atau efek lain sesuai kebutuhan
        }
    }
}