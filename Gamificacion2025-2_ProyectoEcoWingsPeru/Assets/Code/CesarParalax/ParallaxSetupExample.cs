using UnityEngine;

public class ParallaxSetupExample : MonoBehaviour
{
    public ParallaxManagerAuto manager;


    void Reset()
    {
        if (manager == null) manager = FindAnyObjectByType<ParallaxManagerAuto>();
    }


    void Start()
    {
        if (manager == null) return;
        // Asigna factores sugeridos de fondo a frente
        float[] factors = { 0.1f, 0.2f, 0.35f, 0.5f, 0.7f, 0.9f };
        for (int i = 0; i < manager.layers.Length && i < factors.Length; i++)
        {
            if (manager.layers[i] == null) manager.layers[i] = new ParallaxManagerAuto.Layer();
            manager.layers[i].factor = factors[i];
            manager.layers[i].infiniteX = true;
            manager.layers[i].lockY = true;
        }
    }
}