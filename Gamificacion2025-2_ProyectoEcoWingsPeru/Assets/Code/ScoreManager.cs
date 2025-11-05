using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoScore;

    private PillarSpawner obstacleSpawner;
    private int score = 0;


    private void Awake()
    {
        obstacleSpawner = FindAnyObjectByType<PillarSpawner>();
    }

    void Start()
    {
        score = 0;
        UpdateUI();
    }

    // Método público para añadir puntos
    public void AddScore(int cantidad)
    {
        score += cantidad;

        if (score != 0 && score % 4 == 0)
        {
            obstacleSpawner.SpawnInterval = Mathf.Max(0.8f, obstacleSpawner.SpawnInterval - 0.2f);
            obstacleSpawner.ObstacleSpeed += 0.4f;
        }
        UpdateUI();
        Debug.Log($"Puntos: {score}");
    }

    // Método para obtener la puntuación actual
    public int GetScore()
    {
        return score;
    }

    // Método para resetear la puntuación
    public void ResetScore()
    {
        score = 0;
        UpdateUI();
        obstacleSpawner.SpawnInterval = obstacleSpawner.StartSpawnInterval;
        obstacleSpawner.ObstacleSpeed = obstacleSpawner.StartObstacleSpeed;
        Debug.Log("Puntuación reseteada");
    }

    private void UpdateUI()
    {
        if (textoScore != null)
        {
            textoScore.text = "Puntos: " + score.ToString();
        }
    }
}