using UnityEngine;
using UnityEngine.InputSystem;

public class BirdController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Gravedad personalizada")]
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float maxFallSpeed = -20f;

    [Header("Rotación (look up/down)")]
    [SerializeField] private float upAngle = 30f;
    [SerializeField] private float downAngle = -60f;
    [SerializeField] private float rotationLerpSpeed = 8f;

    [Header("Puntuación")]
    [SerializeField] private int pointValue = 1;
    [SerializeField] private FlappyScoreController scoreController;

    private Rigidbody2D rb;

    private float direction = 1f;
    private float verticalSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            verticalSpeed = jumpForce;
        }

#if UNITY_EDITOR
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            verticalSpeed = jumpForce;
        }
#endif

        verticalSpeed += gravity * Time.deltaTime;
        verticalSpeed = Mathf.Max(verticalSpeed, maxFallSpeed);

        float t = Mathf.InverseLerp(maxFallSpeed, jumpForce, verticalSpeed);
        float targetZ = Mathf.Lerp(downAngle, upAngle, t);
        float currentZ = transform.eulerAngles.z;
        float newZ = Mathf.LerpAngle(currentZ, targetZ, rotationLerpSpeed * Time.deltaTime);

        float newY = (direction > 0f) ? 0f : 180f;

        transform.rotation = Quaternion.Euler(0f, newY, newZ);

        Vector3 movement = new Vector3(
            direction * moveSpeed,
            verticalSpeed,
            0f
        ) * Time.deltaTime;

        transform.position += movement;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1f;
            scoreController.AddScore(pointValue);

            if (SpikeSpawner.Instance != null)
                SpikeSpawner.Instance.OnBirdBounced(direction, transform.position.y);
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

}
