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

    // M�todo p�blico para a�adir puntos
    public void A�adirPuntos(int cantidad)
    {
        puntuacion += cantidad;
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