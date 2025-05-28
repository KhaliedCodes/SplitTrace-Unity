using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIClient geminiAPI;

    // Store personality reference and handle all personality-related logic here
    private NPCPersonality npcPersonality;
    private Animator npcAnimator;

    // Events for sending processed responses back to NPCController
    public event Action<string, string> OnResponseProcessed;
    public event Action<List<string>> OnChoicesReceived;

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

    public void RequestChoices() => geminiAPI.GetChoicesResponse();

    public void ClearChatHistory() => geminiAPI.ClearChatHistory();

    private void ProcessResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        // Check if this is a choices response (JSON array format)
        if (response.Trim().StartsWith("[") && response.Trim().EndsWith("]"))
        {
            ProcessChoicesResponse(response);
            return;
        }

        // Process regular dialogue response
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

    private void ProcessChoicesResponse(string response)
    {
        try
        {
            // Parse JSON array of choices
            List<string> choices = new List<string>();
            
            // Simple JSON parsing for array of strings
            string cleanResponse = response.Trim().TrimStart('[').TrimEnd(']');
            string[] choiceParts = cleanResponse.Split(',');
            
            foreach (string part in choiceParts)
            {
                string choice = part.Trim().Trim('"').Trim();
                if (!string.IsNullOrEmpty(choice))
                {
                    choices.Add(choice);
                }
            }

            // Fallback choices if parsing fails or no choices found
            if (choices.Count == 0)
            {
                choices = GetDefaultChoices();
            }

            OnChoicesReceived?.Invoke(choices);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing choices response: {e.Message}");
            OnChoicesReceived?.Invoke(GetDefaultChoices());
        }
    }

    private List<string> GetDefaultChoices()
    {
        return new List<string>
        {
            "Tell me more about yourself",
            "What do you know about this place?",
            "I have a question for you",
            "I should go now"
        };
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