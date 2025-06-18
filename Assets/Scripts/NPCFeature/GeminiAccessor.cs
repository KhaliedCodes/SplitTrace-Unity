using System;
using System.Collections.Generic;
using UnityEngine;

public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIClient geminiAPI;
    
    [Header("Story Context")]
    [SerializeField] private StoryContextManager storyContext;

    private NPCPersonality npcPersonality;
    private Animator npcAnimator;

    public event Action<string, string> OnResponseProcessed;
    public event Action<List<string>> OnChoicesReceived;

    private void Start()
    {
        npcAnimator = GetComponent<Animator>();
        geminiAPI = geminiAPI ?? gameObject.AddComponent<GeminiAPIClient>();
        geminiAPI.OnResponseReceived += ProcessResponse;
        
        // Find StoryContextManager if not assigned
        if (storyContext == null)
        {
            // Try to find it in a GameObject first
            StoryContextHolder contextHolder = FindFirstObjectByType<StoryContextHolder>();
            if (contextHolder != null)
                storyContext = contextHolder.GetStoryContext();
        }
    }

    public void ConfigureWithPersonality(NPCPersonality personality)
    {
        npcPersonality = personality;
        string systemPrompt = personality.GenerateSystemPrompt();
        
        // Add story context to system prompt
        if (storyContext != null)
        {
            systemPrompt += "\n\nCURRENT STORY CONTEXT:\n" + storyContext.GetContextString();
            systemPrompt += "\nRespond appropriately based on this context. Reference discovered clues, known suspects, and revealed information when relevant.";
        }
        
        geminiAPI.SetSystemInstructions(systemPrompt);
        ClearChatHistory();
    }

    public void SetStoryContext(StoryContextManager context)
    {
        storyContext = context;
        // Refresh system instructions with new context
        if (npcPersonality != null)
            ConfigureWithPersonality(npcPersonality);
    }

    public void SendPlayerInput(string input) 
    {
        // Before sending, check if input might reveal new story information
        AnalyzePlayerInputForStoryUpdates(input);
        geminiAPI.GetAIResponse(input);
    }
    
    public void RequestChoices() => geminiAPI.GetChoicesResponse();
    public void ClearChatHistory() => geminiAPI.ClearChatHistory();

    private void AnalyzePlayerInputForStoryUpdates(string input)
    {
        if (storyContext == null) return;
        
        string lowerInput = input.ToLower();
        
        // Example: Player mentions finding evidence
        if (lowerInput.Contains("found") && (lowerInput.Contains("evidence") || lowerInput.Contains("clue")))
        {
            // You could implement more sophisticated parsing here
            // For now, this is a placeholder for potential story updates
            Debug.Log($"Player mentioned finding evidence: {input}");
        }
    }

    private void ProcessResponse(string response)
    {
        if (string.IsNullOrEmpty(response)) return;

        if (response.Trim().StartsWith("["))
        {
            ProcessChoicesResponse(response);
            return;
        }

        var (emotion, cleanResponse) = EmotionParser.Parse(response);
        
        // Check if NPC response reveals new story information
        AnalyzeNPCResponseForStoryUpdates(cleanResponse);
        
        if (npcPersonality != null && !npcPersonality.availableEmotions.Contains(emotion))
            emotion = npcPersonality.defaultEmotion;

        OnResponseProcessed?.Invoke(cleanResponse, emotion);
    }

    private void AnalyzeNPCResponseForStoryUpdates(string response)
    {
        if (storyContext == null || npcPersonality == null) return;
        
        string lowerResponse = response.ToLower();
        
        // Example triggers for story updates - customize these based on your story
        string[] clueKeywords = { "evidence", "clue", "discovered", "found", "hidden", "secret" };
        string[] suspectKeywords = { "suspect", "guilty", "accused", "blame", "culprit" };
        
        foreach (string keyword in clueKeywords)
        {
            if (lowerResponse.Contains(keyword))
            {
                // Extract potential clue information (this is simplified - you might want more sophisticated parsing)
                string potentialClue = $"{npcPersonality.npcName} mentioned: {response.Substring(0, Mathf.Min(50, response.Length))}...";
                storyContext.AddClue(potentialClue);
                Debug.Log($"Story Update: New clue added from {npcPersonality.npcName}");
                break;
            }
        }
        
        foreach (string keyword in suspectKeywords)
        {
            if (lowerResponse.Contains(keyword))
            {
                // You could implement name extraction here
                Debug.Log($"Story Update: {npcPersonality.npcName} mentioned a suspect");
                break;
            }
        }
        
        // Record what this NPC has revealed
        if (response.Length > 20) // Only record substantial information
        {
            storyContext.RevealNPCInfo(npcPersonality.npcName, response);
        }
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
        List<string> defaultChoices = new List<string>
        {
            "Tell me more about yourself",
            "What do you know about this place?",
            "I have a question for you",
            "I should go now"
        };
        
        // Add context-aware choices if story context is available
        if (storyContext != null)
        {
            if (storyContext.discoveredClues.Count > 0)
                defaultChoices.Insert(1, "I found some evidence...");
                
            if (storyContext.knownSuspects.Count > 0)
                defaultChoices.Insert(1, "What do you know about the suspects?");
        }
        
        return defaultChoices;
    }

    private void OnDestroy()
    {
        if (geminiAPI != null)
            geminiAPI.OnResponseReceived -= ProcessResponse;
    }
}