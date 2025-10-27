using System.Collections;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("GameOver Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

    [Header("Extra Life Elements")]
    [SerializeField] private GameObject extraLifePanel;
    [SerializeField] private TMP_Text extraLifeText;

    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;


    private PlayerController playerReference;
    private ScoreManager scoreManager;

    private void Awake()
    {
        Instance = this;
        playerReference = FindAnyObjectByType<PlayerController>();
        scoreManager = FindAnyObjectByType<ScoreManager>();
    }

    public void GameOver()
    {
        playerReference.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        finalScoreText.text = $"Puntaje: {scoreManager.ObtenerPuntuacion()}";
    }

    public IEnumerator ExtraLifeRoutine()
    {
        ClearObstacles();
        GameManager.Instance.ChangeState(GameManager.GameState.Announcement);
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        pausePanel.SetActive(false);
        extraLifePanel.SetActive(true);
        for (int i = 5; i >= 0; i--)
        {
            extraLifeText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        playerReference.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        extraLifePanel.SetActive(false);
    }

    public void RestartGame()
    {
        ClearObstacles();
        Time.timeScale = 1f;
        scoreManager.ResetearPuntuacion();
        gameOverPanel.SetActive(false);
        playerReference.gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        GameManager.Instance.ChangeState(GameManager.GameState.Paused);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    public void ClearObstacles()
    {
        ObstacleController[] obstacles = FindObjectsByType<ObstacleController>(FindObjectsSortMode.None);
        foreach (var obstacle in obstacles)
        {
            Destroy(obstacle.gameObject);
        }
    }

}
