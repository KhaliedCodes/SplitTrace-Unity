using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI chatHistoryText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Image npcPortrait;
    [SerializeField] private Button endConversationButton;

    [Header("Choice System")]
    [SerializeField] private Transform choiceButtonParent;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private int maxChoices = 4;

    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioSource typingSoundEffect;
    [SerializeField] private bool useTypewriterEffect = true;
    private PlayerController playerController;
    private NPCController currentNPC;
    private Coroutine typingCoroutine;
    private List<Button> choiceButtons = new List<Button>();
    private List<string> currentChoices = new List<string>();

    private bool isDialogueActive;
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        dialoguePanel?.SetActive(false);
        endConversationButton?.onClick.AddListener(EndDialogue);
        FindPlayerController();
        InitializeChoiceButtons();
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndDialogue();
        }

        HandleNumberKeySelection();
    }

    private void FindPlayerController()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
            Debug.LogWarning("PlayerController not found in scene!");
    }

    private void InitializeChoiceButtons()
{
    if (choiceButtonPrefab == null || choiceButtonParent == null)
    {
        Debug.LogWarning("Choice button prefab or parent not assigned!");
        return;
    }

    // Configure the parent layout group for better button arrangement
    SetupChoiceButtonParentLayout();

    // Clear existing buttons
    foreach (Transform child in choiceButtonParent)
    {
        if (Application.isPlaying)
            Destroy(child.gameObject);
        else
            DestroyImmediate(child.gameObject);
    }
    choiceButtons.Clear();

    // Create new buttons and store them in the list
    for (int i = 0; i < maxChoices; i++)
    {
        GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
        buttonObj.name = $"ChoiceButton_{i}";

        // Configure button for dynamic scaling
        SetupDynamicScalingButton(buttonObj, i);

        // Initially hide the button
        buttonObj.SetActive(false);
    }
}

private void SetupChoiceButtonParentLayout()
{
    // Add Vertical Layout Group to parent for better button arrangement
    VerticalLayoutGroup layoutGroup = choiceButtonParent.GetComponent<VerticalLayoutGroup>();
    if (layoutGroup == null)
    {
        layoutGroup = choiceButtonParent.gameObject.AddComponent<VerticalLayoutGroup>();
    }
    
    // Configure layout group settings
    layoutGroup.childAlignment = TextAnchor.UpperLeft;
    layoutGroup.childControlWidth = true;   // Force uniform width
    layoutGroup.childControlHeight = true;  // Force uniform height
    layoutGroup.childForceExpandWidth = true;  // Make buttons expand to fill width
    layoutGroup.childForceExpandHeight = false;
    layoutGroup.spacing = 0; // Space between buttons
    layoutGroup.padding = new RectOffset(5, 5, 2, 2); // Left, Right, Top, Bottom padding

    // Add Content Size Fitter to parent if needed
    ContentSizeFitter parentSizeFitter = choiceButtonParent.GetComponent<ContentSizeFitter>();
    if (parentSizeFitter == null)
    {
        parentSizeFitter = choiceButtonParent.gameObject.AddComponent<ContentSizeFitter>();
    }
    parentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    parentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    
    // Ensure the parent RectTransform is set to stretch horizontally
    RectTransform parentRect = choiceButtonParent.GetComponent<RectTransform>();
    if (parentRect != null)
    {
        parentRect.anchorMin = new Vector2(0, parentRect.anchorMin.y);
        parentRect.anchorMax = new Vector2(1, parentRect.anchorMax.y);
        parentRect.offsetMin = new Vector2(0, parentRect.offsetMin.y);
        parentRect.offsetMax = new Vector2(0, parentRect.offsetMax.y);
    }
}

