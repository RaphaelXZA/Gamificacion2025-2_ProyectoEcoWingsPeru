using UnityEngine;
using UnityEngine.InputSystem;

public class DraggingPillarController : MonoBehaviour
{
    [Header("Pillar Settings")]
    [SerializeField] private float upperBoundary = 12f;
    [SerializeField] private float lowerBoundary = -10f;
    [SerializeField] private float sensitivity = 1f;
    [SerializeField] private int pointsGiven = 1;

    private Camera mainCamera;
    private bool isBeingDragged = false;
    private float offsetY;
    private bool hasGivenPoints = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Manejar input táctil
        ManageInput();

        // Destruir pilar si sale de la pantalla
        DestroyIfOffScreen();
    }

    private void DestroyIfOffScreen()
    {
        if (transform.position.x < -15f)
        {
            Destroy(gameObject);
        }
    }

    private void GivePoints()
    {
        // Dar puntos (si aún no se han dado)
        if (!hasGivenPoints)
        {
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointsGiven);
            }
            hasGivenPoints = true;
        }
    }

    private void ManageInput()
    {
        // Usar el nuevo Input System para touch
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.isPressed)
            {
                Vector2 touchPosition = touch.position.ReadValue();
                UnityEngine.InputSystem.TouchPhase phase = GetTouchPhase();
                ManageTouch(touchPosition, phase);
            }
            else if (isBeingDragged)
            {
                // Si se soltó el toque pero este pilar estaba siendo arrastrado, terminar arrastre
                EndDragging();
            }
        }
        // Fallback para mouse en editor/PC
        else if (Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ManageTouch(mousePosition, UnityEngine.InputSystem.TouchPhase.Began);
            }
            else if (Mouse.current.leftButton.isPressed && isBeingDragged)
            {
                ManageTouch(mousePosition, UnityEngine.InputSystem.TouchPhase.Moved);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isBeingDragged)
            {
                ManageTouch(mousePosition, UnityEngine.InputSystem.TouchPhase.Ended);
            }
        }
    }

    private UnityEngine.InputSystem.TouchPhase GetTouchPhase()
    {
        var touch = Touchscreen.current.primaryTouch;

        if (touch.press.wasPressedThisFrame)
            return UnityEngine.InputSystem.TouchPhase.Began;
        else if (touch.press.wasReleasedThisFrame)
            return UnityEngine.InputSystem.TouchPhase.Ended;
        else if (touch.press.isPressed)
            return UnityEngine.InputSystem.TouchPhase.Moved;
        else
            return UnityEngine.InputSystem.TouchPhase.Canceled;
    }

    private void ManageTouch(Vector2 posicionPantalla, UnityEngine.InputSystem.TouchPhase fase)
    {
        switch (fase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                // Solo iniciar arrastre si no hay ningún pilar siendo arrastrado
                if (!isBeingDragged)
                {
                    StartDragging(posicionPantalla);
                }
                break;

            case UnityEngine.InputSystem.TouchPhase.Moved:
                // Solo continuar si ESTE pilar está siendo arrastrado
                if (isBeingDragged)
                {
                    ContinueDragging(posicionPantalla);
                }
                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                // Solo terminar si ESTE pilar estaba siendo arrastrado
                if (isBeingDragged)
                {
                    EndDragging();
                }
                break;
        }
    }

    private void StartDragging(Vector2 posicionPantalla)
    {
        // Convertir posición de pantalla a mundo
        Ray rayo = mainCamera.ScreenPointToRay(posicionPantalla);
        RaycastHit hit;

        // Verificar si el rayo golpea este pilar (ignorando triggers)
        if (Physics.Raycast(rayo, out hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.transform == transform)
        {
            isBeingDragged = true;

            // Calcular offset para un arrastre suave
            Vector3 puntoMundo = mainCamera.ScreenToWorldPoint(
                new Vector3(posicionPantalla.x, posicionPantalla.y,
                mainCamera.WorldToScreenPoint(transform.position).z));

            offsetY = transform.position.y - puntoMundo.y;
        }
    }

    private void ContinueDragging(Vector2 posicionPantalla)
    {
        // Convertir posición de pantalla a mundo
        Vector3 puntoMundo = mainCamera.ScreenToWorldPoint(
            new Vector3(posicionPantalla.x, posicionPantalla.y,
            mainCamera.WorldToScreenPoint(transform.position).z));

        // Calcular nueva posición Y con offset y sensibilidad
        float nuevaY = (puntoMundo.y + offsetY) * sensitivity;

        // Aplicar límites
        nuevaY = Mathf.Clamp(nuevaY, lowerBoundary, upperBoundary);

        // Actualizar posición manteniendo X y Z
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
    }

    private void EndDragging()
    {
        isBeingDragged = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GivePoints();
        }
    }
}