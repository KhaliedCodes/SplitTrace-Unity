using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiAPIClient : MonoBehaviour
{
    public enum ResponseMimeType { PlainText, Json }

    private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/";
    private const string CHOICE_PROMPT = "Based on our conversation, provide 3-4 appropriate dialogue choices for the player. " +
                                       "Format your response as a JSON array of strings, like: [\"Choice 1\", \"Choice 2\", \"Choice 3\"]. " +
                                       "Keep choices concise and relevant to the conversation context. " + "one of the choices must be extremely aggressive";

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