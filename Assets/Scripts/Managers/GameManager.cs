using UnityEngine;

/// <summary>
/// Main game manager - coordinates all systems
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Core References")]
    public TeenAgent teenAgent;
    public PlayerController playerController;
    public ConversationManager conversationManager;
    public ScenarioManager scenarioManager;
    public DialogueUI dialogueUI;
    
    [Header("Game Mode")]
    public GameMode currentMode = GameMode.Play;  // Changed to Play mode for testing
    
    [Header("Training Settings")]
    public bool autoStartEpisodes = true;
    public float episodeStartDelay = 1f;
    
    [Header("Play Mode Settings")]
    public bool showTips = true;
    public bool showEmotionalState = true;
    
    public enum GameMode
    {
        Training,    // ML-Agents training mode
        Play,        // Human player mode
        Demo         // Watch AI play
    }
    
    private void Start()
    {
        InitializeGame();
    }
    
    private void InitializeGame()
    {
        // Find components if not assigned
        if (teenAgent == null) teenAgent = FindFirstObjectByType<TeenAgent>();
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (conversationManager == null) conversationManager = FindFirstObjectByType<ConversationManager>();
        if (scenarioManager == null) scenarioManager = FindFirstObjectByType<ScenarioManager>();
        if (dialogueUI == null) dialogueUI = FindFirstObjectByType<DialogueUI>();
        
        // Verify critical components
        if (teenAgent == null)
        {
            Debug.LogError("GameManager: TeenAgent not found! Please add TeenAgent component to the scene.");
            return;
        }
        
        if (conversationManager == null)
        {
            Debug.LogError("GameManager: ConversationManager not found!");
            return;
        }
        
        // Setup based on mode FIRST (before starting episodes)
        switch (currentMode)
        {
            case GameMode.Training:
                SetupTrainingMode();
                break;
            case GameMode.Play:
                SetupPlayMode();
                break;
            case GameMode.Demo:
                SetupDemoMode();
                break;
        }
        
        // Start first episode ONLY in Training mode
        if (autoStartEpisodes && currentMode == GameMode.Training)
        {
            Invoke(nameof(StartFirstEpisode), episodeStartDelay);
        }
        else if (currentMode == GameMode.Play)
        {
            // In Play mode, DON'T auto-start - wait for manual trigger or button press
            Debug.Log("✅ Play Mode Ready. Waiting for player to start conversation...");
        }
        
        Debug.Log($"GameManager initialized in {currentMode} mode");
    }
    
    private void SetupTrainingMode()
    {
        // In training mode, episodes auto-reset
        Debug.Log("Training Mode: ML-Agent will learn through reinforcement learning");
        
        // Optionally disable UI for faster training
        if (dialogueUI != null && !showEmotionalState)
        {
            // dialogueUI.gameObject.SetActive(false);
        }
    }
    
    private void SetupPlayMode()
    {
        // In play mode, human controls everything
        Debug.Log("Play Mode: Human player vs AI Teen");
        
        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(true);
        }
        
        // Disable teen agent's training behavior
        if (teenAgent != null)
        {
            teenAgent.MaxStep = 0; // Disable step limit
            
            // DISABLE auto decision requesting - wait for player input
            var decisionRequester = teenAgent.GetComponent<Unity.MLAgents.DecisionRequester>();
            if (decisionRequester != null)
            {
                decisionRequester.enabled = false;
                Debug.Log("✅ Decision Requester DISABLED - Agent waits for player");
            }
        }
    }
    
    private void SetupDemoMode()
    {
        // In demo mode, watch the trained AI play
        Debug.Log("Demo Mode: Watching trained AI");
        
        if (dialogueUI != null)
        {
            dialogueUI.gameObject.SetActive(true);
        }
    }
    
    private void StartFirstEpisode()
    {
        if (conversationManager != null && teenAgent != null)
        {
            conversationManager.StartConversation(teenAgent.currentScenario);
        }
    }
    
    private void Update()
    {
        // Update UI with current emotional state
        if (showEmotionalState && dialogueUI != null && teenAgent != null)
        {
            dialogueUI.UpdateEmotionalStateDisplay(teenAgent.emotionalState);
        }
        
        // Debug commands (disabled during training to avoid Input System conflicts)
        // Uncomment if using old Input System or configure new Input System
        /*
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartEpisode();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMode();
        }
        */
    }
    
    /// <summary>
    /// Restart current episode
    /// </summary>
    public void RestartEpisode()
    {
        if (teenAgent != null)
        {
            Debug.Log("Restarting episode...");
            teenAgent.EndEpisode();
            Invoke(nameof(StartFirstEpisode), 0.5f);
        }
    }
    
    /// <summary>
    /// Toggle between game modes
    /// </summary>
    public void ToggleMode()
    {
        currentMode = (GameMode)(((int)currentMode + 1) % 3);
        Debug.Log($"Switched to {currentMode} mode");
        InitializeGame();
    }
    
    /// <summary>
    /// Get current game statistics
    /// </summary>
    public string GetGameStats()
    {
        if (scenarioManager != null)
        {
            return $"Success Rate: {scenarioManager.GetSuccessRate():F1}% " +
                   $"({scenarioManager.successfulOutcomes}/{scenarioManager.totalScenariosGenerated} scenarios)";
        }
        return "No statistics available";
    }
    
    private void OnGUI()
    {
        // Debug overlay (always show in Training mode, disable F1 toggle due to Input System)
        if (currentMode == GameMode.Training)
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));
            GUILayout.Box("=== Teen Persuasion ML-Agents ===");
            GUILayout.Label($"Mode: {currentMode}");
            GUILayout.Label($"Scenario: {teenAgent?.currentScenario}");
            GUILayout.Label($"Relationship: {teenAgent?.emotionalState.relationshipLevel:F0}");
            GUILayout.Label($"Mood: {teenAgent?.emotionalState.currentMood:F0}");
            GUILayout.Label($"Emotion: {teenAgent?.emotionalState.currentEmotion}");
            GUILayout.Label($"Interactions: {teenAgent?.totalInteractionsThisEpisode}/{teenAgent?.maxInteractionsPerEpisode}");
            
            if (scenarioManager != null)
            {
                GUILayout.Label(GetGameStats());
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("R - Restart Episode");
            GUILayout.Label("T - Toggle Mode");
            GUILayout.Label("1-7 - Player Actions (when options shown)");
            GUILayout.Label("F1 - Toggle this debug info");
            
            GUILayout.EndArea();
        }
    }
}

