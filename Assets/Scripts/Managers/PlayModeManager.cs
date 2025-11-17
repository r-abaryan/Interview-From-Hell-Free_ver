using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// Simple manager for Play Mode ONLY - no training logic
/// </summary>
public class PlayModeManager : MonoBehaviour
{
    [Header("References")]
    public TeenAgent teenAgent;
    public DialogueUI dialogueUI;
    public TeenAnimationController animationController;
    public MemorySystem memorySystem;
    public VoiceSystem voiceSystem;
    
    [Header("Current Scenario")]
    public ScenarioType currentScenario;
    
    private DialogueDatabase dialogueDatabase;
    private bool conversationStarted = false;
    
    private void Start()
    {
        // Completely disable training components
        if (teenAgent != null)
        {
            var decisionRequester = teenAgent.GetComponent<Unity.MLAgents.DecisionRequester>();
            if (decisionRequester != null)
            {
                decisionRequester.enabled = false;
                // Debug.Log("✅ Decision Requester DISABLED");
            }
            
            // Set to heuristic only
            var behaviorParams = teenAgent.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>();
            if (behaviorParams != null)
            {
                behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
                // Debug.Log("✅ Set to Heuristic Only");
            }
            
            // Disable agent itself during play
            teenAgent.enabled = false;
            // Debug.Log("✅ TeenAgent component DISABLED");
        }
        
        // Setup dialogue database
        dialogueDatabase = gameObject.AddComponent<DialogueDatabase>();
        dialogueDatabase.Initialize();
        
        // Setup memory system
        if (memorySystem == null)
        {
            memorySystem = FindFirstObjectByType<MemorySystem>();
            if (memorySystem == null)
            {
                memorySystem = gameObject.AddComponent<MemorySystem>();
                Debug.Log("✅ Memory System created");
            }
        }
        
        // Setup voice system
        if (voiceSystem == null)
        {
            voiceSystem = FindFirstObjectByType<VoiceSystem>();
            if (voiceSystem == null)
            {
                voiceSystem = gameObject.AddComponent<VoiceSystem>();
                Debug.Log("✅ Voice System created");
            }
        }
        
        // Find animation controller if not assigned
        if (animationController == null && teenAgent != null)
        {
            animationController = teenAgent.GetComponent<TeenAnimationController>();
            if (animationController == null)
            {
                Debug.LogWarning("⚠️ No TeenAnimationController found. Animations will be disabled.");
            }
        }
        
        // Setup UI
        if (dialogueUI != null)
        {
            dialogueUI.OnPlayerActionSelected += HandlePlayerAction;
            // Debug.Log("✅ Play Mode Ready! Click a button to start.");
        }
        
        // Random scenario
        currentScenario = (ScenarioType)Random.Range(0, System.Enum.GetValues(typeof(ScenarioType)).Length);
    }
    
    private void HandlePlayerAction(PlayerActionType actionType)
    {
        if (!conversationStarted)
        {
            StartConversation();
        }
        
        // Show player dialogue
        string playerDialogue = dialogueDatabase.GetPlayerDialogue(actionType, currentScenario);
        if (dialogueUI != null)
        {
            dialogueUI.ShowPlayerDialogue(playerDialogue);
        }
        
        // Update emotional state
        UpdateEmotionalState(actionType);
        
        // Update animation based on emotional state
        if (animationController != null)
        {
            animationController.PlayEmotionAnimation(teenAgent.emotionalState.currentEmotion);
        }
        
        // Check for relevant memories
        string memoryReference = "";
        if (memorySystem != null)
        {
            MemorySystem.Memory relevantMemory = memorySystem.GetRelevantMemory(currentScenario, teenAgent.emotionalState);
            if (relevantMemory != null)
            {
                memoryReference = memorySystem.GetMemoryDialogue(relevantMemory);
            }
        }
        
        // Get teen response manually (no agent decision)
        TeenAgent.TeenResponse response = ChooseTeenResponse();
        
        // Trigger response animation
        if (animationController != null)
        {
            animationController.StartTalking();
            TriggerResponseAnimation(response);
            animationController.PlayContextualReaction(teenAgent.emotionalState.currentEmotion, response);
            animationController.PlayReactionToPlayerAction(actionType, teenAgent.emotionalState.relationshipLevel);
        }
        
        // Get teen dialogue
        string teenDialogue = dialogueDatabase.GetTeenResponseDialogue(response, currentScenario, teenAgent.emotionalState.currentEmotion);
        
        // Add memory reference if available
        if (!string.IsNullOrEmpty(memoryReference))
        {
            teenDialogue = $"{memoryReference} {teenDialogue}";
        }
        
        // Show teen response
        if (dialogueUI != null)
        {
            dialogueUI.ShowTeenDialogue(teenDialogue, teenAgent.emotionalState.currentEmotion);
        }
        
        // Speak dialogue (TTS)
        if (voiceSystem != null)
        {
            voiceSystem.SpeakText(teenDialogue, teenAgent.emotionalState.currentEmotion);
        }
        
        // Record this interaction in memory
        if (memorySystem != null)
        {
            memorySystem.RecordInteraction(actionType, response, teenAgent.emotionalState, currentScenario);
        }
        
        // Stop talking animation after dialogue
        if (animationController != null)
        {
            Invoke(nameof(StopTalkingAnimation), 2f); // Stop after 2 seconds
        }
        
        // Check if conversation ends
        if (IsConversationEnding(response))
        {
            EndConversation(response);
        }
        else
        {
            // Conversation continues - show options again!
            if (dialogueUI != null)
            {
                dialogueUI.ShowPlayerOptions(currentScenario);
            }
        }
        
        Debug.Log($"Player: {actionType} | Teen: {response} | Mood: {teenAgent.emotionalState.currentMood:F0}");
    }
    
