using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// Whisper Speech-to-Text integration (local or API)
/// </summary>
public class WhisperSTT : MonoBehaviour
{
    [Header("Whisper Settings")]
    [SerializeField] private string whisperEndpoint = "http://localhost:9000/inference";
    [SerializeField] private bool useLocalWhisper = true;
    [SerializeField] private float maxRecordingTime = 30f;
    
    private AudioClip recordedClip;
    private bool isRecording = false;
    
    public bool IsRecording => isRecording;
    
    public void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("[Whisper] Already recording");
            return;
        }
        
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[Whisper] No microphone found!");
            return;
        }
        
        string device = Microphone.devices[0];
        recordedClip = Microphone.Start(device, false, (int)maxRecordingTime, 44100);
        isRecording = true;
        
        Debug.Log($"[Whisper] Recording started on {device}");
    }
    
    public void StopRecording(Action<string, AudioClip> onTranscribed)
    {
        if (!isRecording)
        {
            Debug.LogWarning("[Whisper] Not currently recording");
            return;
        }
        
        Microphone.End(null);
        isRecording = false;
        
        Debug.Log("[Whisper] Recording stopped, transcribing...");
        
        StartCoroutine(TranscribeAudio(recordedClip, onTranscribed));
    }
    
    private IEnumerator TranscribeAudio(AudioClip clip, Action<string, AudioClip> onComplete)
    {
        if (useLocalWhisper)
        {
            yield return StartCoroutine(TranscribeLocal(clip, onComplete));
        }
        else
        {
            // Fallback: simulate transcription for testing
            yield return new WaitForSeconds(1f);
            string mockTranscript = "This is a mock transcript for testing purposes.";
            Debug.Log($"[Whisper] Mock transcript: {mockTranscript}");
            onComplete?.Invoke(mockTranscript, clip);
        }
    }
    
    private IEnumerator TranscribeLocal(AudioClip clip, Action<string, AudioClip> onComplete)
    {
        // Convert AudioClip to WAV
        byte[] wavData = ConvertToWav(clip);
        
        // Send to local Whisper server
        using (UnityWebRequest www = new UnityWebRequest(whisperEndpoint, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(wavData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "audio/wav");
            www.timeout = 30;
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Parse JSON response: {"text": "transcribed text"}
                    string json = www.downloadHandler.text;
                    WhisperResponse response = JsonUtility.FromJson<WhisperResponse>(json);
                    
                    Debug.Log($"[Whisper] Transcribed: {response.text}");
                    onComplete?.Invoke(response.text, clip);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Whisper] Parse error: {e.Message}");
                    // Fallback: try to extract text directly
                    string text = ExtractTextFromResponse(www.downloadHandler.text);
                    onComplete?.Invoke(text, clip);
                }
            }
            else
            {
                Debug.LogError($"[Whisper] Transcription failed: {www.error}");
                Debug.LogWarning("[Whisper] Falling back to mock transcript");
                onComplete?.Invoke("[Unable to transcribe - check Whisper server]", clip);
            }
        }
    }
    
    private string ExtractTextFromResponse(string response)
    {
        // Try to extract text from various response formats
        if (response.Contains("\"text\""))
        {
            int startIdx = response.IndexOf("\"text\"") + 8;
            int endIdx = response.IndexOf("\"", startIdx);
            if (endIdx > startIdx)
            {
                return response.Substring(startIdx, endIdx - startIdx);
            }
        }
        return response; // Return raw if can't parse
    }
    
    private byte[] ConvertToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        
        Int16[] intData = new Int16[samples.Length];
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * 32767f);
        }
        
        byte[] bytesData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        
        // Create WAV header
        int hz = clip.frequency;
        int channels = clip.channels;
        int sampleCount = clip.samples;
        
        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // RIFF header
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + bytesData.Length);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                
                // fmt chunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // chunk size
                writer.Write((short)1); // PCM
                writer.Write((short)channels);
                writer.Write(hz);
                writer.Write(hz * channels * 2); // byte rate
                writer.Write((short)(channels * 2)); // block align
                writer.Write((short)16); // bits per sample
                
                // data chunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(bytesData.Length);
                writer.Write(bytesData);
            }
            
            return stream.ToArray();
        }
    }
    
    [System.Serializable]
    private class WhisperResponse
    {
        public string text;
    }
    
    public void TestMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("[Whisper] No microphone devices found!");
            return;
        }
        
        Debug.Log($"[Whisper] Found {Microphone.devices.Length} microphone(s):");
        foreach (string device in Microphone.devices)
        {
            Debug.Log($"  - {device}");
        }
    }
}

