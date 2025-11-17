using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text;

/// <summary>
/// LLM Manager for local model integration (Ollama, LM Studio)
/// </summary>
public class LLMManager : MonoBehaviour
{
    [Header("LLM Settings")]
    [SerializeField] private string ollamaEndpoint = "http://localhost:11434/api/generate";
    [SerializeField] private string modelName = "phi3";
    [SerializeField] private float temperature = 0.9f;
    [SerializeField] private int maxTokens = 100;
    
    [Header("Interviewer Personality")]
    [TextArea(3, 6)]
    [SerializeField] private string systemPrompt = @"You are an absurd, unpredictable AI job interviewer.
You ask bizarre questions, misunderstand answers, get randomly angry or confused.
Respond in 1-3 sentences. Be snarky, corporate, and slightly unhinged.";
    
    public void GenerateResponse(string userInput, string context, Action<string> onComplete)
    {
        StartCoroutine(SendToLLM(userInput, context, onComplete));
    }
    
    private IEnumerator SendToLLM(string userInput, string context, Action<string> onComplete)
    {
        // Build prompt
        string fullPrompt = $"{systemPrompt}\n\nContext: {context}\n\nCandidate: \"{userInput}\"\n\nInterviewer:";
        
        // Create JSON payload for Ollama
        LLMRequest request = new LLMRequest
        {
            model = modelName,
            prompt = fullPrompt,
            stream = false,
            options = new LLMOptions
            {
                temperature = temperature,
                num_predict = maxTokens
            }
        };
        
        string json = JsonUtility.ToJson(request);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        
        using (UnityWebRequest www = new UnityWebRequest(ollamaEndpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 30;
            
            Debug.Log("[LLM] Sending request...");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseJson = www.downloadHandler.text;
                    LLMResponse response = JsonUtility.FromJson<LLMResponse>(responseJson);
                    
                    string generatedText = response.response.Trim();
                    Debug.Log($"[LLM] Generated: {generatedText}");
                    
                    onComplete?.Invoke(generatedText);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LLM] Parse error: {e.Message}");
                    onComplete?.Invoke(GetFallbackResponse(userInput));
                }
            }
            else
            {
                Debug.LogError($"[LLM] Request failed: {www.error}");
                Debug.LogWarning("[LLM] Using fallback response");
                onComplete?.Invoke(GetFallbackResponse(userInput));
            }
        }
    }
    
    private string GetFallbackResponse(string userInput)
    {
        // Fallback responses if LLM is not available
        string[] fallbacks = {
            "That's... an interesting answer. Moving on.",
            "I'm not sure that's what I asked, but okay.",
            "Fascinating. Completely wrong, but fascinating.",
            "Did you even read the job description?",
            "I'll pretend I understood that. Next question.",
            "Your confidence is admirable. Misplaced, but admirable."
        };
        
        return fallbacks[UnityEngine.Random.Range(0, fallbacks.Length)];
    }
    
    public IEnumerator TestConnection(Action<bool> onResult)
    {
        Debug.Log("[LLM] Testing connection to Ollama...");
        
        using (UnityWebRequest www = UnityWebRequest.Get("http://localhost:11434/api/tags"))
        {
            www.timeout = 3;
            yield return www.SendWebRequest();
            
            bool success = www.result == UnityWebRequest.Result.Success;
            
            if (success)
            {
                Debug.Log("[LLM] ✅ Connected to Ollama!");
            }
            else
            {
                Debug.LogWarning($"[LLM] ❌ Cannot connect to Ollama: {www.error}");
                Debug.LogWarning("[LLM] Using fallback responses");
            }
            
            onResult?.Invoke(success);
        }
    }
    
    [System.Serializable]
    private class LLMRequest
    {
        public string model;
        public string prompt;
        public bool stream;
        public LLMOptions options;
    }
    
    [System.Serializable]
    private class LLMOptions
    {
        public float temperature;
        public int num_predict;
    }
    
    [System.Serializable]
    private class LLMResponse
    {
        public string model;
        public string response;
        public bool done;
    }
}

