using UnityEngine;

/// <summary>
/// Controls teen character animations based on emotional state
/// </summary>
[RequireComponent(typeof(Animator))]
public class TeenAnimationController : MonoBehaviour
{
    [Header("References")]
    public TeenAgent teenAgent;
    
    [Header("Animation Settings")]
    [Tooltip("Speed multiplier for animations")]
    public float animationSpeed = 1f;
    
    [Tooltip("Time between idle variations (seconds)")]
    public float idleVariationInterval = 5f;
    
    private Animator animator;
    private EmotionalState.Emotion lastEmotion;
    private float lastIdleVariation;
    private int idleVariationCount = 3; // Number of idle variations
    
    // Animation parameter names (hashes for performance)
    // Base emotions
    private static readonly int IsAngryHash = Animator.StringToHash("IsAngry");
    private static readonly int IsHappyHash = Animator.StringToHash("IsHappy");
    private static readonly int IsSadHash = Animator.StringToHash("IsSad");
    private static readonly int IsDefiantHash = Animator.StringToHash("IsDefiant");
    private static readonly int IsNeutralHash = Animator.StringToHash("IsNeutral");
    private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");
    private static readonly int EmotionIndexHash = Animator.StringToHash("EmotionIndex");
    private static readonly int IdleVariationHash = Animator.StringToHash("IdleVariation");
    private static readonly int TriggerReactionHash = Animator.StringToHash("TriggerReaction");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    
    // Nuanced reactions (NEW!)
    private static readonly int EyeRollHash = Animator.StringToHash("EyeRoll");
    private static readonly int LaughHash = Animator.StringToHash("Laugh");
    private static readonly int ShockedHash = Animator.StringToHash("Shocked");
    private static readonly int AnnoyedHash = Animator.StringToHash("Annoyed");
    private static readonly int SighHash = Animator.StringToHash("Sigh");
    private static readonly int SmirkHash = Animator.StringToHash("Smirk");
    private static readonly int FacepalHash = Animator.StringToHash("Facepalm");
    private static readonly int ThinkingHash = Animator.StringToHash("Thinking");
    private static readonly int NodHash = Animator.StringToHash("Nod");
    private static readonly int ShakeHeadHash = Animator.StringToHash("ShakeHead");
    private static readonly int LookAwayHash = Animator.StringToHash("LookAway");
    private static readonly int ScoffHash = Animator.StringToHash("Scoff");
    private static readonly int PoutHash = Animator.StringToHash("Pout");
    private static readonly int GiggleHash = Animator.StringToHash("Giggle");
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogWarning("⚠️ TeenAnimationController: No Animator found!");
            return;
        }
        
        // Find teen agent if not assigned
        if (teenAgent == null)
        {
            teenAgent = GetComponent<TeenAgent>();
            if (teenAgent == null)
            {
                teenAgent = FindFirstObjectByType<TeenAgent>();
            }
        }
        
        if (teenAgent == null)
        {
            Debug.LogWarning("⚠️ TeenAnimationController: No TeenAgent found!");
        }
        else
        {
            Debug.Log("✅ TeenAnimationController initialized");
        }
        
        animator.speed = animationSpeed;
        lastIdleVariation = Time.time;
    }
    
    void Update()
    {
        if (animator == null || teenAgent == null) return;
        
        EmotionalState.Emotion currentEmotion = teenAgent.emotionalState.currentEmotion;
        
        // Update emotion-based animations
        if (currentEmotion != lastEmotion)
        {
            UpdateEmotionAnimation(currentEmotion);
            lastEmotion = currentEmotion;
            
            // Trigger reaction on emotion change
            animator.SetTrigger(TriggerReactionHash);
        }
        
        // Idle variations
        if (Time.time - lastIdleVariation > idleVariationInterval)
        {
            int variation = Random.Range(0, idleVariationCount);
            animator.SetInteger(IdleVariationHash, variation);
            lastIdleVariation = Time.time;
        }
    }
    
    /// <summary>
    /// Update animator parameters based on current emotion
    /// </summary>
    private void UpdateEmotionAnimation(EmotionalState.Emotion emotion)
    {
        // Reset all emotion states
        animator.SetBool(IsAngryHash, false);
        animator.SetBool(IsHappyHash, false);
        animator.SetBool(IsSadHash, false);
        animator.SetBool(IsDefiantHash, false);
        animator.SetBool(IsNeutralHash, false);
        
        // Set current emotion
        int emotionIndex = (int)emotion;
        animator.SetInteger(EmotionIndexHash, emotionIndex);
        
        switch (emotion)
        {
            case EmotionalState.Emotion.Happy:
            case EmotionalState.Emotion.Receptive:
                animator.SetBool(IsHappyHash, true);
                break;
                
            case EmotionalState.Emotion.Angry:
            case EmotionalState.Emotion.Annoyed:
                animator.SetBool(IsAngryHash, true);
                break;
                
            case EmotionalState.Emotion.Sad:
            case EmotionalState.Emotion.Anxious:
                animator.SetBool(IsSadHash, true);
                break;
                
            case EmotionalState.Emotion.Defiant:
                animator.SetBool(IsDefiantHash, true);
                break;
                
            case EmotionalState.Emotion.Neutral:
            default:
                animator.SetBool(IsNeutralHash, true);
                break;
        }
        
        Debug.Log($"🎭 Animation: {emotion}");
    }
    
    /// <summary>
    /// Play talking animation
    /// </summary>
    public void StartTalking()
    {
        if (animator != null)
        {
            animator.SetBool(IsTalkingHash, true);
        }
    }
    
    /// <summary>
    /// Stop talking animation
    /// </summary>
    public void StopTalking()
    {
        if (animator != null)
        {
            animator.SetBool(IsTalkingHash, false);
        }
    }
    
    /// <summary>
    /// Trigger a specific reaction animation
    /// </summary>
    public void TriggerReaction(string reactionType)
    {
        if (animator != null)
        {
            animator.SetTrigger(reactionType);
            Debug.Log($"🎭 Triggered reaction: {reactionType}");
        }
    }
    
    /// <summary>
    /// Play specific emotion animation immediately
    /// </summary>
    public void PlayEmotionAnimation(EmotionalState.Emotion emotion)
    {
        UpdateEmotionAnimation(emotion);
        animator.SetTrigger(TriggerReactionHash);
    }
    
    /// <summary>
    /// Set animation speed
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animationSpeed = speed;
            animator.speed = speed;
            animator.SetFloat(SpeedHash, speed);
        }
    }
    
    /// <summary>
    /// Play celebration animation (when conversation succeeds)
    /// </summary>
    public void PlayCelebration()
    {
        if (animator != null)
        {
            animator.SetTrigger("Celebrate");
        }
    }
    
    /// <summary>
    /// Play angry storm off animation (when teen refuses)
    /// </summary>
    public void PlayStormOff()
    {
        if (animator != null)
        {
            animator.SetTrigger("StormOff");
        }
    }
    
    /// <summary>
    /// Play shrug/whatever animation
    /// </summary>
    public void PlayShrug()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shrug");
        }
    }
    
    /// <summary>
    /// Play cross arms animation (defiant)
    /// </summary>
    public void PlayCrossArms()
    {
        if (animator != null)
        {
            animator.SetTrigger("CrossArms");
        }
    }
    
    #region Nuanced Reactions (UPGRADE C)
    
    /// <summary>
    /// Eye roll - Teen is dismissive/annoyed
    /// Perfect for: Sarcasm, boredom, "Whatever..."
    /// </summary>
    public void PlayEyeRoll()
    {
        if (animator != null)
        {
            animator.SetTrigger(EyeRollHash);
            Debug.Log("👀 *eye roll*");
        }
    }
    
    /// <summary>
    /// Laughing - Teen finds something funny
    /// </summary>
    public void PlayLaugh()
    {
        if (animator != null)
        {
            animator.SetTrigger(LaughHash);
            Debug.Log("😂 *laughs*");
        }
    }
    
    /// <summary>
    /// Giggle - Lighter laugh, more playful
    /// </summary>
    public void PlayGiggle()
    {
        if (animator != null)
        {
            animator.SetTrigger(GiggleHash);
            Debug.Log("🤭 *giggles*");
        }
    }
    
    /// <summary>
    /// Shocked/Surprised - Teen is taken aback
    /// Perfect for: Unexpected punishment, surprising news
    /// </summary>
    public void PlayShocked()
    {
        if (animator != null)
        {
            animator.SetTrigger(ShockedHash);
            Debug.Log("😲 *shocked*");
        }
    }
    
    /// <summary>
    /// Annoyed - Teen is irritated but not fully angry
    /// Perfect for: Nagging, repeated requests
    /// </summary>
    public void PlayAnnoyed()
    {
        if (animator != null)
        {
            animator.SetTrigger(AnnoyedHash);
            Debug.Log("😒 *annoyed*");
        }
    }
    
    /// <summary>
    /// Heavy sigh - Exasperation, resignation
    /// Perfect for: "Fine, I'll do it", reluctant agreement
    /// </summary>
    public void PlaySigh()
    {
        if (animator != null)
        {
            animator.SetTrigger(SighHash);
            Debug.Log("😮‍💨 *sighs heavily*");
        }
    }
    
    /// <summary>
    /// Smirk - Teen is being cheeky/sly
    /// Perfect for: Sarcasm, negotiation, "got you"
    /// </summary>
    public void PlaySmirk()
    {
        if (animator != null)
        {
            animator.SetTrigger(SmirkHash);
            Debug.Log("😏 *smirks*");
        }
    }
    
    /// <summary>
    /// Facepalm - Teen is frustrated with situation
    /// Perfect for: "I can't believe this", exasperation
    /// </summary>
    public void PlayFacepalm()
    {
        if (animator != null)
        {
            animator.SetTrigger(FacepalHash);
            Debug.Log("🤦 *facepalm*");
        }
    }
    
    /// <summary>
    /// Thinking - Teen is considering/pondering
    /// Perfect for: When processing player's argument
    /// </summary>
    public void PlayThinking()
    {
        if (animator != null)
        {
            animator.SetTrigger(ThinkingHash);
            Debug.Log("🤔 *thinking*");
        }
    }
    
    /// <summary>
    /// Nod - Agreement, understanding
    /// </summary>
    public void PlayNod()
    {
        if (animator != null)
        {
            animator.SetTrigger(NodHash);
            Debug.Log("🙂 *nods*");
        }
    }
    
    /// <summary>
    /// Shake head - Disagreement, refusal
    /// </summary>
    public void PlayShakeHead()
    {
        if (animator != null)
        {
            animator.SetTrigger(ShakeHeadHash);
            Debug.Log("🙁 *shakes head*");
        }
    }
    
    /// <summary>
    /// Look away - Avoiding eye contact, guilt, defiance
    /// Perfect for: Lying, embarrassment, stubbornness
    /// </summary>
    public void PlayLookAway()
    {
        if (animator != null)
        {
            animator.SetTrigger(LookAwayHash);
            Debug.Log("😑 *looks away*");
        }
    }
    
    /// <summary>
    /// Scoff - Dismissive sound, contempt
    /// Perfect for: "Yeah right", disbelief
    /// </summary>
    public void PlayScoff()
    {
        if (animator != null)
        {
            animator.SetTrigger(ScoffHash);
            Debug.Log("😤 *scoffs*");
        }
    }
    
    /// <summary>
    /// Pout - Sulking, sadness, disappointment
    /// Perfect for: Not getting their way, feeling hurt
    /// </summary>
    public void PlayPout()
    {
        if (animator != null)
        {
            animator.SetTrigger(PoutHash);
            Debug.Log("😔 *pouts*");
        }
    }
    
    /// <summary>
    /// Play contextual micro-reaction based on emotion and action
    /// This intelligently chooses the best reaction
    /// </summary>
    public void PlayContextualReaction(EmotionalState.Emotion emotion, TeenAgent.TeenResponse response)
    {
        if (animator == null) return;
        
        // Map emotion + response to appropriate micro-reaction
        switch (response)
        {
            case TeenAgent.TeenResponse.Sarcastic:
                PlayEyeRoll();
                PlaySmirk();
                break;
            
            case TeenAgent.TeenResponse.Dismissive:
                PlayEyeRoll();
                PlayLookAway();
                break;
            
            case TeenAgent.TeenResponse.Angry:
                PlayShocked(); // Initial shock
                PlayCrossArms(); // Then anger
                break;
            
            case TeenAgent.TeenResponse.Defiant:
                PlayShakeHead();
                PlayCrossArms();
                break;
            
            case TeenAgent.TeenResponse.EmotionalPlead:
                if (emotion == EmotionalState.Emotion.Sad)
                    PlayPout();
                else if (emotion == EmotionalState.Emotion.Anxious)
                    PlayLookAway();
                break;
            
            case TeenAgent.TeenResponse.NegotiateCalm:
                PlayThinking();
                PlaySmirk();
                break;
            
            case TeenAgent.TeenResponse.Compliant:
                if (emotion == EmotionalState.Emotion.Happy)
                    PlayNod();
                else
                    PlaySigh(); // Reluctant compliance
                break;
            
            case TeenAgent.TeenResponse.ReasonableRefusal:
                PlayThinking();
                PlayShakeHead();
                break;
        }
    }
    
    /// <summary>
    /// Play reaction based on player action intensity
    /// </summary>
    public void PlayReactionToPlayerAction(PlayerActionType action, float relationshipLevel)
    {
        if (animator == null) return;
        
        switch (action)
        {
            case PlayerActionType.Authoritarian:
                if (relationshipLevel > 60f)
                    PlayShocked(); // Shocked if usually treated well
                else
                    PlayEyeRoll(); // Used to it
                break;
            
            case PlayerActionType.GuiltTrip:
                PlayLookAway();
                PlayDefiant();
                break;
            
            case PlayerActionType.Bribery:
                PlaySmirk();
                PlayThinking();
                break;
            
            case PlayerActionType.Compromise:
                if (relationshipLevel > 50f)
                    PlayGiggle();
                else
                    PlayEyeRoll(); // Skeptical
                break;
            
            case PlayerActionType.Empathetic:
                PlayLookAway(); // Vulnerability
                break;
            
            case PlayerActionType.Listen:
                PlayThinking();
                PlayNod();
                break;
        }
    }
    
    #endregion
    
    #region Helper: Defiant Animation
    
    private void PlayDefiant()
    {
        if (animator != null)
        {
            animator.SetBool(IsDefiantHash, true);
        }
    }
    
    #endregion
}

