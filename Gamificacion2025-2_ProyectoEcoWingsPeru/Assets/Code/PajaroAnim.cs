using System.Collections;
using UnityEngine;

public class PajaroAnim : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(RotarPajaro());
    }

    // Que no sea tan constante, sino mas random la rotacion en el eje x

    private IEnumerator RotarPajaro()
    {
        while (true)
        {
            float anguloX = Random.Range(-15f, 15f);
            float anguloY = Random.Range(-15f, 15f);
            float anguloZ = Random.Range(-15f, 15f);
            Quaternion rotacionObjetivo = Quaternion.Euler(anguloX, anguloY, anguloZ);
            float duracion = Random.Range(1f, 3f);
            float tiempoTranscurrido = 0f;
            Quaternion rotacionInicial = transform.rotation;
            while (tiempoTranscurrido < duracion)
            {
                transform.rotation = Quaternion.Slerp(rotacionInicial, rotacionObjetivo, tiempoTranscurrido / duracion);
                tiempoTranscurrido += Time.deltaTime;
                yield return null;
            }
            transform.rotation = rotacionObjetivo;
            yield return new WaitForSeconds(Random.Range(0.5f, 2f));
        }
    }

}
