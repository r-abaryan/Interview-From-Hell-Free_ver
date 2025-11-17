using UnityEngine;

/// <summary>
/// Handles player input and action selection
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public TeenAgent teenAgent;
    public ConversationManager conversationManager;
    public DialogueUI dialogueUI;
    
    [Header("Input Settings")]
    public bool allowKeyboardInput = true;
    
    private void Start()
    {
        // Subscribe to UI button clicks
        if (dialogueUI != null)
        {
            dialogueUI.OnPlayerActionSelected += HandlePlayerAction;
        }
    }
    
    private void Update()
    {
        // Keyboard input disabled to avoid Input System conflicts
        // Enable this if you configure the new Input System or switch to legacy input
        /*
        if (allowKeyboardInput && conversationManager.conversationActive)
        {
            HandleKeyboardInput();
        }
        */
    }
    
    /// <summary>
    /// Handle player action selection
    /// </summary>
    private void HandlePlayerAction(PlayerActionType action)
    {
        if (teenAgent != null && conversationManager.conversationActive)
        {
            Debug.Log($"Player chose: {action}");
            // The conversation manager will handle notifying the teen agent
        }
    }
    
    /// <summary>
    /// Keyboard shortcuts for quick testing
    /// </summary>
    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerPlayerAction(PlayerActionType.Authoritarian);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TriggerPlayerAction(PlayerActionType.Empathetic);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TriggerPlayerAction(PlayerActionType.Logical);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TriggerPlayerAction(PlayerActionType.Bribery);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TriggerPlayerAction(PlayerActionType.GuiltTrip);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            TriggerPlayerAction(PlayerActionType.Listen);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            TriggerPlayerAction(PlayerActionType.Compromise);
        }
    }
    
    /// <summary>
    /// Trigger a player action (can be called from UI or keyboard)
    /// </summary>
    public void TriggerPlayerAction(PlayerActionType action)
    {
        if (conversationManager != null && conversationManager.conversationActive)
        {
            // UI will handle the actual flow through the event system
            if (dialogueUI != null)
            {
                dialogueUI.TriggerAction(action);
            }
        }
    }
    
    /// <summary>
    /// Start a new conversation (called at start or after episode ends)
    /// </summary>
    public void InitiateConversation()
    {
        if (conversationManager != null && teenAgent != null)
        {
            conversationManager.StartConversation(teenAgent.currentScenario);
        }
    }
}

