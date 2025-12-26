using UnityEngine;
using UnityEngine.Events;

public class SymbolTarget : MonoBehaviour
{
    [Header("Symbol Configuration")]
    [Tooltip("El símbolo que destruirá este objeto")]
    [SerializeField] private string targetSymbol;

    [Header("Destruction Settings")]
    [SerializeField] private bool destroyOnMatch = true; 
    [SerializeField] private float destroyDelay = 0f;
    [SerializeField] private int pointsGiven = 1;
    private bool hasGavePoints = false;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject destroyEffect; 
    [SerializeField] private AudioClip destroySound; 

    [Header("Events")]
    public UnityEvent onSymbolMatched; 
    public UnityEvent onWrongSymbol; 

    private SymbolPatternSystem patternSystem;
    private AudioSource audioSource;
    private bool isDestroyed = false; //Evita destrucción múltiple

    void Start()
    {
        patternSystem = FindFirstObjectByType<SymbolPatternSystem>();

        if (patternSystem == null)
        {
            Debug.LogError("SymbolPatternSystem no encontrado en la escena!");
            return;
        }

        patternSystem.OnSymbolMatched += OnSymbolDetected;

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
        onSymbolMatched?.Invoke();

        if (destroySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(destroySound);
        }

        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        if (!hasGavePoints)
        {
            ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointsGiven);
            }
            hasGavePoints = true;
        }

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
        onWrongSymbol?.Invoke();

        //agregar feedback de simbolo incorrecto
    }

    void OnDestroy()
    {
        if (patternSystem != null)
        {
            patternSystem.OnSymbolMatched -= OnSymbolDetected;
        }
    }
}