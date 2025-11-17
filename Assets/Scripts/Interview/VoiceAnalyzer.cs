using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Analyzes voice characteristics: pitch, volume, pauses, speech rate
/// </summary>
public class VoiceAnalyzer : MonoBehaviour
{
    public class VoiceMetrics
    {
        public float averagePitch;      // Hz
        public float pitchVariation;    // Standard deviation
        public float averageVolume;     // 0-1
        public float volumeVariation;   
        public float speechRate;        // Words per minute
        public int pauseCount;          // Number of pauses
        public float averagePauseLength; // Seconds
        public float totalDuration;     // Seconds
        public bool soundsConfident;
        public bool soundsNervous;
        public bool soundsEmotional;
    }
    
    private const float PAUSE_THRESHOLD = 0.3f; // 300ms silence = pause
    private const float NERVOUS_PAUSE_THRESHOLD = 0.5f;
    private const int SAMPLE_RATE = 44100;
    
    public VoiceMetrics AnalyzeAudio(AudioClip clip)
    {
        VoiceMetrics metrics = new VoiceMetrics();
        
        if (clip == null || clip.samples == 0)
        {
            Debug.LogWarning("[VoiceAnalyzer] Invalid audio clip");
            return metrics;
        }
        
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);
        
        // Calculate duration
        metrics.totalDuration = clip.length;
        
        // Analyze volume
        AnalyzeVolume(samples, metrics);
        
        // Analyze pitch
        AnalyzePitch(samples, clip.frequency, metrics);
        
        // Detect pauses
        AnalyzePauses(samples, clip.frequency, metrics);
        
        // Calculate confidence indicators
        DetermineEmotionalState(metrics);
        
        Debug.Log($"[Voice] Pitch: {metrics.averagePitch:F0}Hz, Volume: {metrics.averageVolume:F2}, " +
                  $"Rate: {metrics.speechRate:F0}wpm, Pauses: {metrics.pauseCount}, " +
                  $"Confident: {metrics.soundsConfident}, Nervous: {metrics.soundsNervous}");
        
