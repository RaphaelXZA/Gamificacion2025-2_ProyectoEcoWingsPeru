using System.Collections.Generic;
using UnityEngine;

public class DrawingSystem : MonoBehaviour
{
    [Header("Drawing Settings")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private float minDistance = 0.05f; // Reducido para mayor sensibilidad

    private Camera mainCamera;
    private LineRenderer currentLine;
    private List<Vector3> currentPoints;
    private List<Vector3> lastCompletedPoints; // Para el sistema de patrones
    private bool isDrawing = false;
    private bool hasNewPattern = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        currentPoints = new List<Vector3>();
        lastCompletedPoints = new List<Vector3>();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Detectar toque/click
        bool isTouching = false;
        Vector2 inputPosition = Vector2.zero;

        // Para dispositivos táctiles
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isTouching = touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved;
            inputPosition = touch.position;
        }
        // Para mouse (testing en editor)
        else if (Input.GetMouseButton(0))
        {
            isTouching = true;
            inputPosition = Input.mousePosition;
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
        // Convertir posición de pantalla a posición mundial
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
        return worldPos;
    }

    void StartDrawing(Vector3 startPosition)
    {
        isDrawing = true;
        hasNewPattern = false;

        // Crear nuevo GameObject para la línea
        GameObject lineObject = new GameObject("DrawLine");
        currentLine = lineObject.AddComponent<LineRenderer>();

        // Configurar LineRenderer
        currentLine.material = lineMaterial;
        currentLine.startColor = lineColor;
        currentLine.endColor = lineColor;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;
        currentLine.useWorldSpace = true;
        currentLine.positionCount = 0;

        // Limpiar puntos anteriores y agregar el primer punto
        currentPoints.Clear();
        currentPoints.Add(startPosition);
        UpdateLineRenderer();
    }

    void ContinueDrawing(Vector3 newPosition)
    {
        if (currentPoints.Count > 0)
        {
            // Solo agregar punto si está lo suficientemente lejos del anterior
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

        // Guardar los puntos para el sistema de patrones
        if (currentPoints.Count > 2) // Mínimo de puntos para considerar un patrón válido
        {
            lastCompletedPoints = new List<Vector3>(currentPoints);
            hasNewPattern = true;
            Debug.Log($"Dibujo completado con {lastCompletedPoints.Count} puntos");
        }

        // Destruir la línea actual
        if (currentLine != null)
        {
            DestroyImmediate(currentLine.gameObject);
            currentLine = null;
        }

        currentPoints.Clear();
    }

    // Método para que el sistema de patrones verifique si hay un nuevo patrón
    public bool HasNewPattern()
    {
        return hasNewPattern;
    }

    // Método para obtener el último patrón completado
    public List<Vector3> GetLastCompletedPattern()
    {
        hasNewPattern = false; // Marcar como procesado
        return new List<Vector3>(lastCompletedPoints);
    }

    // Métodos públicos simplificados
    public List<Vector3> GetCurrentPoints()
    {
        return new List<Vector3>(currentPoints);
    }

    public bool IsDrawing()
    {
        return isDrawing;
    }
}