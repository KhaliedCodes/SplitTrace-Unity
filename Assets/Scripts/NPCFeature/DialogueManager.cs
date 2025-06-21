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
    [SerializeField] private float typingSpeed = 0.1f;
    [SerializeField] private AudioSource typingSoundEffect;
    [SerializeField] private bool useTypewriterEffect = true;
    
    [Header("Text Styling")]
    [SerializeField] private DialogueTextStyle npcTextStyle;
    [SerializeField] private DialogueTextStyle playerTextStyle;
    [SerializeField] private DialogueTextStyle narrativeTextStyle;
    [SerializeField] private bool useGradientText = true;
    [SerializeField] private bool useTextAnimations = true;
    [SerializeField] private float textPulseSpeed = 2f;
    [SerializeField] private float textWaveAmplitude = 5f;
    [SerializeField] private float textWaveFrequency = 3f;
    
    [Header("Choice Button Text Styling")]
    [SerializeField] private DialogueTextStyle choiceButtonTextStyle;
    [SerializeField] private DialogueTextStyle choiceButtonHoverStyle;
    [SerializeField] private bool useChoiceButtonAnimations = false;
    
    private PlayerController playerController;
    private NPCController currentNPC;
    private Coroutine typingCoroutine;
    private List<Button> choiceButtons = new List<Button>();
    private List<string> currentChoices = new List<string>();

    private bool isDialogueActive;
    public static DialogueManager Instance { get; private set; }

    [System.Serializable]
    public class DialogueTextStyle
    {
        [Header("Colors")]
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.gray;
        public bool useGradient = false;
        public Gradient colorGradient;
        
        [Header("Outline")]
        public bool useOutline = true;
        public Color outlineColor = Color.black;
        public float outlineWidth = 0.2f;
        
        [Header("Shadow")]
        public bool useShadow = false;
        public Color shadowColor = Color.black;
        public Vector2 shadowOffset = new Vector2(2, -2);
        
        [Header("Glow")]
        public bool useGlow = false;
        public Color glowColor = Color.white;
        public float glowIntensity = 0.5f;
        
        [Header("Font")]
        public TMP_FontAsset customFont;
        public float fontSize = 18f;
        public FontStyles fontStyle = FontStyles.Normal;
        
        [Header("Spacing")]
        public float characterSpacing = 0f;
        public float lineSpacing = 0f;
        public float wordSpacing = 0f;
        
        [Header("Animation")]
        public bool enableTextAnimation = false;
        public TextAnimationType animationType = TextAnimationType.None;
        public float animationSpeed = 1f;
        public float animationIntensity = 1f;
    }

    public enum TextAnimationType
    {
        None,
        Wave,
        Bounce,
        Shake,
        Pulse,
        Typewriter
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
            
        InitializeDefaultStyles();
    }

    private void InitializeDefaultStyles()
    {
        // Initialize default styles if not set
        if (npcTextStyle == null)
        {
            npcTextStyle = new DialogueTextStyle();
            npcTextStyle.primaryColor = new Color(0.8f, 0.9f, 1f); // Light blue
            npcTextStyle.useOutline = true;
            npcTextStyle.outlineColor = new Color(0.2f, 0.2f, 0.4f);
        }
        
        if (playerTextStyle == null)
        {
            playerTextStyle = new DialogueTextStyle();
            playerTextStyle.primaryColor = new Color(0.9f, 0.9f, 0.8f); // Warm white
            playerTextStyle.useOutline = true;
            playerTextStyle.outlineColor = new Color(0.3f, 0.2f, 0.1f);
        }
        
        if (narrativeTextStyle == null)
        {
            narrativeTextStyle = new DialogueTextStyle();
            narrativeTextStyle.primaryColor = new Color(0.7f, 0.7f, 0.7f); // Gray
            narrativeTextStyle.fontStyle = FontStyles.Italic;
        }
        
        // Initialize choice button text styles
        if (choiceButtonTextStyle == null)
        {
            choiceButtonTextStyle = new DialogueTextStyle();
            choiceButtonTextStyle.primaryColor = new Color(1f, 0.9f, 0.7f); // Light orange
            choiceButtonTextStyle.useOutline = true;
            choiceButtonTextStyle.outlineColor = new Color(0.4f, 0.2f, 0.1f); // Dark brown outline
            choiceButtonTextStyle.fontSize = 16f;
            choiceButtonTextStyle.fontStyle = FontStyles.Normal;
        }
        
        if (choiceButtonHoverStyle == null)
        {
            choiceButtonHoverStyle = new DialogueTextStyle();
            choiceButtonHoverStyle.primaryColor = new Color(1f, 1f, 0.8f); // Bright yellow
            choiceButtonHoverStyle.useOutline = true;
            choiceButtonHoverStyle.outlineColor = new Color(0.6f, 0.4f, 0.1f); // Golden outline
            choiceButtonHoverStyle.fontSize = 18f;
            choiceButtonHoverStyle.fontStyle = FontStyles.Bold;
            choiceButtonHoverStyle.useGlow = true;
            choiceButtonHoverStyle.glowColor = new Color(1f, 1f, 0.6f);
        }
    }

    private void Start()
    {
        dialoguePanel?.SetActive(false);
        endConversationButton?.onClick.AddListener(EndDialogue);
        FindPlayerController();
        InitializeChoiceButtons();
        
        // Apply initial styling to text components
        ApplyTextStyling(dialogueText, npcTextStyle);
        ApplyTextStyling(npcNameText, npcTextStyle);
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndDialogue();
        }

        HandleNumberKeySelection();
        
        // Update text animations
        if (useTextAnimations)
        {
            UpdateTextAnimations();
        }
        
        // Update choice button animations
        if (useChoiceButtonAnimations)
        {
            UpdateChoiceButtonAnimations();
        }
    }

    private void UpdateTextAnimations()
    {
        if (dialogueText != null && npcTextStyle.enableTextAnimation)
        {
            ApplyTextAnimation(dialogueText, npcTextStyle);
        }
    }

    private void UpdateChoiceButtonAnimations()
    {
        if (choiceButtonTextStyle.enableTextAnimation)
        {
            foreach (Button button in choiceButtons)
            {
                if (button.gameObject.activeSelf)
                {
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        ApplyTextAnimation(buttonText, choiceButtonTextStyle);
                    }
                }
            }
        }
    }

    private void ApplyTextStyling(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
        if (textComponent == null || style == null) return;

        // Apply font
        if (style.customFont != null)
            textComponent.font = style.customFont;

        // Apply basic styling
        textComponent.fontSize = style.fontSize;
        textComponent.fontStyle = style.fontStyle;
        textComponent.characterSpacing = style.characterSpacing;
        textComponent.lineSpacing = style.lineSpacing;
        textComponent.wordSpacing = style.wordSpacing;

        // Apply colors
        if (style.useGradient && style.colorGradient != null)
        {
            textComponent.enableVertexGradient = true;
            var gradient = style.colorGradient;
            textComponent.colorGradient = new VertexGradient(
                gradient.Evaluate(1f), gradient.Evaluate(1f),
                gradient.Evaluate(0f), gradient.Evaluate(0f)
            );
        }
        else
        {
            textComponent.color = style.primaryColor;
            textComponent.enableVertexGradient = false;
        }

        // Apply outline
        if (style.useOutline)
        {
            textComponent.fontMaterial.EnableKeyword("OUTLINE_ON");
            textComponent.outlineColor = style.outlineColor;
            textComponent.outlineWidth = style.outlineWidth;
        }
        else
        {
            textComponent.fontMaterial.DisableKeyword("OUTLINE_ON");
        }

        // Apply shadow
        if (style.useShadow)
        {
            textComponent.fontMaterial.EnableKeyword("UNDERLAY_ON");
            // Note: Shadow implementation may require custom shader or material setup
        }

        // Apply glow
        if (style.useGlow)
        {
            textComponent.fontMaterial.EnableKeyword("GLOW_ON");
            // Note: Glow implementation may require custom shader or material setup
        }
    }

    private void ApplyTextAnimation(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
        if (!style.enableTextAnimation || style.animationType == TextAnimationType.None)
            return;

        textComponent.ForceMeshUpdate();
        var textInfo = textComponent.textInfo;
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            var vertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            
            for (int j = 0; j < 4; j++)
            {
                var originalVertex = vertices[charInfo.vertexIndex + j];
                
                switch (style.animationType)
                {
                    case TextAnimationType.Wave:
                        originalVertex.y += Mathf.Sin(Time.time * style.animationSpeed + i * 0.1f) * style.animationIntensity;
                        break;
                        
                    case TextAnimationType.Bounce:
                        originalVertex.y += Mathf.Abs(Mathf.Sin(Time.time * style.animationSpeed + i * 0.2f)) * style.animationIntensity;
                        break;
                        
                    case TextAnimationType.Shake:
                        originalVertex.x += Random.Range(-style.animationIntensity, style.animationIntensity);
                        originalVertex.y += Random.Range(-style.animationIntensity, style.animationIntensity);
                        break;
                        
                    case TextAnimationType.Pulse:
                        float scale = 1f + Mathf.Sin(Time.time * style.animationSpeed) * style.animationIntensity * 0.1f;
                        originalVertex = Vector3.Scale(originalVertex, Vector3.one * scale);
                        break;
                }
                
                vertices[charInfo.vertexIndex + j] = originalVertex;
            }
        }
        
        textComponent.UpdateVertexData();
    }

    public void SetDialogueTextStyle(DialogueTextStyle style, bool isPlayer = false)
    {
        if (isPlayer)
            playerTextStyle = style;
        else
            npcTextStyle = style;
            
        ApplyTextStyling(dialogueText, isPlayer ? playerTextStyle : npcTextStyle);
    }

    public void DisplayStyledDialogue(string dialogue, DialogueTextStyle customStyle = null)
    {
        if (string.IsNullOrEmpty(dialogue)) return;

        var (emotion, displayText) = EmotionParser.Parse(dialogue);
        
        // Apply custom style if provided
        if (customStyle != null)
        {
            ApplyTextStyling(dialogueText, customStyle);
        }

        // Add rich text formatting based on emotion
        displayText = FormatTextByEmotion(displayText, emotion);

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

    private string FormatTextByEmotion(string text, string emotion)
    {
        switch (emotion?.ToLower())
        {
            case "angry":
                return $"<color=#FF4444><b>{text}</b></color>";
            case "happy":
                return $"<color=#44FF44>{text}</color>";
            case "sad":
                return $"<color=#4444FF><i>{text}</i></color>";
            case "surprised":
                return $"<size=120%>{text}</size>";
            case "whisper":
                return $"<size=80%><alpha=#AA>{text}</alpha></size>";
            case "shout":
                return $"<size=140%><b>{text.ToUpper()}</b></size>";
            default:
                return text;
        }
    }

    // Create preset styles for different dialogue types
    public DialogueTextStyle CreatePresetStyle(string presetName)
    {
        var style = new DialogueTextStyle();
        
        switch (presetName.ToLower())
        {
            case "mysterious":
                style.primaryColor = new Color(0.6f, 0.4f, 0.8f);
                style.useGlow = true;
                style.glowColor = new Color(0.8f, 0.6f, 1f);
                style.enableTextAnimation = true;
                style.animationType = TextAnimationType.Wave;
                break;
                
            case "heroic":
                style.primaryColor = new Color(1f, 0.8f, 0.2f);
                style.useOutline = true;
                style.outlineColor = new Color(0.8f, 0.6f, 0f);
                style.fontStyle = FontStyles.Bold;
                break;
                
            case "villain":
                style.primaryColor = new Color(0.8f, 0.2f, 0.2f);
                style.useOutline = true;
                style.outlineColor = Color.black;
                style.enableTextAnimation = true;
                style.animationType = TextAnimationType.Shake;
                break;
                
            case "narrator":
                style.primaryColor = new Color(0.7f, 0.7f, 0.7f);
                style.fontStyle = FontStyles.Italic;
                style.characterSpacing = 2f;
                break;
        }
        
        return style;
    }

    // Create preset styles specifically for choice buttons
    public DialogueTextStyle CreateChoiceButtonPresetStyle(string presetName)
    {
        var style = new DialogueTextStyle();
        
        switch (presetName.ToLower())
        {
            case "elegant":
                style.primaryColor = new Color(0.9f, 0.8f, 0.6f); // Elegant gold
                style.useOutline = true;
                style.outlineColor = new Color(0.3f, 0.2f, 0.1f);
                style.fontStyle = FontStyles.Italic;
                style.characterSpacing = 1f;
                break;
                
            case "modern":
                style.primaryColor = new Color(0.2f, 0.8f, 1f); // Modern cyan
                style.useOutline = false;
                style.useShadow = true;
                style.shadowColor = new Color(0f, 0f, 0f, 0.5f);
                style.fontStyle = FontStyles.Normal;
                break;
                
            case "fantasy":
                style.primaryColor = new Color(0.8f, 0.6f, 1f); // Fantasy purple
                style.useGlow = true;
                style.glowColor = new Color(1f, 0.8f, 1f);
                style.enableTextAnimation = true;
                style.animationType = TextAnimationType.Pulse;
                break;
                
            case "military":
                style.primaryColor = new Color(0.6f, 0.8f, 0.4f); // Military green
                style.fontStyle = FontStyles.Bold;
                style.useOutline = true;
                style.outlineColor = new Color(0.2f, 0.3f, 0.1f);
                style.characterSpacing = 2f;
                break;
                
            case "retro":
                style.primaryColor = new Color(1f, 0.4f, 0.6f); // Retro pink
                style.useOutline = true;
                style.outlineColor = new Color(0.8f, 0.2f, 0.4f);
                style.enableTextAnimation = true;
                style.animationType = TextAnimationType.Wave;
                break;
        }
        
        return style;
    }

    // Method to apply choice button styling
    public void SetChoiceButtonTextStyle(DialogueTextStyle style)
    {
        choiceButtonTextStyle = style;
        
        // Apply to all existing choice buttons
        foreach (Button button in choiceButtons)
        {
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                ApplyTextStyling(buttonText, choiceButtonTextStyle);
            }
        }
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

        SetupChoiceButtonParentLayout();

        foreach (Transform child in choiceButtonParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        choiceButtons.Clear();

        for (int i = 0; i < maxChoices; i++)
        {
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceButtonParent);
            buttonObj.name = $"ChoiceButton_{i}";

            SetupDynamicScalingButton(buttonObj, i);
            buttonObj.SetActive(false);
        }
    }

    private void SetupChoiceButtonParentLayout()
    {
        VerticalLayoutGroup layoutGroup = choiceButtonParent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = choiceButtonParent.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 0;
        layoutGroup.padding = new RectOffset(200, 5, 2, 2);

        ContentSizeFitter parentSizeFitter = choiceButtonParent.GetComponent<ContentSizeFitter>();
        if (parentSizeFitter == null)
        {
            parentSizeFitter = choiceButtonParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        parentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        parentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
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
        Button buttonComponent = buttonObj.GetComponent<Button>();
        if (buttonComponent == null)
        {
            Debug.LogWarning($"Button component missing on {buttonObj.name}, adding one automatically");
            buttonComponent = buttonObj.AddComponent<Button>();
            
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonComponent.targetGraphic = buttonImage;
            }
        }

        ContentSizeFitter sizeFitter = buttonObj.GetComponent<ContentSizeFitter>();
        if (sizeFitter != null)
        {
            DestroyImmediate(sizeFitter);
        }

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0, 0.5f);
            buttonRect.anchorMax = new Vector2(1, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
        }

        LayoutElement layoutElement = buttonObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = buttonObj.AddComponent<LayoutElement>();
        }
        layoutElement.minWidth = -1;
        layoutElement.preferredWidth = -1;
        layoutElement.flexibleWidth = 1;
        layoutElement.minHeight = 45f;
        layoutElement.preferredHeight = 45f;

        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.textWrappingMode = TextWrappingModes.NoWrap;
            buttonText.overflowMode = TextOverflowModes.Ellipsis;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = 14f;  // Adjusted for button-specific styling
            buttonText.fontSizeMax = 18f;  // Adjusted for button-specific styling
            buttonText.alignment = TextAlignmentOptions.Left;

            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(15, 5); 
                textRect.offsetMax = new Vector2(-15, -5);
            }

            // Apply choice button specific text styling instead of player style
            ApplyTextStyling(buttonText, choiceButtonTextStyle);

            // Add hover effect component for choice buttons
            ChoiceButtonHoverEffect hoverEffect = buttonObj.GetComponent<ChoiceButtonHoverEffect>();
            if (hoverEffect == null)
            {
                hoverEffect = buttonObj.AddComponent<ChoiceButtonHoverEffect>();
            }
            hoverEffect.Initialize(buttonText, choiceButtonTextStyle, choiceButtonHoverStyle);

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
        if (npcNameText == null || npc == null || dialoguePanel == null) 
        {
            Debug.LogError("DialogueManager: Missing required components or NPC is null!");
            return;
        }

        if (playerController != null)
        {
            NPCController expectedNPC = playerController.GetCurrentInteractable();
            if (expectedNPC != null && expectedNPC != npc)
            {
                Debug.LogWarning($"DialogueManager: Mismatch! Expected {expectedNPC.NPCName} but got {npc.NPCName}");
                npc = expectedNPC;
            }
        }

        Debug.Log($"DialogueManager: Starting dialogue with {npc.NPCName}");
        
        dialoguePanel.SetActive(true);
        currentNPC = npc;
        npcNameText.text = npc.NPCName;
        isDialogueActive = true;

        chatHistoryText.text = "";
        dialogueText.text = "";
        HideAllChoices();

        // Apply NPC-specific styling
        ApplyTextStyling(dialogueText, npcTextStyle);
        ApplyTextStyling(npcNameText, npcTextStyle);

        DisplayStyledDialogue(initialMessage);
        playerController?.SetInDialogue(true);
    }

    public void EndDialogue()
    {
        if (!isDialogueActive) return;
        
        Debug.Log($"DialogueManager: Ending dialogue with {currentNPC?.NPCName ?? "unknown NPC"}");
        
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
        // Apply different styling for different speakers
        string styledMessage = speaker == "You" ? 
            $"\n<color={ColorUtility.ToHtmlStringRGBA(playerTextStyle.primaryColor)}><b>{speaker}:</b></color> {message}" :
            $"\n<color={ColorUtility.ToHtmlStringRGBA(npcTextStyle.primaryColor)}><b>{speaker}:</b></color> {message}";
            
        chatHistoryText.text += styledMessage;
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }

    public void DisplayNPCDialogue(string dialogue)
    {
        DisplayStyledDialogue(dialogue);
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
            {
                buttonText.text = $"{i + 1}. {choices[i]}";
                // Apply choice button specific text styling
                ApplyTextStyling(buttonText, choiceButtonTextStyle);
            }
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

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }
}

