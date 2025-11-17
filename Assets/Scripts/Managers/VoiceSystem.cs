using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Voice System: Text-to-Speech (TTS) for teen and Speech-to-Text (STT) for player
/// Supports both Unity's native TTS and local models (Whisper + Bark/Piper)
/// </summary>
public class VoiceSystem : MonoBehaviour
{
    #region Configuration
    
    [Header("Voice Settings")]
    [SerializeField] private bool enableTTS = true;
    [SerializeField] private bool enableSTT = false; // Speech-to-text for player
    [SerializeField] private VoiceMode voiceMode = VoiceMode.UnityNative;
    
    [Header("Voice Personality")]
    [SerializeField] private float basePitch = 1.2f; // Slightly higher for teenager
    [SerializeField] private float baseSpeed = 1.0f;
    [SerializeField] private float emotionPitchVariation = 0.3f; // How much emotion affects pitch
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float volume = 0.8f;
    
    [Header("Local TTS (Optional)")]
    [SerializeField] private string localTTSEndpoint = "http://localhost:5000/tts";
    [SerializeField] private string localSTTEndpoint = "http://localhost:5000/stt";
    
    [Header("Ollama TTS (Optional)")]
    [SerializeField] private string ollamaEndpoint = "http://localhost:11434/api/generate";
    [SerializeField] private bool useOllamaTTS = false; // Use Ollama for TTS (requires TTS model)
    
    public enum VoiceMode
    {
        UnityNative,    // Built-in TTS (Windows SAPI, macOS AVSpeechSynthesizer)
        LocalModel,     // Local Whisper + Bark/Piper server
        WebAPI,         // External TTS service (Google, Azure, etc.)
        Disabled
    }
    
    #endregion
    
    #region State
    
    private bool isSpeaking = false;
    private Queue<TTSRequest> ttsQueue = new Queue<TTSRequest>();
    private bool isProcessingQueue = false;
    private System.Diagnostics.Process currentTTSProcess = null; // Track current TTS process
    
    private class TTSRequest
    {
        public string text;
        public EmotionalState.Emotion emotion;
        public Action onComplete;
        
        public TTSRequest(string text, EmotionalState.Emotion emotion, Action onComplete = null)
        {
            this.text = text;
            this.emotion = emotion;
            this.onComplete = onComplete;
        }
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.volume = volume;
        audioSource.playOnAwake = false;
        
        InitializeVoiceSystem();
    }
    
    private void Update()
    {
        // Process TTS queue
        if (!isProcessingQueue && ttsQueue.Count > 0)
        {
            StartCoroutine(ProcessTTSQueue());
        }
    }
    
    private void OnDisable()
    {
        StopAllTTS();
    }
    
    private void OnDestroy()
    {
        StopAllTTS();
    }
    
    private void OnApplicationQuit()
    {
        StopAllTTS();
    }
    
    #endregion
    
    #region TTS (Text-to-Speech)
    
    public void SpeakText(string text, EmotionalState.Emotion emotion = EmotionalState.Emotion.Neutral, Action onComplete = null)
    {
        if (!enableTTS)
        {
            Debug.LogWarning("[Voice] TTS is disabled!");
            onComplete?.Invoke();
            return;
        }
        
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("[Voice] Cannot speak empty text");
            onComplete?.Invoke();
            return;
        }
        
