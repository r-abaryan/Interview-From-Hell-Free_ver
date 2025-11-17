using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically builds the complete UI for Job Interview From Hell
/// </summary>
public class InterviewUIBuilder : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private bool mobileFriendly = true;
    
    private Canvas canvas;
    private InterviewUI interviewUI;
    
    void Start()
    {
        if (buildOnStart)
        {
            BuildUI();
        }
    }
    
    [ContextMenu("Build UI")]
    public void BuildUI()
    {
        Debug.Log("🏗️ Building Interview UI...");
        
        // Find or create Canvas
        canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        else
        {
            // Ensure GraphicRaycaster exists
            if (canvas.GetComponent<GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        // Setup EventSystem with Input System support
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            
            // Use InputSystemUIInputModule if available, otherwise fallback to StandaloneInputModule
            #if UNITY_INPUT_SYSTEM_AVAILABLE
            if (UnityEngine.InputSystem.UI.InputSystemUIInputModule.available)
            {
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            else
            {
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            #else
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            #endif
        }
        
        // Create main container
        GameObject mainContainer = CreatePanel("MainContainer", canvas.transform);
        RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
        mainRect.anchorMin = Vector2.zero;
        mainRect.anchorMax = Vector2.one;
        mainRect.offsetMin = Vector2.zero;
        mainRect.offsetMax = Vector2.zero;
        
        // Remove dark background from main container
        Image mainImage = mainContainer.GetComponent<Image>();
        if (mainImage != null)
        {
            mainImage.enabled = false; // No background - scene fully visible
        }
        
        // Build Start Panel
        GameObject startPanel = BuildStartPanel(mainContainer.transform);
        
        // Build Interview Panel
        GameObject interviewPanel = BuildInterviewPanel(mainContainer.transform);
        
        // Build Result Panel
        GameObject resultPanel = BuildResultPanel(mainContainer.transform);
        
        // Create InterviewUI component and link everything
        interviewUI = mainContainer.GetComponent<InterviewUI>();
        if (interviewUI == null)
        {
            interviewUI = mainContainer.AddComponent<InterviewUI>();
        }
        
        LinkUIComponents(interviewUI, startPanel, interviewPanel, resultPanel);
        
        Debug.Log("✅ Interview UI Built Successfully!");
    }
    
    private GameObject BuildStartPanel(Transform parent)
    {
        GameObject panel = CreatePanel("StartPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelImage.enabled = true; // Keep full screen for start
        
        // Title
        GameObject title = CreateText("Title", panel.transform);
        TextMeshProUGUI titleText = title.GetComponent<TextMeshProUGUI>();
        titleText.text = "JOB INTERVIEW FROM HELL";
        titleText.fontSize = 72;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.red;
        
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // Subtitle
        GameObject subtitle = CreateText("Subtitle", panel.transform);
        TextMeshProUGUI subtitleText = subtitle.GetComponent<TextMeshProUGUI>();
        subtitleText.text = "After 7 failed interviews... you finally get accepted.\nBut HR forgot to mention THE FINAL BOSS INTERVIEW.";
        subtitleText.fontSize = 32;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = Color.yellow;
        
        RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.1f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.9f, 0.65f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;
        
        // Start Button
        GameObject startButton = CreateButton("StartButton", panel.transform);
        TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = "START INTERVIEW";
        buttonText.fontSize = 48;
        buttonText.color = Color.white;
        
        RectTransform buttonRect = startButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.3f, 0.25f);
        buttonRect.anchorMax = new Vector2(0.7f, 0.4f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
        
        Image buttonImage = startButton.GetComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 0.2f);
        
        startButton.GetComponent<Button>().onClick.AddListener(() => {
            if (interviewUI != null)
                interviewUI.StartInterview();
        });
        
        return panel;
    }
    
    private GameObject BuildInterviewPanel(Transform parent)
    {
        GameObject panel = CreatePanel("InterviewPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.GetComponent<Image>();
        panelImage.enabled = false; // No background - transparent overlay
        
        panel.SetActive(false); // Hidden initially
        
        // Top Bar - Question Number & Strikes (Minimal, top-right corner)
        GameObject topBar = CreatePanel("TopBar", panel.transform);
        RectTransform topBarRect = topBar.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0.75f, 0.9f);
        topBarRect.anchorMax = new Vector2(0.98f, 0.98f);
        topBarRect.offsetMin = Vector2.zero;
        topBarRect.offsetMax = Vector2.zero;
        
        Image topBarImage = topBar.GetComponent<Image>();
        topBarImage.color = new Color(0f, 0f, 0f, 0.7f); // Semi-transparent black
        
        // Question Number (Compact)
        GameObject questionNum = CreateText("QuestionNumber", topBar.transform);
        questionNum.name = "QuestionNumberText";
        TextMeshProUGUI qNumText = questionNum.GetComponent<TextMeshProUGUI>();
        qNumText.text = "Q1/5";
        qNumText.fontSize = 24;
        qNumText.color = Color.white;
        qNumText.alignment = TextAlignmentOptions.Center;
        
        RectTransform qNumRect = questionNum.GetComponent<RectTransform>();
        qNumRect.anchorMin = new Vector2(0.05f, 0.5f);
        qNumRect.anchorMax = new Vector2(0.95f, 0.95f);
        qNumRect.offsetMin = Vector2.zero;
        qNumRect.offsetMax = Vector2.zero;
        
        // Strike Count (Compact, below question)
        GameObject strikes = CreateText("StrikeCount", topBar.transform);
        strikes.name = "StrikeCountText";
        TextMeshProUGUI strikeText = strikes.GetComponent<TextMeshProUGUI>();
        strikeText.text = "⚡ 0/3";
        strikeText.fontSize = 20;
        strikeText.color = Color.yellow;
        strikeText.alignment = TextAlignmentOptions.Center;
        
        RectTransform strikeRect = strikes.GetComponent<RectTransform>();
        strikeRect.anchorMin = new Vector2(0.05f, 0f);
        strikeRect.anchorMax = new Vector2(0.95f, 0.45f);
        strikeRect.offsetMin = Vector2.zero;
        strikeRect.offsetMax = Vector2.zero;
        
        // Interviewer Dialogue Box (Bottom-left, minimal)
        GameObject interviewerBox = CreatePanel("InterviewerBox", panel.transform);
        RectTransform interviewerRect = interviewerBox.GetComponent<RectTransform>();
        interviewerRect.anchorMin = new Vector2(0.02f, 0.15f);
        interviewerRect.anchorMax = new Vector2(0.45f, 0.35f);
        interviewerRect.offsetMin = Vector2.zero;
        interviewerRect.offsetMax = Vector2.zero;
        
        Image interviewerImage = interviewerBox.GetComponent<Image>();
        interviewerImage.color = new Color(0f, 0f, 0f, 0.75f); // Semi-transparent
        
        GameObject interviewerText = CreateText("InterviewerText", interviewerBox.transform);
        TextMeshProUGUI interviewerTxt = interviewerText.GetComponent<TextMeshProUGUI>();
        interviewerTxt.text = "Welcome to your FINAL interview...";
        interviewerTxt.fontSize = 22;
        interviewerTxt.color = Color.white;
        interviewerTxt.alignment = TextAlignmentOptions.TopLeft;
        interviewerTxt.enableWordWrapping = true;
        
        RectTransform interviewerTxtRect = interviewerText.GetComponent<RectTransform>();
        interviewerTxtRect.anchorMin = new Vector2(0.05f, 0.05f);
        interviewerTxtRect.anchorMax = new Vector2(0.95f, 0.95f);
        interviewerTxtRect.offsetMin = Vector2.zero;
        interviewerTxtRect.offsetMax = Vector2.zero;
        
        // Player Transcript Box (Bottom-right, minimal)
        GameObject transcriptBox = CreatePanel("TranscriptBox", panel.transform);
        RectTransform transcriptRect = transcriptBox.GetComponent<RectTransform>();
        transcriptRect.anchorMin = new Vector2(0.55f, 0.15f);
        transcriptRect.anchorMax = new Vector2(0.98f, 0.35f);
        transcriptRect.offsetMin = Vector2.zero;
        transcriptRect.offsetMax = Vector2.zero;
        
        Image transcriptImage = transcriptBox.GetComponent<Image>();
        transcriptImage.color = new Color(0f, 0.2f, 0.3f, 0.75f); // Semi-transparent cyan
        
        GameObject transcriptText = CreateText("TranscriptText", transcriptBox.transform);
        TextMeshProUGUI transcriptTxt = transcriptText.GetComponent<TextMeshProUGUI>();
        transcriptTxt.text = "Your answer...";
        transcriptTxt.fontSize = 20;
        transcriptTxt.color = Color.cyan;
        transcriptTxt.alignment = TextAlignmentOptions.TopLeft;
        transcriptTxt.enableWordWrapping = true;
        
        RectTransform transcriptTxtRect = transcriptText.GetComponent<RectTransform>();
        transcriptTxtRect.anchorMin = Vector2.zero;
        transcriptTxtRect.anchorMax = Vector2.one;
        transcriptTxtRect.offsetMin = Vector2.zero;
        transcriptTxtRect.offsetMax = Vector2.zero;
        
        // Feedback Box (Top-center, minimal banner)
        GameObject feedbackBox = CreatePanel("FeedbackBox", panel.transform);
        RectTransform feedbackRect = feedbackBox.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.3f, 0.88f);
        feedbackRect.anchorMax = new Vector2(0.7f, 0.95f);
        feedbackRect.offsetMin = Vector2.zero;
        feedbackRect.offsetMax = Vector2.zero;
        
        Image feedbackImage = feedbackBox.GetComponent<Image>();
        feedbackImage.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent
        
        GameObject feedbackText = CreateText("FeedbackText", feedbackBox.transform);
        TextMeshProUGUI feedbackTxt = feedbackText.GetComponent<TextMeshProUGUI>();
        feedbackTxt.text = "";
        feedbackTxt.fontSize = 24;
        feedbackTxt.color = Color.white;
        feedbackTxt.alignment = TextAlignmentOptions.Center;
        feedbackTxt.fontStyle = FontStyles.Bold;
        
        RectTransform feedbackTxtRect = feedbackText.GetComponent<RectTransform>();
        feedbackTxtRect.anchorMin = Vector2.zero;
        feedbackTxtRect.anchorMax = Vector2.one;
        feedbackTxtRect.offsetMin = Vector2.zero;
        feedbackTxtRect.offsetMax = Vector2.zero;
        
        // Status Text (Bottom-center, small)
        GameObject statusText = CreateText("StatusText", panel.transform);
        TextMeshProUGUI statusTxt = statusText.GetComponent<TextMeshProUGUI>();
        statusTxt.text = "Get ready...";
        statusTxt.fontSize = 20;
        statusTxt.color = Color.gray;
        statusTxt.alignment = TextAlignmentOptions.Center;
        
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.3f, 0.05f);
        statusRect.anchorMax = new Vector2(0.7f, 0.12f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        // Mic Button (Bottom-center, compact)
        GameObject micButton = CreateButton("MicButton", panel.transform);
        TextMeshProUGUI micButtonText = micButton.GetComponentInChildren<TextMeshProUGUI>();
        micButtonText.text = "🎤";
        micButtonText.fontSize = 36;
        micButtonText.color = Color.white;
        
        RectTransform micRect = micButton.GetComponent<RectTransform>();
        micRect.anchorMin = new Vector2(0.45f, 0.02f);
        micRect.anchorMax = new Vector2(0.55f, 0.08f);
        micRect.offsetMin = Vector2.zero;
        micRect.offsetMax = Vector2.zero;
        
        Image micImage = micButton.GetComponent<Image>();
        micImage.color = new Color(0.3f, 0.3f, 0.5f, 0.9f);
        
        // Make button circular/rounded
        micRect.sizeDelta = new Vector2(80, 80);
        
        return panel;
    }
    
    private GameObject BuildResultPanel(Transform parent)
    {
        GameObject panel = CreatePanel("ResultPanel", parent);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        
        panel.SetActive(false); // Hidden initially
        
        // Result Text
        GameObject resultText = CreateText("ResultText", panel.transform);
        TextMeshProUGUI resultTxt = resultText.GetComponent<TextMeshProUGUI>();
        resultTxt.text = "";
        resultTxt.fontSize = 64;
        resultTxt.alignment = TextAlignmentOptions.Center;
        resultTxt.enableWordWrapping = true;
        
        RectTransform resultRect = resultText.GetComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.1f, 0.4f);
        resultRect.anchorMax = new Vector2(0.9f, 0.7f);
        resultRect.offsetMin = Vector2.zero;
        resultRect.offsetMax = Vector2.zero;
        
        // Restart Button
        GameObject restartButton = CreateButton("RestartButton", panel.transform);
        TextMeshProUGUI restartTxt = restartButton.GetComponentInChildren<TextMeshProUGUI>();
        restartTxt.text = "TRY AGAIN";
        restartTxt.fontSize = 48;
        restartTxt.color = Color.white;
        
        RectTransform restartRect = restartButton.GetComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.3f, 0.2f);
        restartRect.anchorMax = new Vector2(0.7f, 0.35f);
        restartRect.offsetMin = Vector2.zero;
        restartRect.offsetMax = Vector2.zero;
        
        Image restartImage = restartButton.GetComponent<Image>();
        restartImage.color = new Color(0.2f, 0.4f, 0.6f);
        
        Button restartBtn = restartButton.GetComponent<Button>();
        if (restartBtn != null)
        {
            restartBtn.onClick.RemoveAllListeners();
            restartBtn.onClick.AddListener(() => {
                if (interviewUI != null)
                    interviewUI.RestartInterview();
            });
        }
        
        // Quit Button
        GameObject quitButton = CreateButton("QuitButton", panel.transform);
        TextMeshProUGUI quitTxt = quitButton.GetComponentInChildren<TextMeshProUGUI>();
        quitTxt.text = "QUIT";
        quitTxt.fontSize = 36;
        quitTxt.color = Color.white;
        
        RectTransform quitRect = quitButton.GetComponent<RectTransform>();
        quitRect.anchorMin = new Vector2(0.3f, 0.05f);
        quitRect.anchorMax = new Vector2(0.7f, 0.15f);
        quitRect.offsetMin = Vector2.zero;
        quitRect.offsetMax = Vector2.zero;
        
        Image quitImage = quitButton.GetComponent<Image>();
        quitImage.color = new Color(0.4f, 0.2f, 0.2f);
        
        Button quitBtn = quitButton.GetComponent<Button>();
        if (quitBtn != null)
        {
            quitBtn.onClick.RemoveAllListeners();
            quitBtn.onClick.AddListener(() => {
                if (interviewUI != null)
                    interviewUI.QuitToMenu();
            });
        }
        
        return panel;
    }
    
    private void LinkUIComponents(InterviewUI ui, GameObject startPanel, GameObject interviewPanel, GameObject resultPanel)
    {
        // Link panels
        ui.startScreen = startPanel;
        ui.interviewPanel = interviewPanel;
        ui.resultPanel = resultPanel;
        
        // Link text elements from interview panel
        Transform interviewTransform = interviewPanel.transform;
        ui.interviewerText = interviewTransform.Find("InterviewerBox/InterviewerText")?.GetComponent<TextMeshProUGUI>();
        ui.feedbackText = interviewTransform.Find("FeedbackBox/FeedbackText")?.GetComponent<TextMeshProUGUI>();
        ui.questionNumberText = interviewTransform.Find("TopBar/QuestionNumberText")?.GetComponent<TextMeshProUGUI>();
        ui.strikeCountText = interviewTransform.Find("TopBar/StrikeCountText")?.GetComponent<TextMeshProUGUI>();
        ui.transcriptText = interviewTransform.Find("TranscriptBox/TranscriptText")?.GetComponent<TextMeshProUGUI>();
        ui.statusText = interviewTransform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        
        // Link mic button
        GameObject micButton = interviewTransform.Find("MicButton")?.gameObject;
        if (micButton != null)
        {
            ui.micButton = micButton.GetComponent<Button>();
            ui.micButtonImage = micButton.GetComponent<Image>();
            ui.micButtonText = micButton.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Link result text
        ui.resultText = resultPanel.transform.Find("ResultText")?.GetComponent<TextMeshProUGUI>();
        
        Debug.Log("✅ UI Components Linked!");
    }
    
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        return panel;
    }
    
    private GameObject CreateText(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = name;
        text.fontSize = 24;
        text.color = Color.white;
        
        return textObj;
    }
    
    private GameObject CreateButton(string name, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f);
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = name;
        text.fontSize = 36;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        return buttonObj;
    }
}

