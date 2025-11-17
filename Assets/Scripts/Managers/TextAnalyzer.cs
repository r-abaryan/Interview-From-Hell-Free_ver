using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Advanced text analyzer for player input
/// Uses keywords, tone, and context to determine action type
/// </summary>
public class TextAnalyzer : MonoBehaviour
{
    // Keyword dictionaries with weights
    private Dictionary<PlayerActionType, List<(string keyword, float weight)>> keywordDatabase;
    
    public void Initialize()
    {
        BuildKeywordDatabase();
    }
    
    /// <summary>
    /// Analyze text and return action type with confidence score
    /// </summary>
    public (PlayerActionType action, float confidence) AnalyzeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (PlayerActionType.Logical, 0f);
        
        string lower = text.ToLower();
        
        // Score each action type
        Dictionary<PlayerActionType, float> scores = new Dictionary<PlayerActionType, float>();
        foreach (var actionType in System.Enum.GetValues(typeof(PlayerActionType)).Cast<PlayerActionType>())
        {
            scores[actionType] = ScoreActionType(lower, actionType);
        }
        
        // Add tone modifiers
        scores = ApplyToneModifiers(lower, scores);
        
        // Get highest score
        var best = scores.OrderByDescending(x => x.Value).First();
        
        return (best.Key, best.Value);
    }
    
    private float ScoreActionType(string text, PlayerActionType actionType)
    {
        if (!keywordDatabase.ContainsKey(actionType))
            return 0f;
        
        float score = 0f;
        foreach (var (keyword, weight) in keywordDatabase[actionType])
        {
            if (text.Contains(keyword))
            {
                score += weight;
            }
        }
        
        return score;
    }
    
    private Dictionary<PlayerActionType, float> ApplyToneModifiers(string text, Dictionary<PlayerActionType, float> scores)
    {
        // Exclamation marks = authoritarian
        int exclamations = text.Count(c => c == '!');
        scores[PlayerActionType.Authoritarian] += exclamations * 2f;
        
        // Question marks = listening/empathetic
        int questions = text.Count(c => c == '?');
        scores[PlayerActionType.Listen] += questions * 1.5f;
        scores[PlayerActionType.Empathetic] += questions * 1.0f;
        
        // Length - longer = more empathetic/logical, shorter = more authoritarian
        int wordCount = text.Split(' ').Length;
        if (wordCount > 15)
        {
            scores[PlayerActionType.Empathetic] += 1f;
            scores[PlayerActionType.Logical] += 1f;
        }
        else if (wordCount < 5)
        {
            scores[PlayerActionType.Authoritarian] += 1.5f;
        }
        
        // ALL CAPS = angry/authoritarian
        if (text.Any(char.IsLetter) && text.Where(char.IsLetter).All(char.IsUpper))
        {
            scores[PlayerActionType.Authoritarian] += 3f;
        }
        
        return scores;
    }
    
    private void BuildKeywordDatabase()
    {
        keywordDatabase = new Dictionary<PlayerActionType, List<(string, float)>>();
        
        // EMPATHETIC - Understanding, caring, supportive
        keywordDatabase[PlayerActionType.Empathetic] = new List<(string, float)>
        {
            ("understand", 3f), ("feel", 2f), ("feeling", 2f), ("feelings", 2f),
            ("sorry", 2.5f), ("care", 2f), ("caring", 2f), ("love", 2f),
            ("support", 2f), ("here for you", 3f), ("worried about", 2.5f),
            ("i know", 2f), ("must be hard", 3f), ("difficult", 1.5f),
            ("empathize", 3f), ("sympathize", 2.5f), ("appreciate", 2f),
            ("see why", 2f), ("makes sense", 2f), ("valid", 2f)
        };
        
        // LISTEN - Asking, inquiring, opening dialogue
        keywordDatabase[PlayerActionType.Listen] = new List<(string, float)>
        {
            ("tell me", 3f), ("what's wrong", 3f), ("what happened", 2.5f),
            ("listen", 2.5f), ("talk", 2f), ("talking", 2f), ("hear", 2f),
            ("share", 2f), ("explain", 2f), ("help me understand", 3f),
            ("going on", 2f), ("bothering", 2.5f), ("upset", 2f),
            ("want to know", 2.5f), ("tell you what", 1f), ("let's talk", 3f),
            ("open up", 2f), ("comfortable", 1.5f)
        };
        
        // COMPROMISE - Negotiation, middle ground, flexibility
        keywordDatabase[PlayerActionType.Compromise] = new List<(string, float)>
        {
            ("how about", 3f), ("what if", 3f), ("together", 2.5f),
            ("compromise", 3f), ("meet", 2f), ("halfway", 2.5f),
            ("work something out", 3f), ("find a way", 2.5f), ("both", 2f),
            ("deal", 2f), ("agree", 2f), ("fair", 2f),
            ("let's", 2f), ("we can", 2f), ("middle ground", 3f),
            ("alternative", 2f), ("option", 1.5f), ("instead", 1.5f)
        };
        
        // LOGICAL - Reasoning, facts, explanation
        keywordDatabase[PlayerActionType.Logical] = new List<(string, float)>
        {
            ("because", 2.5f), ("important", 2f), ("need to", 2f),
            ("should", 2f), ("reason", 2.5f), ("think", 1.5f),
            ("consider", 2f), ("fact", 2.5f), ("studies", 2f),
            ("research", 2f), ("evidence", 2.5f), ("makes sense", 2f),
            ("logically", 3f), ("rationally", 2.5f), ("understand that", 2f),
            ("consequence", 2.5f), ("result", 2f), ("leads to", 2f),
            ("future", 2f), ("goal", 1.5f)
        };
        
        // BRIBERY - Rewards, incentives, transactions
        keywordDatabase[PlayerActionType.Bribery] = new List<(string, float)>
        {
            ("if you", 3f), ("reward", 3f), ("give you", 2.5f),
            ("money", 2.5f), ("buy", 2f), ("get you", 2.5f),
            ("allowance", 2.5f), ("pay", 2f), ("extra", 2f),
            ("treat", 2f), ("prize", 2f), ("gift", 2f),
            ("earn", 2f), ("bonus", 2f), ("special", 1.5f),
            ("then you can", 2.5f), ("in exchange", 3f), ("deal", 2f)
        };
        
        // GUILT TRIP - Manipulation, disappointment, sacrifice
        keywordDatabase[PlayerActionType.GuiltTrip] = new List<(string, float)>
        {
            ("after all", 3f), ("disappoint", 3f), ("disappointed", 3f),
            ("how could", 3f), ("ungrateful", 3f), ("sacrifice", 3f),
            ("everything i", 2.5f), ("all i do", 3f), ("for you", 2f),
            ("never", 2f), ("always", 2f), ("ashamed", 3f),
            ("embarrassed", 2.5f), ("let me down", 3f), ("expected more", 3f),
            ("thought you", 2f), ("used to", 2f), ("what happened to", 2.5f),
            ("selfish", 3f), ("only think", 2.5f)
        };
        
        // AUTHORITARIAN - Commands, demands, force
        keywordDatabase[PlayerActionType.Authoritarian] = new List<(string, float)>
        {
            ("must", 3f), ("will", 2.5f), ("now", 2.5f),
            ("immediately", 3f), ("right now", 3f), ("do it", 2.5f),
            ("told you", 2.5f), ("order", 2.5f), ("command", 3f),
            ("obey", 3f), ("listen to me", 2.5f), ("do what i say", 3f),
            ("no choice", 2.5f), ("don't care", 2.5f), ("end of discussion", 3f),
            ("that's final", 3f), ("i said", 2f), ("don't question", 3f),
            ("because i said so", 3f), ("my house", 2.5f), ("my rules", 2.5f)
        };
    }
    
    /// <summary>
    /// Get detailed analysis for debugging/logging
    /// </summary>
    public string GetDetailedAnalysis(string text)
    {
        var (action, confidence) = AnalyzeText(text);
        
        string analysis = $"Text: \"{text}\"\n";
        analysis += $"Detected Action: {action}\n";
        analysis += $"Confidence: {confidence:F2}\n\n";
        analysis += "Scores:\n";
        
        string lower = text.ToLower();
        foreach (var actionType in System.Enum.GetValues(typeof(PlayerActionType)).Cast<PlayerActionType>())
        {
            float score = ScoreActionType(lower, actionType);
            analysis += $"  {actionType}: {score:F2}\n";
        }
        
        return analysis;
    }
}

