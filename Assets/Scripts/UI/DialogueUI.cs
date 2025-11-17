using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the UI for displaying dialogues and player choices
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Dialogue Display")]
    public TextMeshProUGUI teenDialogueText;
    public TextMeshProUGUI playerDialogueText;
    public Image teenEmotionIcon;
    public GameObject dialoguePanel;
    
    [Header("Emotion Icons")]
    public Sprite happyIcon;
    public Sprite neutralIcon;
    public Sprite annoyedIcon;
    public Sprite angryIcon;
    public Sprite sadIcon;
    public Sprite defiantIcon;
    public Sprite anxiousIcon;
    public Sprite receptiveIcon;
    
    [Header("Player Options")]
    public GameObject optionsPanel;
    public Button authoritarianButton;
    public Button empatheticButton;
    public Button logicalButton;
    public Button briberyButton;
    public Button guiltTripButton;
    public Button listenButton;
    public Button compromiseButton;
    
    [Header("Custom Input")]
    public TMP_InputField customInputField;
    public Button customSendButton;
    
    [Header("Scenario Display")]
    public TextMeshProUGUI scenarioTitleText;
    public TextMeshProUGUI relationshipText;
    public Image relationshipBar;
    public TextMeshProUGUI moodText;
    public Image moodBar;
    
    [Header("Outcome Display")]
    public GameObject outcomePanel;
    public TextMeshProUGUI outcomeText;
    public Image outcomeBackground;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    
    [Header("Animation")]
    public float typewriterSpeed = 0.05f;
    public bool useTypewriterEffect = true;
    
    // Events
    public event Action<PlayerActionType> OnPlayerActionSelected;
    
    private Dictionary<PlayerActionType, Button> actionButtons;
    private bool isTyping = false;
    private TextAnalyzer textAnalyzer;
    
    private void Awake()
    {
        // Setup button mappings
        actionButtons = new Dictionary<PlayerActionType, Button>
        {
            { PlayerActionType.Authoritarian, authoritarianButton },
            { PlayerActionType.Empathetic, empatheticButton },
            { PlayerActionType.Logical, logicalButton },
            { PlayerActionType.Bribery, briberyButton },
            { PlayerActionType.GuiltTrip, guiltTripButton },
            { PlayerActionType.Listen, listenButton },
            { PlayerActionType.Compromise, compromiseButton }
        };
        
        // Setup button listeners
        if (authoritarianButton != null) authoritarianButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Authoritarian));
        if (empatheticButton != null) empatheticButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Empathetic));
        if (logicalButton != null) logicalButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Logical));
        if (briberyButton != null) briberyButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Bribery));
        if (guiltTripButton != null) guiltTripButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.GuiltTrip));
        if (listenButton != null) listenButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Listen));
        if (compromiseButton != null) compromiseButton.onClick.AddListener(() => OnOptionSelected(PlayerActionType.Compromise));
        
        // Setup custom input
        if (customSendButton != null) customSendButton.onClick.AddListener(OnCustomInputSubmit);
        
        // Setup text analyzer
        textAnalyzer = gameObject.AddComponent<TextAnalyzer>();
        textAnalyzer.Initialize();
        
        // Setup button labels
        SetupButtonLabels();
    }
    
    private void SetupButtonLabels()
    {
        SetButtonLabel(authoritarianButton, "Be Firm", "Command them directly");
        SetButtonLabel(empatheticButton, "Be Empathetic", "Show understanding");
        SetButtonLabel(logicalButton, "Use Logic", "Explain reasoning");
        SetButtonLabel(briberyButton, "Offer Reward", "Incentivize compliance");
        SetButtonLabel(guiltTripButton, "Guilt Trip", "Make them feel guilty");
        SetButtonLabel(listenButton, "Listen", "Hear them out");
        SetButtonLabel(compromiseButton, "Compromise", "Find middle ground");
    }
    
    private void SetButtonLabel(Button button, string title, string description)
    {
        if (button == null) return;
        
        TextMeshProUGUI[] texts = button.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            texts[0].text = title;
            if (texts.Length > 1)
            {
                texts[1].text = description;
            }
        }
    }
    
    /// <summary>
    /// Display teen's dialogue
    /// </summary>
    public void ShowTeenDialogue(string dialogue, EmotionalState.Emotion emotion)
    {
        if (teenDialogueText != null)
        {
            if (useTypewriterEffect && !isTyping)
            {
                StartCoroutine(TypewriterEffect(teenDialogueText, dialogue));
            }
            else
            {
                teenDialogueText.text = dialogue;
            }
        }
        
        // Update emotion icon
        if (teenEmotionIcon != null)
        {
            teenEmotionIcon.sprite = GetEmotionSprite(emotion);
        }
        
        // Clear player dialogue
        if (playerDialogueText != null)
        {
            playerDialogueText.text = "";
        }
    }
    
    /// <summary>
    /// Display player's dialogue
    /// </summary>
    public void ShowPlayerDialogue(string dialogue)
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = dialogue;
        }
    }
    
    /// <summary>
    /// Show player action options
    /// </summary>
    public void ShowPlayerOptions(ScenarioType scenario)
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
        
        // Update scenario title
        if (scenarioTitleText != null)
        {
            scenarioTitleText.text = GetScenarioTitle(scenario);
        }
    }
    
    /// <summary>
    /// Hide player options
    /// </summary>
    public void HidePlayerOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update emotional state UI
    /// </summary>
    public void UpdateEmotionalStateDisplay(EmotionalState emotionalState)
    {
        // Update relationship bar
        if (relationshipBar != null)
        {
            float normalizedRelationship = (emotionalState.relationshipLevel + 100f) / 200f; // Convert from [-100,100] to [0,1]
            relationshipBar.fillAmount = normalizedRelationship;
            relationshipBar.color = Color.Lerp(Color.red, Color.green, normalizedRelationship);
        }
        
        if (relationshipText != null)
        {
            relationshipText.text = $"Relationship: {emotionalState.relationshipLevel:F0}";
        }
        
        // Update mood bar
        if (moodBar != null)
        {
            float normalizedMood = (emotionalState.currentMood + 100f) / 200f; // Convert from [-100,100] to [0,1]
            moodBar.fillAmount = normalizedMood;
            moodBar.color = Color.Lerp(Color.red, Color.yellow, normalizedMood);
        }
        
        if (moodText != null)
        {
            moodText.text = $"Mood: {emotionalState.currentEmotion}";
        }
    }
    
    /// <summary>
    /// Show outcome of conversation
    /// </summary>
    public void ShowOutcome(string message, bool wasSuccessful)
    {
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(true);
        }
        
        if (outcomeText != null)
        {
            outcomeText.text = message;
        }
        
        if (outcomeBackground != null)
        {
            outcomeBackground.color = wasSuccessful ? successColor : failColor;
        }
        
        HidePlayerOptions();
        
        // Auto-hide after 3 seconds
        Invoke(nameof(HideOutcome), 3f);
    }
    
    private void HideOutcome()
    {
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Handle player option selection
    /// </summary>
    private void OnOptionSelected(PlayerActionType actionType)
    {
        HidePlayerOptions(); // Hide during processing, manager will show again
        OnPlayerActionSelected?.Invoke(actionType);
    }
    
    /// <summary>
    /// Handle custom text input
    /// </summary>
    private void OnCustomInputSubmit()
    {
        if (customInputField == null || string.IsNullOrWhiteSpace(customInputField.text))
            return;
        
        string customText = customInputField.text;
        
        // Show custom text as player dialogue
        if (playerDialogueText != null)
        {
            playerDialogueText.text = customText;
        }
        
        // Analyze text to determine action type (advanced analyzer)
        var (actionType, confidence) = textAnalyzer != null ? 
            textAnalyzer.AnalyzeText(customText) : 
            (AnalyzeCustomInput(customText), 0f);
        
        Debug.Log($"Analyzed: '{customText}' → {actionType} (confidence: {confidence:F2})");
        
        // Clear input
        customInputField.text = "";
        
        // Trigger action
        OnPlayerActionSelected?.Invoke(actionType);
    }
    
    /// <summary>
    /// Analyze custom text to determine action type
    /// </summary>
    private PlayerActionType AnalyzeCustomInput(string text)
    {
        string lower = text.ToLower();
        
        // Empathetic keywords
        if (lower.Contains("understand") || lower.Contains("feel") || lower.Contains("sorry") || 
            lower.Contains("care") || lower.Contains("love"))
            return PlayerActionType.Empathetic;
        
        // Listen keywords
        if (lower.Contains("tell me") || lower.Contains("what's wrong") || lower.Contains("listen") || 
            lower.Contains("talk") || lower.Contains("hear"))
            return PlayerActionType.Listen;
        
        // Compromise keywords
        if (lower.Contains("how about") || lower.Contains("what if") || lower.Contains("together") || 
            lower.Contains("compromise") || lower.Contains("meet"))
            return PlayerActionType.Compromise;
        
        // Logical keywords
        if (lower.Contains("because") || lower.Contains("important") || lower.Contains("need to") || 
            lower.Contains("should") || lower.Contains("reason"))
            return PlayerActionType.Logical;
        
        // Bribery keywords
        if (lower.Contains("if you") || lower.Contains("reward") || lower.Contains("give you") || 
            lower.Contains("money") || lower.Contains("buy"))
            return PlayerActionType.Bribery;
        
        // Guilt trip keywords
        if (lower.Contains("after all") || lower.Contains("disappoint") || lower.Contains("how could") || 
            lower.Contains("ungrateful") || lower.Contains("sacrifice"))
            return PlayerActionType.GuiltTrip;
        
        // Authoritarian keywords or commands
        if (lower.Contains("!") || lower.Contains("must") || lower.Contains("will") || 
            lower.Contains("now") || lower.Contains("immediately"))
            return PlayerActionType.Authoritarian;
        
        // Default to logical
        return PlayerActionType.Logical;
    }
    
    /// <summary>
    /// Trigger action externally (for keyboard shortcuts)
    /// </summary>
    public void TriggerAction(PlayerActionType actionType)
    {
        OnOptionSelected(actionType);
    }
    
    /// <summary>
    /// Get emotion sprite based on emotion type
    /// </summary>
    private Sprite GetEmotionSprite(EmotionalState.Emotion emotion)
    {
        switch (emotion)
        {
            case EmotionalState.Emotion.Happy: return happyIcon;
            case EmotionalState.Emotion.Angry: return angryIcon;
            case EmotionalState.Emotion.Annoyed: return annoyedIcon;
            case EmotionalState.Emotion.Sad: return sadIcon;
            case EmotionalState.Emotion.Defiant: return defiantIcon;
            case EmotionalState.Emotion.Anxious: return anxiousIcon;
            case EmotionalState.Emotion.Receptive: return receptiveIcon;
            default: return neutralIcon;
        }
    }
    
    /// <summary>
    /// Get scenario title text
    /// </summary>
    private string GetScenarioTitle(ScenarioType scenario)
    {
        switch (scenario)
        {
            case ScenarioType.GoToSchool: return "Getting Teen to Go to School";
            case ScenarioType.DoHomework: return "Homework Time";
            case ScenarioType.CleanRoom: return "Clean Your Room";
            case ScenarioType.LimitScreenTime: return "Screen Time Limits";
            case ScenarioType.Bedtime: return "Bedtime";
            case ScenarioType.ComeToFamily: return "Family Time";
            default: return "Conversation";
        }
    }
    
    /// <summary>
    /// Typewriter effect for dialogue
    /// </summary>
    private System.Collections.IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string text)
    {
        isTyping = true;
        textComponent.text = "";
        
        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
}