private void SetupDynamicScalingButton(GameObject buttonObj, int index)
{
    // Get or add the Button component
    Button buttonComponent = buttonObj.GetComponent<Button>();
    if (buttonComponent == null)
    {
        Debug.LogWarning($"Button component missing on {buttonObj.name}, adding one automatically");
        buttonComponent = buttonObj.AddComponent<Button>();
        
        // If there's an Image component, set it as the button's target graphic
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonComponent.targetGraphic = buttonImage;
        }
    }

    // Remove conflicting Content Size Fitter since we're using Layout Group control
    ContentSizeFitter sizeFitter = buttonObj.GetComponent<ContentSizeFitter>();
    if (sizeFitter != null)
    {
        DestroyImmediate(sizeFitter);
    }

    // Set up RectTransform to stretch horizontally
    RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
    if (buttonRect != null)
    {
        buttonRect.anchorMin = new Vector2(0, 0.5f);
        buttonRect.anchorMax = new Vector2(1, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
    }

    // Add Layout Element for height control while allowing width to expand
    LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
    if (layoutElement == null)
    {
        layoutElement = buttonObj.AddComponent<LayoutElement>();
    }
    // Remove fixed width constraints to let buttons stretch
    layoutElement.minWidth = -1; // No minimum width constraint
    layoutElement.preferredWidth = -1; // No preferred width constraint
    layoutElement.flexibleWidth = 1; // Allow flexible width expansion
    layoutElement.minHeight = 45f; // Set consistent height
    layoutElement.preferredHeight = 45f;

    // Configure text component
    TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
    if (buttonText != null)
    {
        // Text settings for better display
        buttonText.textWrappingMode = TextWrappingModes.NoWrap;
        buttonText.overflowMode = TextOverflowModes.Ellipsis;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 16f;
        buttonText.fontSizeMax = 20f;
        buttonText.alignment = TextAlignmentOptions.Left;

        // Outline settings
        buttonText.fontMaterial.EnableKeyword("OUTLINE_ON");
        buttonText.outlineColor = new Color32(255, 0, 0, 255); 
        buttonText.outlineWidth = 0.01f;

        // Ensure text fills the button properly
        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(15, 5); 
            textRect.offsetMax = new Vector2(-15, -5);
        }

        TMPButtonTextColorChanger colorChanger = buttonObj.GetComponent<TMPButtonTextColorChanger>();
        if (colorChanger == null)
        {
            colorChanger = buttonObj.AddComponent<TMPButtonTextColorChanger>();
        }
        colorChanger.text = buttonText;
    }
    else
    {
        Debug.LogError($"Choice button prefab is missing TextMeshProUGUI component! Button: {buttonObj.name}");
    }

    choiceButtons.Add(buttonComponent);

    
    int choiceIndex = index; 
    buttonComponent.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
}

    public void StartDialogue(NPCController npc, string initialMessage)
    {
        if (npcNameText == null || npc == null || dialoguePanel == null) return;

        dialoguePanel.SetActive(true);
        currentNPC = npc;
        npcNameText.text = npc.NPCName;
        isDialogueActive = true;

        chatHistoryText.text = "";
        dialogueText.text = "";
        HideAllChoices();

        DisplayNPCDialogue(initialMessage);
        playerController?.SetInDialogue(true);
    }

    public void EndDialogue()
{
   
    if (!isDialogueActive) return;
    
    dialoguePanel.SetActive(false);
    
   
    currentNPC?.OnDialogueEnded(); 
    currentNPC = null;
    
    isDialogueActive = false;
    chatHistoryText.text = "";
    dialogueText.text = "";
    HideAllChoices();
    
    playerController?.SetInDialogue(false);
    playerController?.EnableControls();
}

    public void DisplayNPCResponse(string message)
    {
        if (currentNPC != null && chatHistoryText != null)
            AppendToChatHistory(currentNPC.NPCName, message);
    }

    private void AppendToChatHistory(string speaker, string message)
    {
        chatHistoryText.text += $"\n<b>{speaker}:</b> {message}";
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void DisplayNPCDialogue(string dialogue)
    {
        if (string.IsNullOrEmpty(dialogue)) return;

        var (emotion, displayText) = EmotionParser.Parse(dialogue);


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
        StartCoroutine(RequestChoicesAfterDelay());
    }

    private IEnumerator RequestChoicesAfterDelay()
    {
        if (useTypewriterEffect && typingCoroutine != null)
            yield return typingCoroutine;

        yield return new WaitForSeconds(0.5f);
        currentNPC?.RequestDialogueChoices();
    }
    private IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = "";
        foreach (char c in text)
        {
            dialogueText.text += c;
            if (typingSoundEffect != null)
            {
                typingSoundEffect?.Play();
            }
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void DisplayChoices(List<string> choices)
    {
        currentChoices = choices ?? new List<string>();
        HideAllChoices();

        for (int i = 0; i < Mathf.Min(choices.Count, choiceButtons.Count); i++)
        {
            if (string.IsNullOrEmpty(choices[i])) continue;
            
            choiceButtons[i].gameObject.SetActive(true);
            TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = $"{i + 1}. {choices[i]}";
        }
    }

    private void HideAllChoices()
    {
        foreach (Button button in choiceButtons)
            button?.gameObject.SetActive(false);
    }

    private void HandleNumberKeySelection()
    {
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (!choiceButtons[i].gameObject.activeSelf) continue;
            
            if ((i == 0 && Keyboard.current.digit1Key.wasPressedThisFrame) ||
                (i == 1 && Keyboard.current.digit2Key.wasPressedThisFrame) ||
                (i == 2 && Keyboard.current.digit3Key.wasPressedThisFrame) ||
                (i == 3 && Keyboard.current.digit4Key.wasPressedThisFrame))
            {
                OnChoiceSelected(i);
                break;
            }
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count) return;
        
        AppendToChatHistory("You", currentChoices[choiceIndex]);
        HideAllChoices();
        currentNPC?.SendPlayerChoice(currentChoices[choiceIndex], choiceIndex);
    }
}