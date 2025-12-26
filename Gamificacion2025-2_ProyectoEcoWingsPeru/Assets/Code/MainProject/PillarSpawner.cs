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
    [Header("Spawn Settings")]
    [SerializeField] private List<ObstacleWithProbability> obstaclePrefabs = new List<ObstacleWithProbability>();
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform spawnPointHigh;
    [SerializeField] private Transform spawnPointCenter;
    [SerializeField] private Transform spawnPointLow;

    [Header("Tutorial")]
    [SerializeField] private bool startWithTutorial = false;
    [SerializeField] private float intervalForTutorial = 4f;
    [SerializeField] private TextMeshProUGUI tutorialMessage;

    [Header("Obstacle Settings")]
    [SerializeField] private float obstacleSpeed = 3f;

    private float startSpawnInterval;
    private float startObstacleSpeed;

    //Tutorial
    private bool isInTutorial = false;
    private int tutorialIndex = 0;
    private int tutorialInitialScore = 0;

    //Secuencia del tutorial (0=alta, 1=central, 2=baja)
    private readonly (int prefabIndex, int position)[] tutorialSequence = new[]
    {
        (0, 0), 
        (0, 2), 
        (0, 0), 
        (0, 2), 
        (1, 1), 
        (2, 1)  
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

        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogError("¡No se han asignado prefabs de obstaculos!");
            return;
        }

        InitializeTutorial();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            if (isInTutorial)
            {
                CheckTutorialEnd();
            }

            SpawnObstaclesIfNeccesary();
        }
    }

    private void SpawnObstaclesIfNeccesary()
    {
        float currentInterval = isInTutorial ? intervalForTutorial : spawnInterval;

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
            GameObject chosenPrefab = ChooseRandomPrefab(isCentralPosition);

            if (chosenPrefab != null)
            {
                GameObject newObstacle = Instantiate(chosenPrefab, chosenPosition.position, chosenPosition.rotation);

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
        List<ObstacleWithProbability> validPrefabs = new List<ObstacleWithProbability>();

        foreach (var obstacle in obstaclePrefabs)
        {
            if (obstacle.prefab != null)
            {
                if (isCentralPosition)
                {
                    validPrefabs.Add(obstacle);
                }
                else if (!obstacle.onlyCenterPosition)
                {
                    validPrefabs.Add(obstacle);
                }
            }
        }

        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("No hay prefabs válidos para esta posición");
            return null;
        }

        float totalProbability = 0f;
        foreach (var obstacle in validPrefabs)
        {
            totalProbability += obstacle.probability;
        }

        if (totalProbability <= 0f)
        {
            return validPrefabs[Random.Range(0, validPrefabs.Count)].prefab;
        }

        float randomValue = Random.Range(0f, totalProbability);

        float total = 0f;
        foreach (var pilar in validPrefabs)
        {
            total += pilar.probability;
            if (randomValue <= total)
            {
                return pilar.prefab;
            }
        }

        return validPrefabs[0].prefab;
    }

    private void InitializeTutorial()
    {
        if (startWithTutorial)
        {
            StartTutorial();
        }
        else
        {
            if (tutorialMessage != null)
            {
                tutorialMessage.gameObject.SetActive(false);
            }
        }
    }

    private void StartTutorial()
    {
        isInTutorial = true;
        tutorialIndex = 0;
        lastSpawnTime = Time.time;

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            tutorialInitialScore = scoreManager.GetScore();
        }

        if (tutorialMessage != null)
        {
            tutorialMessage.gameObject.SetActive(true);
            tutorialMessage.text = "Desliza";
        }

        Debug.Log("Tutorial iniciado");
    }

    private void SpawnTutorialObstacles()
    {
        if (tutorialIndex >= tutorialSequence.Length)
        {
            return;
        }

        var step = tutorialSequence[tutorialIndex];
        int prefabIndex = step.prefabIndex;
        int position = step.position;

        if (prefabIndex >= obstaclePrefabs.Count || obstaclePrefabs[prefabIndex].prefab == null)
        {
            Debug.LogError($"Tutorial: Prefab en índice {prefabIndex} no existe");
            return;
        }

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
            GameObject chosenPrefab = obstaclePrefabs[prefabIndex].prefab;
            GameObject newObstacle = Instantiate(chosenPrefab, chosenPoint.position, chosenPoint.rotation);

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
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            int currentScore = scoreManager.GetScore();
            int gainedScore = currentScore - tutorialInitialScore;

            if (gainedScore == 4 && tutorialMessage != null)
            {
                tutorialMessage.text = "Dibuja";
            }

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

        if (tutorialMessage != null)
        {
            tutorialMessage.gameObject.SetActive(false);
        }

        Debug.Log("Tutorial completado");
    }

    public void ResetTutorial()
    {
        isInTutorial = false;
        tutorialIndex = 0;
        tutorialInitialScore = 0;
        lastSpawnTime = Time.time;

        if (tutorialMessage != null)
        {
            tutorialMessage.gameObject.SetActive(false);
        }

        InitializeTutorial();
    }
}