    private void StartConversation()
    {
        conversationStarted = true;
        
        // Initialize emotional state
        if (teenAgent != null && teenAgent.emotionalState == null)
        {
            teenAgent.emotionalState = new EmotionalState();
        }
        
        // Show opening line
        string opening = dialogueDatabase.GetTeenOpeningLine(currentScenario, teenAgent.emotionalState);
        
        // Check if teen wants to reference a past interaction
        if (memorySystem != null)
        {
            MemorySystem.Memory openingMemory = memorySystem.GetRelevantMemory(currentScenario, teenAgent.emotionalState);
            if (openingMemory != null && Random.value < 0.3f) // 30% chance to mention memory
            {
                string memoryRef = memorySystem.GetMemoryDialogue(openingMemory);
                opening = $"{memoryRef} {opening}";
            }
        }
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowTeenDialogue(opening, teenAgent.emotionalState.currentEmotion);
            dialogueUI.ShowPlayerOptions(currentScenario);
        }
        
        // Speak opening (TTS)
        if (voiceSystem != null)
        {
            voiceSystem.SpeakText(opening, teenAgent.emotionalState.currentEmotion);
        }
        
        // Start talking animation
        if (animationController != null)
        {
            animationController.StartTalking();
            Invoke(nameof(StopTalkingAnimation), 2f);
        }
    }
    
    private void UpdateEmotionalState(PlayerActionType action)
    {
        if (teenAgent == null || teenAgent.emotionalState == null) return;
        
        float relationshipChange = 0f;
        float moodChange = 0f;
        float respectChange = 0f;
        bool wasRespectful = false;
        
        switch (action)
        {
            case PlayerActionType.Authoritarian:
                relationshipChange = -5f;
                moodChange = -10f;
                respectChange = -8f;
                break;
                
            case PlayerActionType.Empathetic:
                relationshipChange = 8f;
                moodChange = 10f;
                respectChange = 10f;
                wasRespectful = true;
                break;
                
            case PlayerActionType.Logical:
                relationshipChange = 2f;
                moodChange = 3f;
                respectChange = 5f;
                wasRespectful = true;
                break;
                
            case PlayerActionType.Bribery:
                relationshipChange = -3f;
                moodChange = 5f;
                respectChange = -5f;
                break;
                
            case PlayerActionType.GuiltTrip:
                relationshipChange = -8f;
                moodChange = -8f;
                respectChange = -10f;
                break;
                
            case PlayerActionType.Listen:
                relationshipChange = 10f;
                moodChange = 8f;
                respectChange = 12f;
                wasRespectful = true;
                break;
                
            case PlayerActionType.Compromise:
                relationshipChange = 7f;
                moodChange = 10f;
                respectChange = 10f;
                wasRespectful = true;
                break;
        }
        
        teenAgent.emotionalState.UpdateFromInteraction(relationshipChange, moodChange, respectChange, wasRespectful);
    }
    
    private TeenAgent.TeenResponse ChooseTeenResponse()
    {
        if (teenAgent == null) return TeenAgent.TeenResponse.Dismissive;
        
        var state = teenAgent.emotionalState;
        
        // Rule-based response selection
        if (state.currentMood > 30f && state.relationshipLevel > 20f)
            return TeenAgent.TeenResponse.Compliant;
        else if (state.currentMood < -40f)
            return TeenAgent.TeenResponse.Angry;
        else if (state.respectReceived < 40f && state.autonomyNeed > 70f)
            return TeenAgent.TeenResponse.Defiant;
        else if (state.currentMood < -20f)
            return TeenAgent.TeenResponse.Sarcastic;
        else if (state.trustLevel > 50f && state.currentMood > 0f)
            return TeenAgent.TeenResponse.NegotiateCalm;
        else if (state.currentMood < -10f)
            return TeenAgent.TeenResponse.EmotionalPlead;
        else if (state.trustLevel > 40f)
            return TeenAgent.TeenResponse.ReasonableRefusal;
        else
            return TeenAgent.TeenResponse.Dismissive;
    }
    
    private bool IsConversationEnding(TeenAgent.TeenResponse response)
    {
        return response == TeenAgent.TeenResponse.Compliant ||
               response == TeenAgent.TeenResponse.Angry ||
               response == TeenAgent.TeenResponse.Defiant;
    }
    
    private void EndConversation(TeenAgent.TeenResponse finalResponse)
    {
        string message = "";
        bool success = false;
        
        if (finalResponse == TeenAgent.TeenResponse.Compliant)
        {
            message = "Success! The teen agreed.";
            success = true;
            
            // Play success animation
            if (animationController != null)
            {
                animationController.PlayCelebration();
            }
        }
        else if (finalResponse == TeenAgent.TeenResponse.Angry)
        {
            message = "The teen got angry and left.";
            
            // Play angry animation
            if (animationController != null)
            {
                animationController.PlayStormOff();
            }
        }
        else if (finalResponse == TeenAgent.TeenResponse.Defiant)
        {
            message = "The teen refused completely.";
            
            // Play defiant animation
            if (animationController != null)
            {
                animationController.PlayCrossArms();
            }
        }
        
        if (dialogueUI != null)
        {
            dialogueUI.ShowOutcome(message, success);
            dialogueUI.HidePlayerOptions();
        }
        
        conversationStarted = false;
        
        // Restart after delay
        Invoke(nameof(RestartConversation), 3f);
    }
    
    private void RestartConversation()
    {
        // Reset state
        if (teenAgent != null)
        {
            teenAgent.emotionalState = new EmotionalState
            {
                relationshipLevel = Random.Range(-30f, 50f),
                currentMood = Random.Range(-40f, 40f),
                trustLevel = Random.Range(20f, 70f),
                stressLevel = Random.Range(20f, 70f),
                autonomyNeed = Random.Range(60f, 90f),
                respectReceived = Random.Range(30f, 70f)
            };
            teenAgent.emotionalState.UpdateCurrentEmotion();
        }
        
        // New scenario
        currentScenario = (ScenarioType)Random.Range(0, System.Enum.GetValues(typeof(ScenarioType)).Length);
        
        // Start new conversation
        StartConversation();
    }
    
    /// <summary>
    /// Trigger specific animations based on teen response
    /// </summary>
    private void TriggerResponseAnimation(TeenAgent.TeenResponse response)
    {
        if (animationController == null) return;
        
        switch (response)
        {
            case TeenAgent.TeenResponse.Compliant:
                animationController.PlayCelebration();
                break;
                
            case TeenAgent.TeenResponse.Angry:
                animationController.PlayStormOff();
                break;
                
            case TeenAgent.TeenResponse.Defiant:
                animationController.PlayCrossArms();
                break;
                
            case TeenAgent.TeenResponse.Sarcastic:
            case TeenAgent.TeenResponse.Dismissive:
                animationController.PlayShrug();
                break;
                
            case TeenAgent.TeenResponse.NegotiateCalm:
            case TeenAgent.TeenResponse.EmotionalPlead:
            case TeenAgent.TeenResponse.ReasonableRefusal:
            default:
                // Just use talking animation
                break;
        }
    }
    
    /// <summary>
    /// Stop talking animation
    /// </summary>
    private void StopTalkingAnimation()
    {
        if (animationController != null)
        {
            animationController.StopTalking();
        }
    }
}

