using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Automatically builds mobile-friendly UI at runtime
/// Attach this to an empty GameObject and press Play
/// </summary>
public class UIBuilder : MonoBehaviour
{
    [Header("Auto-Build UI")]
    [Tooltip("Build UI on Start?")]
    public bool autoBuild = true;
    
    [Header("References (Auto-assigned)")]
    public Canvas canvas;
    public DialogueUI dialogueUI;
    
    private void Start()
    {
        if (autoBuild)
        {
            BuildCompleteUI();
        }
    }
    
    [ContextMenu("Build UI")]
    public void BuildCompleteUI()
    {
        // Add camera controller
        AddCameraController();
        
        // Create Canvas (camera is free to move)
        canvas = CreateCanvas();
        
        // Create all UI panels
        CreateDialoguePanel(canvas.transform);
        CreateEmotionalStatePanel(canvas.transform);
        CreatePlayerOptionsPanel(canvas.transform);
        CreateOutcomePanel(canvas.transform);
        
        // Setup DialogueUI component
        SetupDialogueUI();
        
        Debug.Log("✅ Mobile-friendly UI built successfully!");
    }
    
    private void AddCameraController()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Remove existing camera controller if any
            var existingController = mainCam.GetComponent<SimpleCameraController>();
            if (existingController != null)
            {
                DestroyImmediate(existingController);
            }
            
            // Add new camera controller
            SimpleCameraController controller = mainCam.gameObject.AddComponent<SimpleCameraController>();
            
            // Setup camera position
            mainCam.transform.position = new Vector3(0, 1.5f, -5);
            mainCam.transform.LookAt(Vector3.zero + Vector3.up * 1.5f);
            
