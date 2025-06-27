using System;
using System.Collections.Generic;
using UnityEngine;

public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIClient geminiAPI;
    
    [Header("Story Context")]
    private StoryContextManager storyContext; // Automatically retrieved from StoryContextHolder

    private NPCPersonality npcPersonality;

    public event Action<string, string> OnResponseProcessed;
    public event Action<List<string>> OnChoicesReceived;

    private void Start()
    {
        geminiAPI = geminiAPI ?? gameObject.AddComponent<GeminiAPIClient>();
        geminiAPI.OnResponseReceived += ProcessResponse;
        
        // Get story context from StoryContextHolder
        InitializeStoryContext();
    }

    private void InitializeStoryContext()
    {
        // Always get story context from StoryContextHolder (no inspector assignment)
        if (StoryContextHolder.Instance != null)
        {
            storyContext = StoryContextHolder.Instance.GetStoryContext();
          //  Debug.Log($"GeminiAccessor on {gameObject.name} got story context from StoryContextHolder");
        }
        else
        {
            StoryContextHolder contextHolder = FindFirstObjectByType<StoryContextHolder>();
            if (contextHolder != null)
            {
                storyContext = contextHolder.GetStoryContext();
              //  Debug.Log($"GeminiAccessor on {gameObject.name} found StoryContextHolder in scene");
            }
            else
            {
                Debug.LogWarning($"GeminiAccessor on {gameObject.name} could not find StoryContextHolder in scene");
            }
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
       // Debug.Log($"GeminiAccessor on {gameObject.name} story context updated");
        
        // Refresh system instructions with new context
        if (npcPersonality != null)
            ConfigureWithPersonality(npcPersonality);
    }

    // Public method to manually refresh story context from holder
    public void RefreshStoryContextFromHolder()
    {
        if (StoryContextHolder.Instance != null)
        {
            var newContext = StoryContextHolder.Instance.GetStoryContext();
            if (newContext != storyContext)
            {
                SetStoryContext(newContext);
            }
        }
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
            // Extract potential clue information
            string potentialClue = $"{npcPersonality.npcName} mentioned: {input.Substring(0, Mathf.Min(50, input.Length))}...";
            storyContext.AddClue(potentialClue, "Player");
          //  Debug.Log($"Story Update: New clue added from player input");
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
        float reliability = npcPersonality.reliability;
        
        // Record what this NPC has revealed
        if (response.Length > 20) // Only record substantial information
        {
            storyContext.RevealNPCInfo(npcPersonality.npcName, response, reliability);
        }

        // Example triggers for story updates - customize these based on your story
        string[] clueKeywords = { "evidence", "clue", "discovered", "found", "hidden", "secret" };
        string[] suspectKeywords = { "suspect", "guilty", "accused", "blame", "culprit" };
        string[] eventKeywords = { "event", "happened", "occurred", "incident", "situation" };
        
        foreach (string keyword in clueKeywords)
        {
            if (lowerResponse.Contains(keyword))
            {
                // Extract potential clue information
                string potentialClue = $"{npcPersonality.npcName} mentioned: {response.Substring(0, Mathf.Min(50, response.Length))}...";
                storyContext.AddClue(potentialClue, npcPersonality.npcName, reliability);
              //  Debug.Log($"Story Update: New clue added from {npcPersonality.npcName}");
                break;
            }
        }
        
        foreach (string keyword in suspectKeywords)
        {
            if (lowerResponse.Contains(keyword))
            {
                // Extract potential suspect information
                string potentialSuspect = ExtractSuspectName(response);
                if (!string.IsNullOrEmpty(potentialSuspect))
                {
                    storyContext.AddSuspect(potentialSuspect, $"{npcPersonality.npcName} implicated this person");
                  //  Debug.Log($"Story Update: New suspect added from {npcPersonality.npcName}");
                }
                break;
            }
        }
        
        foreach (string keyword in eventKeywords)
        {
            if (lowerResponse.Contains(keyword))
            {
                // Extract potential event information
                string potentialEvent = $"{npcPersonality.npcName} mentioned: {response.Substring(0, Mathf.Min(70, response.Length))}...";
                storyContext.AddKeyEvent(potentialEvent);
                //Debug.Log($"Story Update: New key event added from {npcPersonality.npcName}");
                break;
            }
        }
    }

    private string ExtractSuspectName(string response)
    {
        // Simple name extraction - you should implement more sophisticated NLP here
        string[] nameKeywords = { "mr.", "mrs.", "ms.", "dr." };
        string[] words = response.Split(' ');
        
        for (int i = 0; i < words.Length; i++)
        {
            if (Array.Exists(nameKeywords, keyword => words[i].StartsWith(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                if (i + 1 < words.Length)
                {
                    return $"{words[i]} {words[i+1]}";
                }
            }
        }
        
        return null;
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
          //  Debug.LogError($"Choice parsing error: {e.Message}");
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
            // Add clue-related choice if any clues exist
            if (storyContext.discoveredClues.Count > 0)
                defaultChoices.Insert(1, "I found some evidence...");
                
            // Add suspect-related choice if any suspects exist
            if (storyContext.knownSuspects.Count > 0)
                defaultChoices.Insert(1, "What do you know about the suspects?");
                
            // Add contradiction-related choice if any contradictions exist
            if (storyContext.GetUnresolvedContradictions().Count > 0)
                defaultChoices.Insert(1, "I found some contradictions...");
        }
        
        return defaultChoices;
    }

    private void OnDestroy()
    {
        if (geminiAPI != null)
            geminiAPI.OnResponseReceived -= ProcessResponse;
    }
}