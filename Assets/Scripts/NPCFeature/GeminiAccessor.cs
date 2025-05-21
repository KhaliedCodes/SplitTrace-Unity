using System;
using UnityEngine;
using System.Text.RegularExpressions;

public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIClient geminiAPI;

    // Store personality reference and handle all personality-related logic here
    private NPCPersonality npcPersonality;
    private Animator npcAnimator;

    // Event for sending processed responses back to NPCController
    public event Action<string, string> OnResponseProcessed;

    private void Start()
    {
        npcAnimator = GetComponent<Animator>();

        // Initialize the API client
        if (geminiAPI == null)
        {
            geminiAPI = gameObject.AddComponent<GeminiAPIClient>();
        }

        geminiAPI.OnResponseReceived += ProcessResponse;
    }

    public void ConfigureWithPersonality(NPCPersonality personality)
    {
        if (personality == null) return;

        npcPersonality = personality;

        // Generate the system prompt from the personality
        string systemPrompt = personality.GenerateSystemPrompt();

        // Configure the API client with the system prompt
        geminiAPI.SetSystemInstructions(systemPrompt);
        ClearChatHistory();
    }

    public void SendPlayerInput(string input) => geminiAPI.GetAIResponse(input);

    public void ClearChatHistory() => geminiAPI.ClearChatHistory();

    private void ProcessResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        var emotionMatch = Regex.Match(response, @"{\s*""emotion""\s*:\s*""(\w+)""\s*}");
        string emotion = npcPersonality != null ? npcPersonality.defaultEmotion : "neutral";
        string cleanResponse = response;

        if (emotionMatch.Success)
        {
            emotion = emotionMatch.Groups[1].Value.ToLower();
            cleanResponse = Regex.Replace(response, @"{\s*""emotion""\s*:\s*""\w+""\s*}", "").Trim();
        }

        // Validate the emotion is one of the allowed emotions
        if (npcPersonality != null && npcPersonality.availableEmotions.Count > 0)
        {
            if (!npcPersonality.availableEmotions.Contains(emotion))
            {
                emotion = npcPersonality.defaultEmotion;
            }
        }

        UpdateAnimation(emotion);
        OnResponseProcessed?.Invoke(cleanResponse, emotion);
    }

    private void UpdateAnimation(string emotion)
    {
        if (npcAnimator == null) return;

        npcAnimator.ResetTrigger("Neutral");
        npcAnimator.SetTrigger("Neutral");

        switch (emotion.ToLower())
        {
            case "happy":
                npcAnimator.ResetTrigger("Happy");
                npcAnimator.SetTrigger("Happy");
                break;
            case "sad":
                npcAnimator.ResetTrigger("Sad");
                npcAnimator.SetTrigger("Sad");
                break;
            case "angry":
                npcAnimator.ResetTrigger("Angry");
                npcAnimator.SetTrigger("Angry");
                break;
        }
    }

    private void OnDestroy() => geminiAPI.OnResponseReceived -= ProcessResponse;
}