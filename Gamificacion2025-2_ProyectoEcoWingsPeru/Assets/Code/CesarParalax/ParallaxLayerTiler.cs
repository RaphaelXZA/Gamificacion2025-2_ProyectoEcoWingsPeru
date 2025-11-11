using System.Linq;
using UnityEngine;


public class ParallaxLayerTiler : MonoBehaviour
{
    [Tooltip("Si no se asigna, se usa Camera.main")] public Transform cameraTransform;


    [Header("Segmentos a lo ancho")]
    [Tooltip("Deja vacío para autodescubrir hijos directos como segmentos.")]
    public Transform[] segments; // cada hijo es una pieza del fondo, alineada en X


    [Tooltip("Separación horizontal entre segmentos (si 0, se intenta inferir del SpriteRenderer.bounds).")]
    public float tileWidth = 0f; // se calculará por segmento si se deja en 0


    [Tooltip("Margen adicional para reciclar a la derecha cuando sale del viewport a la izquierda.")]
    public float recycleMargin = 2f; // unidades


    float _avgWidth = 1f;


    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;


        if (segments == null || segments.Length == 0)
        {
            // Autodescubrir: usa los hijos directos como segmentos
            int childCount = transform.childCount;
            segments = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
                segments[i] = transform.GetChild(i);
        }


        // Ordenarlos por posición X
        segments = segments.OrderBy(t => t.position.x).ToArray();


        // Calcular ancho promedio si no se fijó
        if (tileWidth <= 0f)
        {
            float sum = 0f; int c = 0;
            foreach (var s in segments)
            {
                float w = InferWidth(s);
                if (w > 0f) { sum += w; c++; }
            }
            _avgWidth = (c > 0) ? sum / c : 1f;
        }
        else
        {
            _avgWidth = tileWidth;
        }
    }


    float InferWidth(Transform t)
    {
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) return sr.bounds.size.x;
        var mr = t.GetComponentInChildren<MeshRenderer>();
        if (mr != null) return mr.bounds.size.x;
        return 0f; // fallback
    }


    public void TickInfiniteX()
    {
        if (segments == null || segments.Length == 0) return;
        if (cameraTransform == null) return;


        // Encontrar el más izquierdo y el más derecho
        int leftIdx = 0, rightIdx = 0;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        for (int i = 0; i < segments.Length; i++)
        {
            float x = segments[i].position.x;
            if (x < minX) { minX = x; leftIdx = i; }
            if (x > maxX) { maxX = x; rightIdx = i; }
        }


        // Si el más izquierdo está suficientemente fuera de cámara por la izquierda, lo mandamos a la derecha
        float camX = cameraTransform.position.x;
        float leftEdge = segments[leftIdx].position.x + (-_avgWidth * 0.5f);
        float recycleX = camX - GetViewportWorldHalfWidth() - recycleMargin;


        if (leftEdge < recycleX)
        {
            // Reposicionar a continuación del más derecho
            float newX = segments[rightIdx].position.x + _avgWidth;
            Vector3 p = segments[leftIdx].position;
            segments[leftIdx].position = new Vector3(newX, p.y, p.z);
        }
    }

    float GetViewportWorldHalfWidth()
    {
        var cam = cameraTransform.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam == null) return 10f; // fallback
        float halfHeight = cam.orthographicSize; // ortográfica
        float halfWidth = halfHeight * cam.aspect;
        return halfWidth;
    }
}