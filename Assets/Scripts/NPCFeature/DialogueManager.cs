using Newtonsoft.Json.Linq;
using StarterAssets;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image npcPortrait;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button endConversationButton;

    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioSource typingSoundEffect;
    [SerializeField] private bool useTypewriterEffect = true;

    [Header("Emotion Expressions")]
    [SerializeField] private Sprite neutralExpression;
    [SerializeField] private Sprite happyExpression;
    [SerializeField] private Sprite sadExpression;
    [SerializeField] private Sprite angryExpression;

    // Cached player control components for fallback
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private PlayerInput playerInput;

    // Player references
    private PlayerController playerController;

    // Current dialogue state
    private NPCController currentNPC;
    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool isDialogueActive;

    // Singleton
    public static DialogueManager Instance { get; private set; }

    #region Unity Methods
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        dialoguePanel?.SetActive(false);

        if (sendButton != null)
            sendButton.onClick.AddListener(OnSendButtonClicked);

        if (endConversationButton != null)
            endConversationButton.onClick.AddListener(EndDialogue);

        if (npcPortrait != null && neutralExpression != null)
            npcPortrait.sprite = neutralExpression;

        CachePlayerControlComponents();
        FindPlayerController();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            OnSendButtonClicked();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndDialogue();
        }
    }
    #endregion

    #region Initialization and Caching
    private void CachePlayerControlComponents()
    {
        thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void FindPlayerController()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("DialogueManager couldn't find PlayerController in the scene!");
        }
    }
    #endregion

    #region Dialogue Control
    public void StartDialogue(NPCController npc, string initialGreeting = null)
    {
        if (isDialogueActive || npc == null) return;

        currentNPC = npc;
        isDialogueActive = true;

        dialoguePanel?.SetActive(true);
        npcNameText.text = npc.name;
        dialogueText.text = string.Empty;

        if (playerController != null)
        {
            playerController.DisableControls();
        }
        else
        {
            DisableFallbackPlayerControls();
        }

        if (!string.IsNullOrEmpty(initialGreeting))
            DisplayNPCDialogue(initialGreeting);

        StartCoroutine(FocusInputFieldRoutine());
    }

    public void EndDialogue()
    {
        if (!isDialogueActive) return;

        isDialogueActive = false;

        currentNPC?.EndInteraction();
        currentNPC = null;

        dialoguePanel?.SetActive(false);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        if (playerController != null)
        {
            playerController.EnableControls();
        }
        else
        {
            EnableFallbackPlayerControls();
        }
    }
    #endregion

    #region Player Controls Fallback
    private void DisableFallbackPlayerControls()
    {
        if (thirdPersonController != null)
            thirdPersonController.enabled = false;

        if (starterAssetsInputs != null)
            starterAssetsInputs.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void EnableFallbackPlayerControls()
    {
        if (thirdPersonController != null)
            thirdPersonController.enabled = true;

        if (starterAssetsInputs != null)
            starterAssetsInputs.enabled = true;

        if (playerInput != null)
            playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Dialogue Display
    public void DisplayNPCDialogue(string dialogue)
    {
        if (string.IsNullOrEmpty(dialogue) || dialogueText == null)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            isTyping = false;
        }

        // Extract emotion JSON if present
        string displayText = dialogue;
        string emotion = "neutral";

        int jsonStartIndex = dialogue.LastIndexOf("{");
        if (jsonStartIndex != -1 && dialogue.EndsWith("}"))
        {
            try
            {
                string jsonPart = dialogue.Substring(jsonStartIndex);
                JObject emotionData = JObject.Parse(jsonPart);

                if (emotionData["emotion"] != null)
                {
                    emotion = emotionData["emotion"].ToString().ToLowerInvariant();
                    displayText = dialogue.Substring(0, jsonStartIndex).Trim();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse emotion data: {e.Message}");
            }
        }

        UpdateNPCExpression(emotion);

        if (useTypewriterEffect)
        {
            typingCoroutine = StartCoroutine(TypeDialogue(displayText));
        }
        else
        {
            dialogueText.text = displayText;
        }

        StartCoroutine(FocusInputFieldRoutine());
    }

    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;

            if (typingSoundEffect != null && !typingSoundEffect.isPlaying)
                typingSoundEffect.Play();

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void UpdateNPCExpression(string emotion)
    {
        if (npcPortrait == null) return;

        switch (emotion)
        {
            case "happy":
                if (happyExpression != null) npcPortrait.sprite = happyExpression;
                break;
            case "sad":
                if (sadExpression != null) npcPortrait.sprite = sadExpression;
                break;
            case "angry":
                if (angryExpression != null) npcPortrait.sprite = angryExpression;
                break;
            default:
                if (neutralExpression != null) npcPortrait.sprite = neutralExpression;
                break;
        }
    }
    #endregion

    #region Player Input Handling
    private void OnSendButtonClicked()
    {
        if (currentNPC == null || playerInputField == null)
            return;

        string playerInput = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(playerInput))
            return;

        ProcessPlayerInput(playerInput);

        playerInputField.text = "";
        playerInputField.ActivateInputField();
        playerInputField.Select();
    }

    private void ProcessPlayerInput(string input)
    {
        if (currentNPC == null) return;

        var geminiAccessor = currentNPC.GetComponent<GeminiAccessor>();
        if (geminiAccessor != null)
        {
            geminiAccessor.SendPlayerInput(input);
        }
        else
        {
            Debug.LogWarning("NPC missing GeminiAccessor component!");
        }
    }

    private IEnumerator FocusInputFieldRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        if (playerInputField != null)
        {
            playerInputField.text = "";

            EventSystem.current.SetSelectedGameObject(playerInputField.gameObject);

            for (int i = 0; i < 3; i++)
            {
                playerInputField.Select();
                playerInputField.ActivateInputField();
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
    #endregion
}
