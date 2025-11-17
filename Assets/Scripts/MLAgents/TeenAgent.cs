using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// ML-Agent representing a teenager that learns realistic responses to player interactions
/// </summary>
public class TeenAgent : Agent
{
    [Header("Emotional State")]
    public EmotionalState emotionalState = new EmotionalState();
    
    [Header("Current Scenario")]
    public ScenarioType currentScenario = ScenarioType.GoToSchool;
    public float scenarioStartTime;
    
    [Header("Interaction Tracking")]
    public PlayerActionType lastPlayerAction;
    public int totalInteractionsThisEpisode = 0;
    public bool hasComplied = false;
    public bool episodeEndedWell = false;
    
    [Header("References")]
    public ConversationManager conversationManager;
    
    [Header("Training Parameters")]
    [Tooltip("Maximum interactions per episode")]
    public int maxInteractionsPerEpisode = 10;
    
    [Tooltip("Reward for compliance with good relationship")]
    public float complianceReward = 1.0f;
    
    [Tooltip("Reward for maintaining good relationship")]
    public float relationshipReward = 0.5f;
    
    [Tooltip("Penalty for poor relationship")]
    public float poorRelationshipPenalty = -0.5f;
    
    // Teen's possible response types
    public enum TeenResponse
    {
        Compliant,          // "Okay, I'll go"
        NegotiateCalm,      // "Can we talk about this?"
        Sarcastic,          // "Oh sure, whatever you say..."
        Angry,              // "I don't want to! Leave me alone!"
        Dismissive,         // "Yeah, yeah, I heard you"
        EmotionalPlead,     // "Please, I really don't want to"
        Defiant,            // "You can't make me!"
        ReasonableRefusal   // "I understand, but I have a good reason..."
    }
    
    public TeenResponse currentResponse;
    
    /// <summary>
    /// Initialize agent at start of episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset or randomize emotional state for training variety
        emotionalState = new EmotionalState
        {
            relationshipLevel = Random.Range(-30f, 50f),
            currentMood = Random.Range(-40f, 40f),
            trustLevel = Random.Range(20f, 70f),
            stressLevel = Random.Range(20f, 70f),
            autonomyNeed = Random.Range(60f, 90f),  // Teens naturally want autonomy
            respectReceived = Random.Range(30f, 70f),
            tiredness = Random.Range(10f, 80f),
            hunger = Random.Range(10f, 60f)
        };
        
        emotionalState.UpdateCurrentEmotion();
        
        // Randomize scenario
        currentScenario = (ScenarioType)Random.Range(0, System.Enum.GetValues(typeof(ScenarioType)).Length);
        
        // Reset episode tracking
        totalInteractionsThisEpisode = 0;
        hasComplied = false;
        episodeEndedWell = false;
        scenarioStartTime = Time.time;
        
        if (conversationManager != null)
        {
            conversationManager.ResetConversation();
        }
        
