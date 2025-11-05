using UnityEngine;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class ObstacleWithProbability
{
    public GameObject prefab;
    [Range(0f, 100f)]
    public float probability = 100f;
    [Tooltip("Si está marcado, este prefab SOLO aparecerá en la posición central")]
    public bool onlyCenterPosition = false;
}

public class PillarSpawner : MonoBehaviour
{
    [Header("Configuración de Generación")]
    [SerializeField] private List<ObstacleWithProbability> obstaclePrefabs = new List<ObstacleWithProbability>();
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnPointHigh;
    [SerializeField] private Transform spawnPointCenter;
    [SerializeField] private Transform spawnPointLow;

    [Header("Tutorial")]
    [SerializeField] private bool startWithTutorial = false;
    [SerializeField] private float intervalForTutorial = 4f;
    [SerializeField] private TextMeshProUGUI tutorialMessage;

    [Header("Configuración de Obstaculos")]
    [SerializeField] private float obstacleSpeed = 3f;

    private float startSpawnInterval;
    private float startObstacleSpeed;

    // Variables para el tutorial
    private bool isInTutorial = false;
    private int tutorialIndex = 0;
    private int tutorialInitialScore = 0;

    // Secuencia del tutorial: índice de prefab y posición (0=alta, 1=central, 2=baja)
    private readonly (int prefabIndex, int position)[] tutorialSequence = new[]
    {
        (0, 0), // Prefab 0, posición alta
        (0, 2), // Prefab 0, posición baja
        (0, 0), // Prefab 0, posición alta
        (0, 2), // Prefab 0, posición baja
        (1, 1), // Prefab 1, posición central
        (2, 1)  // Prefab 2, posición central
    };

    public float StartSpawnInterval
    {
        get { return startSpawnInterval; }
    }

    public float StartObstacleSpeed
    {
        get { return startObstacleSpeed; }
    }

    public float SpawnInterval
    {
        get { return spawnInterval; }
        set { spawnInterval = value; }
    }

    public float ObstacleSpeed
    {
        get { return obstacleSpeed; }
        set { obstacleSpeed = value; }
    }

    private float lastSpawnTime;

    void Start()
    {
        lastSpawnTime = Time.time;
        startSpawnInterval = spawnInterval;
        startObstacleSpeed = obstacleSpeed;

        // Verificar que tenemos prefabs
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogError("¡No se han asignado prefabs de obstaculos!");
            return;
        }

        // Inicializar tutorial si está activado
        if (startWithTutorial)
        {
            StartTutorial();
        }
        else
        {
            // Asegurarse de que el mensaje esté oculto si no hay tutorial
            if (tutorialMessage != null)
            {
                tutorialMessage.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            // Verificar si el tutorial debe terminar
            if (isInTutorial)
            {
                CheckTutorialEnd();
            }

            SpanwObstaclesIfNeccesary();
        }
    }

    private void SpanwObstaclesIfNeccesary()
    {
        float currentInterval = isInTutorial ? intervalForTutorial : spawnInterval;

        // Verificar si es tiempo de generar un nuevo pilar
        if (Time.time - lastSpawnTime >= currentInterval)
        {
            if (isInTutorial)
            {
                SpawnTutorialObstacles();
            }
            else
            {
                SpawnPilar();
            }
            lastSpawnTime = Time.time;
        }
    }

    private void SpawnPilar()
    {
        // Elegir posición aleatoriamente
        int randomPosition = Random.Range(0, 3);
        Transform chosenPosition = null;
        bool isCentralPosition = false;

        switch (randomPosition)
        {
            case 0:
                chosenPosition = spawnPointHigh;
                break;
            case 1:
                chosenPosition = spawnPointCenter;
                isCentralPosition = true;
                break;
            case 2:
                chosenPosition = spawnPointLow;
                break;
        }

        if (chosenPosition != null)
        {
            // Elegir un prefab aleatorio basado en la posición y probabilidades
            GameObject chosenPrefab = ChooseRandomPrefab(isCentralPosition);

            if (chosenPrefab != null)
            {
                // Crear el pilar en la posición elegida
                GameObject newObstacle = Instantiate(chosenPrefab, chosenPosition.position, chosenPosition.rotation);

                // Configurar el pilar generado
                ObstacleController obstacleController = newObstacle.GetComponent<ObstacleController>();
                if (obstacleController != null)
                {
                    obstacleController.SetUpSpeed(obstacleSpeed);
                }

                Debug.Log($"Pilar '{chosenPrefab.name}' generado en posición: {chosenPosition.position}");
            }
        }
    }

