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

        string fullPrompt =$"You are {npcName}, {characterDescription}. " +
                           $"{personalityTraits} " +
                           $"{(!string.IsNullOrEmpty(backgroundStory) ? backgroundStory : "")} " +
                           $"{knowledgeString} " +
                           $"{lengthInstruction} " +
                           $"{styleInstructions} " +
                           $"{(!string.IsNullOrEmpty(uniqueQuirks) ? uniqueQuirks : "")} " +
                           $"{emotionInstructions}";

        // If custom system prompt is provided, use it instead
        if (!string.IsNullOrEmpty(customSystemPrompt))
        {
            return customSystemPrompt + emotionInstructions;
        }

        return fullPrompt;
    }
}