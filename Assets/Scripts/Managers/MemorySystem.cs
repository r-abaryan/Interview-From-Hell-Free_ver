using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Memory System: Teen remembers past interactions with emotional context
/// Stores conversation history, promises, repeated behaviors, and emotional patterns
/// </summary>
public class MemorySystem : MonoBehaviour
{
    #region Memory Data Structures
    
    [System.Serializable]
    public class Memory
    {
        public string id;
        public MemoryType type;
        public string content;
        public float emotionalWeight; // -1 (negative) to +1 (positive)
        public DateTime timestamp;
        public int timesRecalled; // How often this memory is referenced
        public bool isImportant; // Flagged for long-term retention
        
        // Context
        public ScenarioType scenario;
        public EmotionalState.Emotion emotionAtTime;
        public float relationshipLevelAtTime;
        
        public Memory(MemoryType type, string content, float emotionalWeight, 
                     ScenarioType scenario, EmotionalState.Emotion emotion, float relationship)
        {
            this.id = System.Guid.NewGuid().ToString();
            this.type = type;
            this.content = content;
            this.emotionalWeight = emotionalWeight;
            this.timestamp = DateTime.Now;
            this.timesRecalled = 0;
            this.isImportant = Mathf.Abs(emotionalWeight) > 0.7f; // Strong emotions = important
            
            this.scenario = scenario;
            this.emotionAtTime = emotion;
            this.relationshipLevelAtTime = relationship;
        }
    }
    
    public enum MemoryType
    {
        Promise,           // "You said you'd help me!"
        BrokenPromise,     // "You promised but didn't follow through"
        RepeatedAction,    // "You always yell at me"
        PositiveMoment,    // "Remember when you helped me with homework?"
        Betrayal,          // "You told my secret!"
        Achievement,       // "I got an A because you helped"
        Punishment,        // "You grounded me last week"
        Reward,            // "You bought me that game"
        Conversation,      // Regular dialogue memory
        EmotionalOutburst  // "You made me cry"
    }
    
    [System.Serializable]
    public class PatternMemory
    {
        public string patternKey; // e.g., "player_yells_at_homework"
        public int occurrences;
        public DateTime lastOccurrence;
        public float averageEmotionalImpact;
        
        public PatternMemory(string key)
        {
            this.patternKey = key;
            this.occurrences = 1;
            this.lastOccurrence = DateTime.Now;
            this.averageEmotionalImpact = 0f;
        }
    }
    
    #endregion
    
    #region Configuration
    
    [Header("Memory Settings")]
    [SerializeField] private int maxShortTermMemories = 20;
    [SerializeField] private int maxLongTermMemories = 50;
    [SerializeField] private float memoryDecayRate = 0.1f; // Per day
    [SerializeField] private float importanceThreshold = 0.7f;
    
    [Header("Recall Settings")]
    [SerializeField] private float recentMemoryBias = 0.3f; // Bias towards recent events
    [SerializeField] private float emotionalMemoryBias = 0.5f; // Bias towards emotional events
    
    #endregion
    
    #region Memory Storage
    
    private List<Memory> shortTermMemory = new List<Memory>(); // Last 20 interactions
    private List<Memory> longTermMemory = new List<Memory>();  // Important/repeated memories
    private Dictionary<string, PatternMemory> patterns = new Dictionary<string, PatternMemory>();
    
    // Quick lookup
    private Dictionary<MemoryType, List<Memory>> memoriesByType = new Dictionary<MemoryType, List<Memory>>();
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Initialize type dictionary
        foreach (MemoryType type in System.Enum.GetValues(typeof(MemoryType)))
        {
            memoriesByType[type] = new List<Memory>();
        }
        
