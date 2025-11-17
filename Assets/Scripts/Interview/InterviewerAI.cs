using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Main Interviewer AI - State machine controlling the interview flow
/// </summary>
public class InterviewerAI : MonoBehaviour
{
    public enum InterviewerMood
    {
        Professional,   // Start state
        Confused,       // Misunderstood answer
        Annoyed,        // Bad answer or nervous player
        Aggressive,     // Player messed up
        Amused,         // Player said something funny
        Unhinged        // Random chaos mode
    }
    
    [Header("References")]
    public WhisperSTT whisperSTT;
    public VoiceAnalyzer voiceAnalyzer;
    public SentimentAnalyzer sentimentAnalyzer;
    public LLMManager llmManager;
    public VoiceSystem voiceSystem;
    
    [Header("Current State")]
    public InterviewerMood currentMood = InterviewerMood.Professional;
    public int currentQuestionIndex = 0;
    public int strikeCount = 0;
    public int maxStrikes = 3;
    
    private QuestionManager questionManager;
    private bool isProcessing = false;
    
    public event Action<string> OnInterviewerSpeak;
    public event Action<string> OnFeedback;
    public event Action OnInterviewFailed;
    public event Action OnInterviewPassed;
    
    private void Start()
    {
        questionManager = GetComponent<QuestionManager>();
        if (questionManager == null)
        {
            questionManager = gameObject.AddComponent<QuestionManager>();
        }
        
        // Test LLM connection
        if (llmManager != null)
        {
            StartCoroutine(llmManager.TestConnection((success) => {
                Debug.Log(success ? "[Interview] LLM ready!" : "[Interview] LLM offline, using fallbacks");
            }));
        }
    }
    
    public void StartInterview()
    {
        currentQuestionIndex = 0;
        strikeCount = 0;
        currentMood = InterviewerMood.Professional;
        
        string opening = "Welcome to your FINAL interview. Let's see if you can handle this...";
        SpeakAndNotify(opening);
        
        Invoke(nameof(AskNextQuestion), 3f);
    }
    
    public void AskNextQuestion()
    {
        if (currentQuestionIndex >= questionManager.TotalQuestions)
        {
            // Interview complete!
            PassInterview();
            return;
        }
        
        QuestionManager.Question question = questionManager.GetQuestion(currentQuestionIndex);
        string questionText = GetQuestionWithMood(question);
        
        SpeakAndNotify(questionText);
    }
    
    public void ProcessPlayerAnswer(string transcript, AudioClip audioClip)
    {
        if (isProcessing) return;
        isProcessing = true;
        
        StartCoroutine(EvaluateAnswer(transcript, audioClip));
    }
    
