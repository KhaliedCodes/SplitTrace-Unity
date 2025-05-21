using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

// This component serves as a bridge between NPCs and the GeminiAPIPersonality
public class GeminiAccessor : MonoBehaviour
{
    [Header("Gemini Configuration")]
    [SerializeField] private GeminiAPIPersonality geminiAPI;

    [Header("NPC Information")]
    [SerializeField] private string npcName;
    [SerializeField, TextArea(3, 5)] private string characterDescription;
    [SerializeField, TextArea(3, 10)] private string customSystemPrompt;

    [Header("Response Processing")]
    [SerializeField] private bool parseEmotions = true;
    [SerializeField] private bool includeEmotionInstructions = true;

    // Reference to the NPC's animator component
    private Animator npcAnimator;

    private void Start()
    {
        // Get references
        npcAnimator = GetComponent<Animator>();

        // Create Gemini API component if not assigned
        if (geminiAPI == null)
        {
            geminiAPI = GetComponent<GeminiAPIPersonality>();
            if (geminiAPI == null)
            {
                geminiAPI = gameObject.AddComponent<GeminiAPIPersonality>();
            }
        }

        // Initialize with character description
        InitializeGeminiWithCharacter();
    }

    private void InitializeGeminiWithCharacter()
    {
        if (string.IsNullOrEmpty(npcName))
            npcName = gameObject.name;

        // Create system prompt if not provided
        if (string.IsNullOrEmpty(customSystemPrompt))
        {
            string emotionInstructions = "";
            if (includeEmotionInstructions)
            {
                emotionInstructions = " Include an emotion in JSON format at the end of each response like this: {\"emotion\": \"happy\"} " +
                                     "Possible emotions are: happy, sad, angry, neutral. Choose the most appropriate emotion for your response.";
            }

            customSystemPrompt = $"You are {npcName}, {characterDescription}. " +
                                "Respond in character with brief, natural dialogue that would fit in a video game. " +
                                "Keep responses concise (1-3 sentences). " +
                                $"{emotionInstructions}";
        }

        // Set the system instructions field using reflection since it's private
        var field = typeof(GeminiAPIPersonality).GetField("_systemInstructions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
            field.SetValue(geminiAPI, customSystemPrompt);

        // Clear any existing chat history
        geminiAPI.ClearChatHistory();
    }

    // Send player input to Gemini and get a response
    public void SendPlayerInput(string input)
    {
        // Exit if API reference is missing
        if (geminiAPI == null)
        {
            Debug.LogError($"GeminiAPI reference missing for NPC {npcName}!");
            return;
        }

        // Process the input through Gemini
        geminiAPI.GetAIResponse(input);

        // Response handling is done in the GeminiAPIPersonality class
        // which will update the UI via the _responseText reference
    }

    // Process AI response and extract emotion
    private void ProcessResponse(string response)
    {
        if (!parseEmotions || string.IsNullOrEmpty(response))
            return;

        // Extract emotion JSON if present
        int jsonStart = response.LastIndexOf("{");
        if (jsonStart != -1 && response.EndsWith("}"))
        {
            string jsonPart = response.Substring(jsonStart);
            try
            {
                var json = JsonUtility.FromJson<EmotionData>(jsonPart);
                if (!string.IsNullOrEmpty(json.emotion))
                {
                    UpdateAnimation(json.emotion);
                }
            }
            catch
            {
                // Failed to parse emotion, just continue
                Debug.LogWarning($"Failed to parse emotion from: {jsonPart}");
            }
        }
    }

    // Update animation based on emotion
    private void UpdateAnimation(string emotion)
    {
        if (npcAnimator == null)
            return;

        // Reset all emotion triggers
        npcAnimator.ResetTrigger("Happy");
        npcAnimator.ResetTrigger("Sad");
        npcAnimator.ResetTrigger("Angry");

        // Set the appropriate trigger
        switch (emotion.ToLower())
        {
            case "happy":
                npcAnimator.SetTrigger("Happy");
                break;

            case "sad":
                npcAnimator.SetTrigger("Sad");
                break;

            case "angry":
                npcAnimator.SetTrigger("Angry");
                break;

            default:
                // Default to neutral/idle
                break;
        }
    }

    // Emotion data structure for JSON parsing
    [System.Serializable]
    private class EmotionData
    {
        public string emotion;
    }
}