        LoadMemoriesFromDisk();
    }
    
    private void OnApplicationQuit()
    {
        SaveMemoriesToDisk();
    }
    
    #endregion
    
    #region Memory Creation
    
    public void RecordInteraction(PlayerActionType playerAction, 
                                   TeenAgent.TeenResponse teenResponse,
                                   EmotionalState emotionalState,
                                   ScenarioType scenario)
    {
        float emotionalWeight = CalculateEmotionalWeight(playerAction, teenResponse, emotionalState);
        string content = GenerateMemoryContent(playerAction, teenResponse, scenario);
        MemoryType type = DetermineMemoryType(playerAction, teenResponse, emotionalState);
        
        Memory memory = new Memory(
            type,
            content,
            emotionalWeight,
            scenario,
            emotionalState.currentEmotion,
            emotionalState.relationshipLevel
        );
        
        AddMemory(memory);
        UpdatePatterns(playerAction, teenResponse, scenario, emotionalWeight);
        
        Debug.Log($"[Memory] Recorded: {content} (Weight: {emotionalWeight:F2})");
    }
    
    public void RecordPromise(string promiseContent, ScenarioType scenario, EmotionalState state)
    {
        Memory promise = new Memory(
            MemoryType.Promise,
            $"You promised: {promiseContent}",
            0.5f,
            scenario,
            state.currentEmotion,
            state.relationshipLevel
        );
        promise.isImportant = true;
        AddMemory(promise);
    }
    
    public void RecordBrokenPromise(Memory originalPromise)
    {
        Memory broken = new Memory(
            MemoryType.BrokenPromise,
            $"You broke your promise: {originalPromise.content}",
            -0.8f,
            originalPromise.scenario,
            EmotionalState.Emotion.Angry,
            originalPromise.relationshipLevelAtTime
        );
        broken.isImportant = true;
        AddMemory(broken);
    }
    
    #endregion
    
    #region Memory Retrieval
    
    public Memory GetRelevantMemory(ScenarioType currentScenario, EmotionalState currentState)
    {
        List<Memory> allMemories = new List<Memory>();
        allMemories.AddRange(shortTermMemory);
        allMemories.AddRange(longTermMemory);
        
        if (allMemories.Count == 0) return null;
        
        // Score each memory for relevance
        var scoredMemories = allMemories
            .Select(m => new { Memory = m, Score = CalculateRelevanceScore(m, currentScenario, currentState) })
            .OrderByDescending(sm => sm.Score)
            .ToList();
        
        // Return most relevant memory
        if (scoredMemories.Count > 0 && scoredMemories[0].Score > 0.3f)
        {
            Memory relevant = scoredMemories[0].Memory;
            relevant.timesRecalled++;
            return relevant;
        }
        
        return null;
    }
    
    public List<Memory> GetMemoriesOfType(MemoryType type, int count = 5)
    {
        if (memoriesByType.ContainsKey(type))
        {
            return memoriesByType[type]
                .OrderByDescending(m => m.timestamp)
                .Take(count)
                .ToList();
        }
        return new List<Memory>();
    }
    
    public string GetMemoryDialogue(Memory memory)
    {
        if (memory == null) return null;
        
        TimeSpan timeSince = DateTime.Now - memory.timestamp;
        string timeRef = GetTimeReference(timeSince);
        
        switch (memory.type)
        {
            case MemoryType.Promise:
                return $"You said you'd help me {timeRef}!";
            
            case MemoryType.BrokenPromise:
                return $"You promised {timeRef}, but you didn't keep your word!";
            
            case MemoryType.RepeatedAction:
                return $"You always do this! Just like {timeRef}!";
            
            case MemoryType.PositiveMoment:
                return $"Remember {timeRef} when you helped me? That was nice.";
            
            case MemoryType.Punishment:
                return $"You grounded me {timeRef}, remember?";
            
            case MemoryType.EmotionalOutburst:
                return $"{timeRef} you made me so upset!";
            
            default:
                return $"{timeRef}: {memory.content}";
        }
    }
    
    public PatternMemory GetPattern(string patternKey)
    {
        return patterns.ContainsKey(patternKey) ? patterns[patternKey] : null;
    }
    
    public bool HasRepeatedPattern(string patternKey, int minOccurrences = 3)
    {
        if (patterns.ContainsKey(patternKey))
        {
            return patterns[patternKey].occurrences >= minOccurrences;
        }
        return false;
    }
    
    #endregion
    
    #region Memory Management
    
    private void AddMemory(Memory memory)
    {
        // Add to short-term
        shortTermMemory.Add(memory);
        memoriesByType[memory.type].Add(memory);
        
        // Manage capacity
        if (shortTermMemory.Count > maxShortTermMemories)
        {
            Memory oldest = shortTermMemory[0];
            
            // Move important memories to long-term
            if (oldest.isImportant || oldest.timesRecalled > 2)
            {
                if (!longTermMemory.Contains(oldest))
                {
                    longTermMemory.Add(oldest);
                }
            }
            
            shortTermMemory.RemoveAt(0);
        }
        
        // Manage long-term capacity
        if (longTermMemory.Count > maxLongTermMemories)
        {
            // Remove least important/recalled
            var toRemove = longTermMemory
                .OrderBy(m => m.timesRecalled)
                .ThenBy(m => Mathf.Abs(m.emotionalWeight))
                .First();
            
            longTermMemory.Remove(toRemove);
            memoriesByType[toRemove.type].Remove(toRemove);
        }
    }
    
    private void UpdatePatterns(PlayerActionType action, 
                               TeenAgent.TeenResponse response,
                               ScenarioType scenario,
                               float emotionalWeight)
    {
        string patternKey = $"{action}_{scenario}";
        
        if (patterns.ContainsKey(patternKey))
        {
            PatternMemory pattern = patterns[patternKey];
            pattern.occurrences++;
            pattern.lastOccurrence = DateTime.Now;
            pattern.averageEmotionalImpact = 
                (pattern.averageEmotionalImpact * (pattern.occurrences - 1) + emotionalWeight) / pattern.occurrences;
        }
        else
        {
            PatternMemory pattern = new PatternMemory(patternKey);
            pattern.averageEmotionalImpact = emotionalWeight;
            patterns[patternKey] = pattern;
        }
    }
    
    public void ClearAllMemories()
    {
        shortTermMemory.Clear();
        longTermMemory.Clear();
        patterns.Clear();
        
        foreach (var list in memoriesByType.Values)
        {
            list.Clear();
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private float CalculateEmotionalWeight(PlayerActionType action, 
                                          TeenAgent.TeenResponse response,
                                          EmotionalState state)
    {
        float weight = 0f;
        
        // Player action impact
        switch (action)
        {
            case PlayerActionType.Authoritarian:
                weight -= 0.7f;
                break;
            case PlayerActionType.GuiltTrip:
                weight -= 0.5f;
                break;
            case PlayerActionType.Bribery:
                weight += 0.3f;
                break;
            case PlayerActionType.Logical:
            case PlayerActionType.Compromise:
                weight += 0.5f;
                break;
            case PlayerActionType.Empathetic:
            case PlayerActionType.Listen:
                weight += 0.7f;
                break;
        }
        
        // Response amplification
        if (response == TeenAgent.TeenResponse.Angry || response == TeenAgent.TeenResponse.Defiant)
        {
            weight -= 0.2f;
        }
        else if (response == TeenAgent.TeenResponse.Compliant)
        {
            weight += 0.2f;
        }
        
        // State context
        weight += (state.relationshipLevel - 50f) / 100f; // -0.5 to +0.5
        
        return Mathf.Clamp(weight, -1f, 1f);
    }
    
    private string GenerateMemoryContent(PlayerActionType action,
                                        TeenAgent.TeenResponse response,
                                        ScenarioType scenario)
    {
        return $"During {scenario}: You {action}, I {response}";
    }
    
    private MemoryType DetermineMemoryType(PlayerActionType action,
                                          TeenAgent.TeenResponse response,
                                          EmotionalState state)
    {
        // Determine memory type based on action and emotional weight
        if (action == PlayerActionType.GuiltTrip)
            return MemoryType.Punishment;
        
        if (action == PlayerActionType.Bribery)
            return MemoryType.Reward;
        
        if (response == TeenAgent.TeenResponse.Angry && state.stressLevel > 70f)
            return MemoryType.EmotionalOutburst;
        
        if (state.relationshipLevel > 70f)
            return MemoryType.PositiveMoment;
        
        return MemoryType.Conversation;
    }
    
    private float CalculateRelevanceScore(Memory memory, ScenarioType currentScenario, EmotionalState currentState)
    {
        float score = 0f;
        
        // Same scenario = more relevant
        if (memory.scenario == currentScenario)
            score += 0.3f;
        
        // Recent memories more relevant
        TimeSpan timeSince = DateTime.Now - memory.timestamp;
        float daysSince = (float)timeSince.TotalDays;
        score += recentMemoryBias * Mathf.Exp(-daysSince * memoryDecayRate);
        
        // Emotional memories more relevant
        score += emotionalMemoryBias * Mathf.Abs(memory.emotionalWeight);
        
        // Frequently recalled = important
        score += memory.timesRecalled * 0.1f;
        
        // Current emotional state match
        if (memory.emotionAtTime == currentState.currentEmotion)
            score += 0.2f;
        
        return score;
    }
    
    private string GetTimeReference(TimeSpan timeSince)
    {
        if (timeSince.TotalMinutes < 5)
            return "just now";
        if (timeSince.TotalMinutes < 30)
            return "a few minutes ago";
        if (timeSince.TotalHours < 2)
            return "earlier";
        if (timeSince.TotalHours < 24)
            return "today";
        if (timeSince.TotalDays < 2)
            return "yesterday";
        if (timeSince.TotalDays < 7)
            return "a few days ago";
        if (timeSince.TotalDays < 30)
            return "last week";
        else
            return "a while ago";
    }
    
    #endregion
    
    #region ML-Agents Integration
    
    public float[] GetMemoryObservations()
    {
        // Return memory context for ML agent (8 floats)
        float[] obs = new float[8];
        
        // [0] Average emotional weight of recent memories
        obs[0] = shortTermMemory.Count > 0 
            ? shortTermMemory.Average(m => m.emotionalWeight) 
            : 0f;
        
        // [1] Number of broken promises (normalized)
        obs[1] = Mathf.Clamp01(GetMemoriesOfType(MemoryType.BrokenPromise).Count / 5f);
        
        // [2] Number of positive moments (normalized)
        obs[2] = Mathf.Clamp01(GetMemoriesOfType(MemoryType.PositiveMoment).Count / 10f);
        
        // [3] Recent punishment count (normalized)
        obs[3] = Mathf.Clamp01(GetMemoriesOfType(MemoryType.Punishment).Count / 5f);
        
        // [4] Pattern consistency (are player actions predictable?)
        obs[4] = patterns.Count > 0 
            ? patterns.Values.Average(p => Mathf.Clamp01(p.occurrences / 10f)) 
            : 0f;
        
        // [5] Memory load (how many memories teen is holding)
        obs[5] = Mathf.Clamp01((shortTermMemory.Count + longTermMemory.Count) / 50f);
        
        // [6] Most recalled memory importance
        var mostRecalled = shortTermMemory.Concat(longTermMemory)
            .OrderByDescending(m => m.timesRecalled)
            .FirstOrDefault();
        obs[6] = mostRecalled != null ? Mathf.Abs(mostRecalled.emotionalWeight) : 0f;
        
        // [7] Days since last positive interaction
        var lastPositive = GetMemoriesOfType(MemoryType.PositiveMoment, 1).FirstOrDefault();
        if (lastPositive != null)
        {
            float daysSince = (float)(DateTime.Now - lastPositive.timestamp).TotalDays;
            obs[7] = Mathf.Clamp01(daysSince / 7f); // 0 = today, 1 = week+ ago
        }
        else
        {
            obs[7] = 1f; // No positive memories
        }
        
        return obs;
    }
    
    #endregion
    
    #region Persistence
    
    [System.Serializable]
    private class MemoryData
    {
        public List<Memory> shortTerm;
        public List<Memory> longTerm;
        public List<PatternMemory> patterns;
    }
    
    private void SaveMemoriesToDisk()
    {
        MemoryData data = new MemoryData
        {
            shortTerm = shortTermMemory,
            longTerm = longTermMemory,
            patterns = patterns.Values.ToList()
        };
        
        string json = JsonUtility.ToJson(data, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "teen_memories.json");
        System.IO.File.WriteAllText(path, json);
        
        Debug.Log($"[Memory] Saved {shortTermMemory.Count + longTermMemory.Count} memories to disk");
    }
    
    private void LoadMemoriesFromDisk()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, "teen_memories.json");
        
        if (System.IO.File.Exists(path))
        {
            string json = System.IO.File.ReadAllText(path);
            MemoryData data = JsonUtility.FromJson<MemoryData>(json);
            
            shortTermMemory = data.shortTerm ?? new List<Memory>();
            longTermMemory = data.longTerm ?? new List<Memory>();
            
            patterns.Clear();
            if (data.patterns != null)
            {
                foreach (var pattern in data.patterns)
                {
                    patterns[pattern.patternKey] = pattern;
                }
            }
            
            // Rebuild type dictionary
            foreach (var memory in shortTermMemory.Concat(longTermMemory))
            {
                memoriesByType[memory.type].Add(memory);
            }
            
            Debug.Log($"[Memory] Loaded {shortTermMemory.Count + longTermMemory.Count} memories from disk");
        }
    }
    
    #endregion
}

