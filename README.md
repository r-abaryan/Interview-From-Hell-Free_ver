# Job Interview From Hell Free version🎤💼

A voice-powered interview game where you must survive 5 absurd questions from an AI interviewer. Answer using your **real voice** (or text), and the AI evaluates your emotion, tone, confidence, and meaning. The interviewer gets angry, misunderstands you, changes topics, and tests your patience.

**Goal**: Survive 5 questions without getting rejected!

## 🎮 Features

- **Voice Input**: Speak your answers (Whisper STT) or type them
- **AI Interviewer**: Powered by Ollama (Phi-3) with unpredictable personality
- **Voice Analysis**: Pitch, volume, pauses, confidence, nervousness detection
- **Sentiment Analysis**: Emotion, assertiveness, humor detection
- **Dynamic Questions**: Absurd, unpredictable questions with validation rules
- **Mood System**: Interviewer has 6 moods (Professional, Confused, Annoyed, Aggressive, Amused, Unhinged)
- **Text-to-Speech**: Interviewer speaks responses (optional)

## 📋 Requirements

### Software
- **Unity 2022.3 LTS or newer**
- **Ollama** (for LLM) - [Install](https://ollama.ai)
- **Whisper Server** (optional, for voice transcription) - Default: `http://localhost:9000/inference`

### Hardware
- **Microphone** (for voice input)
- **8GB RAM** minimum
- **GPU** (optional, for faster LLM inference)

## 🚀 Quick Setup

### 1. Install Ollama

```bash
# Download from https://ollama.ai or:
# Windows: Download installer
# Mac/Linux: curl -fsSL https://ollama.ai/install.sh | sh
```

### 2. Pull Phi-3 Model

```bash
ollama pull phi3
```

### 3. Start Ollama (if not running)

```bash
ollama serve
# Should be available at http://localhost:11434
```

### 4. Unity Setup

1. **Open Unity** and load the project
2. **Open the Interview Scene**
3. **Find InterviewManager GameObject** (or create one)
4. **Assign Components**:
   - `InterviewerAI` script
   - `LLMManager` script
   - `WhisperSTT` script (optional)
   - `VoiceAnalyzer` script
   - `SentimentAnalyzer` script
   - `QuestionManager` script
   - `VoiceSystem` script (optional, for TTS)

5. **Link References** in `InterviewerAI`:
   - Assign all component references
   - Set `maxStrikes = 3`

6. **Setup UI** (or use `InterviewUIBuilder`):
   - Canvas with start screen, interview panel, result panel
   - Assign UI elements to `InterviewUI` script

### 5. Test

1. Press **Play** in Unity
2. Click **Start** button
3. Answer questions using **MIC button** or **text input**
4. Survive 5 questions!

## 🎯 How It Works

### Question Types

1. **Forbidden Words**: Can't use specific words
2. **Must Be Emotional**: Requires passionate/emotional response
3. **Must Be Confident**: No hesitation, nervousness
4. **Must Be Creative**: Minimum word count, variety
5. **Voice Acting**: Must sound different from normal speech

### Validation

Each answer is evaluated on:
- **Voice Metrics**: Pitch, volume, pauses, speech rate, confidence
- **Sentiment**: Emotion, assertiveness, nervousness, humor
- **Content**: Word count, forbidden words, creativity

### Interviewer Moods

- **Professional**: Normal questions
- **Confused**: Misunderstands answers
- **Annoyed**: Gets frustrated easily
- **Aggressive**: Angry, confrontational
- **Amused**: Finds things funny
- **Unhinged**: Random chaos mode

## ⚙️ Configuration

### LLM Settings (`LLMManager`)

```csharp
ollamaEndpoint = "http://localhost:11434/api/generate"
modelName = "phi3"
maxTokens = 100
temperature = 0.9f
```

### Whisper STT (`WhisperSTT`)

```csharp
whisperEndpoint = "http://localhost:9000/inference"
useLocalWhisper = true  // Set false to use text input only
```

### Interview Settings (`InterviewerAI`)

```csharp
maxStrikes = 3  // Fail after 3 strikes
```

## 🎤 Voice Setup (Optional)

### Option 1: Use Whisper Server

1. Run a Whisper server on port 9000
2. Set `useLocalWhisper = true` in `WhisperSTT`
3. Voice input will be transcribed

### Option 2: Text Input Only

1. Set `useLocalWhisper = false` in `WhisperSTT`
2. Use the text input field to type answers
3. Voice analysis will use default metrics

## 🐛 Troubleshooting

### "Cannot connect to Ollama"
- Make sure Ollama is running: `ollama serve`
- Check `ollamaEndpoint` in `LLMManager` (default: `http://localhost:11434`)
- Test: `curl http://localhost:11434/api/tags`

### "Cannot transcribe"
- Whisper server not running or wrong endpoint
- Use text input instead (set `useLocalWhisper = false`)
- Check `whisperEndpoint` in `WhisperSTT`

### Interviewer not responding
- Check Unity Console for errors
- Verify `LLMManager` is assigned to `InterviewerAI`
- Test Ollama connection: `ollama list`

### Buttons not working
- Ensure `EventSystem` exists in scene
- Check `GraphicRaycaster` on Canvas
- Verify `InputSystemUIInputModule` is used (if using new Input System)

## 📚 Project Structure

```
Assets/Scripts/Interview/
├── InterviewerAI.cs          # Main game controller
├── LLMManager.cs             # Ollama integration
├── WhisperSTT.cs              # Speech-to-text
├── VoiceAnalyzer.cs           # Voice metrics analysis
├── SentimentAnalyzer.cs       # Text sentiment analysis
├── QuestionManager.cs         # Question database
├── InterviewUI.cs             # UI controller
└── InterviewUIBuilder.cs      # Auto UI builder (optional)
```

## 🎨 Customization

### Add New Questions

Edit `QuestionManager.cs`:
```csharp
new Question {
    questionText = "Your question here",
    validationType = ValidationType.MustBeConfident,
    forbiddenWords = new string[] { "um", "uh" }
}
```

### Change Interviewer Personality

Edit `LLMManager.cs` system prompt:
```csharp
systemPrompt = @"You are an absurd, unpredictable AI job interviewer...";
```

### Adjust Voice Analysis

Edit `VoiceAnalyzer.cs` thresholds:
```csharp
confidenceThreshold = 0.6f
nervousnessThreshold = 0.4f
```

## 📄 License

Educational and research purposes. Feel free to extend!

---

**Good luck surviving the interview from hell!** 😈🎤
