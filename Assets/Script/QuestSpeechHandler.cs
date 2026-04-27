using Whisper;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class QuestSpeechHandler : MonoBehaviour
{
    public WhisperManager whisper;
    public LMStudioClient lmStudio; // Reference to your LLM script
    public TMP_Text text;
    private AudioClip _recordedClip; // This replaces 'myAudioClip'
    private string _microphoneName;

    void Start()
    {
        // Get the default microphone name (Quest Internal Mic)
        if (Microphone.devices.Length > 0)
        {
            _microphoneName = Microphone.devices[0];
        }
    }

    async void Update()
    {
        // Start recording when "A" is pressed
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            StartRecording();
        }

        // Stop and Transcribe when "A" is released
        if (OVRInput.GetUp(OVRInput.RawButton.B))
        {
            await StopAndTranscribe();
        }
    }

    void StartRecording()
    {
        text.text = "Recording started...";
        // Record for up to 30 seconds, 16000Hz (Whisper's preferred frequency)
        _recordedClip = Microphone.Start(_microphoneName, false, 30, 16000);
    }

    async Task StopAndTranscribe()
    {
        text.text = "Processing speech...";

        int lastSamplePos = Microphone.GetPosition(_microphoneName);
        Microphone.End(_microphoneName);

        if (_recordedClip == null || lastSamplePos <= 0) return;

        float[] samples = new float[lastSamplePos * _recordedClip.channels];
        _recordedClip.GetData(samples, 0);

        AudioClip trimmedClip = AudioClip.Create("Trimmed", lastSamplePos, _recordedClip.channels, 16000, false);
        trimmedClip.SetData(samples, 0);

        var result = await whisper.GetTextAsync(trimmedClip);
        string transcribedText = result.Result;

        if (!string.IsNullOrEmpty(transcribedText) && transcribedText != "[BLANK_AUDIO]")
        {
            text.text = "You: " + transcribedText + "\n\n<color=#00FF00>AI is thinking...</color>";

            // BRIDGE: Send the transcribed text to LM Studio
            lmStudio.AskAI(transcribedText);
        }
        else
        {
            text.text = "Sorry, I didn't hear anything. Try holding 'A' while speaking.";
        }
    }
    //async Task StopAndTranscribe()
    //{
    //    text.text = "Recording stopped. Transcribing...";

    //    // 1. Get the exact time the user stopped speaking
    //    int lastSamplePos = Microphone.GetPosition(_microphoneName);
    //    Microphone.End(_microphoneName);

    //    if (_recordedClip == null || lastSamplePos <= 0) return;

    //    // 2. Create a NEW trimmed clip so Whisper doesn't "listen" to 30s of silence
    //    float[] samples = new float[lastSamplePos * _recordedClip.channels];
    //    _recordedClip.GetData(samples, 0);

    //    AudioClip trimmedClip = AudioClip.Create("Trimmed", lastSamplePos, _recordedClip.channels, 16000, false);
    //    trimmedClip.SetData(samples, 0);

    //    // 3. Send the TRIMMED clip to Whisper
    //    var result = await whisper.GetTextAsync(trimmedClip);

    //    text.text = "Final Transcription: " + result.Result;
    //}
}