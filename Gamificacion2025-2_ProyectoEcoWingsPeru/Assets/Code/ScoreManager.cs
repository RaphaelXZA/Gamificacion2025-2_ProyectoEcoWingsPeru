using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textScore;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer backgroundScore;
    [SerializeField] private int umbralChangeColor = 3;
    [SerializeField] private Color nightColor;
    [SerializeField] private float colorTransitionSpeed = 2f;

    [Header("Aumento de Intervalo de Spawn")]
    [SerializeField] private int pointsToReduceInterval = 3;
    [SerializeField] private float spawnIntervalReduction = 0.2f;
    [Tooltip("Lo más bajo que puede llegar a reducirse el intervalo de spawn")]
    [SerializeField] private float minimalInterval = 1f;

    [Header("Aumento de Velocidad")]
    [SerializeField] private int pointsToIncreaseSpeed = 3;
    [SerializeField] private float obstacleSpeedBoost = 0.4f;

    private Color originalColor;
    private Color targetColor;
    private bool isNight = false;
    private PillarSpawner obstacleSpawner;
    private int score = 0;

    void Awake()
    {
        originalColor = backgroundScore.color;
        targetColor = originalColor;
        obstacleSpawner = FindAnyObjectByType<PillarSpawner>();
    }

    void Start()
    {
        score = 0;
        UpdateUI();
    }

    void Update()
    {
        backgroundScore.color = Color.Lerp(
            backgroundScore.color,
            targetColor,
            colorTransitionSpeed * Time.deltaTime
        );
    }

    public void AddScore(int cantidad)
    {
        score += cantidad;

        // Reducir intervalo de spawn cada X puntos
        if (score != 0 && score % pointsToReduceInterval == 0 && obstacleSpawner != null)
        {
            obstacleSpawner.SpawnInterval = Mathf.Max(minimalInterval, obstacleSpawner.SpawnInterval - spawnIntervalReduction);
        }

        // Aumentar velocidad cada X puntos
        if (score != 0 && score % pointsToIncreaseSpeed == 0 && obstacleSpawner != null)
        {
            obstacleSpawner.ObstacleSpeed += obstacleSpeedBoost;
        }

        // Cambiar color de fondo cada X puntos
        if (score != 0 && score % umbralChangeColor == 0)
        {
            isNight = !isNight;
            targetColor = isNight ? nightColor : originalColor;
        }

        UpdateUI();
        Debug.Log($"Puntos: {score}");
    }

    public int GetScore() => score;

    public void ResetScore()
    {
        score = 0;
        isNight = false;
        targetColor = originalColor;
        UpdateUI();

        if (obstacleSpawner != null)
        {
            obstacleSpawner.SpawnInterval = obstacleSpawner.StartSpawnInterval;
            obstacleSpawner.ObstacleSpeed = obstacleSpawner.StartObstacleSpeed;
        }
        Debug.Log("Puntuación reseteada");
    }

    private void UpdateUI()
    {
        if (textScore != null) textScore.text = "Puntos: " + score;
    }
}