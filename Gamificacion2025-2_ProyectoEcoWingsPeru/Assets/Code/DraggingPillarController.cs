using UnityEngine;
using UnityEngine.InputSystem;

public class DraggingPillarController : MonoBehaviour
{
    [Header("Configuración del Pilar")]
    [SerializeField] private float limiteSuperior = 5f;
    [SerializeField] private float limiteInferior = -5f;
    [SerializeField] private float sensibilidad = 1f;
    [SerializeField] private int pointsGiven = 1;

    private Camera camaraPrincipal;
    private bool estaSiendoArrastrado = false;
    private float offsetY;
    private bool hasGavePoints = false;

    void Start()
    {
        camaraPrincipal = Camera.main;
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
        if (!hasGavePoints)
        {
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointsGiven);
            }
            hasGavePoints = true;
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
            else if (estaSiendoArrastrado)
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
            else if (Mouse.current.leftButton.isPressed && estaSiendoArrastrado)
            {
                ManageTouch(mousePosition, UnityEngine.InputSystem.TouchPhase.Moved);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && estaSiendoArrastrado)
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
                if (!estaSiendoArrastrado)
                {
                    StartDragging(posicionPantalla);
                }
                break;

            case UnityEngine.InputSystem.TouchPhase.Moved:
                // Solo continuar si ESTE pilar está siendo arrastrado
                if (estaSiendoArrastrado)
                {
                    ContinueDragging(posicionPantalla);
                }
                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                // Solo terminar si ESTE pilar estaba siendo arrastrado
                if (estaSiendoArrastrado)
                {
                    EndDragging();
                }
                break;
        }
    }

    private void StartDragging(Vector2 posicionPantalla)
    {
        // Convertir posición de pantalla a mundo
        Ray rayo = camaraPrincipal.ScreenPointToRay(posicionPantalla);
        RaycastHit hit;

        // Verificar si el rayo golpea este pilar (ignorando triggers)
        if (Physics.Raycast(rayo, out hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && hit.transform == transform)
        {
            estaSiendoArrastrado = true;

            // Calcular offset para un arrastre suave
            Vector3 puntoMundo = camaraPrincipal.ScreenToWorldPoint(
                new Vector3(posicionPantalla.x, posicionPantalla.y,
                camaraPrincipal.WorldToScreenPoint(transform.position).z));

            offsetY = transform.position.y - puntoMundo.y;
        }
    }

    private void ContinueDragging(Vector2 posicionPantalla)
    {
        // Convertir posición de pantalla a mundo
        Vector3 puntoMundo = camaraPrincipal.ScreenToWorldPoint(
            new Vector3(posicionPantalla.x, posicionPantalla.y,
            camaraPrincipal.WorldToScreenPoint(transform.position).z));

        // Calcular nueva posición Y con offset y sensibilidad
        float nuevaY = (puntoMundo.y + offsetY) * sensibilidad;

        // Aplicar límites
        nuevaY = Mathf.Clamp(nuevaY, limiteInferior, limiteSuperior);

        // Actualizar posición manteniendo X y Z
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);
    }

    private void EndDragging()
    {
        estaSiendoArrastrado = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GivePoints();
        }
    }

    // Método para configurar límites desde otros scripts
    public void SetUpLimits(float superior, float inferior)
    {
        limiteSuperior = superior;
        limiteInferior = inferior;
    }

    // Método para verificar si está siendo arrastrado
    public bool IsDragging()
    {
        return estaSiendoArrastrado;
    }
}