using UnityEngine;

public class ParallaxManagerAuto : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform root; // Contenedor de la capa (que agrupa sus tiles)
        [Range(0f, 1f)] public float factor = 0.5f; // 0 = casi estático (fondo), 1 = casi primer plano
        public bool lockY = true; // Si el parallax solo es en X
        public bool infiniteX = true; // Si hace loop infinito en X
        public ParallaxLayerTiler tiler; // Referencia al tiler de esta capa (opcional, si infiniteX)
    }


    [Header("Velocidad del mundo (X)")]
    [Tooltip("Velocidad base hacia la izquierda (>0) para simular que el mundo viene hacia el jugador.")]
    public float baseWorldSpeed = 6f; // unidades/s; se moverán hacia -X


    [Tooltip("Factor global para escalar todas las capas (por ejemplo, para boosts o slow-motion).")]
    public float globalMultiplier = 1f;


    [Header("Cámara")]
    public Camera targetCamera; // No es estrictamente necesaria si la cámara es estática, pero ayuda al Tiler


    [Header("Capas (de fondo a frente)")]
    public Layer[] layers = new Layer[6];


    void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;


        // Si el usuario olvidó conectar un tiler pero infiniteX está activo, tratamos de localizarlo automáticamente
        foreach (var l in layers)
        {
            if (l == null || l.root == null) continue;
            if (l.infiniteX && l.tiler == null)
            {
                l.tiler = l.root.GetComponent<ParallaxLayerTiler>();
            }
            if (l.tiler != null)
            {
                l.tiler.cameraTransform = targetCamera ? targetCamera.transform : null;
            }
        }
    }


    void Update()
    {
        float signedSpeed = -Mathf.Abs(baseWorldSpeed) * globalMultiplier; // negativo = mueve a la izquierda
        float dt = Time.deltaTime;


        foreach (var l in layers)
        {
            if (l == null || l.root == null) continue;


            // Movimiento automático por parallax
            float xMove = signedSpeed * l.factor * dt;
            float yMove = 0f;
            if (!l.lockY)
            {
                // Si quisieras scroll vertical, podrías añadir otro parámetro de velocidad Y
                yMove = 0f;
            }
            l.root.position += new Vector3(xMove, yMove, 0f);


            // Bucle infinito de tiles
            if (l.infiniteX && l.tiler != null)
            {
                l.tiler.TickInfiniteX();
            }
        }
    }
}