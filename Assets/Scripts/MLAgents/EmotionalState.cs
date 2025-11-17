using UnityEngine;

/// <summary>
/// Represents the emotional and psychological state of the teenager
/// </summary>
[System.Serializable]
public class EmotionalState
{
    // Core emotional metrics
    [Range(-100f, 100f)]
    public float relationshipLevel = 0f;  // How much they like/trust the player
    
    [Range(-100f, 100f)]
    public float currentMood = 0f;  // Current emotional state (-100 = very negative, +100 = very positive)
    
    [Range(0f, 100f)]
    public float trustLevel = 50f;  // How much they trust the player
    
    [Range(0f, 100f)]
    public float stressLevel = 30f;  // Current stress
    
    [Range(0f, 100f)]
    public float autonomyNeed = 70f;  // Desire for independence (high in teens)
    
    [Range(0f, 100f)]
    public float respectReceived = 50f;  // How respected they feel
    
    // Context factors
    [Range(0f, 100f)]
    public float tiredness = 20f;
    
    [Range(0f, 100f)]
    public float hunger = 30f;
    
    public int consecutiveNegativeInteractions = 0;
    public int consecutivePositiveInteractions = 0;
    
    // Emotion categories
    public enum Emotion
    {
        Happy,
        Neutral,
        Annoyed,
        Angry,
        Sad,
        Defiant,
        Anxious,
        Receptive
    }
    
    public Emotion currentEmotion = Emotion.Neutral;
    
    /// <summary>
    /// Update emotional state based on interaction outcome
    /// </summary>
    public void UpdateFromInteraction(float relationshipChange, float moodChange, float respectChange, bool wasPlayerRespectful)
    {
        relationshipLevel = Mathf.Clamp(relationshipLevel + relationshipChange, -100f, 100f);
        currentMood = Mathf.Clamp(currentMood + moodChange, -100f, 100f);
        respectReceived = Mathf.Clamp(respectReceived + respectChange, 0f, 100f);
        
        // Update interaction streaks
        if (moodChange < 0)
        {
            consecutiveNegativeInteractions++;
            consecutivePositiveInteractions = 0;
        }
        else if (moodChange > 0)
        {
            consecutivePositiveInteractions++;
            consecutiveNegativeInteractions = 0;
        }
        
        // Adjust trust based on respectful treatment
        if (wasPlayerRespectful)
        {
            trustLevel = Mathf.Clamp(trustLevel + 2f, 0f, 100f);
        }
        else
        {
            trustLevel = Mathf.Clamp(trustLevel - 5f, 0f, 100f);
        }
        
        UpdateCurrentEmotion();
    }
    
    /// <summary>
    /// Determine current emotion based on state values
    /// </summary>
    public void UpdateCurrentEmotion()
    {
        if (currentMood > 50f && relationshipLevel > 30f)
            currentEmotion = Emotion.Happy;
        else if (currentMood > 20f && respectReceived > 60f)
            currentEmotion = Emotion.Receptive;
        else if (currentMood < -50f && stressLevel > 60f)
            currentEmotion = Emotion.Angry;
        else if (currentMood < -30f && autonomyNeed > 70f && respectReceived < 40f)
            currentEmotion = Emotion.Defiant;
        else if (currentMood < -20f)
            currentEmotion = Emotion.Annoyed;
        else if (trustLevel < 30f && stressLevel > 50f)
            currentEmotion = Emotion.Anxious;
        else if (relationshipLevel < -40f)
            currentEmotion = Emotion.Sad;
        else
            currentEmotion = Emotion.Neutral;
    }
    
    /// <summary>
    /// Gradually decay extreme emotions over time
    /// </summary>
    public void DecayEmotions(float deltaTime)
    {
        float decayRate = 2f * deltaTime;
        
        // Mood drifts toward neutral
        if (currentMood > 0)
            currentMood = Mathf.Max(0, currentMood - decayRate);
        else
            currentMood = Mathf.Min(0, currentMood + decayRate);
        
        // Stress slowly decreases
        stressLevel = Mathf.Max(10f, stressLevel - decayRate * 0.5f);
        
        UpdateCurrentEmotion();
    }
    
    /// <summary>
    /// Get normalized observation array for ML-Agent
    /// </summary>
    public float[] GetObservationArray()
    {
        return new float[]
        {
            relationshipLevel / 100f,  // Normalize to [-1, 1]
            currentMood / 100f,
            trustLevel / 100f,
            stressLevel / 100f,
            autonomyNeed / 100f,
            respectReceived / 100f,
            tiredness / 100f,
            hunger / 100f,
            consecutiveNegativeInteractions / 10f,  // Normalize assuming max ~10
            consecutivePositiveInteractions / 10f,
            (int)currentEmotion / 8f  // 8 emotion types
        };
    }
}

