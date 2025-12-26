using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [Header("Configuración del Pilar")]
    [SerializeField] private float movementSpeed = 3f;

    void Start()
    {
        // Asegurar que tiene un Collider para detectar el toque
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>();
        }
    }

    void Update()
    {
        if(GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            MovePillar();
            //DestroyIfOffScreen();
        }
    }

    private void MovePillar()
    {
        transform.Translate(Vector3.left * movementSpeed * Time.deltaTime);
    }

    //private void DestroyIfOffScreen()
    //{
    //    if (transform.position.x < -20f)
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    public void SetUpSpeed(float newSpeed)
    {
        movementSpeed = newSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DespawnZone"))
        {
            Destroy(this.gameObject);
        }
    }
}