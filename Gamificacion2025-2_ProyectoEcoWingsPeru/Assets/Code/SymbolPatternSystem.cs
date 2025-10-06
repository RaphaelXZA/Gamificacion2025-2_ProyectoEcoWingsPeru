using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[System.Serializable]
public class DrawingPattern
{
    public List<Vector2> normalizedPoints; // Puntos normalizados (0-1)
    public List<Vector2> directions; // Direcciones vectoriales
    public float totalDistance;
    public int pointCount;

    public DrawingPattern(List<Vector3> rawPoints)
    {
        normalizedPoints = new List<Vector2>();
        directions = new List<Vector2>();

        if (rawPoints.Count < 2) return;

        // Normalizar puntos a un espacio 0-1
        NormalizePoints(rawPoints);

        // Calcular direcciones entre puntos consecutivos
        CalculateDirections();

        pointCount = normalizedPoints.Count;
    }

    private void NormalizePoints(List<Vector3> rawPoints)
    {
        if (rawPoints.Count == 0) return;

        // Encontrar bounds
        float minX = rawPoints.Min(p => p.x);
        float maxX = rawPoints.Max(p => p.x);
        float minY = rawPoints.Min(p => p.y);
        float maxY = rawPoints.Max(p => p.y);

        float width = maxX - minX;
        float height = maxY - minY;

        // Evitar división por cero
        if (width < 0.001f) width = 0.001f;
        if (height < 0.001f) height = 0.001f;

        totalDistance = 0f;

        foreach (var point in rawPoints)
        {
            Vector2 normalizedPoint = new Vector2(
                (point.x - minX) / width,
                (point.y - minY) / height
            );
            normalizedPoints.Add(normalizedPoint);
        }

        // Calcular distancia total
        for (int i = 1; i < normalizedPoints.Count; i++)
        {
            totalDistance += Vector2.Distance(normalizedPoints[i - 1], normalizedPoints[i]);
        }
    }

    private void CalculateDirections()
    {
        for (int i = 1; i < normalizedPoints.Count; i++)
        {
            Vector2 direction = (normalizedPoints[i] - normalizedPoints[i - 1]).normalized;
            if (!float.IsNaN(direction.x) && !float.IsNaN(direction.y))
            {
                directions.Add(direction);
            }
        }
    }
}

[System.Serializable]
public class SymbolData
{
    public string symbolName;
    public List<DrawingPattern> patterns;
    public int maxPatterns = 10; // Máximo de patrones por símbolo

    public SymbolData(string name)
    {
        symbolName = name;
        patterns = new List<DrawingPattern>();
    }

    public void AddPattern(DrawingPattern pattern)
    {
        patterns.Add(pattern);

        // Mantener solo los patrones más recientes
        if (patterns.Count > maxPatterns)
        {
            patterns.RemoveAt(0);
        }
    }

    public float CompareWithPattern(DrawingPattern inputPattern)
    {
        if (patterns.Count == 0) return 0f;

        float bestScore = 0f;

        foreach (var storedPattern in patterns)
        {
            float score = ComparePatterns(inputPattern, storedPattern);
            if (score > bestScore)
            {
                bestScore = score;
            }
        }

        return bestScore;
    }

    private float ComparePatterns(DrawingPattern pattern1, DrawingPattern pattern2)
    {
        float shapeScore = CompareShapes(pattern1.normalizedPoints, pattern2.normalizedPoints);
        float directionScore = CompareDirections(pattern1.directions, pattern2.directions);
        float lengthScore = CompareLengths(pattern1.totalDistance, pattern2.totalDistance);

        // Peso combinado
        return (shapeScore * 0.5f) + (directionScore * 0.3f) + (lengthScore * 0.2f);
    }

    private float CompareShapes(List<Vector2> points1, List<Vector2> points2)
    {
        if (points1.Count == 0 || points2.Count == 0) return 0f;

        // Resample both patterns to same point count for comparison
        int targetPoints = Mathf.Min(20, Mathf.Max(points1.Count, points2.Count));
        List<Vector2> resampled1 = ResamplePath(points1, targetPoints);
        List<Vector2> resampled2 = ResamplePath(points2, targetPoints);

        float totalDistance = 0f;
        for (int i = 0; i < targetPoints; i++)
        {
            totalDistance += Vector2.Distance(resampled1[i], resampled2[i]);
        }

        // Normalizar score (menor distancia = mayor similitud)
        float avgDistance = totalDistance / targetPoints;
        return Mathf.Clamp01(1f - (avgDistance * 2f)); // Ajustar multiplicador según necesidad
    }