    private GameObject ChooseRandomPrefab(bool isCentralPosition)
    {
        // Filtrar prefabs válidos según la posición
        List<ObstacleWithProbability> validPrefabs = new List<ObstacleWithProbability>();

        foreach (var obstacle in obstaclePrefabs)
        {
            if (obstacle.prefab != null)
            {
                // Si es posición central, incluir TODOS los prefabs
                if (isCentralPosition)
                {
                    validPrefabs.Add(obstacle);
                }
                // Si NO es posición central, solo incluir los que NO son exclusivos del centro
                else if (!obstacle.onlyCenterPosition)
                {
                    validPrefabs.Add(obstacle);
                }
            }
        }

        // Si no hay prefabs válidos, devolver null
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("No hay prefabs válidos para esta posición");
            return null;
        }

        // Calcular la probabilidad total de los prefabs válidos
        float totalProbability = 0f;
        foreach (var obstacle in validPrefabs)
        {
            totalProbability += obstacle.probability;
        }

        // Si no hay probabilidades válidas, elegir al azar
        if (totalProbability <= 0f)
        {
            return validPrefabs[Random.Range(0, validPrefabs.Count)].prefab;
        }

        // Generar número aleatorio
        float randomValue = Random.Range(0f, totalProbability);

        // Seleccionar prefab basado en probabilidad
        float total = 0f;
        foreach (var pilar in validPrefabs)
        {
            total += pilar.probability;
            if (randomValue <= total)
            {
                return pilar.prefab;
            }
        }

        // Por seguridad, devolver el primer prefab válido
        return validPrefabs[0].prefab;
    }

    // Métodos del Tutorial
    private void StartTutorial()
    {
        isInTutorial = true;
        tutorialIndex = 0;

        // Obtener puntuación actual del ScoreManager
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            tutorialInitialScore = scoreManager.GetScore();
        }

        // Mostrar mensaje inicial del tutorial
        if (tutorialMessage != null)
        {
            tutorialMessage.gameObject.SetActive(true);
            tutorialMessage.text = "Desliza";
        }

        Debug.Log("Tutorial iniciado");
    }

    private void SpawnTutorialObstacles()
    {
        // Verificar que no hayamos completado la secuencia
        if (tutorialIndex >= tutorialSequence.Length)
        {
            return;
        }

        // Obtener el prefab y posición de la secuencia
        var step = tutorialSequence[tutorialIndex];
        int prefabIndex = step.prefabIndex;
        int position = step.position;

        // Verificar que el índice del prefab es válido
        if (prefabIndex >= obstaclePrefabs.Count || obstaclePrefabs[prefabIndex].prefab == null)
        {
            Debug.LogError($"Tutorial: Prefab en índice {prefabIndex} no existe");
            return;
        }

        // Obtener el punto de generación según la posición
        Transform chosenPoint = null;
        switch (position)
        {
            case 0:
                chosenPoint = spawnPointHigh;
                break;
            case 1:
                chosenPoint = spawnPointCenter;
                break;
            case 2:
                chosenPoint = spawnPointLow;
                break;
        }

        if (chosenPoint != null)
        {
            // Crear el pilar
            GameObject chosenPrefab = obstaclePrefabs[prefabIndex].prefab;
            GameObject newObstacle = Instantiate(chosenPrefab, chosenPoint.position, chosenPoint.rotation);

            // Configurar el pilar generado
            ObstacleController controladorPilar = newObstacle.GetComponent<ObstacleController>();
            if (controladorPilar != null)
            {
                controladorPilar.SetUpSpeed(obstacleSpeed);
            }

            Debug.Log($"Tutorial: Obstaculo {tutorialIndex + 1}/{tutorialSequence.Length} generado");
        }

        tutorialIndex++;
    }

    private void CheckTutorialEnd()
    {
        // Obtener puntuación actual
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            int currentScore = scoreManager.GetScore();
            int gainedScore = currentScore - tutorialInitialScore;

            // Cambiar mensaje a "Dibuja" cuando haya ganado 4 puntos
            if (gainedScore == 4 && tutorialMessage != null)
            {
                tutorialMessage.text = "Dibuja";
            }

            // Si ha ganado 6 puntos (los 6 obstáculos del tutorial), terminar tutorial
            if (gainedScore >= 6)
            {
                EndTutorial();
            }
        }
    }

    private void EndTutorial()
    {
        isInTutorial = false;
        startWithTutorial = false;

        // Ocultar mensaje del tutorial
        if (tutorialMessage != null)
        {
            tutorialMessage.gameObject.SetActive(false);
        }

        Debug.Log("Tutorial completado");
    }
}