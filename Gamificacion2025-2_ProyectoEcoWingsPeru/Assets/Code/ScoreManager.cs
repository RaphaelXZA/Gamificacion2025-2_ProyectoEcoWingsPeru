using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textScore;
    [SerializeField] private SpriteRenderer backgroundScore;
    [SerializeField] private int umbralChangeColor = 3;
    [SerializeField] private int umbralSpeedIncrease = 3;
    [SerializeField] private Color nightColor;
    [SerializeField] private float colorTransitionSpeed = 2f;

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

        if (score != 0 && score % umbralSpeedIncrease == 0 && obstacleSpawner != null)
        {
            obstacleSpawner.SpawnInterval = Mathf.Max(0.8f, obstacleSpawner.SpawnInterval - 0.2f);
            obstacleSpawner.ObstacleSpeed += 0.4f;
        }

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
        targetColor = originalColor; // volver a día
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