    private List<Vector2> ResamplePath(List<Vector2> points, int targetCount)
    {
        if (points.Count <= 1) return points;

        List<Vector2> resampled = new List<Vector2>();
        float totalLength = 0f;

        // Calcular longitud total
        for (int i = 1; i < points.Count; i++)
        {
            totalLength += Vector2.Distance(points[i - 1], points[i]);
        }

        if (totalLength == 0f) return points;

        float segmentLength = totalLength / (targetCount - 1);
        resampled.Add(points[0]);

        float currentLength = 0f;
        int currentIndex = 0;

        for (int i = 1; i < targetCount - 1; i++)
        {
            float targetLength = segmentLength * i;

            while (currentIndex < points.Count - 1 && currentLength < targetLength)
            {
                currentLength += Vector2.Distance(points[currentIndex], points[currentIndex + 1]);
                currentIndex++;
            }

            if (currentIndex >= points.Count - 1)
            {
                resampled.Add(points[points.Count - 1]);
            }
            else
            {
                // Interpolar entre puntos
                float excess = currentLength - targetLength;
                float segLen = Vector2.Distance(points[currentIndex - 1], points[currentIndex]);
                float t = segLen > 0 ? (segLen - excess) / segLen : 0;
                Vector2 interpolated = Vector2.Lerp(points[currentIndex - 1], points[currentIndex], t);
                resampled.Add(interpolated);
            }
        }

        resampled.Add(points[points.Count - 1]);
        return resampled;
    }

    private float CompareDirections(List<Vector2> dirs1, List<Vector2> dirs2)
    {
        if (dirs1.Count == 0 || dirs2.Count == 0) return 0f;

        int minCount = Mathf.Min(dirs1.Count, dirs2.Count);
        float totalSimilarity = 0f;

        for (int i = 0; i < minCount; i++)
        {
            float dot = Vector2.Dot(dirs1[i], dirs2[i]);
            totalSimilarity += (dot + 1f) / 2f; // Convertir de [-1,1] a [0,1]
        }

        return totalSimilarity / minCount;
    }

    private float CompareLengths(float length1, float length2)
    {
        if (length1 == 0f || length2 == 0f) return 0f;

        float ratio = Mathf.Min(length1, length2) / Mathf.Max(length1, length2);
        return ratio;
    }
}

public class SymbolPatternSystem : MonoBehaviour
{
    [Header("Pattern Settings")]
    [SerializeField] private float matchThreshold = 0.7f; // Umbral para considerar una coincidencia
    [SerializeField] private bool isLearningMode = false; // Modo aprendizaje vs gameplay
    [SerializeField] private string currentLearningSymbol = ""; // Símbolo actual en modo aprendizaje

    private Dictionary<string, SymbolData> symbolDatabase;
    private DrawingSystem drawingSystem;

    // Events para notificar matches
    public event Action<string, float> OnSymbolMatched;
    public event Action<string> OnPatternLearned;

    void Start()
    {
        symbolDatabase = new Dictionary<string, SymbolData>();
        drawingSystem = FindFirstObjectByType<DrawingSystem>();

        if (drawingSystem == null)
        {
            Debug.LogError("DrawingSystem no encontrado!");
        }

        LoadSymbolDatabase(); // Cargar patrones guardados
    }

    void Update()
    {
        // Verificar si se completó un dibujo
        if (drawingSystem != null && drawingSystem.HasNewPattern())
        {
            ProcessLastDrawing();
        }
    }

    private void ProcessLastDrawing()
    {
        var points = drawingSystem.GetLastCompletedPattern();
        if (points.Count < 2) return;

        DrawingPattern newPattern = new DrawingPattern(points);

        if (isLearningMode && !string.IsNullOrEmpty(currentLearningSymbol))
        {
            LearnPattern(currentLearningSymbol, newPattern);
        }
        else
        {
            CheckForMatches(newPattern);
        }
    }