// Separate component for handling choice button hover effects
public class ChoiceButtonHoverEffect : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
{
    private TextMeshProUGUI buttonText;
    private DialogueManager.DialogueTextStyle normalStyle;
    private DialogueManager.DialogueTextStyle hoverStyle;
    private bool isHovering = false;

    public void Initialize(TextMeshProUGUI text, DialogueManager.DialogueTextStyle normal, DialogueManager.DialogueTextStyle hover)
    {
        buttonText = text;
        normalStyle = normal;
        hoverStyle = hover;
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (buttonText != null && hoverStyle != null)
        {
            isHovering = true;
            ApplyHoverStyle();
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (buttonText != null && normalStyle != null)
        {
            isHovering = false;
            ApplyNormalStyle();
        }
    }

    private void ApplyHoverStyle()
    {
        if (DialogueManager.Instance != null)
        {
            // Use the DialogueManager's ApplyTextStyling method through reflection or make it public
            ApplyTextStyling(buttonText, hoverStyle);
        }
    }

    private void ApplyNormalStyle()
    {
        if (DialogueManager.Instance != null)
        {
            ApplyTextStyling(buttonText, normalStyle);
        }
    }

    // Simplified version of ApplyTextStyling for the hover effect
    private void ApplyTextStyling(TextMeshProUGUI textComponent, DialogueManager.DialogueTextStyle style)
    {
        if (textComponent == null || style == null) return;

        // Apply font
        if (style.customFont != null)
            textComponent.font = style.customFont;

        // Apply basic styling
        textComponent.fontSize = style.fontSize;
        textComponent.fontStyle = style.fontStyle;
        textComponent.characterSpacing = style.characterSpacing;
        textComponent.lineSpacing = style.lineSpacing;
        textComponent.wordSpacing = style.wordSpacing;

        // Apply colors
        if (style.useGradient && style.colorGradient != null)
        {
            textComponent.enableVertexGradient = true;
            var gradient = style.colorGradient;
            textComponent.colorGradient = new VertexGradient(
                gradient.Evaluate(1f), gradient.Evaluate(1f),
                gradient.Evaluate(0f), gradient.Evaluate(0f)
            );
        }
        else
        {
            textComponent.color = style.primaryColor;
            textComponent.enableVertexGradient = false;
        }

        // Apply outline
        if (style.useOutline)
        {
            textComponent.fontMaterial.EnableKeyword("OUTLINE_ON");
            textComponent.outlineColor = style.outlineColor;
            textComponent.outlineWidth = style.outlineWidth;
        }
        else
        {
            textComponent.fontMaterial.DisableKeyword("OUTLINE_ON");
        }
    }
}