using UnityEngine;

/// <summary>
/// One-click setup for Interview game - creates everything automatically
/// </summary>
public class InterviewSetup : MonoBehaviour
{
    [ContextMenu("Setup Complete Interview Scene")]
    public void SetupCompleteScene()
    {
        Debug.Log("🚀 Setting up Interview Scene...");
        
        // 1. Create InterviewManager with all components
        GameObject manager = CreateInterviewManager();
        
        // 2. Build UI
        GameObject uiBuilder = new GameObject("UIBuilder");
        InterviewUIBuilder builder = uiBuilder.AddComponent<InterviewUIBuilder>();
        builder.BuildUI();
        
        // 3. Link UI to InterviewerAI
        InterviewerAI interviewer = manager.GetComponent<InterviewerAI>();
        InterviewUI ui = FindFirstObjectByType<InterviewUI>();
        
        if (interviewer != null && ui != null)
        {
            // UI is already linked via InterviewUI component
            Debug.Log("✅ UI linked to InterviewerAI");
        }
        
        Debug.Log("✅ Complete Interview Scene Setup Done!");
        Debug.Log("📝 Next Steps:");
        Debug.Log("   1. Press Play");
        Debug.Log("   2. Click 'START INTERVIEW'");
        Debug.Log("   3. Answer questions with your voice!");
    }
    
    private GameObject CreateInterviewManager()
    {
        GameObject manager = GameObject.Find("InterviewManager");
        
        if (manager == null)
        {
            manager = new GameObject("InterviewManager");
        }
        
        // Add all required components
        if (manager.GetComponent<InterviewerAI>() == null)
            manager.AddComponent<InterviewerAI>();
        
        if (manager.GetComponent<WhisperSTT>() == null)
            manager.AddComponent<WhisperSTT>();
        
        if (manager.GetComponent<VoiceAnalyzer>() == null)
            manager.AddComponent<VoiceAnalyzer>();
        
        if (manager.GetComponent<SentimentAnalyzer>() == null)
            manager.AddComponent<SentimentAnalyzer>();
        
        if (manager.GetComponent<LLMManager>() == null)
            manager.AddComponent<LLMManager>();
        
        if (manager.GetComponent<QuestionManager>() == null)
            manager.AddComponent<QuestionManager>();
        
        if (manager.GetComponent<VoiceSystem>() == null)
            manager.AddComponent<VoiceSystem>();
        
        // Link references in InterviewerAI
        InterviewerAI interviewer = manager.GetComponent<InterviewerAI>();
        if (interviewer != null)
        {
            interviewer.whisperSTT = manager.GetComponent<WhisperSTT>();
            interviewer.voiceAnalyzer = manager.GetComponent<VoiceAnalyzer>();
            interviewer.sentimentAnalyzer = manager.GetComponent<SentimentAnalyzer>();
            interviewer.llmManager = manager.GetComponent<LLMManager>();
            interviewer.voiceSystem = manager.GetComponent<VoiceSystem>();
        }
        
        Debug.Log("✅ InterviewManager created with all components");
        
        return manager;
    }
}

