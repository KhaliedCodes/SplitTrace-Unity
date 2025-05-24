using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
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
    [SerializeField] private TextMeshProUGUI chatHistoryText;
    [SerializeField] private ScrollRect scrollRect;
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

    private PlayerController playerController;
    private NPCController currentNPC;
    private Coroutine typingCoroutine;

    private bool isDialogueActive;
    private readonly string[] validEmotions = { "neutral", "happy", "sad", "angry" };

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
        sendButton?.onClick.AddListener(OnSendButtonClicked);
        endConversationButton?.onClick.AddListener(EndDialogue);
        if (npcPortrait != null) npcPortrait.sprite = neutralExpression;
        FindPlayerController();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            OnSendButtonClicked();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndDialogue();
        }
    }
    #endregion

    #region Initialization
    private void FindPlayerController()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
            Debug.LogWarning("DialogueManager couldn't find PlayerController!");
    }
    #endregion

    #region Dialogue Control
    public void StartDialogue(NPCController npc, string initialMessage)
    {
        // Null-check critical components
        if (npcNameText == null)
        {
            Debug.LogError("npcNameText is not assigned in DialogueManager!");
            return;
        }

        if (npc == null)
        {
            Debug.LogError("NPC parameter is null in StartDialogue!");
            return;
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("dialoguePanel is not assigned!");
            return;
        }

        dialoguePanel.SetActive(true);
        currentNPC = npc;
        npcNameText.text = npc.NPCName;
        DisplayNPCResponse(initialMessage);
        npcPortrait.sprite = neutralExpression;
        playerInputField.ActivateInputField();
        isDialogueActive = true;
        DisplayNPCDialogue(initialMessage);

        if (playerController != null)
            playerController.SetInDialogue(true);
    }

    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentNPC?.EndInteraction();
        currentNPC = null;

        chatHistoryText.text = "";
        dialogueText.text = ""; // Clear main dialogue text
        playerInputField.text = "";
        playerInputField.DeactivateInputField(); // Ensure it's not active when closing
        EventSystem.current.SetSelectedGameObject(null); // Remove selection

        isDialogueActive = false;

        if (playerController != null)
            playerController.SetInDialogue(false);
    }

    public void DisplayNPCResponse(string message)
    {

        if (currentNPC != null && chatHistoryText != null)
        {
            
            AppendToChatHistory(currentNPC.NPCName, message);

        }
    }

    private void AppendToChatHistory(string speaker, string message)
    {


        chatHistoryText.text += $"\n<b>{speaker}:</b> {message}";
        Canvas.ForceUpdateCanvases();
    }
    #endregion

    #region Response Processing
    public void DisplayNPCDialogue(string dialogue)
    {
        if (string.IsNullOrEmpty(dialogue)) return;

        var emotionMatch = Regex.Match(dialogue, @"{\s*""emotion""\s*:\s*""(\w+)""\s*}");
        string emotion = "neutral";
        string displayText = dialogue;

        if (emotionMatch.Success)
        {
            emotion = emotionMatch.Groups[1].Value.ToLower();
            displayText = Regex.Replace(dialogue, @"{\s*""emotion""\s*:\s*""\w+""\s*}", "").Trim();
        }

        if (!validEmotions.Contains(emotion)) emotion = "neutral";
        UpdateNPCExpression(emotion);

        if (useTypewriterEffect)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeDialogue(displayText));
        }
        else
        {
            dialogueText.text = displayText;
        }

        DisplayNPCResponse(displayText);
        StartCoroutine(FocusInputFieldRoutine());
    }

    private void UpdateNPCExpression(string emotion)
    {
        if (!validEmotions.Contains(emotion)) emotion = "neutral";
        if (npcPortrait == null) return;

        switch (emotion)
        {
            case "happy": npcPortrait.sprite = happyExpression; break;
            case "sad": npcPortrait.sprite = sadExpression; break;
            case "angry": npcPortrait.sprite = angryExpression; break;
            default: npcPortrait.sprite = neutralExpression; break;
        }
    }

    private IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            if (typingSoundEffect != null)
                typingSoundEffect.Play();
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    #endregion

    #region Input Handling
    private void OnSendButtonClicked()
    {
        if (currentNPC == null || playerInputField == null) return;

        string input = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(input)) return;

        ProcessPlayerInput(input);
        playerInputField.text = "";
        playerInputField.ActivateInputField();
    }

    private void ProcessPlayerInput(string input)
    {
        AppendToChatHistory("You", input);
        currentNPC?.SendPlayerMessage(input);
    }

    private IEnumerator FocusInputFieldRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        playerInputField.text = "";
        EventSystem.current.SetSelectedGameObject(playerInputField.gameObject);
        playerInputField.ActivateInputField();
    }
    #endregion
}