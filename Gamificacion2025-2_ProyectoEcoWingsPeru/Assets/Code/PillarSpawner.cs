using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PilarConProbabilidad
{
    public GameObject prefab;
    [Range(0f, 100f)]
    public float probabilidad = 100f;
    [Tooltip("Si est� marcado, este prefab SOLO aparecer� en la posici�n central")]
    public bool soloPosicionCentral = false;
}

public class PillarSpawner : MonoBehaviour
{
    [Header("Configuraci�n de Generaci�n")]
    [SerializeField] private List<PilarConProbabilidad> pilaresPrefabs = new List<PilarConProbabilidad>();
    [SerializeField] private float intervaloGeneracion = 2f;
    [SerializeField] private Transform puntoGeneracionAlta; // Posici�n alta
    [SerializeField] private Transform puntoGeneracionCentral; // Posici�n central
    [SerializeField] private Transform puntoGeneracionBaja; // Posici�n baja

    [Header("Configuraci�n de Pilares")]
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
            Debug.LogError("�No se han asignado prefabs de pilares!");
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
        // Elegir posici�n aleatoriamente
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
            // Elegir un prefab aleatorio basado en la posici�n y probabilidades
            GameObject prefabElegido = ElegirPrefabAleatorio(esPosicionCentral);

            if (prefabElegido != null)
            {
                // Crear el pilar en la posici�n elegida
                GameObject nuevoPilar = Instantiate(prefabElegido, puntoElegido.position, puntoElegido.rotation);

                // Configurar el pilar generado
                ObstacleController controladorPilar = nuevoPilar.GetComponent<ObstacleController>();
                if (controladorPilar != null)
                {
                    controladorPilar.SetUpSpeed(velocidadPilares);
                }

                Debug.Log($"Pilar '{prefabElegido.name}' generado en posici�n: {puntoElegido.position}");
            }
        }
    }

    private GameObject ElegirPrefabAleatorio(bool esPosicionCentral)
    {
        // Filtrar prefabs v�lidos seg�n la posici�n
        List<PilarConProbabilidad> prefabsValidos = new List<PilarConProbabilidad>();

        foreach (var pilar in pilaresPrefabs)
        {
            if (pilar.prefab != null)
            {
                // Si es posici�n central, incluir TODOS los prefabs
                if (esPosicionCentral)
                {
                    prefabsValidos.Add(pilar);
                }
                // Si NO es posici�n central, solo incluir los que NO son exclusivos del centro
                else if (!pilar.soloPosicionCentral)
                {
                    prefabsValidos.Add(pilar);
                }
            }
        }

        // Si no hay prefabs v�lidos, devolver null
        if (prefabsValidos.Count == 0)
        {
            Debug.LogWarning("No hay prefabs v�lidos para esta posici�n");
            return null;
        }

        // Calcular la probabilidad total de los prefabs v�lidos
        float probabilidadTotal = 0f;
        foreach (var pilar in prefabsValidos)
        {
            probabilidadTotal += pilar.probabilidad;
        }

        // Si no hay probabilidades v�lidas, elegir al azar
        if (probabilidadTotal <= 0f)
        {
            return prefabsValidos[Random.Range(0, prefabsValidos.Count)].prefab;
        }

        // Generar n�mero aleatorio
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

        // Por seguridad, devolver el primer prefab v�lido
        return prefabsValidos[0].prefab;
    }

    // M�todo para cambiar el intervalo en tiempo real
    public void CambiarIntervalo(float nuevoIntervalo)
    {
        intervaloGeneracion = nuevoIntervalo;
    }

    // M�todo para pausar/reanudar la generaci�n
    public void PausarGeneracion(bool pausar)
    {
        enabled = !pausar;
    }

    // M�todo para generar un pilar manualmente (�til para pruebas)
    public void GenerarPilarManual()
    {
        GenerarPilar();
    }

    // M�todo para configurar las posiciones de generaci�n
    public void ConfigurarPosiciones(Vector3 posicion1, Vector3 posicion2, Vector3 posicion3)
    {
        if (puntoGeneracionAlta != null)
            puntoGeneracionAlta.position = posicion1;
        if (puntoGeneracionCentral != null)
            puntoGeneracionCentral.position = posicion2;
        if (puntoGeneracionBaja != null)
            puntoGeneracionBaja.position = posicion3;
    }

    // M�todo para agregar un prefab con probabilidad
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