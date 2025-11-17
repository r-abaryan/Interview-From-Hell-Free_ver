using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the conversation flow between player and teen
/// </summary>
public class ConversationManager : MonoBehaviour
{
    [Header("References")]
    public TeenAgent teenAgent;
    public PlayerController playerController;
    public DialogueUI dialogueUI;
    
    [Header("Conversation State")]
    public bool conversationActive = false;
    public int turnCount = 0;
    
    [Header("Dialogue Database")]
    public DialogueDatabase dialogueDatabase;
    
    private List<string> conversationHistory = new List<string>();
    
    private void Start()
    {
        if (dialogueDatabase == null)
        {
            dialogueDatabase = gameObject.AddComponent<DialogueDatabase>();
            dialogueDatabase.Initialize();
        }
        
        if (dialogueUI != null)
        {
            dialogueUI.OnPlayerActionSelected += HandlePlayerActionSelected;
        }
    }
    
    /// <summary>
    /// Start a new conversation scenario
    /// </summary>
    public void StartConversation(ScenarioType scenario)
    {
        conversationActive = true;
        turnCount = 0;
        conversationHistory.Clear();
        
        // Get opening line from teen
        string teenOpening = dialogueDatabase.GetTeenOpeningLine(scenario, teenAgent.emotionalState);
        
        // Display in UI
        if (dialogueUI != null)
        {
            dialogueUI.ShowTeenDialogue(teenOpening, teenAgent.emotionalState.currentEmotion);
            dialogueUI.ShowPlayerOptions(scenario);
        }
        
        conversationHistory.Add($"Teen: {teenOpening}");
        
        Debug.Log($"Conversation started: {scenario}");
    }
    
    /// <summary>
    /// Handle when player selects an action
    /// </summary>
    private void HandlePlayerActionSelected(PlayerActionType actionType)
    {
        // Start conversation if not active yet
        if (!conversationActive && teenAgent != null)
        {
            StartConversation(teenAgent.currentScenario);
            Debug.Log("Conversation started by player action!");
        }
        
        if (!conversationActive) return;
        
        turnCount++;
        
        // Get player dialogue based on action
        string playerDialogue = dialogueDatabase.GetPlayerDialogue(actionType, teenAgent.currentScenario);
        conversationHistory.Add($"Player: {playerDialogue}");
        
        // Show player's chosen dialogue
        if (dialogueUI != null)
        {
            dialogueUI.ShowPlayerDialogue(playerDialogue);
        }
        
        // Notify teen agent of player action (this will trigger teen's response)
        teenAgent.OnPlayerAction(actionType);
    }
    
    /// <summary>
    /// Called by TeenAgent when it generates a response
    /// </summary>
    public void OnTeenResponse(TeenAgent.TeenResponse response, EmotionalState.Emotion emotion)
    {
        // Get appropriate dialogue for the response
        string teenDialogue = dialogueDatabase.GetTeenResponseDialogue(response, teenAgent.currentScenario, emotion);
        conversationHistory.Add($"Teen: {teenDialogue}");
        
        // Display teen response
        if (dialogueUI != null)
        {
            dialogueUI.ShowTeenDialogue(teenDialogue, emotion);
            
            // Check if conversation should continue
            if (ShouldContinueConversation(response))
            {
                dialogueUI.ShowPlayerOptions(teenAgent.currentScenario);
            }
            else
            {
                EndConversation(response);
            }
        }
        
        Debug.Log($"Teen Response: {response} | Emotion: {emotion} | Relationship: {teenAgent.emotionalState.relationshipLevel:F1}");
    }
    
    /// <summary>
    /// Check if conversation should continue or end
    /// </summary>
    private bool ShouldContinueConversation(TeenAgent.TeenResponse response)
    {
        // End on these responses
        if (response == TeenAgent.TeenResponse.Compliant || 
            response == TeenAgent.TeenResponse.Angry || 
            response == TeenAgent.TeenResponse.Defiant)
        {
            return false;
        }
        
        // End if too many turns
        if (turnCount >= teenAgent.maxInteractionsPerEpisode)
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// End the current conversation
    /// </summary>
    private void EndConversation(TeenAgent.TeenResponse finalResponse)
    {
        conversationActive = false;
        
        // Show outcome message
        if (dialogueUI != null)
        {
            string outcomeMessage = GetOutcomeMessage(finalResponse);
            dialogueUI.ShowOutcome(outcomeMessage, teenAgent.episodeEndedWell);
        }
        
        Debug.Log($"Conversation ended. Outcome: {finalResponse} | Relationship: {teenAgent.emotionalState.relationshipLevel:F1}");
        
        // Only auto-restart in Training mode, NOT in Play mode
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.currentMode == GameManager.GameMode.Training)
        {
            Invoke(nameof(StartNewEpisode), 3f);
        }
        else
        {
            Debug.Log("Conversation ended. Press 'R' or start new conversation manually.");
        }
    }
    
    /// <summary>
    /// Get outcome message based on how conversation ended
    /// </summary>
    private string GetOutcomeMessage(TeenAgent.TeenResponse finalResponse)
    {
        switch (finalResponse)
        {
            case TeenAgent.TeenResponse.Compliant:
                return teenAgent.emotionalState.relationshipLevel > 40f 
                    ? "Success! The teen agreed and feels good about it." 
                    : "The teen agreed, but seems reluctant.";
                
            case TeenAgent.TeenResponse.Angry:
                return "The teen stormed off angry. The relationship suffered.";
                
            case TeenAgent.TeenResponse.Defiant:
                return "The teen refused and is being defiant. This didn't go well.";
                
            default:
                return "The conversation ended inconclusively.";
        }
    }
    
    /// <summary>
    /// Start a new episode
    /// </summary>
    private void StartNewEpisode()
    {
        if (teenAgent != null)
        {
            teenAgent.EndEpisode();
            StartConversation(teenAgent.currentScenario);
        }
    }
    
    /// <summary>
    /// Reset conversation state
    /// </summary>
    public void ResetConversation()
    {
        conversationActive = false;
        turnCount = 0;
        conversationHistory.Clear();
    }
    
    /// <summary>
    /// Get full conversation history (useful for debugging/analysis)
    /// </summary>
    public string GetConversationHistory()
    {
        return string.Join("\n", conversationHistory);
    }
}

