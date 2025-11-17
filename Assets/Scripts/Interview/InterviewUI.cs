using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI for Job Interview From Hell - Works with existing Canvas structure
/// </summary>
public class InterviewUI : MonoBehaviour
{
    [Header("Panels - Assign in Inspector")]
    public GameObject startScreen;
    public GameObject interviewPanel;
    public GameObject resultPanel;
    
    [Header("Start Screen - Assign in Inspector")]
    public Button startButton;
    
    [Header("Interview Panel - Assign in Inspector")]
    public TextMeshProUGUI interviewerText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI questionNumberText;
    public TextMeshProUGUI strikeCountText;
    public TextMeshProUGUI transcriptText;
    public TextMeshProUGUI statusText;
    
    public Button micButton;
    public Image micButtonImage;
    public TextMeshProUGUI micButtonText;
    
    [Header("Text Input - Assign in Inspector")]
    public TMP_InputField textInputField;
    public Button submitTextButton;
    public GameObject textInputPanel; // Optional: panel containing text input
    
    [Header("TTS Control - Assign in Inspector")]
    public Button stopTTSButton; // Button to stop TTS speech
    public TextMeshProUGUI stopTTSButtonText; // Optional: text on stop button
    
    [Header("Result Panel - Assign in Inspector")]
    public TextMeshProUGUI resultText;
    public Button restartButton;
    public Button quitButton;
    
    [Header("Colors")]
    public Color recordingColor = Color.red;
    public Color idleColor = Color.white;
    public Color passColor = Color.green;
    public Color failColor = Color.red;
    
    private InterviewerAI interviewer;
    private bool isRecording = false;
    
    private void Start()
    {
        // Setup start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartInterview);
        }
        else
        {
            Debug.LogWarning("[InterviewUI] Start button not assigned!");
        }
        
        // Setup mic button
        if (micButton != null)
        {
            micButton.onClick.AddListener(OnMicButtonClicked);
        }
        
        // Setup text input
        if (submitTextButton != null)
        {
            submitTextButton.onClick.AddListener(OnTextSubmit);
        }
        
        if (textInputField != null)
        {
            // Allow Enter key to submit
            textInputField.onSubmit.AddListener((text) => OnTextSubmit());
        }
        
