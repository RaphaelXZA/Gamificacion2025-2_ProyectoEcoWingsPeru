using UnityEngine;
using UnityEngine.Events;

public class SymbolTarget : MonoBehaviour
{
    [Header("Symbol Configuration")]
    [SerializeField] private string targetSymbol; // El símbolo que destruirá este objeto

    [Header("Destruction Settings")]
    [SerializeField] private bool destroyOnMatch = true; // Si se destruye automáticamente
    [SerializeField] private float destroyDelay = 0f; // Delay antes de destruir (para animaciones)
    [SerializeField] private int pointsGiven = 1;
    private bool hasGavePoints = false;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject destroyEffect; // Prefab de efecto de destrucción
    [SerializeField] private AudioClip destroySound; // Sonido al destruir

    [Header("Events")]
    public UnityEvent onSymbolMatched; // Evento cuando se detecta el símbolo correcto
    public UnityEvent onWrongSymbol; // Evento cuando se dibuja símbolo incorrecto

    private SymbolPatternSystem patternSystem;
    private AudioSource audioSource;
    private bool isDestroyed = false; // Evitar destrucción múltiple

    void Start()
    {
        // Encontrar el sistema de patrones
        patternSystem = FindFirstObjectByType<SymbolPatternSystem>();

        if (patternSystem == null)
        {
            Debug.LogError("SymbolPatternSystem no encontrado en la escena!");
            return;
        }

        // Suscribirse al evento de detección de símbolos
        patternSystem.OnSymbolMatched += OnSymbolDetected;

        // Configurar audio si es necesario
        if (destroySound != null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        Debug.Log($"SymbolTarget '{gameObject.name}' esperando símbolo: '{targetSymbol}'");
    }

    void OnSymbolDetected(string detectedSymbol, float accuracy)
    {
        if (isDestroyed) return; 

        if (detectedSymbol.Equals(targetSymbol, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"Símbolo correcto '{detectedSymbol}' detectado para '{gameObject.name}'!");
            OnCorrectSymbol(accuracy);
        }
        else
        {
            Debug.Log($"Símbolo incorrecto. Esperaba '{targetSymbol}', se dibujó '{detectedSymbol}'");
            OnIncorrectSymbol(detectedSymbol);
        }
    }

    void OnCorrectSymbol(float accuracy)
    {
        // Invocar evento personalizado
        onSymbolMatched?.Invoke();

        // Reproducir sonido si existe
        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        // Instanciar efecto de destrucción si existe
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        //Dar puntuacion
        if (!hasGavePoints)
        {
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointsGiven);
            }
            hasGavePoints = true;
        }

        // Destruir el objeto si está configurado
        if (destroyOnMatch)
        {
            isDestroyed = true;

            if (destroyDelay > 0)
            {
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    void OnIncorrectSymbol(string wrongSymbol)
    {
        // Invocar evento de símbolo incorrecto
        onWrongSymbol?.Invoke();

        // Aquí puedes agregar feedback negativo (shake, color rojo, etc.)
    }

    void OnDestroy()
    {
        // Desuscribirse del evento al destruir
        if (patternSystem != null)
        {
            patternSystem.OnSymbolMatched -= OnSymbolDetected;
        }
    }

    // Métodos públicos útiles

    /// <summary>
    /// Cambiar el símbolo objetivo en runtime
    /// </summary>
    public void SetTargetSymbol(string newSymbol)
    {
        targetSymbol = newSymbol;
        Debug.Log($"Símbolo objetivo cambiado a: '{targetSymbol}'");
    }

    /// <summary>
    /// Obtener el símbolo objetivo actual
    /// </summary>
    public string GetTargetSymbol()
    {
        return targetSymbol;
    }

    /// <summary>
    /// Destruir manualmente el objeto
    /// </summary>
    public void DestroyTarget()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Verificar si este target ya fue destruido
    /// </summary>
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
}