    private IEnumerator EvaluateAnswer(string transcript, AudioClip audioClip)
    {
        // 1. Analyze voice (if audio provided)
        VoiceAnalyzer.VoiceMetrics voiceMetrics = null;
        if (audioClip != null && voiceAnalyzer != null)
        {
            voiceMetrics = voiceAnalyzer.AnalyzeAudio(audioClip);
        }
        else
        {
            // Create default metrics for text-only input
            voiceMetrics = new VoiceAnalyzer.VoiceMetrics
            {
                soundsConfident = true,
                soundsNervous = false,
                soundsEmotional = false,
                speechRate = 150f, // Average WPM
                pauseCount = 0
            };
        }
        
        // 2. Analyze sentiment
        SentimentAnalyzer.SentimentResult sentiment = sentimentAnalyzer.Analyze(transcript);
        
        // 3. Get current question
        QuestionManager.Question question = questionManager.GetQuestion(currentQuestionIndex);
        
        // 4. Validate answer
        bool passed = ValidateAnswer(question, transcript, voiceMetrics, sentiment);
        
        // 5. Update mood based on answer
        UpdateMood(passed, sentiment, voiceMetrics);
        
        // 6. Generate LLM response
        string context = BuildContext(question, voiceMetrics, sentiment, passed);
        
        yield return new WaitForSeconds(0.5f); // Dramatic pause
        
        if (llmManager != null)
        {
            bool llmCompleted = false;
            llmManager.GenerateResponse(transcript, context, (response) => {
                HandleInterviewerResponse(response, passed);
                llmCompleted = true;
            });
            
            // Wait for LLM
            float timeout = 0f;
            while (!llmCompleted && timeout < 10f)
            {
                timeout += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            string fallbackResponse = GetFallbackResponse(passed, sentiment);
            HandleInterviewerResponse(fallbackResponse, passed);
        }
        
        isProcessing = false;
    }
    
    private bool ValidateAnswer(QuestionManager.Question question, string transcript, 
                                VoiceAnalyzer.VoiceMetrics voice, SentimentAnalyzer.SentimentResult sentiment)
    {
        // Question-specific validation
        bool passed = true;
        
        switch (question.validationType)
        {
            case QuestionManager.ValidationType.ForbiddenWords:
                // Check if contains forbidden words
                foreach (string forbidden in question.forbiddenWords)
                {
                    if (transcript.ToLower().Contains(forbidden.ToLower()))
                    {
                        passed = false;
                        OnFeedback?.Invoke($"❌ You said '{forbidden}'! FORBIDDEN!");
                        break;
                    }
                }
                break;
            
            case QuestionManager.ValidationType.MustBeEmotional:
                // Must sound emotional/passionate (text: check sentiment/word choice)
                if (voice != null && !voice.soundsEmotional && sentiment.assertiveness < 0.5f)
                {
                    passed = false;
                    OnFeedback?.Invoke("❌ That was pathetic. Where's the emotion?");
                }
                else if (voice == null && sentiment.assertiveness < 0.5f && !sentiment.containsHumor)
                {
                    // Text-only: check for emotional words
                    passed = false;
                    OnFeedback?.Invoke("❌ That was pathetic. Where's the emotion?");
                }
                break;
            
            case QuestionManager.ValidationType.MustBeConfident:
                // Must sound confident, no hesitation
                if (voice != null && (voice.soundsNervous || sentiment.soundsUncertain))
                {
                    passed = false;
                    OnFeedback?.Invoke("❌ You sound nervous. Not convinced.");
                }
                else if (voice == null && sentiment.soundsUncertain)
                {
                    // Text-only: check for uncertain words
                    passed = false;
                    OnFeedback?.Invoke("❌ You sound uncertain. Not convinced.");
                }
                break;
            
            case QuestionManager.ValidationType.MustBeCreative:
                // Check for word count and variety
                if (sentiment.wordCount < 10)
                {
                    passed = false;
                    OnFeedback?.Invoke("❌ Too short. Be more creative!");
                }
                break;
            
            case QuestionManager.ValidationType.VoiceActing:
                // Must sound different from normal (text: check for creative/expressive language)
                if (voice != null && voice.pitchVariation < 30f && !voice.soundsEmotional)
                {
                    passed = false;
                    OnFeedback?.Invoke("❌ That's not acting. DO IT AGAIN.");
                }
                else if (voice == null && sentiment.wordCount < 15)
                {
                    // Text-only: must be expressive/creative
                    passed = false;
                    OnFeedback?.Invoke("❌ That's not acting. Be more expressive!");
                }
                break;
        }
        
        if (passed)
        {
            OnFeedback?.Invoke("✅ Acceptable... for now.");
        }
        else
        {
            strikeCount++;
            if (strikeCount >= maxStrikes)
            {
                FailInterview();
            }
        }
        
        return passed;
    }
    
    private void UpdateMood(bool passed, SentimentAnalyzer.SentimentResult sentiment, VoiceAnalyzer.VoiceMetrics voice)
    {
        if (!passed)
        {
            // Bad answer = more aggressive
            if (currentMood == InterviewerMood.Annoyed)
                currentMood = InterviewerMood.Aggressive;
            else if (currentMood != InterviewerMood.Aggressive)
                currentMood = InterviewerMood.Annoyed;
        }
        else
        {
            // Good answer = random mood shift
            float rand = UnityEngine.Random.value;
            
            if (sentiment.containsHumor)
                currentMood = InterviewerMood.Amused;
            else if (rand < 0.2f)
                currentMood = InterviewerMood.Unhinged; // Random chaos
            else if (rand < 0.4f)
                currentMood = InterviewerMood.Confused;
            else
                currentMood = InterviewerMood.Professional;
        }
        
        // Voice metrics not needed for mood update (works with text too)
    }
    
    private string BuildContext(QuestionManager.Question question, VoiceAnalyzer.VoiceMetrics voice, 
                                SentimentAnalyzer.SentimentResult sentiment, bool passed)
    {
        string voiceDesc = voice != null && voiceAnalyzer != null 
            ? voiceAnalyzer.GetConfidenceDescription(voice) 
            : "Text input";
        
        return $@"Question: {question.questionText}
Candidate input: {voiceDesc}
Sentiment: {sentiment.sentiment}, Nervousness: {sentiment.nervousness:F2}
Answer quality: {(passed ? "Passed" : "Failed")}
Your mood: {currentMood}
Strikes: {strikeCount}/{maxStrikes}";
    }
    
    private void HandleInterviewerResponse(string response, bool passed)
    {
        SpeakAndNotify(response);
        
        if (passed && strikeCount < maxStrikes)
        {
            currentQuestionIndex++;
            Invoke(nameof(AskNextQuestion), 4f);
        }
    }
    
    private string GetQuestionWithMood(QuestionManager.Question question)
    {
        string prefix = currentMood switch
        {
            InterviewerMood.Confused => "Wait, what? Uh... ",
            InterviewerMood.Annoyed => "*sigh* Fine. ",
            InterviewerMood.Aggressive => "LISTEN. ",
            InterviewerMood.Amused => "*chuckles* Okay, okay. ",
            InterviewerMood.Unhinged => "HAHAHA! Wait, serious now. ",
            _ => ""
        };
        
        return prefix + question.questionText;
    }
    
    private string GetFallbackResponse(bool passed, SentimentAnalyzer.SentimentResult sentiment)
    {
        if (passed)
        {
            string[] positive = {
                "Hmm. Not terrible.",
                "I'll allow it.",
                "Interesting. Moving on.",
                "That'll do... I guess."
            };
            return positive[UnityEngine.Random.Range(0, positive.Length)];
        }
        else
        {
            string[] negative = {
                "What was that?!",
                "Are you even trying?",
                "That's strike " + strikeCount + ".",
                "Unbelievable."
            };
            return negative[UnityEngine.Random.Range(0, negative.Length)];
        }
    }
    
    private void SpeakAndNotify(string text)
    {
        OnInterviewerSpeak?.Invoke(text);
        
        if (voiceSystem != null)
        {
            EmotionalState.Emotion emotion = currentMood switch
            {
                InterviewerMood.Aggressive => EmotionalState.Emotion.Angry,
                InterviewerMood.Annoyed => EmotionalState.Emotion.Annoyed,
                InterviewerMood.Amused => EmotionalState.Emotion.Happy,
                _ => EmotionalState.Emotion.Neutral
            };
            
            voiceSystem.SpeakText(text, emotion);
        }
    }
    
    private void FailInterview()
    {
        string failMessage = currentMood switch
        {
            InterviewerMood.Aggressive => "GET OUT. You're REJECTED.",
            InterviewerMood.Unhinged => "I can't believe it. You actually failed. SECURITY!",
            _ => "I'm sorry, but this isn't working out. Interview TERMINATED."
        };
        
        SpeakAndNotify(failMessage);
        OnInterviewFailed?.Invoke();
    }
    
    private void PassInterview()
    {
        string passMessage = "You know what? Against all odds... you're HIRED. Congratulations, I guess.";
        SpeakAndNotify(passMessage);
        OnInterviewPassed?.Invoke();
    }
}

