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

    [Tooltip("Slightly lift the line above the ground to avoid z-fighting / flickering")]
    public float lineHoverOffset = 0.25f;

    [Tooltip("Minimum distance in world space before adding a new segment to the freehand line")]
    public float minDrawSegmentDistance = 0.2f;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public float clickRadiusPixels = 70f;
    private EnemySpawner spawner;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        spawner = FindAnyObjectByType<EnemySpawner>();
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

    GameObject FindEnemyAtMousePosition(Vector2 mousePos)
    {
        EnemyMovement[] allEnemies = FindObjectsByType<EnemyMovement>(FindObjectsInactive.Exclude);
        GameObject selectedEnemy = null;
        float closestDistance = clickRadiusPixels;

        foreach (EnemyMovement enemy in allEnemies)
        {
            if (enemy == null) continue;

            // Convert enemy's 3D position to 2D screen coordinates
            Vector2 enemyOnScreen = Camera.main.WorldToScreenPoint(enemy.transform.position);
            float distance = Vector2.Distance(mousePos, enemyOnScreen);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                selectedEnemy = enemy.gameObject;
            }
        }
        return selectedEnemy;
    }

    void HandleMouseClick()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        GameObject enemyNearMouse = FindEnemyAtMousePosition(mousePosition);
        
        // 1. Project mouse position directly into 3D space relative to camera depth
        // Estimate constant distance from camera to target
        
        if (enemyNearMouse != null)
        {
            lastSelectedDot = enemyNearMouse;
            connectedDots.Clear();
            connectedDots.Add(lastSelectedDot);

            EnemyMovement movement = lastSelectedDot.GetComponent<EnemyMovement>();
            if (movement != null) movement.isFrozen = true;

            Vector3 startPos = lastSelectedDot.transform.position;
            startPos.y += lineHoverOffset;
            CreateNewLine(startPos);
        }
    }

    void HandleMouseDrag()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        GameObject enemyNearMouse = FindEnemyAtMousePosition(mousePosition);

        if (enemyNearMouse != null && !connectedDots.Contains(enemyNearMouse))
        {
            GameObject currentDot = enemyNearMouse;

            EnemyMovement movement = currentDot.GetComponent<EnemyMovement>();
            if (movement != null) movement.isFrozen = true;

            Vector3 dotPos = currentDot.transform.position;
            dotPos.y += lineHoverOffset;

            linePoints.Add(dotPos);

            lastSelectedDot = currentDot;
            connectedDots.Add(currentDot);
        }

        if (currentLine == null) return;

        // Visualize dynamic line pointing to mouse cursor over the ground
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        Vector3 mouseWorldPos;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer.value))
        {
            mouseWorldPos = hit.point + (Vector3.up * lineHoverOffset);
        }
        else
        {
            float targetZDepth = Mathf.Abs(Camera.main.transform.position.z - lastSelectedDot.transform.position.z);
            mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, targetZDepth)) + (Vector3.up * lineHoverOffset);
        }

        // Add intermediate points to linePoints if the mouse has moved far enough from the last point
        if (linePoints.Count > 0)
        {
            float dist = Vector3.Distance(mouseWorldPos, linePoints[linePoints.Count - 1]);
            if (dist > minDrawSegmentDistance)
            {
                linePoints.Add(mouseWorldPos);
            }
        }

        // Render the permanent points plus the current mouse position as the last point
        currentLine.positionCount = linePoints.Count + 1;
        for (int i = 0; i < linePoints.Count; i++)
        {
            currentLine.SetPosition(i, linePoints[i]);
        }
        currentLine.SetPosition(linePoints.Count, mouseWorldPos);
    }

    void StopDrawing()
    {
        if (currentLine != null)
        {
            if (connectedDots.Count > 1)
            {
                // --- SENTENCE VALIDATION LOGIC ---
                if (IsSentenceCorrect())
                {
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null) Destroy(dot);
                    }
                    
                    // Notify spawner to proceed to the next sentence queue if available
                    if (spawner != null) spawner.ProceedToNextSentence();
                }
                else
                {
                    // If incorrect, unfreeze enemies so they resume attacking
                    foreach (GameObject dot in connectedDots)
                    {
                        if (dot != null)
                        {
                            EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                            if (movement != null) movement.isFrozen = false;
                        }
                    }
                    Debug.Log("Incorrect sentence structure! Try again!");
                }
            }
            else
            {
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

    bool IsSentenceCorrect()
    {
        if (spawner == null || spawner.levelData == null) return false;
        
        // Get the active sentence answer key from ScriptableObject via Spawner
        int sentenceIndex = spawner.GetCurrentSentenceIndex();
        List<string> answerKey = spawner.levelData.sentences[sentenceIndex].correctWordFragments;

        // If word count doesn't match the answer key, it's incorrect
        if (connectedDots.Count != answerKey.Count) return false;

        // Check word sequence one by one
        for (int i = 0; i < connectedDots.Count; i++)
        {
            EnemyMovement moveComponent = connectedDots[i].GetComponent<EnemyMovement>();
            if (moveComponent == null || moveComponent.wordCarried != answerKey[i])
            {
                return false;
            }
        }

        return true; // Sequence perfectly matches the answer key
    }

    void CreateNewLine(Vector3 startPosition)
    {
        linePoints.Clear();
        linePoints.Add(startPosition);

        GameObject lineObj = new GameObject("Line_" + System.DateTime.Now.Ticks);
        lineObj.transform.position = startPosition;
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
