using UnityEngine;

public class PillarController : MonoBehaviour
{
    [Header("Configuración del Pilar")]
    [SerializeField] private float limiteSuperior = 5f;
    [SerializeField] private float limiteInferior = -5f;
    [SerializeField] private float sensibilidad = 1f;
    [SerializeField] private float velocidadMovimiento = 3f;

    private Camera camaraPrincipal;
    private bool estaSiendoArrastrado = false;
    private float offsetY;

    void Start()
    {
        camaraPrincipal = Camera.main;

        // Asegurar que tiene un Collider para detectar el toque
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    void Update()
    {
        // Mover el pilar hacia la izquierda
        MovePillar();

        // Manejar input táctil
        ManageInput();

        // Destruir pilar si sale de la pantalla
        DestroyIfOffScreen();
    }

    private void MovePillar()
    {
        // Solo mover si no está siendo arrastrado (opcional, puedes quitarlo si quieres que se mueva siempre)
        // if (!estaSiendoArrastrado)
        // {
        transform.Translate(Vector3.left * velocidadMovimiento * Time.deltaTime);
        // }
    }

    private void DestroyIfOffScreen()
    {
        // Destruir el pilar si se sale por el lado izquierdo de la pantalla
        if (transform.position.x < -20f)
        {
            Destroy(gameObject);
        }
    }

    private void ManageInput()
    {
        // Para Android (toque táctil)
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);
            ManageTouch(toque.position, toque.phase);
        }
        // Para PC/Editor (mouse) - útil para pruebas
        else if (Application.isEditor)
        {
            Vector2 posicionMouse = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                ManageTouch(posicionMouse, TouchPhase.Began);
            }
            else if (Input.GetMouseButton(0) && estaSiendoArrastrado)
            {
                ManageTouch(posicionMouse, TouchPhase.Moved);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ManageTouch(posicionMouse, TouchPhase.Ended);
            }
        }
    }

    private void ManageTouch(Vector2 posicionPantalla, TouchPhase fase)
    {
        switch (fase)
        {
            case TouchPhase.Began:
                StartDragging(posicionPantalla);
                break;

            case TouchPhase.Moved:
                if (estaSiendoArrastrado)
                {
                    ContinueDragging(posicionPantalla);
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndDragging();
                break;
        }
    }

    private void StartDragging(Vector2 posicionPantalla)
    {
        // Convertir posición de pantalla a mundo
        Ray rayo = camaraPrincipal.ScreenPointToRay(posicionPantalla);
        RaycastHit hit;

        // Verificar si el rayo golpea este pilar
        if (Physics.Raycast(rayo, out hit) && hit.transform == transform)
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

    // Método para configurar velocidad desde otros scripts
    public void SetUpSpeed(float nuevaVelocidad)
    {
        velocidadMovimiento = nuevaVelocidad;
    }
}