using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Announcement,
        Paused,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public GameState currentState;


    public GameState CurrentState => currentState;

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        Debug.Log($"Game State changed to {newState}");
    }

    public void GoToMenu()
    {
        ChangeState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }

    public void ExtraLife()
    {
        Time.timeScale = 1f;
        HUDManager.Instance.StartCoroutine(HUDManager.Instance.ExtraLifeRoutine());
    }

    public void RetryGame()
    {
        HUDManager.Instance.RestartGame();
    }

    public void Pause()
    {
        HUDManager.Instance.Pause();
    }

    public void Resume()
    {
        HUDManager.Instance.Resume();
    }

    public void GameOver()
    {
        HUDManager.Instance.GameOver();
    }


}
