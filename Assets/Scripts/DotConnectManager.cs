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

    void Start()
    {
        // Memaksa kursor mouse untuk tetap kelihatan dan bebas bergerak di layar
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                lastSelectedDot = hit.collider.gameObject;

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
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("Enemy") && !connectedDots.Contains(hit.collider.gameObject))
            {
                GameObject currentDot = hit.collider.gameObject;

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

        Vector3 mouseWorldPos;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            mouseWorldPos = hit.point + (Vector3.up * lineHoverOffset);
        }
        else
        {
            float distanceToCamera = Vector3.Distance(Camera.main.transform.position, lastSelectedDot.transform.position);
            mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distanceToCamera));
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
                foreach (GameObject dot in connectedDots)
                {
                    if (dot != null)
                    {
                        Destroy(dot);
                    }
                }
                Destroy(currentLine.gameObject);
            }
            else
            {
                foreach (GameObject dot in connectedDots)
                {
                    if (dot != null)
                    {
                        EnemyMovement movement = dot.GetComponent<EnemyMovement>();
                        if (movement != null)
                        {
                            movement.isFrozen = false;
                        }
                    }
                }
                Destroy(currentLine.gameObject);
            }

            currentLine = null;
            lastSelectedDot = null;
            connectedDots.Clear();
        }
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