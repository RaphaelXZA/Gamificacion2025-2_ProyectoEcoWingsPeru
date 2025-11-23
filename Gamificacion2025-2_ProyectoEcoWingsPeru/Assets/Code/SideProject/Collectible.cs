using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField] private int pointValue = 1;
    [SerializeField] private FlappyScoreController scoreController;

    private void Start()
    {
        scoreController = FindFirstObjectByType<FlappyScoreController>().GetComponent<FlappyScoreController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            scoreController.AddScore(pointValue);
            Destroy(gameObject);
        }
    }
}