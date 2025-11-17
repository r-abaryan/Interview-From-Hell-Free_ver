using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the 5 absurd interview questions
/// </summary>
public class QuestionManager : MonoBehaviour
{
    public enum ValidationType
    {
        ForbiddenWords,     // Can't say certain words
        MustBeEmotional,    // Must show emotion/passion
        MustBeConfident,    // No hesitation
        MustBeCreative,     // Unique/long answer
        VoiceActing         // Must roleplay
    }
    
    [System.Serializable]
    public class Question
    {
        public int number;
        public string questionText;
        public ValidationType validationType;
        public string[] forbiddenWords;
        public string hint;
    }
    
    private List<Question> questions = new List<Question>();
    
    public int TotalQuestions => questions.Count;
    
    private void Awake()
    {
        InitializeQuestions();
    }
    
    private void InitializeQuestions()
    {
        questions.Clear();
        
        // Q1: The Warm-Up (with a twist)
        questions.Add(new Question
        {
            number = 1,
            questionText = "Tell me about yourself... but do it as if you're hiding a terrible secret.",
            validationType = ValidationType.VoiceActing,
            hint = "Sound mysterious and guilty!"
        });
        
        // Q2: Cognitive Torture
        questions.Add(new Question
        {
            number = 2,
            questionText = "Describe cloud computing... WITHOUT using the words: cloud, data, server, internet, or compute.",
            validationType = ValidationType.ForbiddenWords,
            forbiddenWords = new string[] { "cloud", "data", "server", "internet", "compute", "computing" },
            hint = "Get creative with synonyms!"
        });
        
        // Q3: Psychological Warfare
        questions.Add(new Question
        {
            number = 3,
            questionText = "You paused for 0.7 seconds. Are you lying, or are you simply nervous? Explain.",
            validationType = ValidationType.MustBeConfident,
            hint = "Be confident, no hesitation!"
        });
        
        // Q4: Roleplay Insanity
        questions.Add(new Question
        {
            number = 4,
            questionText = "For this question, answer as a MEDIEVAL WARRIOR trying to get a software job. Why should we hire you?",
            validationType = ValidationType.VoiceActing,
            hint = "ACT like a warrior! Deep voice, passion!"
        });
        
        // Q5: Pure Emotion Test
        questions.Add(new Question
        {
            number = 5,
            questionText = "Final question. Convince me why YOU deserve this job... using PURE EMOTION. No smart words. Just feeling.",
            validationType = ValidationType.MustBeEmotional,
            hint = "Show passion and intensity!"
        });
    }
    
    public Question GetQuestion(int index)
    {
        if (index >= 0 && index < questions.Count)
        {
            return questions[index];
        }
        
        Debug.LogError($"[Questions] Invalid index: {index}");
        return null;
    }
    
    public Question GetRandomQuestion()
    {
        return questions[Random.Range(0, questions.Count)];
    }
    
    // Alternative questions pool for variety
    public void AddBonusQuestions()
    {
        var bonusQuestions = new List<Question>
        {
            new Question
            {
                number = 99,
                questionText = "If you were a sandwich, what salary would you expect?",
                validationType = ValidationType.MustBeCreative
            },
            new Question
            {
                number = 99,
                questionText = "Explain your biggest weakness... but make it FUNNY.",
                validationType = ValidationType.MustBeCreative
            },
            new Question
            {
                number = 99,
                questionText = "Wait, what position did you even apply for? Tell me again.",
                validationType = ValidationType.MustBeConfident
            },
            new Question
            {
                number = 99,
                questionText = "Sing your resume. I'm serious.",
                validationType = ValidationType.VoiceActing
            }
        };
        
        // Randomly replace one question with a bonus
        int replaceIndex = Random.Range(1, questions.Count - 1);
        questions[replaceIndex] = bonusQuestions[Random.Range(0, bonusQuestions.Count)];
    }
}

