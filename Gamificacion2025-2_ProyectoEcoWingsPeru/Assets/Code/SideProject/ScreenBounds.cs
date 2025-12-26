using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ScreenBounds : MonoBehaviour
{
    public Camera cam;
    public Transform top;
    public Transform bottom;
    public Transform left;
    public Transform right;

    [Header("Grosor de los bordes (en unidades del mundo)")]
    public float thickness = 0.5f;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector3 camPos = cam.transform.position;

        //TOP
        top.position = new Vector3(camPos.x, camPos.y + height / 2.1f + thickness / 2f, 0f);
        top.localScale = new Vector3(width, thickness, 1f);

        //BOTTOM
        bottom.position = new Vector3(camPos.x, camPos.y - height / 2.1f - thickness / 2f, 0f);
        bottom.localScale = new Vector3(width, thickness, 1f);

        //LEFT
        left.position = new Vector3(camPos.x - width / 2f - thickness / 3f, camPos.y, 0f);
        left.localScale = new Vector3(thickness, height, 1f);

        //RIGHT
        right.position = new Vector3(camPos.x + width / 2f + thickness / 3f, camPos.y, 0f);
        right.localScale = new Vector3(thickness, height, 1f);
    }
}
