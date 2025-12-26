using UnityEngine;

public class ParallaxAutoScrollLoose : MonoBehaviour
{
    [Range(0f, 1f)] public float parallaxFactor = 0.5f; // 0=fijo, 1=igual que hazards
    public float localSpeed = 4f;

    [Tooltip("Asigna los 3 sprites de la capa, de izquierda a derecha")]
    public Transform[] segments; // tamaño 3
    float segmentWidth;

    void Start()
    {
        var sr = segments[0].GetComponent<SpriteRenderer>();
        segmentWidth = sr.bounds.size.x; // considera escala
    }

    void Update()
    {
        float baseSpeed = localSpeed;
        float speed = baseSpeed * parallaxFactor;
        Vector3 delta = Vector3.left * speed * Time.deltaTime;

        for (int i = 0; i < segments.Length; i++)
            segments[i].position += delta;

        // recicla el de la izquierda cuando sale completo
        Transform leftMost = segments[0];
        Transform rightMost = segments[segments.Length - 1];

        if (leftMost.position.x <= rightMost.position.x - segmentWidth)
        {
            leftMost.position = new Vector3(rightMost.position.x + segmentWidth,
                                            leftMost.position.y, leftMost.position.z);
            for (int i = 0; i < segments.Length - 1; i++)
                segments[i] = segments[i + 1];
            segments[segments.Length - 1] = leftMost;
        }
    }
}
