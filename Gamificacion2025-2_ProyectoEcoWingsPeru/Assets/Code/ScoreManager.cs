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

    // M�todo p�blico para a�adir puntos
    public void A�adirPuntos(int cantidad)
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

    // M�todo para obtener la puntuaci�n actual
    public int ObtenerPuntuacion()
    {
        return puntuacion;
    }

    // M�todo para resetear la puntuaci�n
    public void ResetearPuntuacion()
    {
        puntuacion = 0;
        ActualizarUI();
        pillarSpawner.IntervaloGeneracion = pillarSpawner.StartIntervaloGeneracion;
        pillarSpawner.VelocidadPilares = pillarSpawner.StartVelocidadPilares;
        Debug.Log("Puntuaci�n reseteada");
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = "Puntos: " + puntuacion.ToString();
        }
    }
}