        // Setup result panel buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartInterview);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitToMenu);
        }
        
        // Setup stop TTS button
        if (stopTTSButton != null)
        {
            stopTTSButton.onClick.AddListener(StopTTS);
            stopTTSButton.gameObject.SetActive(false); // Hidden by default
        }
        
        // Find interviewer
        interviewer = FindFirstObjectByType<InterviewerAI>();
        
        if (interviewer != null)
        {
            interviewer.OnInterviewerSpeak += ShowInterviewerMessage;
            interviewer.OnFeedback += ShowFeedback;
            interviewer.OnInterviewFailed += ShowFailScreen;
            interviewer.OnInterviewPassed += ShowPassScreen;
        }
        
        // Show start screen initially
        ShowStartScreen();
    }
    
    private void Update()
    {
        if (interviewer != null)
        {
            UpdateStatusDisplay();
            UpdateTTSButtonVisibility();
        }
    }
    
    private void UpdateTTSButtonVisibility()
    {
        // Show/hide stop TTS button based on whether TTS is speaking
        if (stopTTSButton != null && interviewer != null && interviewer.voiceSystem != null)
        {
            bool isSpeaking = interviewer.voiceSystem.IsSpeaking;
            stopTTSButton.gameObject.SetActive(isSpeaking);
        }
    }
    
    private void StopTTS()
    {
        if (interviewer != null && interviewer.voiceSystem != null)
        {
            interviewer.voiceSystem.StopSpeaking();
            Debug.Log("[InterviewUI] TTS stopped by user");
            
            if (statusText != null)
            {
                statusText.text = "TTS stopped.";
            }
        }
    }
    
    public void StartInterview()
    {
        // Hide start screen, show interview panel
        if (startScreen != null) startScreen.SetActive(false);
        if (interviewPanel != null) interviewPanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
        
        // Remove dark background from interview panel if it exists
        if (interviewPanel != null)
        {
            Image panelImage = interviewPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.enabled = false; // No dark background - scene fully visible
            }
        }
        
        ClearAllText();
        
        // Find interviewer if not found
        if (interviewer == null)
        {
            interviewer = FindFirstObjectByType<InterviewerAI>();
            if (interviewer != null)
            {
                interviewer.OnInterviewerSpeak += ShowInterviewerMessage;
                interviewer.OnFeedback += ShowFeedback;
                interviewer.OnInterviewFailed += ShowFailScreen;
                interviewer.OnInterviewPassed += ShowPassScreen;
            }
        }
        
        if (interviewer != null)
        {
            interviewer.StartInterview();
        }
        else
        {
            Debug.LogWarning("[InterviewUI] No InterviewerAI found! Make sure InterviewManager exists in scene.");
        }
    }
    
    private void OnMicButtonClicked()
    {
        if (isRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }
    
    private void StartRecording()
    {
        if (interviewer == null || interviewer.whisperSTT == null) return;
        
        interviewer.whisperSTT.StartRecording();
        isRecording = true;
        
        if (micButtonImage != null)
            micButtonImage.color = recordingColor;
        
        if (micButtonText != null)
            micButtonText.text = "🎤 STOP";
        
        if (statusText != null)
            statusText.text = "Recording... Speak now!";
        
        if (transcriptText != null)
            transcriptText.text = "...";
    }
    
    private void StopRecording()
    {
        if (interviewer == null || interviewer.whisperSTT == null) return;
        
        interviewer.whisperSTT.StopRecording((transcript, audioClip) => {
            OnTranscriptionComplete(transcript, audioClip);
        });
        
        isRecording = false;
        
        if (micButtonImage != null)
            micButtonImage.color = idleColor;
        
        if (micButtonText != null)
            micButtonText.text = "🎤 ANSWER";
        
        if (statusText != null)
            statusText.text = "Processing...";
    }
    
    private void OnTranscriptionComplete(string transcript, AudioClip audioClip)
    {
        if (transcriptText != null)
        {
            transcriptText.text = $"You: \"{transcript}\"";
        }
        
        if (statusText != null)
        {
            statusText.text = "Analyzing...";
        }
        
        if (interviewer != null)
        {
            interviewer.ProcessPlayerAnswer(transcript, audioClip);
        }
    }
    
    private void ShowInterviewerMessage(string message)
    {
        if (interviewerText != null)
        {
            interviewerText.text = $"<b>Interviewer:</b> {message}";
        }
        
        if (statusText != null)
        {
            statusText.text = "Interviewer speaking...";
        }
        
        // Disable inputs while interviewer speaks
        if (micButton != null)
            micButton.interactable = false;
        
        if (submitTextButton != null)
            submitTextButton.interactable = false;
        
        if (textInputField != null)
            textInputField.interactable = false;
        
        // Enable inputs after interviewer finishes (simulate delay)
        Invoke(nameof(EnableMicButton), 3f);
    }
    
    private void EnableMicButton()
    {
        if (micButton != null)
            micButton.interactable = true;
        
        if (submitTextButton != null)
            submitTextButton.interactable = true;
        
        if (textInputField != null)
        {
            textInputField.interactable = true;
            textInputField.ActivateInputField(); // Focus on text field
        }
        
        if (statusText != null)
            statusText.text = "Your turn. Use MIC or type your answer.";
    }
    
    private void OnTextSubmit()
    {
        if (textInputField == null || string.IsNullOrWhiteSpace(textInputField.text))
            return;
        
        string playerAnswer = textInputField.text.Trim();
        
        // Show what player typed
        if (transcriptText != null)
        {
            transcriptText.text = $"You: \"{playerAnswer}\"";
        }
        
        if (statusText != null)
        {
            statusText.text = "Analyzing...";
        }
        
        // Disable input while processing
        if (textInputField != null)
        {
            textInputField.interactable = false;
            textInputField.text = "";
        }
        
        if (submitTextButton != null)
            submitTextButton.interactable = false;
        
        if (micButton != null)
            micButton.interactable = false;
        
        // Process text answer (no audio clip needed)
        if (interviewer != null)
        {
            // Create dummy audio clip for compatibility, or modify InterviewerAI to accept text directly
            interviewer.ProcessPlayerAnswer(playerAnswer, null);
        }
    }
    
    private void ShowFeedback(string feedback)
    {
        if (feedbackText != null)
        {
            feedbackText.text = feedback;
            feedbackText.color = feedback.Contains("✅") ? passColor : failColor;
        }
    }
    
    private void UpdateStatusDisplay()
    {
        if (questionNumberText != null)
        {
            questionNumberText.text = $"Question {interviewer.currentQuestionIndex + 1}/5";
        }
        
        if (strikeCountText != null)
        {
            strikeCountText.text = $"Strikes: {interviewer.strikeCount}/{interviewer.maxStrikes}";
            strikeCountText.color = interviewer.strikeCount >= 2 ? failColor : Color.white;
        }
    }
    
    private void ShowStartScreen()
    {
        if (startScreen != null) startScreen.SetActive(true);
        if (interviewPanel != null) interviewPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }
    
    private void ShowFailScreen()
    {
        if (interviewPanel != null) interviewPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
        
        if (resultText != null)
        {
            resultText.text = "❌ REJECTED ❌\n\nYou failed the interview.\n\nBetter luck next time...";
            resultText.color = failColor;
        }
    }
    
    private void ShowPassScreen()
    {
        if (interviewPanel != null) interviewPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);
        
        if (resultText != null)
        {
            resultText.text = "✅ HIRED! ✅\n\nYou survived the interview from hell!\n\nCongratulations!";
            resultText.color = passColor;
        }
    }
    
    private void ClearAllText()
    {
        if (interviewerText != null) interviewerText.text = "";
        if (feedbackText != null) feedbackText.text = "";
        if (transcriptText != null) transcriptText.text = "";
        if (statusText != null) statusText.text = "Get ready...";
        if (textInputField != null) textInputField.text = "";
    }
    
    public void RestartInterview()
    {
        StartInterview();
    }
    
    public void QuitToMenu()
    {
        ShowStartScreen();
    }
}

