using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class GeminiAPIPersonality : MonoBehaviour
{
    #region Enums & Constants
    public enum ResponseMimeType
    {
        PlainText,
        Json
    }

    private const string BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models/";
    #endregion

    #region Serialized Fields
    [SerializeField] private string _modelName = "gemini-2.0-flash";
    [SerializeField] private string _apiKey;
    [SerializeField] private TextMeshProUGUI _responseText; // UI display for AI responses
    [TextArea(3, 10)] public string _systemInstructions = "You are Commander Gogo, a ruthless yet honorable warlord. You command The Vindicator and The Phantom Lance. You are a master of strategic warfare, known for your precision, adaptability, and unyielding dominance in space battles. You speak with authority, respect worthy opponents, and use advanced AI-driven tactics.";
    [SerializeField] private ResponseMimeType _responseMimeType = ResponseMimeType.Json;
    [SerializeField] private bool _enableChatHistory = true;
    [SerializeField] private int _maxHistory = 5; // Maximum messages to store in history
    [SerializeField] public List<Content> _chatHistory = new List<Content>();
    #endregion

    #region Chat History Management
    private void InitializeChatHistory()
    {
        _chatHistory.Clear();
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
        InitializeChatHistory();
    }
    #endregion

    #region API Communication
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

    private GeminiRequestBody CreateRequestBody(string prompt)
    {
        var requestBody = new GeminiRequestBody
        {
            contents = new List<Content>(),
            generationConfig = new GenerationConfig()
        };

        // Add system instructions if provided
        if (!string.IsNullOrEmpty(_systemInstructions))
        {
            requestBody.systemInstruction = new Content
            {
                role = "system",
                parts = new List<Part> { new Part { text = _systemInstructions } }
            };
        }

        // Add chat history if enabled
        if (_enableChatHistory)
        {
            requestBody.contents = _chatHistory.Select(msg => new Content
            {
                role = msg.role,
                parts = new List<Part> { new Part { text = msg.parts[0].text } }
            }).ToList();
        }

        // Add the current user prompt
        requestBody.contents.Add(new Content
        {
            role = "user",
            parts = new List<Part> { new Part { text = prompt } }
        });

        // Set the response MIME type
        if (_responseMimeType == ResponseMimeType.Json)
        {
            requestBody.generationConfig = new GenerationConfig
            {
                responseMimeType = "application/json"
            };
        }

        return requestBody;
    }
    public async Task<string> GenerateContentAsync(string prompt)
    {
        string url = $"{BASE_URL}{_modelName}:generateContent?key={_apiKey}";

        var requestBody = CreateRequestBody(prompt);
        string jsonData = JsonUtility.ToJson(requestBody);
        Debug.Log($"Sending request: {jsonData}");

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
                Debug.LogError($"API Request failed: {request.error}\nResponse: {request.downloadHandler.text}");
                return null;
            }

            // Parse the JSON response
            var responseJObject = JObject.Parse(request.downloadHandler.text);
            string aiResponse = responseJObject["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            // Clean the AI response
            aiResponse = CleanAIResponse(aiResponse);

            // Log the AI response
            Debug.Log($"AI Response: {aiResponse}");

            // Update chat history
            if (_enableChatHistory && !string.IsNullOrEmpty(aiResponse))
            {
                _chatHistory.Add(CreateMessageObject("model", aiResponse));
                if (_chatHistory.Count > _maxHistory * 2)
                {
                    _chatHistory.RemoveAt(0);
                    _chatHistory.RemoveAt(0);
                }
            }

            return aiResponse;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during API request: {e.Message}");
            return null;
        }
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

    private void DetectEmotion(string emotion)
    {
        if (emotion.Contains("happy"))
        {
            Debug.Log("Happy");
        }
        else if (emotion.Contains("sad"))
        {
            Debug.Log("Sad");
        }
        else if (emotion.Contains("angry"))
        {
            Debug.Log("Angry");
        }
        else
        {
            Debug.Log("Unknown");
        }
    }
    #endregion

    #region Public Method for Sending Messages
    public async void GetAIResponse(string playerMessage)
    {
        if (string.IsNullOrEmpty(playerMessage))
        {
            Debug.LogError("Player message is empty!");
            return;
        }

        // Store player message in history
        _chatHistory.Add(CreateMessageObject("user", playerMessage));

        // Limit history size
        if (_chatHistory.Count > _maxHistory * 2)
        {
            _chatHistory.RemoveAt(0);
            _chatHistory.RemoveAt(0);
        }

        // Get AI response
        string aiResponse = await GenerateContentAsync(playerMessage);

        // Display AI response
        if (_responseText != null)
        {
            _responseText.text = "Cammander GOGO: " + aiResponse ?? "Failed to generate response";
        }
    }
    #endregion
}