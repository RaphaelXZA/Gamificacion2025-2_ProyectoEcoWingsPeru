using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class SpikeSpawner : MonoBehaviour
{
    public static SpikeSpawner Instance { get; private set; }

    [Header("Walls (BoxCollider2D de los 4 bordes)")]
    public BoxCollider2D topWall;
    public BoxCollider2D bottomWall;
    public BoxCollider2D leftWall;
    public BoxCollider2D rightWall;

    [Header("Prefabs")]
    public GameObject spikePrefab;   //Sprite apuntando hacia ARRIBA
    public GameObject candyPrefab;

    [Header("Espaciado")]
    public float horizontalSpacing = 0.5f;   //distancia entre espinas de arriba/abajo
    public float verticalSpacing = 0.5f;     //distancia entre espinas de izquierda/derecha

    [Header("Caramelos")]
    [Range(0f, 1f)]
    public float candySpawnChance = 0.3f;    //probabilidad de que salga un caramelo en cada rebote

    //Arrays de spikes
    private GameObject[] topSpikes;
    private GameObject[] bottomSpikes;
    private GameObject[] leftSpikes;
    private GameObject[] rightSpikes;

    //Arrays de caramelos para lados
    private GameObject[] leftCandies;
    private GameObject[] rightCandies;

    //Info de slots verticales (para lados)
    private int leftSlotCount;
    private int rightSlotCount;
    private float leftMinY;
    private float rightMinY;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Esperamos un frame para que ScreenBounds acomode los colliders
    private IEnumerator Start()
    {
        yield return null;
        InitSpikes();
    }

    private void InitSpikes()
    {
        //1) Espinas superiores e inferiores (siempre llenas)
        InitHorizontalWall(topWall, true, ref topSpikes);
        InitHorizontalWall(bottomWall, false, ref bottomSpikes);

        //2) Slots en izquierda/derecha (espinas + caramelos)
        InitVerticalWall(leftWall, ref leftSpikes, ref leftCandies, out leftSlotCount, out leftMinY);
        InitVerticalWall(rightWall, ref rightSpikes, ref rightCandies, out rightSlotCount, out rightMinY);
    }

    // ---------------- HORIZONTALES: ARRIBA / ABAJO ----------------
    private void InitHorizontalWall(BoxCollider2D wall, bool isTop, ref GameObject[] spikes)
    {
        if (wall == null || spikePrefab == null)
            return;

        Bounds b = wall.bounds;
        float width = b.size.x;
        int count = Mathf.CeilToInt(width / horizontalSpacing);
        spikes = new GameObject[count];

        float startX = b.min.x;
        float y = isTop ? b.min.y : b.max.y;   //cara interior del collider

        for (int i = 0; i < count; i++)
        {
            float x = startX + horizontalSpacing * 0.5f + i * horizontalSpacing;
            Vector3 pos = new Vector3(x, y, 0f);

            //Spike apunta hacia arriba por defecto:
            //- Top: mirar hacia ABAJO -> 180°
            //- Bottom: hacia ARRIBA -> 0°
            float rotZ = isTop ? 180f : 0f;

            spikes[i] = Instantiate(
                spikePrefab,
                pos,
                Quaternion.Euler(0f, 0f, rotZ),
                transform
            );
        }
    }

    // ---------------- VERTICALES: IZQUIERDA / DERECHA ----------------
    private void InitVerticalWall(
        BoxCollider2D wall,
        ref GameObject[] spikes,
        ref GameObject[] candies,
        out int slotCount,
        out float minYOut)
    {
        slotCount = 0;
        minYOut = 0f;

        if (wall == null || spikePrefab == null)
            return;

        Bounds b = wall.bounds;
        float height = b.size.y;
        slotCount = Mathf.CeilToInt(height / verticalSpacing);
        spikes = new GameObject[slotCount];
        candies = new GameObject[slotCount];

        float minY = b.min.y;
        minYOut = minY;

        bool isLeft = wall == leftWall;
        float x = isLeft ? b.max.x : b.min.x;  //cara interior

        for (int i = 0; i < slotCount; i++)
        {
            float y = minY + verticalSpacing * 0.5f + i * verticalSpacing;
            Vector3 pos = new Vector3(x, y, 0f);

            // Spike apunta hacia arriba:
            //- Izquierda: mirar DERECHA -> -90°
            //- Derecha: mirar IZQUIERDA -> 90°
            float rotZ = isLeft ? -90f : 90f;

            spikes[i] = Instantiate(
                spikePrefab,
                pos,
                Quaternion.Euler(0f, 0f, rotZ),
                transform
            );
            spikes[i].SetActive(false);

            if (candyPrefab != null)
            {
                candies[i] = Instantiate(
                    candyPrefab,
                    pos,
                    Quaternion.identity,
                    transform
                );
                candies[i].SetActive(false);
            }
        }
    }

    // ---------------- API PÚBLICA (llamada desde el pájaro) ----------------
    /// <summary>
    /// Llamar cuando el pájaro rebota en una pared.
    /// direction: +1 si AHORA va hacia la derecha, -1 si AHORA va hacia la izquierda.
    /// birdY: posición Y actual del pájaro.
    /// </summary>
    public void OnBirdBounced(float direction, float birdY)
    {
        if (direction > 0f)
        {
            //Ahora va hacia la DERECHA -> acaba de chocar con la IZQUIERDA
            ClearWall(leftSpikes, leftCandies);  // quitar spikes de la pared tocada
            GenerateVerticalPattern(
                rightSpikes, rightCandies,
                rightSlotCount, rightMinY,
                birdY
            );
        }
        else
        {
            //Ahora va hacia la IZQUIERDA -> acaba de chocar con la DERECHA
            ClearWall(rightSpikes, rightCandies);
            GenerateVerticalPattern(
                leftSpikes, leftCandies,
                leftSlotCount, leftMinY,
                birdY
            );
        }
    }

    private void ClearWall(GameObject[] spikes, GameObject[] candies)
    {
        if (spikes != null)
        {
            for (int i = 0; i < spikes.Length; i++)
                if (spikes[i] != null)
                    spikes[i].SetActive(false);
        }

        if (candies != null)
        {
            for (int i = 0; i < candies.Length; i++)
                if (candies[i] != null)
                    candies[i].SetActive(false);
        }
    }

    // ---------------- PATRÓN DE GRUPOS 2–3 SPIKES + ESPACIO ----------------
    private void GenerateVerticalPattern(
        GameObject[] spikes, GameObject[] candies,
        int slotCount, float minY,
        float birdY)
    {
        if (spikes == null || slotCount <= 0)
            return;

        //1) Grupos de spikes (2–3) + gaps (1–2)
        bool[] hasSpike = new bool[slotCount];

        int i = 0;
        while (i < slotCount)
        {
            //grupo de spikes
            int spikesLen = Random.Range(2, 4); // 2 o 3
            for (int k = 0; k < spikesLen && i < slotCount; k++, i++)
            {
                hasSpike[i] = true;
            }

            //gap
            int gapLen = Random.Range(1, 3); // 1 o 2 espacios
            i += gapLen; // quedan en false -> huecos
        }

        //2) Garantizar un hueco cerca del pájaro
        int safeIndex = GetNearestSlotIndex(birdY, minY, verticalSpacing, slotCount);
        hasSpike[safeIndex] = false;

        //3) Aplicar al mundo (activar / desactivar)
        List<int> gapIndices = new List<int>();

        for (int idx = 0; idx < slotCount; idx++)
        {
            bool activeSpike = hasSpike[idx];

            if (spikes[idx] != null)
                spikes[idx].SetActive(activeSpike);

            if (candies != null && idx < candies.Length && candies[idx] != null)
                candies[idx].SetActive(false);

            if (!activeSpike)
                gapIndices.Add(idx);
        }

        //4) Caramelo en uno de los huecos (a veces)
        if (candies != null && gapIndices.Count > 0 && Random.value < candySpawnChance)
        {
            int chosenGap = gapIndices[Random.Range(0, gapIndices.Count)];
            GameObject candy = candies[chosenGap];

            if (candy == null && candyPrefab != null)
            {
                float y = minY + verticalSpacing * 0.5f + chosenGap * verticalSpacing;
                float x = spikes[chosenGap] != null
                    ? spikes[chosenGap].transform.position.x
                    : 0f;

                Vector3 pos = new Vector3(x, y, 0f);
                candy = Instantiate(candyPrefab, pos, Quaternion.identity, transform);
                candies[chosenGap] = candy;
            }

            if (candy != null)
                candy.SetActive(true);
        }
    }

    private int GetNearestSlotIndex(float y, float minY, float spacing, int slotCount)
    {
        float maxY = minY + spacing * slotCount;
        float clamped = Mathf.Clamp(y, minY, maxY);
        float offset = clamped - (minY + spacing * 0.5f);
        int index = Mathf.RoundToInt(offset / spacing);
        return Mathf.Clamp(index, 0, slotCount - 1);
    }
}
