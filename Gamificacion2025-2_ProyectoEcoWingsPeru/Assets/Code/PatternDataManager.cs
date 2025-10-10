using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

// Clases serializables para JSON
[System.Serializable]
public class SerializablePattern
{
    public List<Vector2> normalizedPoints;
    public List<Vector2> directions;
    public float totalDistance;
    public int pointCount;

    public SerializablePattern(DrawingPattern pattern)
    {
        normalizedPoints = new List<Vector2>(pattern.normalizedPoints);
        directions = new List<Vector2>(pattern.directions);
        totalDistance = pattern.totalDistance;
        pointCount = pattern.pointCount;
    }

    public DrawingPattern ToDrawingPattern()
    {
        DrawingPattern pattern = new DrawingPattern(new List<Vector3>());
        pattern.normalizedPoints = new List<Vector2>(normalizedPoints);
        pattern.directions = new List<Vector2>(directions);
        pattern.totalDistance = totalDistance;
        pattern.pointCount = pointCount;
        return pattern;
    }
}

[System.Serializable]
public class SerializableSymbol
{
    public string symbolName;
    public List<SerializablePattern> patterns;
    public int maxPatterns;

    public SerializableSymbol(SymbolData symbolData)
    {
        symbolName = symbolData.symbolName;
        maxPatterns = symbolData.maxPatterns;
        patterns = new List<SerializablePattern>();

        foreach (var pattern in symbolData.patterns)
        {
            patterns.Add(new SerializablePattern(pattern));
        }
    }

    public SymbolData ToSymbolData()
    {
        SymbolData symbolData = new SymbolData(symbolName);
        symbolData.maxPatterns = maxPatterns;
        symbolData.patterns = new List<DrawingPattern>();

        foreach (var serPattern in patterns)
        {
            symbolData.patterns.Add(serPattern.ToDrawingPattern());
        }

        return symbolData;
    }
}

[System.Serializable]
public class SymbolDatabase
{
    public List<SerializableSymbol> symbols;

    public SymbolDatabase()
    {
        symbols = new List<SerializableSymbol>();
    }
}

public class PatternDataManager : MonoBehaviour
{
    private static PatternDataManager instance;
    public static PatternDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PatternDataManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PatternDataManager");
                    instance = go.AddComponent<PatternDataManager>();
                }
            }
            return instance;
        }
    }

    [Header("Development Settings")]
    [SerializeField] private bool useProjectFolder = true; // Cambiar a false para testing local
    [SerializeField] private string projectFolderPath = "Assets/Resources/SymbolData"; // Carpeta en el proyecto

    private string SaveFilePath
    {
        get
        {
            if (useProjectFolder)
            {
                // Guarda en la carpeta del proyecto (versionable en Git)
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "Resources/SymbolData/symbol_patterns.json");
#else
                // En build, carga desde Resources
                return Path.Combine(Application.streamingAssetsPath, "symbol_patterns.json");
#endif
            }
            else
            {
                // Guarda en persistentDataPath (para testing individual)
                return Path.Combine(Application.persistentDataPath, "symbol_patterns.json");
            }
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Guardar base de datos completa
    public void SaveDatabase(Dictionary<string, SymbolData> symbolDatabase)
    {
        SymbolDatabase database = new SymbolDatabase();

        foreach (var kvp in symbolDatabase)
        {
            database.symbols.Add(new SerializableSymbol(kvp.Value));
        }

        string json = JsonUtility.ToJson(database, true);

        try
        {
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"Base de datos guardada en: {SaveFilePath}");
            Debug.Log($"S�mbolos guardados: {database.symbols.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar: {e.Message}");
        }
    }

    // Cargar base de datos completa
    public Dictionary<string, SymbolData> LoadDatabase()
    {
        Dictionary<string, SymbolData> symbolDatabase = new Dictionary<string, SymbolData>();

        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("No existe archivo de guardado previo. Creando nueva base de datos.");
            return symbolDatabase;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SymbolDatabase database = JsonUtility.FromJson<SymbolDatabase>(json);

            if (database != null && database.symbols != null)
            {
                foreach (var serSymbol in database.symbols)
                {
                    SymbolData symbolData = serSymbol.ToSymbolData();
                    symbolDatabase[symbolData.symbolName] = symbolData;
                }

                Debug.Log($"Base de datos cargada: {symbolDatabase.Count} s�mbolos");
                foreach (var kvp in symbolDatabase)
                {
                    Debug.Log($"  - '{kvp.Key}': {kvp.Value.patterns.Count} patrones");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar base de datos: {e.Message}");
        }

        return symbolDatabase;
    }

    // Guardar un �nico s�mbolo (�til para actualizaciones incrementales)
    public void SaveSingleSymbol(string symbolName, SymbolData symbolData)
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        database[symbolName] = symbolData;
        SaveDatabase(database);
    }

    // Eliminar un s�mbolo
    public void DeleteSymbol(string symbolName)
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        if (database.ContainsKey(symbolName))
        {
            database.Remove(symbolName);
            SaveDatabase(database);
            Debug.Log($"S�mbolo '{symbolName}' eliminado del guardado");
        }
    }

    // Borrar toda la base de datos
    public void ClearAllData()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("Base de datos eliminada completamente");
        }
    }

    // Obtener informaci�n de la base de datos sin cargarla completamente
    public List<string> GetSavedSymbolNames()
    {
        if (!File.Exists(SaveFilePath))
            return new List<string>();

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SymbolDatabase database = JsonUtility.FromJson<SymbolDatabase>(json);

            if (database != null && database.symbols != null)
            {
                return database.symbols.Select(s => s.symbolName).ToList();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al leer nombres de s�mbolos: {e.Message}");
        }

        return new List<string>();
    }

    // Verificar si existe un s�mbolo guardado
    public bool HasSymbol(string symbolName)
    {
        return GetSavedSymbolNames().Contains(symbolName);
    }

    // Exportar base de datos a texto legible (para debug)
    public string ExportToReadableText()
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        string output = "=== BASE DE DATOS DE S�MBOLOS ===\n\n";

        foreach (var kvp in database)
        {
            output += $"S�mbolo: {kvp.Key}\n";
            output += $"Patrones: {kvp.Value.patterns.Count}\n";

            for (int i = 0; i < kvp.Value.patterns.Count; i++)
            {
                var pattern = kvp.Value.patterns[i];
                output += $"  Patr�n {i + 1}: {pattern.pointCount} puntos, distancia: {pattern.totalDistance:F2}\n";
            }
            output += "\n";
        }

        return output;
    }
}