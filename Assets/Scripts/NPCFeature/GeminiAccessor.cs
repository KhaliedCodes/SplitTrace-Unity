using System;
using System.Collections.Generic;
using UnityEngine;

public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIClient geminiAPI;

    private NPCPersonality npcPersonality;
    private Animator npcAnimator;

    public event Action<string, string> OnResponseProcessed;
    public event Action<List<string>> OnChoicesReceived;

    private void Start()
    {
        npcAnimator = GetComponent<Animator>();
        geminiAPI = geminiAPI ?? gameObject.AddComponent<GeminiAPIClient>();
        geminiAPI.OnResponseReceived += ProcessResponse;
    }

    public void ConfigureWithPersonality(NPCPersonality personality)
    {
        npcPersonality = personality;
        geminiAPI.SetSystemInstructions(personality.GenerateSystemPrompt());
        ClearChatHistory();
    }

    public void SendPlayerInput(string input) => geminiAPI.GetAIResponse(input);
    public void RequestChoices() => geminiAPI.GetChoicesResponse();
    public void ClearChatHistory() => geminiAPI.ClearChatHistory();

    private void ProcessResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        if (response.Trim().StartsWith("["))
        {
            ProcessChoicesResponse(response);
            return;
        }

        var (emotion, cleanResponse) = EmotionParser.Parse(response);
        
        if (npcPersonality != null && !npcPersonality.availableEmotions.Contains(emotion))
            emotion = npcPersonality.defaultEmotion;

        OnResponseProcessed?.Invoke(cleanResponse, emotion);
    }

    private void ProcessChoicesResponse(string response)
    {
        try
        {
            response = response.Trim().TrimStart('[').TrimEnd(']');
            string[] choices = response.Split(',');
            
            List<string> cleanedChoices = new List<string>();
            foreach (string choice in choices)
            {
                cleanedChoices.Add(choice.Trim().Trim('"', ' '));
            }
            
            OnChoicesReceived?.Invoke(cleanedChoices.Count > 0 ? 
                cleanedChoices : GetDefaultChoices());
        }
        catch (Exception e)
        {
            Debug.LogError($"Choice parsing error: {e.Message}");
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

    private void OnDestroy()
    {
        if (geminiAPI != null)
            geminiAPI.OnResponseReceived -= ProcessResponse;
    }
}