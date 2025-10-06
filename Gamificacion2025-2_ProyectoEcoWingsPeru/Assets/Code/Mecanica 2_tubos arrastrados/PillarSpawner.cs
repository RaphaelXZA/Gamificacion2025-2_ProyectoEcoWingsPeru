using UnityEngine;

public class PillarSpawner : MonoBehaviour
{
    [Header("Configuración de Generación")]
    [SerializeField] private GameObject pilarPrefab;
    [SerializeField] private float intervaloGeneracion = 2f;
    [SerializeField] private Transform puntoGeneracion1; // Posición alta
    [SerializeField] private Transform puntoGeneracion2; // Posición baja

    [Header("Configuración de Pilares")]
    [SerializeField] private float velocidadPilares = 3f;
    [SerializeField] private float limiteSuperior = 5f;
    [SerializeField] private float limiteInferior = -5f;

    private float tiempoUltimaGeneracion = 0f;

    void Start()
    {
        // Verificar que tenemos el prefab
        if (pilarPrefab == null)
        {
            Debug.LogError("¡No se ha asignado el prefab del pilar!");
            return;
        }

        // Crear puntos de generación automáticamente si no están asignados
        CrearPuntosGeneracionAutomaticos();
    }

    void Update()
    {
        GenerarPilarSiEsNecesario();
    }

    private void GenerarPilarSiEsNecesario()
    {
        // Verificar si es tiempo de generar un nuevo pilar
        if (Time.time - tiempoUltimaGeneracion >= intervaloGeneracion)
        {
            GenerarPilar();
            tiempoUltimaGeneracion = Time.time;
        }
    }

    private void GenerarPilar()
    {
        // Elegir aleatoriamente entre los dos puntos de generación
        Transform puntoElegido = Random.Range(0, 2) == 0 ? puntoGeneracion1 : puntoGeneracion2;

        if (puntoElegido != null)
        {
            // Crear el pilar en la posición elegida
            GameObject nuevoPilar = Instantiate(pilarPrefab, puntoElegido.position, puntoElegido.rotation);

            // Configurar el pilar generado
            PillarController controladorPilar = nuevoPilar.GetComponent<PillarController>();
            if (controladorPilar != null)
            {
                controladorPilar.SetUpSpeed(velocidadPilares);
                controladorPilar.SetUpLimits(limiteSuperior, limiteInferior);
            }

            Debug.Log($"Pilar generado en posición: {puntoElegido.position}");
        }
    }

    private void CrearPuntosGeneracionAutomaticos()
    {
        // Si no hay puntos asignados, crearlos automáticamente
        if (puntoGeneracion1 == null)
        {
            GameObject punto1 = new GameObject("PuntoGeneracion1");
            punto1.transform.position = new Vector3(10f, 2f, 0f); // Posición alta
            punto1.transform.parent = transform;
            puntoGeneracion1 = punto1.transform;
        }

        if (puntoGeneracion2 == null)
        {
            GameObject punto2 = new GameObject("PuntoGeneracion2");
            punto2.transform.position = new Vector3(10f, -2f, 0f); // Posición baja
            punto2.transform.parent = transform;
            puntoGeneracion2 = punto2.transform;
        }
    }

    // Método para cambiar el intervalo en tiempo real
    public void CambiarIntervalo(float nuevoIntervalo)
    {
        intervaloGeneracion = nuevoIntervalo;
    }

    // Método para pausar/reanudar la generación
    public void PausarGeneracion(bool pausar)
    {
        enabled = !pausar;
    }

    // Método para generar un pilar manualmente (útil para pruebas)
    public void GenerarPilarManual()
    {
        GenerarPilar();
    }

    // Método para configurar las posiciones de generación
    public void ConfigurarPosiciones(Vector3 posicion1, Vector3 posicion2)
    {
        if (puntoGeneracion1 != null)
            puntoGeneracion1.position = posicion1;
        if (puntoGeneracion2 != null)
            puntoGeneracion2.position = posicion2;
    }
}