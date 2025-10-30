using UnityEngine;

/// Parallax estable: scroll hacia la IZQUIERDA con loop sin desbordes.
/// Coloca el 1º en cámara y dos pegados a la derecha. Asigna 3+ segmentos.
public class ParalaxAutomatic : MonoBehaviour
{
    [Header("Segmentos (mín 3)")]
    public Transform[] segments;

    [Header("Movimiento")]
    [Tooltip("Unidades/seg a la IZQUIERDA antes de aplicar parallaxFactor.")]
    public float baseSpeed = 6f;
    [Range(0f, 1f), Tooltip("0=fijo, 1=igual que hazards")]
    public float parallaxFactor = 0.5f;

    [Header("Colocación (opcional)")]
    public bool autoArrange = true;
    public bool snapFirstToCamera = false; // solo cámaras ortográficas
    public Camera cam;

    [Header("Ajustes")]
    [Tooltip("Solape para evitar líneas entre segmentos (en unidades de mundo).")]
    public float tinyOverlap = 0.001f;

    float segmentWidth;       // ancho en mundo
    float offset;             // 0..segmentWidth
    float baseX;              // ancla estable para recalcular
    float y0, z0;             // altura y profundidad fijas
    bool ready;

    void Awake() { if (cam == null) cam = Camera.main; }

    void Start()
    {
        if (segments == null || segments.Length < 3)
        {
            Debug.LogError("[ParalaxAutomatic] Asigna al menos 3 segmentos.");
            enabled = false; return;
        }

        // Ordenar izquierda→derecha para medir correctamente
        System.Array.Sort(segments, (a, b) => a.position.x.CompareTo(b.position.x));

        // Medir ancho del sprite (considerando escala)
        var sr = segments[0].GetComponent<SpriteRenderer>();
        if (!sr) { Debug.LogError("[ParalaxAutomatic] Cada segmento necesita SpriteRenderer."); enabled = false; return; }

        // Cálculo robusto del ancho
        segmentWidth = sr.sprite.bounds.size.x * segments[0].lossyScale.x;
        if (segmentWidth <= 0f)
        {
            Debug.LogError("[ParalaxAutomatic] El ancho del segmento es 0. Revisa escala/sprite.");
            enabled = false; return;
        }

        // Estado base
        y0 = segments[0].position.y;
        z0 = segments[0].position.z;

        if (autoArrange)
        {
            // opcional: pegar el primero al borde izquierdo de la cámara
            if (snapFirstToCamera && cam && cam.orthographic)
            {
                float left = cam.transform.position.x - cam.orthographicSize * cam.aspect;
                float halfW = segmentWidth * 0.5f;
                segments[0].position = new Vector3(left + halfW, y0, z0);
            }

            // poner 2º y 3º a +ancho y +2×ancho
            for (int i = 1; i < segments.Length; i++)
            {
                float x = segments[0].position.x + i * (segmentWidth - tinyOverlap);
                segments[i].position = new Vector3(x, y0, z0);
            }
        }

        // ancla estable
        baseX = segments[0].position.x;
        // normalizar Y/Z de todos por si venían distintos
        for (int i = 0; i < segments.Length; i++)
            segments[i].position = new Vector3(segments[i].position.x, y0, z0);

        ready = true;
    }

    void Update()
    {
        if (!ready) return;

        // Avanza offset (izquierda) y envuélvelo con Repeat
        float speed = Mathf.Max(0f, baseSpeed) * Mathf.Clamp01(parallaxFactor);
        offset += speed * Time.deltaTime;

        // Mantener offset en [0, segmentWidth)
        if (offset >= segmentWidth) offset = Mathf.Repeat(offset, segmentWidth);

        // Reposicionar determinísticamente cada segmento:
        // el 0 siempre cerca de baseX, los demás a +i*ancho - offset
        float step = segmentWidth - tinyOverlap;
        for (int i = 0; i < segments.Length; i++)
        {
            float x = baseX - offset + i * step;
            segments[i].position = new Vector3(x, y0, z0);
        }
    }
}
