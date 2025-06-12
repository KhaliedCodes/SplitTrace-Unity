using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


[Serializable]
    public struct Content
    {
        public string role;
        public List<Part> parts;
    }

    [Serializable]
    public struct Part
    {
        public string text;
    }

    [Serializable]
    public struct GenerationConfig
    {
        public string responseMimeType;
    }
public class GeminiAPIClient : MonoBehaviour
{
    public enum ResponseMimeType { PlainText, Json }

    private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/";
 private const string CHOICE_PROMPT = @"
   You are generating dialogue choices for a detective player.
    Based on our chat history AND the overall story context provided (including story progress, discovered clues, the NPC's personality, and their connection to the case), generate 3-4 appropriate dialogue choices for the player.

    Your primary goal is to make the choices reflect a detective's investigative approach, focusing on gathering information, making deductions, and strategically influencing the conversation.

    - **Inquiry/Investigation:** Prioritize choices that ask detailed questions, probe for specifics, or seek clarification on past statements.
    - **Confrontation/Pressure (if warranted by context):** If the story context or chat history indicates suspicion, deception, or rising tension, **at least two of the choices must be direct, highly accusatory, threatening, or overtly aggressive, specifically designed to provoke a strong negative reaction from the NPC.** These choices should reference clues or inconsistencies forcefully.
    - **Empathy/Strategic Approach:** If the NPC is revealing a secret or is in a vulnerable state, provide choices that allow the player to be compassionate, exploit the vulnerability for information, or ask for more details in a supportive (or manipulative) manner.
    - **Neutral/Observational:** Include options that allow the player to remain neutral, observe, or steer the conversation in a different direction for information gathering.

    ALWAYS format your response as a single, clean JSON array of strings. The choices must be concise and directly relevant to the last thing the NPC said, but framed from a detective's perspective.

    Example for a tense situation (detective, with hostility): [""I know you're lying, and I have the evidence to prove it. Confess now!"", ""Don't even think about running, I'll have you locked up for this."", ""What are you not telling me about that night?"", ""Let's review your alibi for the last time before I bring in the precinct.""]
    Example for a neutral situation (detective): [""Can you elaborate on your relationship with the victim?"", ""What's your theory on what happened?"", ""I noticed you seemed distressed earlier, is everything alright?"", ""Thank you for your cooperation.""]
";

    [Header("API Configuration")]
    [SerializeField] private string _modelName = "gemini-2.0-flash";
    [SerializeField] private string _apiKey;
    [SerializeField] private ResponseMimeType _responseMimeType = ResponseMimeType.Json;

    [Header("Conversation Settings")]
    [SerializeField] private bool _enableChatHistory = true;
    [SerializeField] private int _maxHistory = 5;
    [SerializeField] public List<Content> _chatHistory = new List<Content>();

    private string _systemInstructions = "";

    public delegate void AIResponseCallback(string response);
    public event AIResponseCallback OnResponseReceived;

    [Serializable]
    private struct GeminiRequestBody
    {
        public List<Content> contents;
        public Content systemInstruction;
        public GenerationConfig generationConfig;
    }
    public void SetSystemInstructions(string instructions)
    {
        _systemInstructions = instructions;
    }

    public async Task<string> GenerateContentAsync(string prompt, bool forChoices = false)
    {
        string url = $"{BASE_URL}{_modelName}:generateContent?key={_apiKey}";
        var requestBody = CreateRequestBody(prompt, forChoices);
        string jsonData = JsonUtility.ToJson(requestBody);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        try
        {
            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Error: {request.error}");
                return null;
            }

            JObject responseJObject = JObject.Parse(request.downloadHandler.text);
            string aiResponse = responseJObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
            string rawResponse = forChoices ? aiResponse : CleanAIResponse(aiResponse);

            if (_enableChatHistory && !string.IsNullOrEmpty(rawResponse) && !forChoices)
            {
                _chatHistory.Add(CreateMessageObject("model", rawResponse));
                if (_chatHistory.Count > _maxHistory * 2)
                {
                    _chatHistory.RemoveAt(0);
                    _chatHistory.RemoveAt(0);
                }
            }

            return rawResponse;
        }
        catch (Exception e)
        {
            Debug.LogError($"API Exception: {e.Message}");
            return null;
        }
    }

    private GeminiRequestBody CreateRequestBody(string prompt, bool forChoices = false)
    {
        var requestBody = new GeminiRequestBody
        {
            contents = new List<Content>(),
            generationConfig = new GenerationConfig()
        };

        if (!string.IsNullOrEmpty(_systemInstructions))
        {
            requestBody.systemInstruction = new Content
            {
                role = "system",
                parts = new List<Part> { new Part { text = _systemInstructions } }
            };
        }

        if (_enableChatHistory && !forChoices)
        {
            requestBody.contents = _chatHistory.Select(msg => new Content
            {
                role = msg.role,
                parts = new List<Part> { new Part { text = msg.parts[0].text } }
            }).ToList();
        }

        requestBody.contents.Add(new Content
        {
            role = "user",
            parts = new List<Part> { new Part { text = prompt } }
        });

        // Force JSON response for choices, use configured type for regular responses
        if (forChoices || _responseMimeType == ResponseMimeType.Json)
        {
            requestBody.generationConfig.responseMimeType = "application/json";
        }

        return requestBody;
    }

    public async void GetAIResponse(string playerMessage)
    {
        if (string.IsNullOrEmpty(playerMessage)) return;

        _chatHistory.Add(CreateMessageObject("user", playerMessage));
        if (_chatHistory.Count > _maxHistory * 2)
        {
            _chatHistory.RemoveAt(0);
            _chatHistory.RemoveAt(0);
        }

        string aiResponse = await GenerateContentAsync(playerMessage, false);
        OnResponseReceived?.Invoke(aiResponse);
    }

    public async void GetChoicesResponse()
    {
        string aiResponse = await GenerateContentAsync(CHOICE_PROMPT, true);
        OnResponseReceived?.Invoke(aiResponse);
    }

    private string CleanAIResponse(string response)
    {
        // Remove unwanted characters (brackets, quotes, etc.)
        response = response.Replace("{", "").Replace("}", "").Replace("\"", "").Replace("[", "").Replace("]", "").Trim();

        // Remove the "response:" prefix if it exists
        if (response.StartsWith("response:"))
        {
            response = response.Substring("response:".Length).Trim();
        }

        return response;
    }

    private Content CreateMessageObject(string role, string text)
    {
        return new Content
        {
            role = role,
            parts = new List<Part> { new Part { text = text } }
        };
    }

    public void ClearChatHistory()
    {
        _chatHistory.Clear();
    }
}