            Debug.Log("✅ Camera controller added - Right-click & drag or touch to rotate, scroll to zoom");
        }
    }
    
    private Canvas CreateCanvas()
    {
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); // Landscape
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure EventSystem exists with NEW Input System
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>(); // New Input System!
        }
        else
        {
            // Replace old input module if exists
            var existingEventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>().gameObject;
            var oldModule = existingEventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null)
            {
                DestroyImmediate(oldModule);
                existingEventSystem.AddComponent<InputSystemUIInputModule>();
                Debug.Log("✅ Replaced old Input Module with new Input System");
            }
        }
        
        return canvas;
    }
    
    private void CreateDialoguePanel(Transform parent)
    {
        // Main dialogue panel at top (stretched horizontally)
        GameObject panel = CreatePanel("DialoguePanel", parent, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0, -150), new Vector2(-40, 280));
        panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        
        // Teen dialogue text (properly stretched)
        GameObject teenText = CreateText("TeenDialogueText", panel.transform, new Vector2(0f, 1f),
            "Teen will speak here...", 32, TextAlignmentOptions.TopLeft);
        RectTransform teenRect = teenText.GetComponent<RectTransform>();
        teenRect.anchorMin = new Vector2(0f, 0.5f);
        teenRect.anchorMax = new Vector2(1f, 1f);
        teenRect.pivot = new Vector2(0.5f, 1f);
        teenRect.offsetMin = new Vector2(20, 10); // Left, Bottom padding
        teenRect.offsetMax = new Vector2(-100, -20); // Right, Top padding
        TextMeshProUGUI teenTMP = teenText.GetComponent<TextMeshProUGUI>();
        teenTMP.color = Color.white;
        teenTMP.enableWordWrapping = true;
        teenTMP.overflowMode = TextOverflowModes.Overflow;
        
        // Player dialogue text (properly stretched)
        GameObject playerText = CreateText("PlayerDialogueText", panel.transform, new Vector2(0f, 0f),
            "Your response appears here...", 24, TextAlignmentOptions.TopLeft);
        RectTransform playerRect = playerText.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0f, 0f);
        playerRect.anchorMax = new Vector2(1f, 0.5f);
        playerRect.pivot = new Vector2(0.5f, 0f);
        playerRect.offsetMin = new Vector2(20, 20); // Left, Bottom padding
        playerRect.offsetMax = new Vector2(-100, -10); // Right, Top padding
        TextMeshProUGUI playerTMP = playerText.GetComponent<TextMeshProUGUI>();
        playerTMP.color = new Color(0.8f, 0.8f, 1f);
        playerTMP.enableWordWrapping = true;
        playerTMP.overflowMode = TextOverflowModes.Truncate;
        
        // Emotion icon
        GameObject emotionIcon = new GameObject("EmotionIcon");
        emotionIcon.transform.SetParent(panel.transform, false);
        Image icon = emotionIcon.AddComponent<Image>();
        icon.color = Color.yellow;
        RectTransform iconRect = emotionIcon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(1f, 1f);
        iconRect.anchorMax = new Vector2(1f, 1f);
        iconRect.pivot = new Vector2(1f, 1f);
        iconRect.anchoredPosition = new Vector2(-20, -20);
        iconRect.sizeDelta = new Vector2(80, 80);
    }
    
    private void CreateEmotionalStatePanel(Transform parent)
    {
        // Emotional state panel (top-left)
        GameObject panel = CreatePanel("EmotionalStatePanel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20, -20), new Vector2(400, 200));
        panel.GetComponent<Image>().color = new Color(0.15f, 0.1f, 0.1f, 0.9f);
        
        // Scenario title
        CreateText("ScenarioTitle", panel.transform, new Vector2(0.5f, 0.9f), "Scenario", 28, TextAlignmentOptions.Center);
        
        // Relationship bar
        CreateStatBar("RelationshipBar", panel.transform, new Vector2(0.5f, 0.65f), "Relationship", Color.green);
        
        // Mood bar
        CreateStatBar("MoodBar", panel.transform, new Vector2(0.5f, 0.35f), "Mood", Color.yellow);
    }
    
    private void CreateStatBar(string name, Transform parent, Vector2 anchorPos, string label, Color barColor)
    {
        // Label
        GameObject labelObj = CreateText(name + "Text", parent, anchorPos + new Vector2(0, 0.08f), label, 20, TextAlignmentOptions.Left);
        labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 30);
        
        // Background
        GameObject bg = CreatePanel(name + "BG", parent, anchorPos, anchorPos, Vector2.zero, new Vector2(350, 25));
        bg.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f);
        
        // Fill bar
        GameObject fill = CreatePanel(name, bg.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(175, 25));
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = barColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchorMin = new Vector2(0f, 0.5f);
        fillRect.anchorMax = new Vector2(0f, 0.5f);
    }
    
    private void CreatePlayerOptionsPanel(Transform parent)
    {
        // Options panel at bottom - LANDSCAPE layout
        GameObject panel = CreatePanel("OptionsPanel", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0, 20), new Vector2(1800, 220));
        panel.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.1f, 0.95f);
        
        // Horizontal Grid layout for landscape
        GridLayoutGroup grid = panel.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(240, 180); // Smaller buttons in grid
        grid.spacing = new Vector2(15, 10);
        grid.padding = new RectOffset(20, 20, 20, 20);
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 2; // 2 rows of buttons
        grid.childAlignment = TextAnchor.MiddleCenter;
        
        // Create 7 action buttons
        CreateActionButton("AuthoritarianButton", panel.transform, "Be Firm", "Command them directly", new Color(0.8f, 0.3f, 0.3f));
        CreateActionButton("EmpatheticButton", panel.transform, "Be Empathetic", "Show understanding", new Color(0.3f, 0.7f, 0.8f));
        CreateActionButton("LogicalButton", panel.transform, "Use Logic", "Explain reasoning", new Color(0.5f, 0.5f, 0.8f));
        CreateActionButton("BriberyButton", panel.transform, "Offer Reward", "Incentivize compliance", new Color(0.8f, 0.7f, 0.3f));
        CreateActionButton("GuiltTripButton", panel.transform, "Guilt Trip", "Make them feel guilty", new Color(0.7f, 0.4f, 0.7f));
        CreateActionButton("ListenButton", panel.transform, "Listen", "Hear them out", new Color(0.3f, 0.8f, 0.5f));
        CreateActionButton("CompromiseButton", panel.transform, "Compromise", "Find middle ground", new Color(0.4f, 0.8f, 0.8f));
        
        // Custom input field (takes 2 grid slots)
        CreateCustomInputField(panel.transform);
    }
    
    private void CreateCustomInputField(Transform parent)
    {
        // Input field container
        GameObject container = new GameObject("CustomInputContainer");
        container.transform.SetParent(parent, false);
        
        // Input field
        GameObject inputFieldObj = new GameObject("CustomInputField");
        inputFieldObj.transform.SetParent(container.transform, false);
        
        Image inputBg = inputFieldObj.AddComponent<Image>();
        inputBg.color = new Color(0.2f, 0.2f, 0.2f);
        
        TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
        
        // Text area for input
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(inputFieldObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 10);
        textAreaRect.offsetMax = new Vector2(-10, -10);
        
        // Placeholder
        GameObject placeholder = CreateText("Placeholder", textArea.transform, new Vector2(0.5f, 0.5f), "Type custom response...", 16, TextAlignmentOptions.Left);
        RectTransform placeRect = placeholder.GetComponent<RectTransform>();
        placeRect.anchorMin = Vector2.zero;
        placeRect.anchorMax = Vector2.one;
        placeRect.offsetMin = Vector2.zero;
        placeRect.offsetMax = Vector2.zero;
        placeholder.GetComponent<TextMeshProUGUI>().color = new Color(0.5f, 0.5f, 0.5f);
        
        // Text component
        GameObject textComp = CreateText("Text", textArea.transform, new Vector2(0.5f, 0.5f), "", 16, TextAlignmentOptions.Left);
        RectTransform textRect = textComp.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Setup input field
        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComp.GetComponent<TextMeshProUGUI>();
        inputField.placeholder = placeholder.GetComponent<TextMeshProUGUI>();
        
        RectTransform inputRect = inputFieldObj.GetComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = new Vector2(0.7f, 1f);
        inputRect.offsetMin = Vector2.zero;
        inputRect.offsetMax = Vector2.zero;
        
        // Send button
        GameObject sendButton = new GameObject("CustomSendButton");
        sendButton.transform.SetParent(container.transform, false);
        
        Image buttonImage = sendButton.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.8f, 0.3f);
        
        Button button = sendButton.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        
        GameObject buttonText = CreateText("Text", sendButton.transform, new Vector2(0.5f, 0.5f), "Send", 18, TextAlignmentOptions.Center);
        buttonText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        RectTransform buttonRect = sendButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.72f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
    }
    
    private void CreateActionButton(string name, Transform parent, string title, string description, Color color)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);
        
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.color = color;
        
        Button buttonComp = button.AddComponent<Button>();
        buttonComp.targetGraphic = buttonImage;
        
        // Color transition
        ColorBlock colors = buttonComp.colors;
        colors.highlightedColor = color * 1.2f;
        colors.pressedColor = color * 0.8f;
        buttonComp.colors = colors;
        
        // Title text (smaller for landscape)
        GameObject titleObj = CreateText(name + "_Title", button.transform, new Vector2(0.5f, 0.65f), title, 20, TextAlignmentOptions.Center);
        titleObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // Description text (smaller)
        GameObject descObj = CreateText(name + "_Desc", button.transform, new Vector2(0.5f, 0.35f), description, 14, TextAlignmentOptions.Center);
        descObj.GetComponent<TextMeshProUGUI>().color = new Color(0.9f, 0.9f, 0.9f);
    }
    
    private void CreateOutcomePanel(Transform parent)
    {
        // Outcome panel (centered, initially hidden)
        GameObject panel = CreatePanel("OutcomePanel", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(900, 400));
        panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        panel.SetActive(false); // Start hidden
        
        // Background image
        GameObject bg = CreatePanel("OutcomeBackground", panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(900, 400));
        
        // Outcome text
        GameObject text = CreateText("OutcomeText", panel.transform, new Vector2(0.5f, 0.5f), 
            "Outcome message here", 38, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(800, 300);
    }
    
    // Helper methods
    private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        return panel;
    }
    
    private GameObject CreateText(string name, Transform parent, Vector2 anchor, string content, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(300, 50);
        
        return textObj;
    }
    
    private void SetupDialogueUI()
    {
        dialogueUI = canvas.gameObject.AddComponent<DialogueUI>();
        
        // Assign all references
        dialogueUI.teenDialogueText = canvas.transform.Find("DialoguePanel/TeenDialogueText").GetComponent<TextMeshProUGUI>();
        dialogueUI.playerDialogueText = canvas.transform.Find("DialoguePanel/PlayerDialogueText").GetComponent<TextMeshProUGUI>();
        dialogueUI.teenEmotionIcon = canvas.transform.Find("DialoguePanel/EmotionIcon").GetComponent<Image>();
        dialogueUI.dialoguePanel = canvas.transform.Find("DialoguePanel").gameObject;
        
        dialogueUI.scenarioTitleText = canvas.transform.Find("EmotionalStatePanel/ScenarioTitle").GetComponent<TextMeshProUGUI>();
        dialogueUI.relationshipText = canvas.transform.Find("EmotionalStatePanel/RelationshipBarText").GetComponent<TextMeshProUGUI>();
        dialogueUI.relationshipBar = canvas.transform.Find("EmotionalStatePanel/RelationshipBarBG/RelationshipBar").GetComponent<Image>();
        dialogueUI.moodText = canvas.transform.Find("EmotionalStatePanel/MoodBarText").GetComponent<TextMeshProUGUI>();
        dialogueUI.moodBar = canvas.transform.Find("EmotionalStatePanel/MoodBarBG/MoodBar").GetComponent<Image>();
        
        dialogueUI.optionsPanel = canvas.transform.Find("OptionsPanel").gameObject;
        dialogueUI.authoritarianButton = canvas.transform.Find("OptionsPanel/AuthoritarianButton").GetComponent<Button>();
        dialogueUI.empatheticButton = canvas.transform.Find("OptionsPanel/EmpatheticButton").GetComponent<Button>();
        dialogueUI.logicalButton = canvas.transform.Find("OptionsPanel/LogicalButton").GetComponent<Button>();
        dialogueUI.briberyButton = canvas.transform.Find("OptionsPanel/BriberyButton").GetComponent<Button>();
        dialogueUI.guiltTripButton = canvas.transform.Find("OptionsPanel/GuiltTripButton").GetComponent<Button>();
        dialogueUI.listenButton = canvas.transform.Find("OptionsPanel/ListenButton").GetComponent<Button>();
        dialogueUI.compromiseButton = canvas.transform.Find("OptionsPanel/CompromiseButton").GetComponent<Button>();
        
        dialogueUI.outcomePanel = canvas.transform.Find("OutcomePanel").gameObject;
        dialogueUI.outcomeText = canvas.transform.Find("OutcomePanel/OutcomeText").GetComponent<TextMeshProUGUI>();
        dialogueUI.outcomeBackground = canvas.transform.Find("OutcomePanel/OutcomeBackground").GetComponent<Image>();
        
        // Custom input
        dialogueUI.customInputField = canvas.transform.Find("OptionsPanel/CustomInputContainer/CustomInputField").GetComponent<TMP_InputField>();
        dialogueUI.customSendButton = canvas.transform.Find("OptionsPanel/CustomInputContainer/CustomSendButton").GetComponent<Button>();
        
        // Link to managers
        ConversationManager convManager = FindFirstObjectByType<ConversationManager>();
        if (convManager != null)
        {
            convManager.dialogueUI = dialogueUI;
        }
        
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.dialogueUI = dialogueUI;
        }
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.dialogueUI = dialogueUI;
        }
    }
}

