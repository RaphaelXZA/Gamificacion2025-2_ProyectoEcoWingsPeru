using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PatternLearningUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button learningModeButton;
    [SerializeField] private Button gameplayModeButton;
    [SerializeField] private TMP_InputField symbolNameInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private Button clearSymbolButton;
    [SerializeField] private TMP_Dropdown symbolDropdown;
    // debugButton eliminado - solo usaremos tecla D

    private SymbolPatternSystem patternSystem;
    private bool isInLearningMode = false;

    void Start()
    {
        patternSystem = FindFirstObjectByType<SymbolPatternSystem>();

        if (patternSystem == null)
        {
            Debug.LogError("SymbolPatternSystem no encontrado!");
            return;
        }

        // Configurar eventos
        SetupUI();

        // Suscribirse a eventos del sistema de patrones
        patternSystem.OnSymbolMatched += OnSymbolMatched;
        patternSystem.OnPatternLearned += OnPatternLearned;

        UpdateUI();
    }

    void SetupUI()
    {
        if (learningModeButton != null)
            learningModeButton.onClick.AddListener(ToggleLearningMode);

        if (gameplayModeButton != null)
            gameplayModeButton.onClick.AddListener(StartGameplayMode);

        if (clearSymbolButton != null)
            clearSymbolButton.onClick.AddListener(ClearSelectedSymbol);

        if (symbolDropdown != null)
            symbolDropdown.onValueChanged.AddListener(OnSymbolSelected);

        // debugButton eliminado - solo usamos tecla D
    }

    void Update()
    {
        // Debug con tecla D
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugSystem();
        }
    }

    void UpdateUI()
    {
        if (isInLearningMode)
        {
            string currentSymbol = symbolNameInput != null ? symbolNameInput.text : "Sin nombre";
            statusText.text = $"MODO APRENDIZAJE: {currentSymbol}";
            instructionsText.text = "Dibuja el símbolo varias veces para enseñarle al sistema";

            if (learningModeButton != null)
            {
                learningModeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Parar Aprendizaje";
            }
        }
        else
        {
            statusText.text = "MODO GAMEPLAY";
            instructionsText.text = "Dibuja símbolos para que el sistema los detecte";

            if (learningModeButton != null)
            {
                learningModeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Modo Aprendizaje";
            }
        }

        UpdateSymbolDropdown();
    }

    void UpdateSymbolDropdown()
    {
        if (symbolDropdown == null || patternSystem == null) return;

        symbolDropdown.options.Clear();

        var learnedSymbols = patternSystem.GetLearnedSymbols();
        foreach (string symbol in learnedSymbols)
        {
            int patternCount = patternSystem.GetPatternCount(symbol);
            string optionText = $"{symbol} ({patternCount} patrones)";
            symbolDropdown.options.Add(new TMP_Dropdown.OptionData(optionText));
        }

        symbolDropdown.RefreshShownValue();
    }

    public void ToggleLearningMode()
    {
        if (isInLearningMode)
        {
            StopLearningMode();
        }
        else
        {
            StartLearningMode();
        }
    }

    void StartLearningMode()
    {
        string symbolName = symbolNameInput != null ? symbolNameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(symbolName))
        {
            Debug.LogWarning("Ingresa un nombre para el símbolo antes de iniciar el modo aprendizaje");
            return;
        }

        isInLearningMode = true;
        patternSystem.StartLearningMode(symbolName);
        UpdateUI();
    }

    void StopLearningMode()
    {
        isInLearningMode = false;
        patternSystem.StopLearningMode();
        UpdateUI();
    }

    public void StartGameplayMode()
    {
        isInLearningMode = false;
        patternSystem.StartGameplayMode();
        UpdateUI();
    }

    void ClearSelectedSymbol()
    {
        if (symbolDropdown == null || symbolDropdown.options.Count == 0) return;

        string selectedOption = symbolDropdown.options[symbolDropdown.value].text;
        string symbolName = selectedOption.Split('(')[0].Trim(); // Extraer solo el nombre

        patternSystem.ClearSymbol(symbolName);
        UpdateUI();
    }

    void OnSymbolSelected(int index)
    {
        // Opcional: hacer algo cuando se selecciona un símbolo del dropdown
    }

    void DebugSystem()
    {
        if (patternSystem != null)
        {
            patternSystem.DebugSystemState();
        }
    }

    // Event handlers del sistema de patrones
    void OnSymbolMatched(string symbolName, float accuracy)
    {
        if (statusText != null)
        {
            statusText.text = $"¡DETECTADO: {symbolName}! ({accuracy:P1})";

            // Volver al estado normal después de unos segundos
            Invoke(nameof(ResetStatusText), 2f);
        }
    }

    void OnPatternLearned(string symbolName)
    {
        int patternCount = patternSystem.GetPatternCount(symbolName);

        if (statusText != null)
        {
            statusText.text = $"Patrón #{patternCount} aprendido para '{symbolName}'";
        }

        UpdateSymbolDropdown();

        // Volver al estado normal después de un momento
        Invoke(nameof(UpdateUI), 1.5f);
    }

    void ResetStatusText()
    {
        UpdateUI();
    }

    void OnDestroy()
    {
        if (patternSystem != null)
        {
            patternSystem.OnSymbolMatched -= OnSymbolMatched;
            patternSystem.OnPatternLearned -= OnPatternLearned;
        }
    }
}