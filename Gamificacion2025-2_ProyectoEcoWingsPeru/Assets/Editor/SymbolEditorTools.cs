#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class SymbolEditorTools : EditorWindow
{
    [MenuItem("Tools/Symbol Pattern Manager")]
    public static void ShowWindow()
    {
        GetWindow<SymbolEditorTools>("Symbol Manager");
    }

    private Vector2 scrollPosition;

    void OnGUI()
    {
        GUILayout.Label("Symbol Pattern Manager", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Información del archivo
        string filePath = Path.Combine(Application.dataPath, "Resources/SymbolData/symbol_patterns.json");
        bool fileExists = File.Exists(filePath);

        EditorGUILayout.HelpBox(
            $"Archivo: {filePath}\n" +
            $"Estado: {(fileExists ? "✅ Existe" : "❌ No existe")}",
            fileExists ? MessageType.Info : MessageType.Warning
        );

        GUILayout.Space(10);

        // Botón para crear carpeta
        if (!Directory.Exists(Path.Combine(Application.dataPath, "Resources/SymbolData")))
        {
            if (GUILayout.Button("📁 Crear Carpeta Resources/SymbolData", GUILayout.Height(30)))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Resources/SymbolData"));
                AssetDatabase.Refresh();
                Debug.Log("✅ Carpeta creada: Assets/Resources/SymbolData");
            }
            GUILayout.Space(10);
        }

        // Botón para abrir carpeta
        if (GUILayout.Button("📂 Abrir Carpeta en Explorador", GUILayout.Height(30)))
        {
            string folderPath = Path.Combine(Application.dataPath, "Resources/SymbolData");
            if (Directory.Exists(folderPath))
            {
                EditorUtility.RevealInFinder(folderPath);
            }
            else
            {
                Debug.LogWarning("La carpeta no existe aún");
            }
        }

        GUILayout.Space(10);

        if (!fileExists)
        {
            EditorGUILayout.HelpBox(
                "No hay símbolos guardados todavía.\n" +
                "Ve a la escena Training y entrena algunos símbolos.",
                MessageType.Info
            );
            return;
        }

        GUILayout.Space(10);

        // Mostrar contenido del archivo
        if (GUILayout.Button("🔍 Ver Símbolos Guardados", GUILayout.Height(30)))
        {
            if (PatternDataManager.Instance != null)
            {
                string info = PatternDataManager.Instance.ExportToReadableText();
                Debug.Log(info);
                EditorUtility.DisplayDialog("Símbolos Guardados", info, "OK");
            }
            else
            {
                Debug.LogWarning("PatternDataManager no encontrado. Asegúrate de tener la escena abierta.");
            }
        }

        GUILayout.Space(10);

        // Botones de utilidad
        EditorGUILayout.LabelField("Acciones Peligrosas", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("⚠️ Recargar Símbolos desde Archivo", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog(
                "Confirmar Recarga",
                "Esto recargará los símbolos desde el archivo JSON, " +
                "descartando cualquier cambio no guardado en memoria.",
                "Recargar", "Cancelar"))
            {
                if (PatternDataManager.Instance != null)
                {
                    var symbols = PatternDataManager.Instance.LoadDatabase();
                    Debug.Log($"✅ Recargados {symbols.Count} símbolos");
                }
            }
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("🗑️ BORRAR TODOS LOS SÍMBOLOS", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog(
                "⚠️ ADVERTENCIA ⚠️",
                "Esto ELIMINARÁ PERMANENTEMENTE todos los símbolos entrenados.\n\n" +
                "Esta acción NO se puede deshacer.\n\n" +
                "¿Estás COMPLETAMENTE seguro?",
                "SÍ, BORRAR TODO", "NO, Cancelar"))
            {
                if (PatternDataManager.Instance != null)
                {
                    PatternDataManager.Instance.ClearAllData();
                    AssetDatabase.Refresh();
                    Debug.LogWarning("🗑️ Todos los símbolos han sido eliminados");
                }
                else if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    AssetDatabase.Refresh();
                    Debug.LogWarning("🗑️ Archivo de símbolos eliminado");
                }
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(20);

        // Información adicional
        EditorGUILayout.HelpBox(
            "💡 Tips:\n" +
            "• Los símbolos se guardan automáticamente al entrenarlos\n" +
            "• El archivo JSON es versionable en Git\n" +
            "• Puedes editar el JSON manualmente si lo necesitas\n" +
            "• En builds, los símbolos se cargan desde Resources (solo lectura)",
            MessageType.Info
        );

        GUILayout.Space(10);

        // Lista de símbolos
        if (fileExists)
        {
            EditorGUILayout.LabelField("Símbolos en el Archivo:", EditorStyles.boldLabel);

            var savedSymbols = PatternDataManager.Instance != null
                ? PatternDataManager.Instance.GetSavedSymbolNames()
                : new System.Collections.Generic.List<string>();

            if (savedSymbols.Count > 0)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                foreach (string symbolName in savedSymbols)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"📝 {symbolName}");

                    if (GUILayout.Button("❌", GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Eliminar Símbolo",
                            $"¿Eliminar el símbolo '{symbolName}'?",
                            "Eliminar", "Cancelar"))
                        {
                            if (PatternDataManager.Instance != null)
                            {
                                PatternDataManager.Instance.DeleteSymbol(symbolName);
                                AssetDatabase.Refresh();
                            }
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("(Ninguno)");
            }
        }
    }
}
#endif