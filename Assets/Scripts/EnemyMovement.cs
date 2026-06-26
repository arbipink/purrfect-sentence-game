using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class EnemyMovement : MonoBehaviour
{
    public float speed;
    [HideInInspector]
    public bool isFrozen = false;

    [FormerlySerializedAs("kataYangDibawa")]
    public string wordCarried; 
    public TextMeshProUGUI textMeshComponent; 
    
    private Transform playerTarget;
    private Camera mainCamera;


    public void SetTarget(Transform target)
    {
        playerTarget = target;

        if (textMeshComponent != null)
        {
            textMeshComponent.text = wordCarried;
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (isFrozen || playerTarget == null) return;

        // Enemy always moves to chase the player's current position (calculated in X/Y/Z)
        Vector3 nextPosition = Vector3.MoveTowards(transform.position, playerTarget.position, speed * Time.deltaTime);

        // Raycast down to find ground height at the next position
        float raycastStartHeight = nextPosition.y + 10f;
        Vector3 rayOrigin = new Vector3(nextPosition.x, raycastStartHeight, nextPosition.z);
        
        LayerMask mask = LayerMask.GetMask("Ground");
        DotConnectManager dcm = FindAnyObjectByType<DotConnectManager>();
        if (dcm != null)
        {
            mask = dcm.groundLayer;
        }

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 30f, mask.value))
        {
            nextPosition.y = hit.point.y;
        }
        else
        {
            // Fallback: match player's Y if raycast misses
            nextPosition.y = playerTarget.position.y;
        }

        transform.position = nextPosition;

        // Always face towards the player
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));


        if (Vector3.Distance(transform.position, playerTarget.position) < 1.0f)
        {
            PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }

    void LateUpdate()
    {
        if (textMeshComponent != null)
        {
            // Get the canvas transform (parent of the text object)
            Transform canvasTransform = textMeshComponent.canvas.transform;

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera != null)
            {
                canvasTransform.rotation = mainCamera.transform.rotation;
            }
        }
    }
}