        return metrics;
    }
    
    private void AnalyzeVolume(float[] samples, VoiceMetrics metrics)
    {
        float sum = 0f;
        float sumSquares = 0f;
        
        foreach (float sample in samples)
        {
            float absValue = Mathf.Abs(sample);
            sum += absValue;
            sumSquares += absValue * absValue;
        }
        
        metrics.averageVolume = sum / samples.Length;
        
        // Calculate RMS (Root Mean Square) for better volume measurement
        float rms = Mathf.Sqrt(sumSquares / samples.Length);
        metrics.averageVolume = rms;
        
        // Calculate variation
        float variance = 0f;
        foreach (float sample in samples)
        {
            float diff = Mathf.Abs(sample) - metrics.averageVolume;
            variance += diff * diff;
        }
        metrics.volumeVariation = Mathf.Sqrt(variance / samples.Length);
    }
    
    private void AnalyzePitch(float[] samples, int sampleRate, VoiceMetrics metrics)
    {
        // Simple autocorrelation-based pitch detection
        List<float> pitches = new List<float>();
        
        int windowSize = 2048; // ~46ms at 44.1kHz
        int hopSize = windowSize / 2;
        
        for (int i = 0; i < samples.Length - windowSize; i += hopSize)
        {
            float[] window = new float[windowSize];
            System.Array.Copy(samples, i, window, 0, windowSize);
            
            // Skip if window is too quiet (likely silence/pause)
            float windowEnergy = window.Sum(s => s * s);
            if (windowEnergy < 0.001f) continue;
            
            float pitch = DetectPitch(window, sampleRate);
            if (pitch > 50f && pitch < 500f) // Valid human speech range
            {
                pitches.Add(pitch);
            }
        }
        
        if (pitches.Count > 0)
        {
            metrics.averagePitch = pitches.Average();
            
            // Calculate pitch variation
            float variance = pitches.Sum(p => (p - metrics.averagePitch) * (p - metrics.averagePitch));
            metrics.pitchVariation = Mathf.Sqrt(variance / pitches.Count);
        }
    }
    
    private float DetectPitch(float[] window, int sampleRate)
    {
        // Autocorrelation method
        int minLag = sampleRate / 500; // Max 500 Hz
        int maxLag = sampleRate / 50;  // Min 50 Hz
        
        float maxCorrelation = 0f;
        int bestLag = minLag;
        
        for (int lag = minLag; lag < maxLag && lag < window.Length / 2; lag++)
        {
            float correlation = 0f;
            for (int i = 0; i < window.Length - lag; i++)
            {
                correlation += window[i] * window[i + lag];
            }
            
            if (correlation > maxCorrelation)
            {
                maxCorrelation = correlation;
                bestLag = lag;
            }
        }
        
        return (float)sampleRate / bestLag;
    }
    
    private void AnalyzePauses(float[] samples, int sampleRate, VoiceMetrics metrics)
    {
        List<float> pauseLengths = new List<float>();
        
        int windowSize = (int)(0.05f * sampleRate); // 50ms windows
        bool inPause = false;
        int pauseStart = 0;
        int silentFrames = 0;
        int totalSpeechFrames = 0;
        
        for (int i = 0; i < samples.Length; i += windowSize)
        {
            int endIdx = Mathf.Min(i + windowSize, samples.Length);
            float windowEnergy = 0f;
            
            for (int j = i; j < endIdx; j++)
            {
                windowEnergy += samples[j] * samples[j];
            }
            windowEnergy /= (endIdx - i);
            
            bool isSilent = windowEnergy < 0.001f;
            
            if (isSilent)
            {
                if (!inPause)
                {
                    inPause = true;
                    pauseStart = i;
                }
                silentFrames++;
            }
            else
            {
                if (inPause)
                {
                    float pauseDuration = (i - pauseStart) / (float)sampleRate;
                    if (pauseDuration >= PAUSE_THRESHOLD)
                    {
                        pauseLengths.Add(pauseDuration);
                        metrics.pauseCount++;
                    }
                    inPause = false;
                }
                totalSpeechFrames++;
            }
        }
        
        if (pauseLengths.Count > 0)
        {
            metrics.averagePauseLength = pauseLengths.Average();
        }
        
        // Calculate speech rate (rough estimate)
        // Assuming average word is ~0.5 seconds
        float totalSpeechTime = (totalSpeechFrames * windowSize) / (float)sampleRate;
        if (totalSpeechTime > 0)
        {
            metrics.speechRate = (totalSpeechTime / 0.5f) * (60f / metrics.totalDuration);
            metrics.speechRate = Mathf.Clamp(metrics.speechRate, 60f, 240f); // Reasonable WPM range
        }
    }
    
    private void DetermineEmotionalState(VoiceMetrics metrics)
    {
        // Confidence indicators:
        // - Steady volume (low variation)
        // - Moderate-high pitch
        // - Few/short pauses
        // - Steady speech rate
        
        bool steadyVolume = metrics.volumeVariation < 0.1f;
        bool goodVolume = metrics.averageVolume > 0.05f;
        bool fewPauses = metrics.pauseCount < 3 || metrics.averagePauseLength < NERVOUS_PAUSE_THRESHOLD;
        bool steadyPitch = metrics.pitchVariation < 30f;
        bool goodRate = metrics.speechRate > 100f && metrics.speechRate < 180f;
        
        metrics.soundsConfident = steadyVolume && goodVolume && fewPauses && steadyPitch && goodRate;
        
        // Nervous indicators:
        // - Many pauses
        // - High pitch variation
        // - Very fast or very slow speech
        // - Low volume or high volume variation
        
        bool manyPauses = metrics.pauseCount > 5 || metrics.averagePauseLength > NERVOUS_PAUSE_THRESHOLD;
        bool unstablePitch = metrics.pitchVariation > 50f;
        bool extremeRate = metrics.speechRate < 80f || metrics.speechRate > 200f;
        bool unstableVolume = metrics.volumeVariation > 0.15f;
        
        metrics.soundsNervous = manyPauses || unstablePitch || extremeRate || unstableVolume;
        
        // Emotional indicators:
        // - High pitch variation
        // - High volume variation
        // - Variable speech rate
        
        bool expressivePitch = metrics.pitchVariation > 40f;
        bool expressiveVolume = metrics.volumeVariation > 0.12f;
        
        metrics.soundsEmotional = expressivePitch && expressiveVolume;
    }
    
    public string GetConfidenceDescription(VoiceMetrics metrics)
    {
        if (metrics.soundsConfident)
            return "Confident and steady";
        else if (metrics.soundsNervous)
            return "Nervous and hesitant";
        else if (metrics.soundsEmotional)
            return "Emotional and expressive";
        else
            return "Neutral";
    }
}

