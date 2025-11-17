using UnityEngine;

/// <summary>
/// Manages scenario generation and variation for training
/// </summary>
public class ScenarioManager : MonoBehaviour
{
    [Header("Scenario Settings")]
    public bool randomizeScenarios = true;
    public ScenarioType forcedScenario = ScenarioType.GoToSchool;
    
    [Header("Environmental Factors")]
    [Tooltip("Randomize time of day for scenarios")]
    public bool randomizeTimeOfDay = true;
    
    [Header("Statistics")]
    public int totalScenariosGenerated = 0;
    public int successfulOutcomes = 0;
    public int failedOutcomes = 0;
    
    private TeenAgent teenAgent;
    
    private void Awake()
    {
        teenAgent = FindFirstObjectByType<TeenAgent>();
    }
    
    /// <summary>
    /// Generate a scenario based on settings
    /// </summary>
    public ScenarioType GenerateScenario()
    {
        totalScenariosGenerated++;
        
        if (randomizeScenarios)
        {
            return (ScenarioType)Random.Range(0, System.Enum.GetValues(typeof(ScenarioType)).Length);
        }
        else
        {
            return forcedScenario;
        }
    }
    
    /// <summary>
    /// Apply environmental modifiers to a scenario
    /// </summary>
    public void ApplyEnvironmentalFactors(EmotionalState emotionalState)
    {
        if (randomizeTimeOfDay)
        {
            int hour = Random.Range(6, 23); // 6 AM to 11 PM
            
            // Morning (6-9): Potentially more tired
            if (hour >= 6 && hour < 9)
            {
                emotionalState.tiredness = Random.Range(50f, 90f);
                emotionalState.hunger = Random.Range(40f, 80f);
            }
            // School hours (9-15): Lower tiredness
            else if (hour >= 9 && hour < 15)
            {
                emotionalState.tiredness = Random.Range(20f, 50f);
                emotionalState.hunger = Random.Range(30f, 60f);
            }
            // After school (15-18): Moderate tiredness
            else if (hour >= 15 && hour < 18)
            {
                emotionalState.tiredness = Random.Range(30f, 60f);
                emotionalState.hunger = Random.Range(50f, 90f);
            }
            // Evening (18-21): Building tiredness
            else if (hour >= 18 && hour < 21)
            {
                emotionalState.tiredness = Random.Range(40f, 70f);
                emotionalState.hunger = Random.Range(20f, 50f);
            }
            // Late night (21-23): Very tired
            else
            {
                emotionalState.tiredness = Random.Range(60f, 95f);
                emotionalState.hunger = Random.Range(10f, 40f);
            }
        }
        
        // Random stress variations
        emotionalState.stressLevel = Random.Range(20f, 70f);
        
        // Teens naturally have high autonomy needs
        emotionalState.autonomyNeed = Random.Range(60f, 90f);
    }
    
    /// <summary>
    /// Get scenario difficulty estimate based on emotional state
    /// </summary>
    public float GetScenarioDifficulty(EmotionalState emotionalState)
    {
        float difficulty = 0f;
        
        // Poor relationship makes it harder
        if (emotionalState.relationshipLevel < 0)
        {
            difficulty += Mathf.Abs(emotionalState.relationshipLevel) / 100f;
        }
        
        // Negative mood adds difficulty
        if (emotionalState.currentMood < 0)
        {
            difficulty += Mathf.Abs(emotionalState.currentMood) / 100f;
        }
        
        // Low trust increases difficulty
        difficulty += (100f - emotionalState.trustLevel) / 100f;
        
        // High stress adds difficulty
        difficulty += emotionalState.stressLevel / 100f;
        
        return difficulty / 4f; // Normalize to 0-1 range
    }
    
    /// <summary>
    /// Record scenario outcome for statistics
    /// </summary>
    public void RecordOutcome(bool wasSuccessful)
    {
        if (wasSuccessful)
        {
            successfulOutcomes++;
        }
        else
        {
            failedOutcomes++;
        }
    }
    
    /// <summary>
    /// Get success rate percentage
    /// </summary>
    public float GetSuccessRate()
    {
        int total = successfulOutcomes + failedOutcomes;
        if (total == 0) return 0f;
        return (float)successfulOutcomes / total * 100f;
    }
    
    /// <summary>
    /// Get contextual description of scenario
    /// </summary>
    public string GetScenarioDescription(ScenarioType scenario)
    {
        switch (scenario)
        {
            case ScenarioType.GoToSchool:
                return "The teen doesn't want to go to school. You need to convince them.";
            case ScenarioType.DoHomework:
                return "The teen is avoiding homework. You need to get them to complete it.";
            case ScenarioType.CleanRoom:
                return "The teen's room is messy. You want them to clean it.";
            case ScenarioType.LimitScreenTime:
                return "The teen has been on their device too long. You need to set limits.";
            case ScenarioType.Bedtime:
                return "It's bedtime but the teen doesn't want to sleep yet.";
            case ScenarioType.ComeToFamily:
                return "You want the teen to spend time with the family, but they're isolated.";
            default:
                return "Interact with the teenager.";
        }
    }
    
    /// <summary>
    /// Get tips for handling this scenario
    /// </summary>
    public string GetScenarioTips(ScenarioType scenario, EmotionalState emotionalState)
    {
        string tips = "";
        
        // Scenario-specific tips
        switch (scenario)
        {
            case ScenarioType.GoToSchool:
                tips = "Tip: Listen to understand why they don't want to go. There might be bullying or anxiety.";
                break;
            case ScenarioType.DoHomework:
                tips = "Tip: Offer to help or break it into smaller tasks. Avoid making it a power struggle.";
                break;
            case ScenarioType.CleanRoom:
                tips = "Tip: Respect their space. Compromise on minimum standards rather than perfection.";
                break;
            case ScenarioType.LimitScreenTime:
                tips = "Tip: Set clear boundaries together. Explain the 'why' behind limits.";
                break;
            case ScenarioType.Bedtime:
                tips = "Tip: Acknowledge they're growing up. Negotiate a reasonable time together.";
                break;
            case ScenarioType.ComeToFamily:
                tips = "Tip: Understand their need for independence. Make family time appealing, not forced.";
                break;
        }
        
        // Emotional state tips
        if (emotionalState.currentMood < -40f)
        {
            tips += "\nWarning: Teen is in a very negative mood. Approach with empathy.";
        }
        
        if (emotionalState.relationshipLevel < -30f)
        {
            tips += "\nWarning: Relationship is strained. Rebuild trust before making demands.";
        }
        
        if (emotionalState.trustLevel < 30f)
        {
            tips += "\nWarning: Trust is very low. They won't respond well to authority.";
        }
        
        return tips;
    }
}

