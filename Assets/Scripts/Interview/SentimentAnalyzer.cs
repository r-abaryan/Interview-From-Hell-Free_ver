using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes text sentiment and emotion from transcribed speech
/// </summary>
public class SentimentAnalyzer : MonoBehaviour
{
    public enum Sentiment
    {
        Positive,
        Neutral,
        Negative
    }
    
    public class SentimentResult
    {
        public Sentiment sentiment;
        public float confidence;      // 0-1
        public float nervousness;     // 0-1 (based on filler words, hesitation)
        public float assertiveness;   // 0-1 (based on strong words)
        public float professionalism; // 0-1
        public bool containsHumor;
        public bool soundsUncertain;
        public bool soundsDefensive;
        public int wordCount;
    }
    
    // Keyword dictionaries
    private static readonly string[] positiveWords = {
        "yes", "great", "excellent", "good", "absolutely", "definitely", 
        "strong", "skilled", "experienced", "passionate", "enthusiastic",
        "love", "enjoy", "excited", "confident"
    };
    
    private static readonly string[] negativeWords = {
        "no", "bad", "terrible", "poor", "unfortunately", "difficult",
        "struggle", "weak", "inexperienced", "unsure", "confused"
    };
    
    private static readonly string[] uncertainWords = {
        "maybe", "perhaps", "possibly", "might", "could", "somewhat",
        "kind of", "sort of", "i think", "i guess", "probably"
    };
    
    private static readonly string[] fillerWords = {
        "um", "uh", "er", "ah", "like", "you know", "basically",
        "actually", "literally", "well", "so", "hmm"
    };
    
    private static readonly string[] professionalWords = {
        "experience", "skills", "professional", "qualified", "competent",
        "expertise", "knowledge", "project", "team", "leadership",
        "achieve", "results", "successful", "efficient"
    };
    
    private static readonly string[] assertiveWords = {
        "will", "can", "must", "certainly", "absolutely", "definitely",
        "ensure", "guarantee", "committed", "determined"
    };
    
    private static readonly string[] defensiveWords = {
        "but", "however", "actually", "technically", "well actually",
        "to be fair", "in my defense"
    };
    
    public SentimentResult Analyze(string text)
    {
        SentimentResult result = new SentimentResult();
        
        if (string.IsNullOrWhiteSpace(text))
        {
            result.sentiment = Sentiment.Neutral;
            return result;
        }
        
        text = text.ToLower();
        string[] words = text.Split(' ', ',', '.', '!', '?', ';', ':');
        result.wordCount = words.Where(w => w.Length > 2).Count();
        
        // Count positive/negative/uncertain words
        int positiveCount = CountWords(words, positiveWords);
        int negativeCount = CountWords(words, negativeWords);
        int uncertainCount = CountWords(words, uncertainWords);
        int fillerCount = CountWords(words, fillerWords);
        int professionalCount = CountWords(words, professionalWords);
        int assertiveCount = CountWords(words, assertiveWords);
        int defensiveCount = CountWords(words, defensiveWords);
        
        // Determine sentiment
        if (positiveCount > negativeCount)
            result.sentiment = Sentiment.Positive;
        else if (negativeCount > positiveCount)
            result.sentiment = Sentiment.Negative;
        else
            result.sentiment = Sentiment.Neutral;
        
        // Calculate confidence
        int totalEmotionalWords = positiveCount + negativeCount;
        result.confidence = Mathf.Clamp01(totalEmotionalWords / Mathf.Max(1f, result.wordCount * 0.3f));
        
        // Calculate nervousness (filler words + uncertainty)
        result.nervousness = Mathf.Clamp01((fillerCount * 2 + uncertainCount) / Mathf.Max(1f, result.wordCount * 0.5f));
        
        // Calculate assertiveness
        result.assertiveness = Mathf.Clamp01(assertiveCount / Mathf.Max(1f, result.wordCount * 0.2f));
        
        // Calculate professionalism
        result.professionalism = Mathf.Clamp01(professionalCount / Mathf.Max(1f, result.wordCount * 0.3f));
        
        // Detect humor (very simple: questions marks, exclamations, certain patterns)
        result.containsHumor = text.Contains("!") || text.Contains("haha") || text.Contains("lol");
        
        // Detect uncertainty
        result.soundsUncertain = uncertainCount > 2 || fillerCount > 3;
        
        // Detect defensiveness
        result.soundsDefensive = defensiveCount > 1 || text.Contains("but");
        
        Debug.Log($"[Sentiment] {result.sentiment}, Confidence: {result.confidence:F2}, " +
                  $"Nervous: {result.nervousness:F2}, Assertive: {result.assertiveness:F2}, " +
                  $"Professional: {result.professionalism:F2}");
        
        return result;
    }
    
    private int CountWords(string[] text, string[] keywords)
    {
        int count = 0;
        foreach (string word in text)
        {
            if (keywords.Contains(word.Trim().ToLower()))
                count++;
        }
        return count;
    }
    
    public string GetFeedback(SentimentResult result)
    {
        if (result.nervousness > 0.6f)
            return "You sound nervous. Take a breath!";
        
        if (result.soundsDefensive)
            return "Getting defensive already?";
        
        if (result.assertiveness < 0.3f && result.confidence < 0.4f)
            return "Where's your confidence?";
        
        if (result.professionalism > 0.7f)
            return "Very professional. TOO professional...";
        
        if (result.containsHumor)
            return "Humor? In an interview? Bold move.";
        
        return "Interesting answer...";
    }
}

