using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawingSystem : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float minDistance = 0.05f;

    private Camera mainCamera;
    private LineRenderer currentLine;
    private List<Vector3> currentPoints;
    private List<Vector3> lastCompletedPoints;
    private bool isDrawing = false;
    private bool hasNewPattern = false;

    // Nuevo Input System
    private Touchscreen touchscreen;
    private Mouse mouse;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        currentPoints = new List<Vector3>();
        lastCompletedPoints = new List<Vector3>();

        // Obtener referencias a dispositivos de entrada
        touchscreen = Touchscreen.current;
        mouse = Mouse.current;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        bool isTouching = false;
        Vector2 inputPosition = Vector2.zero;

        // Prioridad 1: Touch (dispositivos móviles)
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            isTouching = true;
            inputPosition = touchscreen.primaryTouch.position.ReadValue();
        }
        // Prioridad 2: Mouse (editor/PC)
        else if (mouse != null && mouse.leftButton.isPressed)
        {
            isTouching = true;
            inputPosition = mouse.position.ReadValue();
        }

        if (isTouching)
        {
            Vector3 worldPosition = ScreenToWorldPosition(inputPosition);

            if (!isDrawing)
            {
                StartDrawing(worldPosition);
            }
            else
            {
                ContinueDrawing(worldPosition);
            }
        }
        else if (isDrawing)
        {
            EndDrawing();
        }
    }

    Vector3 ScreenToWorldPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        return worldPos;
    }

    void StartDrawing(Vector3 startPosition)
    {
        isDrawing = true;
        hasNewPattern = false;

        GameObject lineObject = new GameObject("DrawLine");
        currentLine = lineObject.AddComponent<LineRenderer>();

        currentLine.material = lineMaterial;
        currentLine.startColor = lineColor;
        currentLine.endColor = lineColor;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.useWorldSpace = true;
        currentLine.positionCount = 0;

        currentPoints.Clear();
        currentPoints.Add(startPosition);
        UpdateLineRenderer();
    }

    void ContinueDrawing(Vector3 newPosition)
    {
        if (currentPoints.Count > 0)
        {
            Vector3 lastPoint = currentPoints[currentPoints.Count - 1];
            float distance = Vector3.Distance(lastPoint, newPosition);

            if (distance >= minDistance)
            {
                currentPoints.Add(newPosition);
                UpdateLineRenderer();
            }
        }
    }

    void UpdateLineRenderer()
    {
        if (currentLine != null && currentPoints.Count > 0)
        {
            currentLine.positionCount = currentPoints.Count;
            for (int i = 0; i < currentPoints.Count; i++)
            {
                currentLine.SetPosition(i, currentPoints[i]);
            }
        }
    }

    void EndDrawing()
    {
        isDrawing = false;

        if (currentPoints.Count > 2)
        {
            lastCompletedPoints = new List<Vector3>(currentPoints);
            hasNewPattern = true;
            Debug.Log($"Dibujo completado con {lastCompletedPoints.Count} puntos");
        }

        if (currentLine != null)
        {
            DestroyImmediate(currentLine.gameObject);
            currentLine = null;
        }

        currentPoints.Clear();
    }

    public bool HasNewPattern()
    {
        return hasNewPattern;
    }

    public List<Vector3> GetLastCompletedPattern()
    {
        hasNewPattern = false;
        return new List<Vector3>(lastCompletedPoints);
    }

    public List<Vector3> GetCurrentPoints()
    {
        return new List<Vector3>(currentPoints);
    }

    public bool IsDrawing()
    {
        return isDrawing;
    }
}