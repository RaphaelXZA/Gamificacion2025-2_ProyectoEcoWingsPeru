using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PilarConProbabilidad
{
    public GameObject prefab;
    [Range(0f, 100f)]
    public float probabilidad = 100f;
    [Tooltip("Si está marcado, este prefab SOLO aparecerá en la posición central")]
    public bool soloPosicionCentral = false;
}

public class PillarSpawner : MonoBehaviour
{
    [Header("Configuración de Generación")]
    [SerializeField] private List<PilarConProbabilidad> pilaresPrefabs = new List<PilarConProbabilidad>();
    [SerializeField] private float intervaloGeneracion = 2f;
    [SerializeField] private Transform puntoGeneracionAlta; // Posición alta
    [SerializeField] private Transform puntoGeneracionCentral; // Posición central
    [SerializeField] private Transform puntoGeneracionBaja; // Posición baja

    [Header("Configuración de Pilares")]
    [SerializeField] private float velocidadPilares = 3f;
    [SerializeField] private float limiteSuperior = 5f;
    [SerializeField] private float limiteInferior = -5f;

    private float startIntervaloGeneracion;
    private float startVelocidadPilares;

    public float StartIntervaloGeneracion
    {
        get { return startIntervaloGeneracion; }
    }

    public float StartVelocidadPilares
    {
        get { return startVelocidadPilares; }
    }

    public float IntervaloGeneracion
    {
        get { return intervaloGeneracion; }
        set { intervaloGeneracion = value; }
    }

    public float VelocidadPilares
    {
        get { return velocidadPilares; }
        set { velocidadPilares = value; }
    }

    private float tiempoUltimaGeneracion = 0f;

    void Start()
    {
        startIntervaloGeneracion = intervaloGeneracion;
        startVelocidadPilares = velocidadPilares;

        // Verificar que tenemos prefabs
        if (pilaresPrefabs == null || pilaresPrefabs.Count == 0)
        {
            Debug.LogError("¡No se han asignado prefabs de pilares!");
            return;
        }
    }

    void Update()
    {
        if(GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            GenerarPilarSiEsNecesario();
        }
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
        // Elegir posición aleatoriamente
        int posicionAleatoria = Random.Range(0, 3);
        Transform puntoElegido = null;
        bool esPosicionCentral = false;

        switch (posicionAleatoria)
        {
            case 0:
                puntoElegido = puntoGeneracionAlta; // Alta
                break;
            case 1:
                puntoElegido = puntoGeneracionCentral; // Central
                esPosicionCentral = true;
                break;
            case 2:
                puntoElegido = puntoGeneracionBaja; // Baja
                break;
        }

        if (puntoElegido != null)
        {
            // Elegir un prefab aleatorio basado en la posición y probabilidades
            GameObject prefabElegido = ElegirPrefabAleatorio(esPosicionCentral);

            if (prefabElegido != null)
            {
                // Crear el pilar en la posición elegida
                GameObject nuevoPilar = Instantiate(prefabElegido, puntoElegido.position, puntoElegido.rotation);

                // Configurar el pilar generado
                ObstacleController controladorPilar = nuevoPilar.GetComponent<ObstacleController>();
                if (controladorPilar != null)
                {
                    controladorPilar.SetUpSpeed(velocidadPilares);
                }

                Debug.Log($"Pilar '{prefabElegido.name}' generado en posición: {puntoElegido.position}");
            }
        }
    }

    private GameObject ElegirPrefabAleatorio(bool esPosicionCentral)
    {
        // Filtrar prefabs válidos según la posición
        List<PilarConProbabilidad> prefabsValidos = new List<PilarConProbabilidad>();

        foreach (var pilar in pilaresPrefabs)
        {
            if (pilar.prefab != null)
            {
                // Si es posición central, incluir TODOS los prefabs
                if (esPosicionCentral)
                {
                    prefabsValidos.Add(pilar);
                }
                // Si NO es posición central, solo incluir los que NO son exclusivos del centro
                else if (!pilar.soloPosicionCentral)
                {
                    prefabsValidos.Add(pilar);
                }
            }
        }

        // Si no hay prefabs válidos, devolver null
        if (prefabsValidos.Count == 0)
        {
            Debug.LogWarning("No hay prefabs válidos para esta posición");
            return null;
        }

        // Calcular la probabilidad total de los prefabs válidos
        float probabilidadTotal = 0f;
        foreach (var pilar in prefabsValidos)
        {
            probabilidadTotal += pilar.probabilidad;
        }

        // Si no hay probabilidades válidas, elegir al azar
        if (probabilidadTotal <= 0f)
        {
            return prefabsValidos[Random.Range(0, prefabsValidos.Count)].prefab;
        }

        // Generar número aleatorio
        float valorAleatorio = Random.Range(0f, probabilidadTotal);

        // Seleccionar prefab basado en probabilidad
        float acumulado = 0f;
        foreach (var pilar in prefabsValidos)
        {
            acumulado += pilar.probabilidad;
            if (valorAleatorio <= acumulado)
            {
                return pilar.prefab;
            }
        }

        // Por seguridad, devolver el primer prefab válido
        return prefabsValidos[0].prefab;
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
    public void ConfigurarPosiciones(Vector3 posicion1, Vector3 posicion2, Vector3 posicion3)
    {
        if (puntoGeneracionAlta != null)
            puntoGeneracionAlta.position = posicion1;
        if (puntoGeneracionCentral != null)
            puntoGeneracionCentral.position = posicion2;
        if (puntoGeneracionBaja != null)
            puntoGeneracionBaja.position = posicion3;
    }

    // Método para agregar un prefab con probabilidad
    public void AgregarPrefab(GameObject prefab, float probabilidad, bool soloCentral = false)
    {
        pilaresPrefabs.Add(new PilarConProbabilidad
        {
            prefab = prefab,
            probabilidad = probabilidad,
            soloPosicionCentral = soloCentral
        });
    }
}