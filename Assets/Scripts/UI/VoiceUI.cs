using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI controls for voice system
/// </summary>
public class VoiceUI : MonoBehaviour
{
    public Button toggleTTSButton;
    public Button toggleSTTButton;
    public Button micButton;
    public Text statusText;
    
    private VoiceSystem voiceSystem;
    
    void Start()
    {
        voiceSystem = FindFirstObjectByType<VoiceSystem>();
        
        if (toggleTTSButton != null)
            toggleTTSButton.onClick.AddListener(() => ToggleTTS());
        
        if (toggleSTTButton != null)
            toggleSTTButton.onClick.AddListener(() => ToggleSTT());
        
        if (micButton != null)
            micButton.onClick.AddListener(() => StartVoiceInput());
    }
    
    void ToggleTTS()
    {
        if (voiceSystem != null)
        {
            voiceSystem.ToggleTTS(!voiceSystem.IsSpeaking);
            UpdateStatus("TTS Toggled");
        }
    }
    
    void ToggleSTT()
    {
        if (voiceSystem != null)
        {
            UpdateStatus("STT Toggled");
        }
    }
    
    void StartVoiceInput()
    {
        if (voiceSystem != null)
        {
            UpdateStatus("Listening...");
            voiceSystem.StartListening((text) => {
                UpdateStatus($"Heard: {text}");
                // Send to DialogueUI
                // Process via TextAnalyzer and trigger action
                var analyzer = FindFirstObjectByType<TextAnalyzer>();
                if (analyzer != null)
                {
                    var (action, _) = analyzer.AnalyzeText(text);
                    
                    // Find PlayModeManager and trigger action
                    var playManager = FindFirstObjectByType<PlayModeManager>();
                    if (playManager != null)
                    {
                        playManager.SendMessage("HandlePlayerAction", action, SendMessageOptions.DontRequireReceiver);
                    }
                }
            });
        }
    }
    
    void UpdateStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}

