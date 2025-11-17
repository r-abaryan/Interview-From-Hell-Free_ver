using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Database of dialogue lines for different scenarios, emotions, and responses
/// </summary>
public class DialogueDatabase : MonoBehaviour
{
    // Store dialogue variations for each scenario and response type
    private Dictionary<ScenarioType, string[]> scenarioOpenings;
    private Dictionary<PlayerActionType, Dictionary<ScenarioType, string[]>> playerDialogues;
    private Dictionary<TeenAgent.TeenResponse, Dictionary<ScenarioType, string[]>> teenResponseDialogues;
    
    public void Initialize()
    {
        InitializeScenarioOpenings();
        InitializePlayerDialogues();
        InitializeTeenResponses();
    }
    
    /// <summary>
    /// Get opening line based on scenario and teen's current emotional state
    /// </summary>
    public string GetTeenOpeningLine(ScenarioType scenario, EmotionalState emotionalState)
    {
        if (scenarioOpenings.ContainsKey(scenario))
        {
            string[] options = scenarioOpenings[scenario];
            
            // Adjust based on mood
            if (emotionalState.currentMood < -40f)
            {
                return options[0];  // Most negative variant
            }
            else if (emotionalState.currentMood > 40f)
            {
                return options[options.Length - 1];  // Most positive variant
            }
            else
            {
                return options[options.Length / 2];  // Neutral variant
            }
        }
        
        return "...";
    }
    
    /// <summary>
    /// Get player dialogue for selected action
    /// </summary>
    public string GetPlayerDialogue(PlayerActionType action, ScenarioType scenario)
    {
        if (playerDialogues.ContainsKey(action) && playerDialogues[action].ContainsKey(scenario))
        {
            string[] options = playerDialogues[action][scenario];
            return options[Random.Range(0, options.Length)];
        }
        
        return "Let's talk about this.";
    }
    
    /// <summary>
    /// Get teen response dialogue
    /// </summary>
    public string GetTeenResponseDialogue(TeenAgent.TeenResponse response, ScenarioType scenario, EmotionalState.Emotion emotion)
    {
        if (teenResponseDialogues.ContainsKey(response) && teenResponseDialogues[response].ContainsKey(scenario))
        {
            string[] options = teenResponseDialogues[response][scenario];
            return options[Random.Range(0, options.Length)];
        }
        
        return GetGenericResponse(response);
    }
    
    private string GetGenericResponse(TeenAgent.TeenResponse response)
    {
        switch (response)
        {
            case TeenAgent.TeenResponse.Compliant:
                return "Okay, fine. I'll do it.";
            case TeenAgent.TeenResponse.Angry:
                return "I can't believe this! Leave me alone!";
            case TeenAgent.TeenResponse.Defiant:
                return "No. You can't make me.";
            default:
                return "Whatever...";
        }
    }
    
    #region Initialization Methods
    
