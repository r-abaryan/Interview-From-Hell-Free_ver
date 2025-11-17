# Teen Persuasion Simulator - ML-Agents Project

An innovative ML-Agents game where you interact with a realistic AI-powered teenager. The teen learns to respond authentically to different parenting approaches through reinforcement learning.

## 🎮 Concept

You play as a parent/guardian trying to convince a teenager to cooperate with various tasks (going to school, doing homework, cleaning their room, etc.). The teen is an ML-Agent that learns realistic emotional responses based on:

- **Relationship quality** with the player
- **Current emotional state** (mood, stress, trust)
- **How you treat them** (respectful vs authoritarian)
- **Context** (time of day, tiredness, hunger)

The goal is to train an AI that responds realistically - not just compliant, but with the full range of teenage emotional complexity!

## 🎯 Features

### Core Systems
- **EmotionalState System**: Tracks relationship, mood, trust, stress, autonomy needs, and respect
- **8 Teen Response Types**: Compliant, Calm Negotiation, Sarcastic, Angry, Dismissive, Emotional Plead, Defiant, Reasonable Refusal
- **7 Player Action Types**: Authoritarian, Empathetic, Logical, Bribery, Guilt Trip, Listen, Compromise
- **6 Scenario Types**: School refusal, homework avoidance, room cleaning, screen time limits, bedtime, family time

### AI Learning
- Teen learns which responses are realistic for different emotional states
- Rewards for consistency between emotion and response
- Learns that good relationships lead to better cooperation
- Develops context-aware behavior patterns

### Gameplay Modes
- **Training Mode**: ML-Agents trains the teen AI
- **Play Mode**: You (human) interact with the trained AI teen
- **Demo Mode**: Watch the trained AI play

## 📋 Requirements

### Software Requirements
- **Unity 2022.3 LTS or newer**
- **ML-Agents Package 2.0.2** (already imported)
- **Python 3.9 - 3.11**
- **PyTorch 1.7.1+**
- **ML-Agents Python Package**

### Hardware Recommendations
- **CPU**: Modern multi-core processor
- **RAM**: 8GB minimum, 16GB recommended
- **GPU**: Optional but speeds up training significantly
- **Storage**: 2GB free space for models and logs

## 🚀 Setup Instructions

### 1. Verify Python Environment

Open your terminal/command prompt and verify your conda environment:

```bash
# Check Python version (should be 3.9-3.11)
python --version

# Check if PyTorch is installed
python -c "import torch; print(torch.__version__)"
```

### 2. Install ML-Agents Python Package

```bash
# Activate your conda environment (if using one)
conda activate your_env_name

# Install ML-Agents
pip install mlagents==0.30.0

# Verify installation
mlagents-learn --help
```

### 3. Unity Scene Setup

1. **Open Unity** and load this project
2. **Open the Sample Scene**: `Assets/Scenes/SampleScene.unity`

3. **Create the Teen Agent GameObject**:
   - Create an empty GameObject named "TeenAgent"
   - Add the `TeenAgent` script component
   - Add a `Behavior Parameters` component:
     - Behavior Name: `TeenAgent`
     - Vector Observation Space Size: `25`
     - Actions: Discrete Branch 0 = `8` (8 response types)
   - Add a `Decision Requester` component:
     - Decision Period: `5`

4. **Create the Managers GameObject**:
   - Create an empty GameObject named "Managers"
   - Add these components:
     - `ConversationManager`
     - `ScenarioManager`
     - `PlayerController`
     - `GameManager`
     - `DialogueDatabase`

5. **Link References**:
   - In `GameManager`, assign all the manager references
   - In `TeenAgent`, assign the `ConversationManager`
   - In `ConversationManager`, assign `TeenAgent`, `PlayerController`, and `DialogueUI`

6. **Create UI (Optional but recommended)**:
   - Create a Canvas with `DialogueUI` script
   - Add UI elements for dialogue display and player buttons
   - Or use the basic debug UI provided by `GameManager`

### 4. Test the Setup

1. **Press Play** in Unity
2. You should see:
   - Debug overlay showing teen's emotional state
   - Console logs about episode starting
   - Teen's opening dialogue (if UI is set up)

3. **Test keyboard controls**:
   - Press `1-7` to trigger different player actions
   - Press `R` to restart episode
   - Press `T` to toggle game modes

## 🏋️ Training the Agent

### Training Workflow

#### Step 1: Start Training

Open terminal in your project root folder:

```bash
mlagents-learn Assets/TrainingConfigs/TeenAgent.yaml --run-id=teen-training-01

```

#### Step 2: Start Unity

When you see `"Start training by pressing the Play button in the Unity Editor"`, press **Play** in Unity.

#### Step 3: Monitor Training

The terminal will show:
- **Mean Reward**: Average reward per episode (should increase)
- **Episode Length**: How many interactions per conversation
- **Policy Loss**: Neural network learning progress

**TensorBoard** (optional visualization):
```bash
# In a new terminal
tensorboard --logdir results
# Open browser to http://localhost:6006
```

### Training Tips

#### Expected Timeline
- **1M steps** (~2-4 hours): Basic responses learned
- **3M steps** (~6-12 hours): Consistent emotional responses
- **5M steps** (~12-24 hours): Sophisticated context-aware behavior

#### What to Look For
- **Mean Reward** trending upward
- **Episode Length** stabilizing around 3-5 interactions
- Successful compliance in ~40-60% of episodes (realistic!)

#### If Training Seems Stuck
- Check that Mean Reward is changing
- Ensure Unity is running and episodes are completing
- Try reducing `learning_rate` to `0.0001`
- Increase `curiosity` strength to encourage exploration

### Training Configurations

