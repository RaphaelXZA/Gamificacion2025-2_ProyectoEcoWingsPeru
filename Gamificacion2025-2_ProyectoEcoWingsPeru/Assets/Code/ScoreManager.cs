using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoPuntuacion;

    private PillarSpawner pillarSpawner;
    private int puntuacion = 0;


    private void Awake()
    {
        pillarSpawner = FindAnyObjectByType<PillarSpawner>();
    }

    void Start()
    {
        puntuacion = 0;
        ActualizarUI();
    }

    // Método público para añadir puntos
    public void AñadirPuntos(int cantidad)
    {
        puntuacion += cantidad;

        if (puntuacion != 0 && puntuacion % 4 == 0)
        {
            pillarSpawner.IntervaloGeneracion = Mathf.Max(0.8f, pillarSpawner.IntervaloGeneracion - 0.2f);
            pillarSpawner.VelocidadPilares += 0.4f;
        }
        ActualizarUI();
        Debug.Log($"Puntos: {puntuacion}");
    }

    // Método para obtener la puntuación actual
    public int ObtenerPuntuacion()
    {
        return puntuacion;
    }

    // Método para resetear la puntuación
    public void ResetearPuntuacion()
    {
        puntuacion = 0;
        ActualizarUI();
        pillarSpawner.IntervaloGeneracion = pillarSpawner.StartIntervaloGeneracion;
        pillarSpawner.VelocidadPilares = pillarSpawner.StartVelocidadPilares;
        Debug.Log("Puntuación reseteada");
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = "Puntos: " + puntuacion.ToString();
        }
    }
}