    private void InitializeScenarioOpenings()
    {
        scenarioOpenings = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] 
                {
                    "I'm not going to school today. I hate it there.",
                    "Do I really have to go to school? I don't feel like it.",
                    "School? Already? Ugh, okay I guess..."
                }
            },
            { ScenarioType.DoHomework, new string[] 
                {
                    "I'm NOT doing homework right now. It's stupid.",
                    "Can I do my homework later? I'm tired.",
                    "Yeah, I'll get to my homework soon."
                }
            },
            { ScenarioType.CleanRoom, new string[] 
                {
                    "My room is MY space. I'll clean it when I want to!",
                    "It's not even that messy. Why do you care so much?",
                    "I know, I know. I'll clean it."
                }
            },
            { ScenarioType.LimitScreenTime, new string[] 
                {
                    "You can't just take away my phone! That's not fair!",
                    "Come on, just a little longer? Please?",
                    "Alright, I guess I've been on it a while."
                }
            },
            { ScenarioType.Bedtime, new string[] 
                {
                    "I'm not a kid anymore! I decide when I sleep!",
                    "But I'm not even tired yet...",
                    "Yeah, I'm actually pretty tired."
                }
            },
            { ScenarioType.ComeToFamily, new string[] 
                {
                    "I don't want to hang out with the family. It's boring.",
                    "Do I have to? I wanted to stay in my room.",
                    "Sure, I guess I can come down."
                }
            }
        };
    }
    
    private void InitializePlayerDialogues()
    {
        playerDialogues = new Dictionary<PlayerActionType, Dictionary<ScenarioType, string[]>>();
        
        // Authoritarian responses
        playerDialogues[PlayerActionType.Authoritarian] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "You're going to school, end of discussion!", "I don't care how you feel, you WILL go to school." } },
            { ScenarioType.DoHomework, new string[] { "Do your homework RIGHT NOW!", "No excuses. Homework. Now." } },
            { ScenarioType.CleanRoom, new string[] { "Clean your room immediately!", "I want that room spotless in 30 minutes!" } },
            { ScenarioType.LimitScreenTime, new string[] { "Give me your phone. Now.", "Screen time is over. Hand it over." } },
            { ScenarioType.Bedtime, new string[] { "It's bedtime. Go to your room now.", "I said bedtime. No arguments." } },
            { ScenarioType.ComeToFamily, new string[] { "Get out here with the family. Now.", "You're part of this family. Come out here." } }
        };
        
        // Empathetic responses
        playerDialogues[PlayerActionType.Empathetic] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "I know school can be tough. Want to talk about what's bothering you?", "I hear you. What's making school hard right now?" } },
            { ScenarioType.DoHomework, new string[] { "I understand you're tired. Is the homework overwhelming?", "I get it, homework isn't fun. What's making it tough today?" } },
            { ScenarioType.CleanRoom, new string[] { "I know it feels like I'm nagging. I just want you to have a nice space.", "I understand your room is your personal space. Can we talk about it?" } },
            { ScenarioType.LimitScreenTime, new string[] { "I know your phone is important to you. But I'm worried about screen time.", "I get that you want to stay connected. Let's talk about balance." } },
            { ScenarioType.Bedtime, new string[] { "I know you feel grown up. But sleep is really important for you.", "I understand you're not tired yet. What's on your mind?" } },
            { ScenarioType.ComeToFamily, new string[] { "I understand you want alone time. But we'd love to spend time with you.", "I know family time might not seem fun, but we miss you." } }
        };
        
        // Logical responses
        playerDialogues[PlayerActionType.Logical] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Education is important for your future. Missing school hurts your grades.", "School attendance affects your academic record." } },
            { ScenarioType.DoHomework, new string[] { "Homework reinforces what you learned. It helps you remember.", "Your grades depend on completing homework consistently." } },
            { ScenarioType.CleanRoom, new string[] { "A clean space helps you focus and find things easier.", "Clutter can affect your mental health and productivity." } },
            { ScenarioType.LimitScreenTime, new string[] { "Too much screen time affects your sleep and health.", "Studies show excessive phone use impacts concentration." } },
            { ScenarioType.Bedtime, new string[] { "Teenagers need 8-10 hours of sleep for development.", "Lack of sleep affects your mood, health, and school performance." } },
            { ScenarioType.ComeToFamily, new string[] { "Family connections are important. Strong relationships matter.", "Spending time together strengthens family bonds." } }
        };
        
        // Bribery responses
        playerDialogues[PlayerActionType.Bribery] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "If you go to school, I'll get you that game you wanted.", "Go to school today and we'll go out for your favorite food." } },
            { ScenarioType.DoHomework, new string[] { "Finish your homework and you can have extra screen time.", "Do your homework now and I'll give you $20." } },
            { ScenarioType.CleanRoom, new string[] { "Clean your room and I'll increase your allowance.", "If you clean up, I'll buy you something nice." } },
            { ScenarioType.LimitScreenTime, new string[] { "Put the phone away and I'll take you shopping tomorrow.", "Give me an hour of family time and you can have it back." } },
            { ScenarioType.Bedtime, new string[] { "Go to bed now and you can sleep in Saturday.", "Sleep early tonight and I'll make your favorite breakfast." } },
            { ScenarioType.ComeToFamily, new string[] { "Come hang with us and I'll give you extra allowance.", "Join us for dinner and I'll let you choose the movie." } }
        };
        
        // Guilt Trip responses
        playerDialogues[PlayerActionType.GuiltTrip] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "After all I do for you, you can't even go to school?", "I work so hard to give you opportunities, and this is how you repay me?" } },
            { ScenarioType.DoHomework, new string[] { "I sacrifice so much for your education and you won't do homework?", "Do you know how disappointed I am in you right now?" } },
            { ScenarioType.CleanRoom, new string[] { "I do everything around here and you can't even clean your room?", "You're so ungrateful. I keep this house nice for you." } },
            { ScenarioType.LimitScreenTime, new string[] { "I pay for that phone and you can't respect simple rules?", "You're addicted. You're breaking my heart with this behavior." } },
            { ScenarioType.Bedtime, new string[] { "You're going to make yourself sick and I'll have to take care of you.", "Why do you fight me on everything? I'm trying to help you." } },
            { ScenarioType.ComeToFamily, new string[] { "You never want to spend time with us anymore. Are you ashamed of your family?", "We used to be close. What happened to you?" } }
        };
        
        // Listen responses
        playerDialogues[PlayerActionType.Listen] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Tell me what's really going on. Why don't you want to go?", "I'm here to listen. What's happening at school?" } },
            { ScenarioType.DoHomework, new string[] { "Talk to me. What's making homework difficult right now?", "I want to understand. What's going on with your assignments?" } },
            { ScenarioType.CleanRoom, new string[] { "Help me understand your perspective on this.", "I'm listening. Why is cleaning your room such an issue?" } },
            { ScenarioType.LimitScreenTime, new string[] { "I want to hear your side. Why is this so important to you?", "Tell me what you're doing that's so important on your phone." } },
            { ScenarioType.Bedtime, new string[] { "What's keeping you up? Let's talk about it.", "I'm listening. Why don't you want to sleep?" } },
            { ScenarioType.ComeToFamily, new string[] { "Talk to me. Why don't you want to join us?", "I want to understand. What would make family time better for you?" } }
        };
        
        // Compromise responses
        playerDialogues[PlayerActionType.Compromise] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "How about you go to school, and we'll talk tonight about what's bothering you?", "What if you go today, and if it's still bad tomorrow, we'll figure something out?" } },
            { ScenarioType.DoHomework, new string[] { "What if you do half now and half after dinner?", "How about you take a 30-minute break, then tackle the homework?" } },
            { ScenarioType.CleanRoom, new string[] { "What if we clean it together? I'll help you.", "How about you just pick up the floor today, and organize tomorrow?" } },
            { ScenarioType.LimitScreenTime, new string[] { "Let's set a time together. When do you think is reasonable?", "What if we agree on 30 more minutes, then phone away?" } },
            { ScenarioType.Bedtime, new string[] { "How about lights off in 30 minutes? Does that work?", "What if you read in bed for a bit, then sleep?" } },
            { ScenarioType.ComeToFamily, new string[] { "Just come for dinner, then you can go back to your room?", "What if you join us for one hour? That's all I ask." } }
        };
    }
    
    private void InitializeTeenResponses()
    {
        teenResponseDialogues = new Dictionary<TeenAgent.TeenResponse, Dictionary<ScenarioType, string[]>>();
        
        // Compliant responses
        teenResponseDialogues[TeenAgent.TeenResponse.Compliant] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Okay, I'll go to school.", "Fine, you're right. I'll get ready." } },
            { ScenarioType.DoHomework, new string[] { "Alright, I'll do my homework.", "Okay, let me get started on it." } },
            { ScenarioType.CleanRoom, new string[] { "Fine, I'll clean my room.", "Okay, I'll clean it up now." } },
            { ScenarioType.LimitScreenTime, new string[] { "Okay, here's my phone.", "You're right. I'll put it away." } },
            { ScenarioType.Bedtime, new string[] { "Alright, I'll go to bed.", "Okay, goodnight." } },
            { ScenarioType.ComeToFamily, new string[] { "Okay, I'll come out.", "Fine, I'll join you guys." } }
        };
        
        // Negotiate Calm responses
        teenResponseDialogues[TeenAgent.TeenResponse.NegotiateCalm] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Can we talk about this? There's something going on...", "What if I stay home today and make it up?" } },
            { ScenarioType.DoHomework, new string[] { "Can I please do it after dinner? I need a break.", "What if I do the important parts first?" } },
            { ScenarioType.CleanRoom, new string[] { "Can I do it this weekend instead?", "What if I just organize the important stuff?" } },
            { ScenarioType.LimitScreenTime, new string[] { "Can I have 20 more minutes? I'm in the middle of something.", "What if I set a timer myself?" } },
            { ScenarioType.Bedtime, new string[] { "Can I at least finish this episode?", "What if I go to bed 30 minutes later on weekends?" } },
            { ScenarioType.ComeToFamily, new string[] { "Can I come out in a few minutes?", "What if I join for part of it?" } }
        };
        
        // Sarcastic responses
        teenResponseDialogues[TeenAgent.TeenResponse.Sarcastic] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Oh sure, school, the best place on Earth...", "Yeah, can't wait to go to that amazing place..." } },
            { ScenarioType.DoHomework, new string[] { "Oh wow, homework, my favorite thing ever.", "Sure, because homework is SO important right this second." } },
            { ScenarioType.CleanRoom, new string[] { "Oh no, the room police are here!", "Right, because a messy room is the end of the world." } },
            { ScenarioType.LimitScreenTime, new string[] { "Sure, take away the only thing I enjoy.", "Oh great, the phone police strike again." } },
            { ScenarioType.Bedtime, new string[] { "Yes, master, whatever you say...", "Right, because I'm five years old." } },
            { ScenarioType.ComeToFamily, new string[] { "Oh boy, family fun time...", "Yeah, that sounds absolutely thrilling." } }
        };
        
        // Angry responses
        teenResponseDialogues[TeenAgent.TeenResponse.Angry] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "I SAID I'm not going! Leave me alone!", "You don't understand anything! I hate school!" } },
            { ScenarioType.DoHomework, new string[] { "Stop nagging me! I'll do it when I want to!", "Get off my back! I'm sick of this!" } },
            { ScenarioType.CleanRoom, new string[] { "It's MY room! Get out!", "Why do you always have to control everything?!" } },
            { ScenarioType.LimitScreenTime, new string[] { "This is ridiculous! You're so unfair!", "I can't believe this! You're the worst!" } },
            { ScenarioType.Bedtime, new string[] { "Stop treating me like a child!", "I'm not tired! Leave me alone!" } },
            { ScenarioType.ComeToFamily, new string[] { "I don't want to! Stop forcing me!", "Why can't you just leave me alone?!" } }
        };
        
        // Dismissive responses
        teenResponseDialogues[TeenAgent.TeenResponse.Dismissive] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Yeah, yeah, I heard you.", "Mmm hmm, sure..." } },
            { ScenarioType.DoHomework, new string[] { "Whatever, I'll do it later.", "Yeah, okay, sure..." } },
            { ScenarioType.CleanRoom, new string[] { "Uh huh, later.", "Yeah, I know..." } },
            { ScenarioType.LimitScreenTime, new string[] { "In a minute...", "Sure, whatever..." } },
            { ScenarioType.Bedtime, new string[] { "Yeah, soon...", "Okay, okay..." } },
            { ScenarioType.ComeToFamily, new string[] { "Maybe later...", "Yeah, in a bit..." } }
        };
        
        // Emotional Plead responses
        teenResponseDialogues[TeenAgent.TeenResponse.EmotionalPlead] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "Please don't make me go... I'm really struggling there...", "I'm begging you, I can't face school today..." } },
            { ScenarioType.DoHomework, new string[] { "I'm so overwhelmed... I can't handle it right now...", "Please, I'm so stressed about everything..." } },
            { ScenarioType.CleanRoom, new string[] { "I'm too tired... Please...", "I just... I can't deal with this right now..." } },
            { ScenarioType.LimitScreenTime, new string[] { "Please, this is the only thing that helps me relax...", "Don't take this away from me... Please..." } },
            { ScenarioType.Bedtime, new string[] { "I can't sleep anyway... My mind won't stop...", "Please, I'm anxious... I need to stay up..." } },
            { ScenarioType.ComeToFamily, new string[] { "I really need to be alone right now... Please understand...", "I just can't... I need space..." } }
        };
        
        // Defiant responses
        teenResponseDialogues[TeenAgent.TeenResponse.Defiant] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "No. I'm not going and you can't force me.", "Make me. Go ahead, try." } },
            { ScenarioType.DoHomework, new string[] { "I'm not doing it. Deal with it.", "You can't make me do homework." } },
            { ScenarioType.CleanRoom, new string[] { "I'm not cleaning it. It's my space.", "No. You have no right to tell me what to do in my room." } },
            { ScenarioType.LimitScreenTime, new string[] { "No. It's my phone, I'll use it how I want.", "You can't control me." } },
            { ScenarioType.Bedtime, new string[] { "I'll sleep when I'm ready. You can't force me.", "I'm not a kid. I don't have a bedtime." } },
            { ScenarioType.ComeToFamily, new string[] { "I'm not coming out. You can't make me.", "No. I don't want to and I won't." } }
        };
        
        // Reasonable Refusal responses
        teenResponseDialogues[TeenAgent.TeenResponse.ReasonableRefusal] = new Dictionary<ScenarioType, string[]>
        {
            { ScenarioType.GoToSchool, new string[] { "I understand, but I have a legitimate reason. Can we discuss it?", "I hear you, but something serious is going on. Let's talk." } },
            { ScenarioType.DoHomework, new string[] { "I get it, but I have a plan. I'll finish it by the deadline.", "I understand it's important, but I need to prioritize this other assignment first." } },
            { ScenarioType.CleanRoom, new string[] { "I understand your concern, but I have a system. It's organized to me.", "Fair point, but can I clean it this weekend when I have more time?" } },
            { ScenarioType.LimitScreenTime, new string[] { "I understand your concern. Can we set a specific time limit together?", "I hear you. What if I track my own time and we review it?" } },
            { ScenarioType.Bedtime, new string[] { "I understand sleep is important, but I'm legitimately not tired yet. Can we compromise?", "I get it, but my sleep schedule is different. Can we work something out?" } },
            { ScenarioType.ComeToFamily, new string[] { "I understand you want family time. Can I join after I finish this important thing?", "I hear you. What if we schedule regular family time I can plan for?" } }
        };
    }
    
    #endregion
}

