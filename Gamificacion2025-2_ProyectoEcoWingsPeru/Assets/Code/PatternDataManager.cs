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
                instance = FindAnyObjectByType<PatternDataManager>();
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
    [SerializeField] private bool useProjectFolder = true; // Cambiar a false para
     [SerializeField] private string projectFolderPath = "Assets/Resources/SymbolData";
    // Carpeta en el proyecto

    private string SaveFilePath
    {
        get
        {
            if (useProjectFolder)
            {
                // Guarda en la carpeta del proyecto (versionable en Git)
#if UNITY_EDITOR
                return Path.Combine(Application.dataPath,
"Resources/SymbolData/symbol_patterns.json");
#else
                // En build, carga desde Resources
                return Path.Combine(Application.streamingAssetsPath,
"symbol_patterns.json");
#endif
            }
            else
            {
                // Guarda en persistentDataPath (para testing individual)
                return Path.Combine(Application.persistentDataPath,
"symbol_patterns.json");
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
#if UNITY_EDITOR
            // En el editor, asegurarse de que la carpeta existe
            string directory = Path.GetDirectoryName(SaveFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"Carpeta creada: {directory}");
            }
#endif

            File.WriteAllText(SaveFilePath, json);

#if UNITY_EDITOR
            // Refrescar el AssetDatabase para que Unity detecte el cambio
            UnityEditor.AssetDatabase.Refresh();
#endif

            Debug.Log($"✅ Base de datos guardada en: {SaveFilePath}");
            Debug.Log($"📊 Símbolos guardados: {database.symbols.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al guardar: {e.Message}");
        }
    }

    // Cargar base de datos completa
    public Dictionary<string, SymbolData> LoadDatabase()
    {
        Dictionary<string, SymbolData> symbolDatabase = new Dictionary<string,
       SymbolData>();
        string json = null;

#if UNITY_EDITOR
        // En el editor, cargar desde la carpeta del proyecto
        string filePath = SaveFilePath;

        if (!File.Exists(filePath))
        {
            Debug.Log("⚠ No existe archivo de guardado previo. Creando nueva base de datos.");
       
            return symbolDatabase;
        }

        try
        {
            json = File.ReadAllText(filePath);
            Debug.Log($"✅ Cargando desde Editor: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar en Editor: {e.Message}");
            return symbolDatabase;
        }
#else
        // En build, cargar desde Resources (sin extensión .json)
        TextAsset jsonFile = Resources.Load<TextAsset>("SymbolData/symbol_patterns");
       
        if (jsonFile != null)
        {
            json = jsonFile.text;
            Debug.Log($"✅ Cargando desde Resources en Build");
        }
        else
        {
            Debug.LogError("❌ No se encontró symbol_patterns en Resources. Asegúrate de que el archivo esté en Assets/Resources/SymbolData/");
            return symbolDatabase;
        }
#endif

        // Parsear el JSON (común para ambos casos)
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                SymbolDatabase database = JsonUtility.FromJson<SymbolDatabase>(json);

                if (database != null && database.symbols != null)
                {
                    foreach (var serSymbol in database.symbols)
                    {
                        SymbolData symbolData = serSymbol.ToSymbolData();
                        symbolDatabase[symbolData.symbolName] = symbolData;
                    }

                    Debug.Log($"✅ Base de datos cargada: {symbolDatabase.Count} símbolos");
                    foreach (var kvp in symbolDatabase)
                    {
                        Debug.Log($"  - '{kvp.Key}': {kvp.Value.patterns.Count} patrones");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠ El archivo JSON está vacío o mal formado");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error al parsear JSON: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No se pudo cargar el JSON");
        }

        return symbolDatabase;
    }

    // Guardar un único símbolo (útil para actualizaciones incrementales)
    public void SaveSingleSymbol(string symbolName, SymbolData symbolData)
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        database[symbolName] = symbolData;
        SaveDatabase(database);
    }

    // Eliminar un símbolo
    public void DeleteSymbol(string symbolName)
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        if (database.ContainsKey(symbolName))
        {
            database.Remove(symbolName);
            SaveDatabase(database);
            Debug.Log($"Símbolo '{symbolName}' eliminado del guardado");
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

    // Obtener información de la base de datos sin cargarla completamente
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
            Debug.LogError($"Error al leer nombres de símbolos: {e.Message}");
        }

        return new List<string>();
    }

    // Verificar si existe un símbolo guardado
    public bool HasSymbol(string symbolName)
    {
        return GetSavedSymbolNames().Contains(symbolName);
    }

    // Exportar base de datos a texto legible (para debug)
    public string ExportToReadableText()
    {
        Dictionary<string, SymbolData> database = LoadDatabase();
        string output = "=== BASE DE DATOS DE SÍMBOLOS ===\n\n";

        foreach (var kvp in database)
        {
            output += $"Símbolo: {kvp.Key}\n";
            output += $"Patrones: {kvp.Value.patterns.Count}\n";

            for (int i = 0; i < kvp.Value.patterns.Count; i++)
            {
                var pattern = kvp.Value.patterns[i];
                output += $"  Patrón {i + 1}: {pattern.pointCount} puntos, distancia: {pattern.totalDistance:F2}\n";
            }
            output += "\n";
        }

        return output;
    }
}