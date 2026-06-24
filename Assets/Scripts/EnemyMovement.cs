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
        if (isFrozen || playerTarget == null) return;

        // Enemy always moves to chase the player's current position
        transform.position = Vector3.MoveTowards(transform.position, playerTarget.position, speed * Time.deltaTime);

        // Always face towards the player
        transform.LookAt(new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z));

        if (textMeshComponent != null)
{
            // Get the canvas transform (parent of the text object)
            Transform canvasTransform = textMeshComponent.canvas.transform;

            string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (activeSceneName == "Scene_Medium")
            {
                canvasTransform.rotation = Quaternion.Euler(0f, 0f, 0f); 
            }
            else if (activeSceneName == "Scene_Easy")
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
            PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }
}