#### Basic Training (Start Here)
```bash
mlagents-learn Assets/TrainingConfigs/TeenAgent.yaml --run-id=teen-basic
```
- 5M steps
- 256 hidden units
- Good for initial training

#### Advanced Training (After Basic Works)
```bash
mlagents-learn Assets/TrainingConfigs/TeenAgent_Advanced.yaml --run-id=teen-advanced
```
- 10M steps
- 512 hidden units with 4 layers
- Includes curiosity and self-play support
- Better quality but slower training

### Resume Training
```bash
mlagents-learn Assets/TrainingConfigs/TeenAgent.yaml --run-id=teen-training-01 --resume
```

## 🎮 Playing with the Trained Agent

### Load Trained Model

1. After training, find your model: `results/teen-training-01/TeenAgent.onnx`
2. Copy it to: `Assets/TrainedModels/` (create folder if needed)
3. In Unity, select the TeenAgent GameObject
4. In `Behavior Parameters`:
   - Model: Drag your `.onnx` file here
   - Inference Device: CPU or GPU

### Play Mode

1. In `GameManager`, set `currentMode = Play`
2. Press Play in Unity
3. Interact with the teen using the UI buttons or keyboard (1-7)
4. Observe how the teen responds based on your approach!

### Testing Different Approaches

Try experimenting with:
- **Authoritarian approach**: See how quickly relationship degrades
- **Empathetic approach**: Build trust and see cooperation improve
- **Mixed strategies**: Realistic parenting!
- **Extreme scenarios**: Very negative initial mood - can you turn it around?

## 📊 Understanding the Reward System

### What Makes a Good Teen Response?

The agent is rewarded for:
1. **Emotional Realism**: Angry responses when mood is bad, compliant when relationship is good
2. **Consistency**: Response matches current emotional state
3. **Relationship Maintenance**: Not alienating the player completely
4. **Situational Appropriateness**: Context-aware reactions

### Not Just About Compliance!

Important: The teen is NOT trained to always obey. It's trained to respond **realistically**:
- High relationship + respectful treatment = more likely to comply
- Low relationship + authoritarian treatment = defiance/anger
- This mimics real teenage psychology!

## 🎨 Customization Ideas

### Adjust Personality
In `TeenAgent.OnEpisodeBegin()`, modify the emotional state ranges:
```csharp
emotionalState.autonomyNeed = Random.Range(40f, 70f);  // Less rebellious teen
emotionalState.relationshipLevel = Random.Range(0f, 60f);  // Start with better relationship
```

### Add New Scenarios
1. Add to `ScenarioType` enum in `TeenAgent.cs`
2. Add dialogue in `DialogueDatabase.cs`
3. Add tips in `ScenarioManager.cs`

### Modify Rewards
In `TeenAgent.ApplyTeenResponse()`, adjust reward values to encourage different behaviors:
```csharp
complianceReward = 2.0f;  // Increase if you want more compliance
```

### Add New Response Types
1. Add to `TeenAgent.TeenResponse` enum
2. Update `TeenAgent.OnActionReceived()` discrete action space
3. Add dialogue in `DialogueDatabase.cs`
4. Update neural network output size in Behavior Parameters

## 🐛 Troubleshooting

### "Python is not installed"
- Ensure Python 3.9-3.11 is installed
- Add Python to your PATH environment variable

### "No module named 'mlagents'"
```bash
pip install mlagents==0.30.0
```

### Unity ML-Agents not responding
- Check console for errors
- Verify Behavior Parameters settings
- Ensure Decision Requester is attached

### Training not starting
- Make sure to press Play in Unity AFTER `mlagents-learn` command
- Check that behavior name matches in both Unity and YAML file

### Agent behaving randomly after training
- Training may need more steps
- Check that correct .onnx model is loaded
- Verify Inference Device is set correctly

### OutOfMemoryError during training
- Reduce `batch_size` in YAML config
- Reduce `buffer_size` in YAML config
- Close other applications

## 📚 Learning Resources

### ML-Agents Documentation
- [Official ML-Agents Docs](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Readme.md)
- [Training Configuration](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md)
- [PPO Algorithm](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-PPO.md)

### Understanding Reinforcement Learning
- Reward shaping is critical for realistic behavior
- This project uses **sparse rewards** (mostly at episode end)
- **Curiosity** helps explore the space of emotional responses

## 🎯 Project Goals & Philosophy

This project demonstrates:
1. **AI for Social Simulation**: Using RL for human-like emotional behavior
2. **Ethical AI Training**: Rewarding realistic responses, not just obedience
3. **Educational Value**: Understanding teenage psychology and communication
4. **Game Design**: Making parenting challenges into engaging gameplay

### Why This Matters
Traditional games have scripted NPC responses. This teen *learns* to respond contextually, creating unique, emergent interactions every time. It's a step toward truly dynamic, believable AI characters in games.

## 🤝 Contributing & Extending

Ideas for extension:
- **Multiple Teen Personalities**: Train different models for different personality types
- **Multiplayer**: Multiple players try to convince the same teen
- **Parenting Metrics**: Score players on long-term relationship building
- **Narrative Integration**: Use in a story-driven game
- **Educational Tool**: Help parents practice communication strategies

## 📄 License

This project is for educational and research purposes. Feel free to extend and modify!

---

## 🎉 Next Steps

1. ✅ **Setup Complete** - You have all the components
2. ⏭️ **Test in Unity** - Press Play and verify everything works
3. 🏋️ **Start Training** - Run `mlagents-learn` command
4. 🎮 **Play with AI** - Test your trained teen agent
5. 🔧 **Customize** - Make it your own!

---

**Good luck, and remember**: The goal isn't to create a perfectly obedient teen, but a *realistically* responsive one! 🧠✨