        Debug.Log($"New Episode: {currentScenario} | Relationship: {emotionalState.relationshipLevel:F1} | Mood: {emotionalState.currentMood:F1}");
    }
    
    /// <summary>
    /// Collect observations for the neural network
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Emotional state observations (11 values)
        float[] emotionObservations = emotionalState.GetObservationArray();
        foreach (float obs in emotionObservations)
        {
            sensor.AddObservation(obs);
        }
        
        // Scenario type (one-hot encoding, 6 values)
        for (int i = 0; i < System.Enum.GetValues(typeof(ScenarioType)).Length; i++)
        {
            sensor.AddObservation(i == (int)currentScenario ? 1f : 0f);
        }
        
        // Last player action (one-hot encoding, 7 values)
        for (int i = 0; i < System.Enum.GetValues(typeof(PlayerActionType)).Length; i++)
        {
            sensor.AddObservation(i == (int)lastPlayerAction ? 1f : 0f);
        }
        
        // Interaction count normalized
        sensor.AddObservation((float)totalInteractionsThisEpisode / maxInteractionsPerEpisode);
        
        // Memory System Observations (8 values) - NEW!
        MemorySystem memorySystem = FindFirstObjectByType<MemorySystem>();
        if (memorySystem != null)
        {
            float[] memoryObs = memorySystem.GetMemoryObservations();
            foreach (float obs in memoryObs)
            {
                sensor.AddObservation(obs);
            }
        }
        else
        {
            // No memory system - add zeros
            for (int i = 0; i < 8; i++)
            {
                sensor.AddObservation(0f);
            }
        }
        
        // Total observations: 11 + 6 + 7 + 1 + 8 = 33
    }
    
    /// <summary>
    /// Execute actions chosen by the neural network
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get discrete action (which response to give)
        int responseIndex = actions.DiscreteActions[0];
        currentResponse = (TeenResponse)responseIndex;
        
        // Apply the response and calculate rewards
        ApplyTeenResponse(currentResponse);
    }
    
    /// <summary>
    /// For manual testing - simple rule-based responses when no model is loaded
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        
        // Simple rule-based behavior based on emotional state
        if (emotionalState.currentMood > 30f && emotionalState.relationshipLevel > 20f)
        {
            discreteActions[0] = 0; // Compliant - good mood and relationship
        }
        else if (emotionalState.currentMood < -40f)
        {
            discreteActions[0] = 3; // Angry - very bad mood
        }
        else if (emotionalState.respectReceived < 40f && emotionalState.autonomyNeed > 70f)
        {
            discreteActions[0] = 6; // Defiant - feeling disrespected
        }
        else if (emotionalState.currentMood < -20f)
        {
            discreteActions[0] = 2; // Sarcastic - annoyed
        }
        else if (emotionalState.trustLevel > 50f)
        {
            discreteActions[0] = 1; // Negotiate Calm - decent trust
        }
        else
        {
            discreteActions[0] = 4; // Dismissive - default teen response
        }
        
        /* Uncomment if using legacy Input System:
        if (Input.GetKey(KeyCode.Alpha1)) discreteActions[0] = 0; // Compliant
        else if (Input.GetKey(KeyCode.Alpha2)) discreteActions[0] = 1; // NegotiateCalm
        else if (Input.GetKey(KeyCode.Alpha3)) discreteActions[0] = 2; // Sarcastic
        else if (Input.GetKey(KeyCode.Alpha4)) discreteActions[0] = 3; // Angry
        else if (Input.GetKey(KeyCode.Alpha5)) discreteActions[0] = 4; // Dismissive
        else if (Input.GetKey(KeyCode.Alpha6)) discreteActions[0] = 5; // EmotionalPlead
        else if (Input.GetKey(KeyCode.Alpha7)) discreteActions[0] = 6; // Defiant
        else if (Input.GetKey(KeyCode.Alpha8)) discreteActions[0] = 7; // ReasonableRefusal
        else discreteActions[0] = 0; // Default to compliant
        */
    }
    
    /// <summary>
    /// Apply the teen's response and calculate rewards
    /// </summary>
    private void ApplyTeenResponse(TeenResponse response)
    {
        totalInteractionsThisEpisode++;
        
        float reward = 0f;
        bool endsEpisode = false;
        
        // Reward structure based on appropriateness of response to emotional state and player action
        switch (response)
        {
            case TeenResponse.Compliant:
                // Good if relationship is positive and player was respectful
                if (emotionalState.relationshipLevel > 20f && IsPlayerActionRespectful(lastPlayerAction))
                {
                    reward += complianceReward;
                    hasComplied = true;
                    endsEpisode = true;
                    episodeEndedWell = true;
                }
                // Bad if relationship is poor (unrealistic to comply immediately)
                else if (emotionalState.relationshipLevel < -20f)
                {
                    reward -= 0.3f;  // Unrealistic behavior penalty
                }
                break;
                
            case TeenResponse.NegotiateCalm:
                // Good response if mood is reasonable
                if (emotionalState.currentMood > -30f && emotionalState.trustLevel > 40f)
                {
                    reward += 0.3f;
                }
                break;
                
            case TeenResponse.Sarcastic:
                // Appropriate if annoyed but not too angry
                if (emotionalState.currentEmotion == EmotionalState.Emotion.Annoyed && emotionalState.relationshipLevel > -40f)
                {
                    reward += 0.2f;
                }
                break;
                
            case TeenResponse.Angry:
                // Realistic if highly negative emotional state
                if (emotionalState.currentMood < -40f || emotionalState.currentEmotion == EmotionalState.Emotion.Angry)
                {
                    reward += 0.2f;
                }
                // But ends episode negatively
                endsEpisode = true;
                episodeEndedWell = false;
                break;
                
            case TeenResponse.Dismissive:
                // Typical teen response when neutral/annoyed
                if (emotionalState.autonomyNeed > 60f && emotionalState.currentMood < 20f)
                {
                    reward += 0.1f;
                }
                break;
                
            case TeenResponse.EmotionalPlead:
                // Realistic when sad or anxious
                if (emotionalState.currentEmotion == EmotionalState.Emotion.Sad || 
                    emotionalState.currentEmotion == EmotionalState.Emotion.Anxious)
                {
                    reward += 0.2f;
                }
                break;
                
            case TeenResponse.Defiant:
                // Realistic when feeling disrespected and high autonomy need
                if (emotionalState.respectReceived < 40f && emotionalState.autonomyNeed > 70f && emotionalState.currentMood < -20f)
                {
                    reward += 0.2f;
                }
                // Ends episode negatively
                endsEpisode = true;
                episodeEndedWell = false;
                break;
                
            case TeenResponse.ReasonableRefusal:
                // Good response if trust and mood are decent
                if (emotionalState.trustLevel > 50f && emotionalState.currentMood > 0f)
                {
                    reward += 0.4f;
                }
                break;
        }
        
        // Bonus for maintaining good relationship
        if (emotionalState.relationshipLevel > 40f)
        {
            reward += relationshipReward * 0.1f;
        }
        
        // Penalty for very poor relationship
        if (emotionalState.relationshipLevel < -60f)
        {
            reward += poorRelationshipPenalty * 0.1f;
        }
        
        // Small reward for realistic emotional consistency
        if (IsResponseConsistentWithEmotion(response, emotionalState.currentEmotion))
        {
            reward += 0.1f;
        }
        
        // Apply reward
        AddReward(reward);
        
        // Update conversation manager
        if (conversationManager != null)
        {
            conversationManager.OnTeenResponse(response, emotionalState.currentEmotion);
        }
        
        // Check episode end conditions
        if (endsEpisode || totalInteractionsThisEpisode >= maxInteractionsPerEpisode)
        {
            // Final reward based on overall outcome
            if (hasComplied && emotionalState.relationshipLevel > 0f)
            {
                AddReward(1.0f);  // Successful episode
            }
            else if (!hasComplied && emotionalState.relationshipLevel < -40f)
            {
                AddReward(-0.5f);  // Failed relationship
            }
            
            EndEpisode();
        }
    }
    
    /// <summary>
    /// Check if player action was respectful
    /// </summary>
    private bool IsPlayerActionRespectful(PlayerActionType action)
    {
        return action == PlayerActionType.Empathetic || 
               action == PlayerActionType.Listen || 
               action == PlayerActionType.Compromise;
    }
    
    /// <summary>
    /// Check if response matches emotional state (for realism)
    /// </summary>
    private bool IsResponseConsistentWithEmotion(TeenResponse response, EmotionalState.Emotion emotion)
    {
        switch (emotion)
        {
            case EmotionalState.Emotion.Happy:
                return response == TeenResponse.Compliant || response == TeenResponse.NegotiateCalm;
            case EmotionalState.Emotion.Angry:
                return response == TeenResponse.Angry || response == TeenResponse.Defiant;
            case EmotionalState.Emotion.Annoyed:
                return response == TeenResponse.Sarcastic || response == TeenResponse.Dismissive;
            case EmotionalState.Emotion.Sad:
                return response == TeenResponse.EmotionalPlead;
            case EmotionalState.Emotion.Defiant:
                return response == TeenResponse.Defiant || response == TeenResponse.Sarcastic;
            case EmotionalState.Emotion.Receptive:
                return response == TeenResponse.NegotiateCalm || response == TeenResponse.ReasonableRefusal;
            default:
                return true;  // Neutral can be anything
        }
    }
    
    /// <summary>
    /// Called by PlayerController when player takes an action
    /// </summary>
    public void OnPlayerAction(PlayerActionType action)
    {
        lastPlayerAction = action;
        
        // Update emotional state based on player's action
        UpdateEmotionalStateFromPlayerAction(action);
        
        // Request decision from agent
        RequestDecision();
    }
    
    /// <summary>
    /// Modify emotional state based on how player interacted
    /// </summary>
    private void UpdateEmotionalStateFromPlayerAction(PlayerActionType action)
    {
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
                wasRespectful = false;
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
                moodChange = 5f;  // Short-term positive
                respectChange = -5f;
                wasRespectful = false;
                break;
                
            case PlayerActionType.GuiltTrip:
                relationshipChange = -8f;
                moodChange = -8f;
                respectChange = -10f;
                wasRespectful = false;
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
        
        emotionalState.UpdateFromInteraction(relationshipChange, moodChange, respectChange, wasRespectful);
    }
    
    /// <summary>
    /// Update called every frame
    /// </summary>
    private void Update()
    {
        // Gradually decay emotions over time
        emotionalState.DecayEmotions(Time.deltaTime);
    }
}

/// <summary>
/// Different scenarios the teen might face
/// </summary>
public enum ScenarioType
{
    GoToSchool,
    DoHomework,
    CleanRoom,
    LimitScreenTime,
    Bedtime,
    ComeToFamily
}

/// <summary>
/// Player's possible action types
/// </summary>
public enum PlayerActionType
{
    Authoritarian,  // "You will do this now!"
    Empathetic,     // "I understand how you feel..."
    Logical,        // "Here's why this is important..."
    Bribery,        // "If you do this, I'll give you..."
    GuiltTrip,      // "After all I've done for you..."
    Listen,         // "Tell me what's going on..."
    Compromise      // "What if we meet halfway?"
}

