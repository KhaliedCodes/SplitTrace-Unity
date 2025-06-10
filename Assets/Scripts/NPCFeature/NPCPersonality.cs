using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Personality", menuName = "RPG/NPC Personality")]
public class NPCPersonality : ScriptableObject
{
    [Header("Basic Information")]
    public string npcName = "NPC";
    [TextArea(3, 5)] public string characterDescription;
    [TextArea(3, 5)] public string initialGreeting = "Hello there!";

    [Header("Personality Traits")]
    [Range(0, 1)] public float friendliness = 0.5f;
    [Range(0, 1)] public float intelligence = 0.5f;
    [Range(0, 1)] public float patience = 0.5f;
    [Range(0, 1)] public float openness = 0.5f;

    [Header("Knowledge & Background")]
    [TextArea(3, 5)] public string backgroundStory;
    public List<string> knowledgeTopics = new List<string>();

    [Header("Story Context Awareness")]
    [Tooltip("How much this NPC knows about the overall story/mystery")]
    [Range(0, 1)] public float storyKnowledge = 0.5f;
    [Tooltip("Keywords that trigger this NPC to reveal story information")]
    public List<string> storyTriggerWords = new List<string>();
    [TextArea(2, 4)] public string secretInformation = "";
    [Tooltip("What evidence or clues this NPC might know about")]
    public List<string> knownEvidence = new List<string>();
    [Tooltip("Information about suspects this NPC might have")]
    public List<string> suspectInformation = new List<string>();

    [Header("Behavioral Settings")]
    public bool usesEmotions = true;
    [TextArea(3, 5)] public string uniqueQuirks;
    [TextArea(3, 10)] public string customSystemPrompt;

    [Header("Conversation Style")]
    public int maxResponseLength = 3; // Number of sentences
    public bool usesSlang = false;
    public bool isFormal = false;

    [Header("Emotions & Expressions")]
    public string defaultEmotion = "neutral";
    public List<string> availableEmotions = new List<string>() { "neutral", "happy", "sad", "angry" };

    // Generate the full system prompt for the AI
    public string GenerateSystemPrompt()
    {
        string emotionInstructions = usesEmotions ?
            " ALWAYS include a valid JSON emotion object at the END of your response from these options: neutral, happy, sad, angry. Example: {\"emotion\": \"happy\"}" : "";

        string lengthInstruction = $"Respond with natural, concise dialogue ({maxResponseLength} sentences max).";

        string styleInstructions = "";
        if (isFormal) styleInstructions += " Use formal language and address the player respectfully.";
        if (usesSlang) styleInstructions += " Occasionally use casual slang terms.";

        string personalityTraits = $"You are {(friendliness > 0.7f ? "very friendly" : friendliness < 0.3f ? "reserved" : "moderately friendly")}, " +
                                  $"{(intelligence > 0.7f ? "highly intelligent" : intelligence < 0.3f ? "simple-minded" : "reasonably intelligent")}, " +
                                  $"{(patience > 0.7f ? "extremely patient" : patience < 0.3f ? "easily irritated" : "generally patient")}, and " +
                                  $"{(openness > 0.7f ? "very open to new ideas" : openness < 0.3f ? "traditional in your views" : "somewhat open-minded")}.";

        string knowledgeString = "";
        if (knowledgeTopics.Count > 0)
        {
            knowledgeString = "You have knowledge about: " + string.Join(", ", knowledgeTopics) + ".";
        }

        // Add story context awareness
        string storyAwarenessInstructions = GenerateStoryAwarenessInstructions();

        string fullPrompt = $"You are {npcName}, {characterDescription}. " +
                           $"{personalityTraits} " +
                           $"{(!string.IsNullOrEmpty(backgroundStory) ? backgroundStory : "")} " +
                           $"{knowledgeString} " +
                           $"{storyAwarenessInstructions} " +
                           $"{lengthInstruction} " +
                           $"{styleInstructions} " +
                           $"{(!string.IsNullOrEmpty(uniqueQuirks) ? uniqueQuirks : "")} " +
                           $"{emotionInstructions}";

        // If custom system prompt is provided, use it instead
        if (!string.IsNullOrEmpty(customSystemPrompt))
        {
            return customSystemPrompt + storyAwarenessInstructions + emotionInstructions;
        }

        return fullPrompt;
    }

    private string GenerateStoryAwarenessInstructions()
    {
        string instructions = "";

        // Story knowledge level
        if (storyKnowledge > 0.7f)
        {
            instructions += " You are well-informed about the current events and story. You can provide detailed information when asked directly.";
        }
        else if (storyKnowledge > 0.3f)
        {
            instructions += " You have some knowledge about current events but may be hesitant to share everything immediately.";
        }
        else
        {
            instructions += " You have limited knowledge about the current situation and mostly know only what directly affects you.";
        }

        // Secret information
        if (!string.IsNullOrEmpty(secretInformation))
        {
            instructions += $" You know this secret information: {secretInformation}. Only reveal this if the player asks the right questions or mentions relevant keywords.";
        }

        // Known evidence
        if (knownEvidence.Count > 0)
        {
            instructions += $" You are aware of the following evidence: {string.Join(", ", knownEvidence)}. Mention these naturally if the conversation turns to relevant topics.";
        }

        // Suspect information
        if (suspectInformation.Count > 0)
        {
            instructions += $" You know this about potential suspects: {string.Join(", ", suspectInformation)}. Share this information if asked about specific people or if the player shows evidence.";
        }

        // Trigger words
        if (storyTriggerWords.Count > 0)
        {
            instructions += $" Pay special attention if the player mentions: {string.Join(", ", storyTriggerWords)}. These topics might prompt you to share more information.";
        }

        return instructions;
    }

    // Method to check if player input contains trigger words
    public bool ContainsTriggerWords(string input)
    {
        if (storyTriggerWords.Count == 0) return false;
        
        string lowerInput = input.ToLower();
        foreach (string trigger in storyTriggerWords)
        {
            if (lowerInput.Contains(trigger.ToLower()))
                return true;
        }
        return false;
    }

    // Get information this NPC should reveal based on context
    public string GetContextualInformation(StoryContextManager storyContext)
    {
        if (storyContext == null) return "";
        
        string info = "";
        
        // Check if any discovered clues relate to this NPC's knowledge
        foreach (string clue in storyContext.discoveredClues)
        {
            foreach (string evidence in knownEvidence)
            {
                if (clue.ToLower().Contains(evidence.ToLower()) || evidence.ToLower().Contains(clue.ToLower()))
                {
                    info += $"I see you've found {evidence}. ";
                    break;
                }
            }
        }
        
        // Check if any suspects relate to this NPC's information
        foreach (string suspect in storyContext.knownSuspects)
        {
            foreach (string suspectInfo in suspectInformation)
            {
                if (suspectInfo.ToLower().Contains(suspect.ToLower()))
                {
                    info += $"About {suspect}... {suspectInfo} ";
                    break;
                }
            }
        }
        
        return info.Trim();
    }
}