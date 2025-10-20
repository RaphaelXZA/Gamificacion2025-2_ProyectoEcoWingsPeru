using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textoPuntuacion;

    private int puntuacion = 0;

    void Start()
    {
        puntuacion = 0;
        ActualizarUI();
    }

    // Método público para añadir puntos
    public void AñadirPuntos(int cantidad)
    {
        puntuacion += cantidad;
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