        Debug.Log($"[Voice] Queuing TTS: '{text}' (emotion: {emotion}, mode: {voiceMode})");
        TTSRequest request = new TTSRequest(text, emotion, onComplete);
        ttsQueue.Enqueue(request);
    }
    
    private IEnumerator ProcessTTSQueue()
    {
        isProcessingQueue = true;
        
        while (ttsQueue.Count > 0)
        {
            TTSRequest request = ttsQueue.Dequeue();
            
            yield return StartCoroutine(SpeakInternal(request.text, request.emotion));
            
            request.onComplete?.Invoke();
            
            // Small pause between sentences
            yield return new WaitForSeconds(0.3f);
        }
        
        isProcessingQueue = false;
    }
    
    private IEnumerator SpeakInternal(string text, EmotionalState.Emotion emotion)
    {
        isSpeaking = true;
        
        switch (voiceMode)
        {
            case VoiceMode.UnityNative:
                yield return StartCoroutine(SpeakUnityNative(text, emotion));
                break;
            
            case VoiceMode.LocalModel:
                yield return StartCoroutine(SpeakLocalModel(text, emotion));
                break;
            
            case VoiceMode.WebAPI:
                yield return StartCoroutine(SpeakWebAPI(text, emotion));
                break;
            
            default:
                Debug.Log($"[Voice] Speaking: {text}");
                yield return new WaitForSeconds(text.Length * 0.05f); // Simulate duration
                break;
        }
        
        isSpeaking = false;
    }
    
    #endregion
    
    #region TTS Implementations
    
    private IEnumerator SpeakUnityNative(string text, EmotionalState.Emotion emotion)
    {
        // Unity doesn't have built-in TTS in the engine itself
        // This would use platform-specific APIs (Windows SAPI, macOS, etc.)
        
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // Windows SAPI via System.Speech (requires .NET)
        yield return StartCoroutine(SpeakWindowsSAPI(text, emotion));
        #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        // macOS 'say' command
        yield return StartCoroutine(SpeakMacOS(text, emotion));
        #elif UNITY_ANDROID || UNITY_IOS
        // Mobile TTS (requires native plugins)
        yield return StartCoroutine(SpeakMobile(text, emotion));
        #else
        Debug.LogWarning("[Voice] Native TTS not supported on this platform");
        yield return new WaitForSeconds(text.Length * 0.05f);
        #endif
    }
    
    private IEnumerator SpeakWindowsSAPI(string text, EmotionalState.Emotion emotion)
    {
        // Windows SAPI implementation using PowerShell
        float duration = EstimateSpeechDuration(text);
        Debug.Log($"[Voice] Windows TTS: Speaking '{text}' (duration: {duration}s)");
        
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        System.Diagnostics.Process process = null;
        bool success = false;
        
        try
        {
            // Use a simpler approach: write text to a temp file and read it
            // This avoids PowerShell escaping issues
            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tts_temp.txt");
            System.IO.File.WriteAllText(tempFile, text);
            
            int rate = CalculateSpeechRate(emotion);
            
            // Use PowerShell with file input to avoid escaping issues
            string command = $"-Command \"$text = Get-Content '{tempFile}' -Raw; Add-Type -AssemblyName System.Speech; $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer; $synth.Rate = {rate}; $synth.Volume = 100; $synth.Speak($text); Remove-Item '{tempFile}'\"";
            
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            Debug.Log($"[Voice] Starting PowerShell TTS process...");
            process = System.Diagnostics.Process.Start(psi);
            
            if (process != null)
            {
                success = true;
                currentTTSProcess = process; // Store reference for cleanup
                Debug.Log($"[Voice] PowerShell process started (PID: {process.Id})");
            }
            else
            {
                Debug.LogError("[Voice] Failed to start PowerShell process");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Voice] Windows TTS failed: {e.Message}\n{e.StackTrace}");
            success = false;
        }
        
        // Wait for speech to complete (estimate duration) - outside try-catch
        yield return new WaitForSeconds(duration);
        
        // Clean up process if still running
        if (success && process != null)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                    Debug.Log("[Voice] TTS process terminated");
                }
                process.Dispose();
                if (currentTTSProcess == process)
                {
                    currentTTSProcess = null; // Clear reference
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Voice] Error cleaning up process: {e.Message}");
            }
        }
        #else
        Debug.LogWarning("[Voice] Windows TTS only works on Windows platform");
        yield return new WaitForSeconds(duration);
        #endif
    }
    
    private IEnumerator SpeakMacOS(string text, EmotionalState.Emotion emotion)
    {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        int rate = CalculateSpeechRate(emotion);
        string voice = "Samantha"; // Female teenager voice
        
        string command = $"say -v {voice} -r {rate} \"{text.Replace("\"", "\\\"")}\"";
        System.Diagnostics.Process.Start("/bin/bash", $"-c \"{command}\"");
        
        float duration = EstimateSpeechDuration(text);
        yield return new WaitForSeconds(duration);
        #else
        yield return null;
        #endif
    }
    
    private IEnumerator SpeakMobile(string text, EmotionalState.Emotion emotion)
    {
        // Mobile platforms require native plugins
        // Example: TextToSpeech plugin from Unity Asset Store
        
        Debug.Log($"[Voice] Mobile TTS: {text}");
        float duration = EstimateSpeechDuration(text);
        yield return new WaitForSeconds(duration);
    }
    
    private IEnumerator SpeakLocalModel(string text, EmotionalState.Emotion emotion)
    {
        // Use local TTS server (Bark, Piper, Coqui TTS, etc.)
        string url = $"{localTTSEndpoint}?text={UnityWebRequest.EscapeURL(text)}&emotion={emotion}";
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.pitch = CalculatePitchForEmotion(emotion);
                audioSource.Play();
                
                // Wait for audio to finish
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
            else
            {
                Debug.LogError($"[Voice] Local TTS error: {www.error}");
                yield return new WaitForSeconds(EstimateSpeechDuration(text));
            }
        }
    }
    
    private IEnumerator SpeakWebAPI(string text, EmotionalState.Emotion emotion)
    {
        // Use Ollama TTS if enabled, otherwise fallback to Windows SAPI
        if (useOllamaTTS)
        {
            yield return StartCoroutine(SpeakOllamaTTS(text, emotion));
        }
        else
        {
            // Fallback: Use Windows SAPI
            Debug.LogWarning("[Voice] Web API TTS not configured. Using Windows SAPI fallback.");
            yield return StartCoroutine(SpeakWindowsSAPI(text, emotion));
        }
    }
    
    private IEnumerator SpeakOllamaTTS(string text, EmotionalState.Emotion emotion)
    {
        // Note: Ollama doesn't have built-in TTS, but we can use it with a TTS model
        // For now, fallback to Windows SAPI
        Debug.LogWarning("[Voice] Ollama TTS not directly supported. Using Windows SAPI fallback.");
        yield return StartCoroutine(SpeakWindowsSAPI(text, emotion));
    }
    
    #endregion
    
    #region STT (Speech-to-Text)
    
    public void StartListening(Action<string> onRecognized)
    {
        if (!enableSTT)
        {
            Debug.LogWarning("[Voice] STT is disabled");
            return;
        }
        
        StartCoroutine(ListenForSpeech(onRecognized));
    }
    
    private IEnumerator ListenForSpeech(Action<string> onRecognized)
    {
        Debug.Log("[Voice] Listening for player speech...");
        
        // Record audio from microphone
        AudioClip recording = Microphone.Start(null, false, 10, 44100);
        
        // Wait for user to speak (or use voice activity detection)
        yield return new WaitForSeconds(3f);
        
        Microphone.End(null);
        
        // Convert to text
        yield return StartCoroutine(TranscribeAudio(recording, onRecognized));
    }
    
    private IEnumerator TranscribeAudio(AudioClip audio, Action<string> onRecognized)
    {
        if (voiceMode == VoiceMode.LocalModel)
        {
            // Send audio to local Whisper server
            yield return StartCoroutine(TranscribeLocal(audio, onRecognized));
        }
        else
        {
            // Fallback: return empty
            Debug.LogWarning("[Voice] STT not configured");
            onRecognized?.Invoke("");
        }
    }
    
    private IEnumerator TranscribeLocal(AudioClip audio, Action<string> onRecognized)
    {
        // Convert AudioClip to WAV bytes
        byte[] wavData = AudioClipToWav(audio);
        
        // Send to local Whisper server
        using (UnityWebRequest www = new UnityWebRequest(localSTTEndpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(wavData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "audio/wav");
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                string transcription = www.downloadHandler.text;
                Debug.Log($"[Voice] Transcribed: {transcription}");
                onRecognized?.Invoke(transcription);
            }
            else
            {
                Debug.LogError($"[Voice] STT error: {www.error}");
                onRecognized?.Invoke("");
            }
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private void InitializeVoiceSystem()
    {
        switch (voiceMode)
        {
            case VoiceMode.UnityNative:
                Debug.Log("[Voice] Using native platform TTS");
                break;
            
            case VoiceMode.LocalModel:
                Debug.Log($"[Voice] Using local TTS server: {localTTSEndpoint}");
                StartCoroutine(TestLocalConnection());
                break;
            
            case VoiceMode.WebAPI:
                Debug.Log("[Voice] Using web API TTS");
                break;
            
            default:
                Debug.Log("[Voice] TTS disabled");
                break;
        }
    }
    
    private IEnumerator TestLocalConnection()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(localTTSEndpoint))
        {
            www.timeout = 2;
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[Voice] Local TTS server not reachable. Falling back to simulation mode.");
                voiceMode = VoiceMode.Disabled;
            }
            else
            {
                Debug.Log("[Voice] Local TTS server connected successfully!");
            }
        }
    }
    
    private float CalculatePitchForEmotion(EmotionalState.Emotion emotion)
    {
        float pitch = basePitch;
        
        switch (emotion)
        {
            case EmotionalState.Emotion.Happy:
            case EmotionalState.Emotion.Receptive:
                pitch += emotionPitchVariation * 0.3f; // Higher pitch when happy
                break;
            
            case EmotionalState.Emotion.Angry:
            case EmotionalState.Emotion.Defiant:
                pitch -= emotionPitchVariation * 0.2f; // Lower, more intense
                break;
            
            case EmotionalState.Emotion.Sad:
            case EmotionalState.Emotion.Anxious:
                pitch -= emotionPitchVariation * 0.4f; // Lower, softer
                break;
            
            case EmotionalState.Emotion.Annoyed:
                pitch += emotionPitchVariation * 0.1f; // Slightly higher, sharp
                break;
        }
        
        return Mathf.Clamp(pitch, 0.5f, 2.0f);
    }
    
    private int CalculateSpeechRate(EmotionalState.Emotion emotion)
    {
        // Words per minute (default ~150)
        int baseRate = 150;
        
        switch (emotion)
        {
            case EmotionalState.Emotion.Angry:
            case EmotionalState.Emotion.Defiant:
                return (int)(baseRate * 1.3f); // Faster when angry
            
            case EmotionalState.Emotion.Sad:
            case EmotionalState.Emotion.Anxious:
                return (int)(baseRate * 0.8f); // Slower when sad
            
            case EmotionalState.Emotion.Happy:
            case EmotionalState.Emotion.Receptive:
                return (int)(baseRate * 1.1f); // Slightly faster when happy
            
            default:
                return baseRate;
        }
    }
    
    private float EstimateSpeechDuration(string text)
    {
        // Rough estimate: ~150 words per minute, ~5 chars per word
        int wordCount = text.Length / 5;
        float minutes = wordCount / 150f;
        return Mathf.Max(minutes * 60f, 1f); // Minimum 1 second
    }
    
    private byte[] AudioClipToWav(AudioClip clip)
    {
        // Convert AudioClip to WAV format
        // This is a simplified version - you may need a more robust implementation
        
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        
        Int16[] intData = new Int16[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (Int16)(samples[i] * Int16.MaxValue);
        }
        
        // Create WAV header + data
        // (Simplified - real implementation needs proper WAV header)
        byte[] wavData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, wavData, 0, wavData.Length);
        
        return wavData;
    }
    
    #endregion
    
    #region Public API
    
    public bool IsSpeaking => isSpeaking;
    
    public void StopSpeaking()
    {
        ttsQueue.Clear();
        audioSource.Stop();
        isSpeaking = false;
        StopAllCoroutines();
        StopCurrentTTSProcess();
    }
    
    private void StopAllTTS()
    {
        Debug.Log("[Voice] Stopping all TTS...");
        ttsQueue.Clear();
        audioSource.Stop();
        isSpeaking = false;
        isProcessingQueue = false;
        StopAllCoroutines();
        StopCurrentTTSProcess();
    }
    
    private void StopCurrentTTSProcess()
    {
        if (currentTTSProcess != null)
        {
            try
            {
                if (!currentTTSProcess.HasExited)
                {
                    Debug.Log($"[Voice] Killing TTS process (PID: {currentTTSProcess.Id})");
                    currentTTSProcess.Kill();
                    // Wait a bit for process to exit
                    currentTTSProcess.WaitForExit(1000);
                }
                currentTTSProcess.Dispose();
                currentTTSProcess = null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Voice] Error stopping TTS process: {e.Message}");
            }
        }
    }
    
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        audioSource.volume = volume;
    }
    
    public void SetVoiceMode(VoiceMode mode)
    {
        voiceMode = mode;
        InitializeVoiceSystem();
    }
    
    public void ToggleTTS(bool enabled)
    {
        enableTTS = enabled;
    }
    
    public void ToggleSTT(bool enabled)
    {
        enableSTT = enabled;
    }
    
    #endregion
}