    public void LearnPattern(string symbolName, DrawingPattern pattern)
    {
        if (!symbolDatabase.ContainsKey(symbolName))
        {
            symbolDatabase[symbolName] = new SymbolData(symbolName);
        }

        symbolDatabase[symbolName].AddPattern(pattern);

        Debug.Log($"Patrón aprendido para símbolo '{symbolName}'. Total patrones: {symbolDatabase[symbolName].patterns.Count}");
        Debug.Log($"Puntos del patrón: {pattern.pointCount}, Distancia total: {pattern.totalDistance:F3}");

        OnPatternLearned?.Invoke(symbolName);
        SaveSymbolDatabase();
    }

    public void CheckForMatches(DrawingPattern inputPattern)
    {
        if (symbolDatabase.Count == 0)
        {
            Debug.Log("No hay símbolos entrenados en la base de datos");
            return;
        }

        Debug.Log($"Comparando patrón con {symbolDatabase.Count} símbolos entrenados...");

        string bestMatch = "";
        float bestScore = 0f;

        foreach (var kvp in symbolDatabase)
        {
            float score = kvp.Value.CompareWithPattern(inputPattern);
            Debug.Log($"Símbolo '{kvp.Key}': {score:P2} similitud");

            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = kvp.Key;
            }
        }

        Debug.Log($"Mejor match: {bestMatch} con {bestScore:P2} (Umbral: {matchThreshold:P2})");

        if (bestScore >= matchThreshold)
        {
            Debug.Log($"¡Símbolo detectado: {bestMatch} (Precisión: {bestScore:P1})");
            OnSymbolMatched?.Invoke(bestMatch, bestScore);
        }
        else
        {
            Debug.Log($"No se detectó ningún símbolo conocido. Mejor match: {bestMatch} ({bestScore:P1})");
        }
    }

    // Métodos públicos para controlar el sistema
    public void StartLearningMode(string symbolName)
    {
        isLearningMode = true;
        currentLearningSymbol = symbolName;
        Debug.Log($"Modo aprendizaje iniciado para símbolo: {symbolName}");
    }

    public void StopLearningMode()
    {
        isLearningMode = false;
        currentLearningSymbol = "";
        Debug.Log("Modo aprendizaje desactivado");
    }

    public void StartGameplayMode()
    {
        isLearningMode = false;
        Debug.Log("Modo gameplay activado");
    }

    public List<string> GetLearnedSymbols()
    {
        if (symbolDatabase == null)
            return new List<string>();

        return new List<string>(symbolDatabase.Keys);
    }

    public int GetPatternCount(string symbolName)
    {
        if (symbolDatabase == null || !symbolDatabase.ContainsKey(symbolName))
            return 0;

        return symbolDatabase[symbolName].patterns.Count;
    }

    public void ClearSymbol(string symbolName)
    {
        if (symbolDatabase.ContainsKey(symbolName))
        {
            symbolDatabase.Remove(symbolName);
            SaveSymbolDatabase();
            Debug.Log($"Símbolo '{symbolName}' eliminado de la base de datos");
        }
    }

    // Método de debug para verificar el estado del sistema
    public void DebugSystemState()
    {
        Debug.Log($"=== ESTADO DEL SISTEMA ===");
        Debug.Log($"Modo aprendizaje: {isLearningMode}");
        Debug.Log($"Símbolo actual: {currentLearningSymbol}");

        if (symbolDatabase == null)
        {
            Debug.Log($"ERROR: symbolDatabase es null!");
            return;
        }

        Debug.Log($"Símbolos entrenados: {symbolDatabase.Count}");
        Debug.Log($"Umbral de detección: {matchThreshold:P2}");

        foreach (var kvp in symbolDatabase)
        {
            Debug.Log($"  - '{kvp.Key}': {kvp.Value.patterns.Count} patrones");
        }
        Debug.Log($"========================");
    }

    private void SaveSymbolDatabase()
    {
        // Guardado simple usando PlayerPrefs (para prototipo)
        foreach (var kvp in symbolDatabase)
        {
            string key = $"Symbol_{kvp.Key}_Count";
            PlayerPrefs.SetInt(key, kvp.Value.patterns.Count);

            // Guardar algunos datos básicos de patrones
            for (int i = 0; i < kvp.Value.patterns.Count; i++)
            {
                string patternKey = $"Symbol_{kvp.Key}_Pattern_{i}_Distance";
                PlayerPrefs.SetFloat(patternKey, kvp.Value.patterns[i].totalDistance);
            }
        }
        PlayerPrefs.Save();
    }

    private void LoadSymbolDatabase()
    {
        // Carga básica (expandir según necesidades)
        Debug.Log("Base de datos de símbolos cargada");
    }
}