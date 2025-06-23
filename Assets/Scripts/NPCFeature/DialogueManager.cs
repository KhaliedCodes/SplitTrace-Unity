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
    [SerializeField] private float choiceButtonFontSizeMin = 22f;
    [SerializeField] private float choiceButtonFontSizeMax = 24f;
    [SerializeField] private bool useChoiceButtonAnimations = false;
    [Header("Choice Button Animation Settings")]
    [SerializeField] private bool enableChoiceButtonTextAnimation = false;
    [SerializeField] private TextAnimationType choiceButtonAnimationType = TextAnimationType.Pulse;
    [SerializeField] private float choiceButtonAnimationSpeed = 2f; 
    [SerializeField] private float choiceButtonAnimationIntensity = 0.5f;

    
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
        public float fontSize = 22f;
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
        // Create default styles using a helper method to reduce duplication
        npcTextStyle = npcTextStyle ?? CreateDefaultTextStyle(new Color(0.8f, 0.9f, 1f), new Color(0.2f, 0.2f, 0.4f));
        playerTextStyle = playerTextStyle ?? CreateDefaultTextStyle(new Color(0.9f, 0.9f, 0.8f), new Color(0.3f, 0.2f, 0.1f));
        narrativeTextStyle = narrativeTextStyle ?? CreateDefaultTextStyle(new Color(0.7f, 0.7f, 0.7f), Color.black, FontStyles.Italic);
        choiceButtonTextStyle = choiceButtonTextStyle ?? CreateChoiceButtonDefaultStyle();
        choiceButtonHoverStyle = choiceButtonHoverStyle ?? CreateChoiceButtonHoverStyle();
    }

    // Helper method to reduce code duplication in style creation
    private DialogueTextStyle CreateDefaultTextStyle(Color primaryColor, Color outlineColor, FontStyles fontStyle = FontStyles.Normal)
    {
        return new DialogueTextStyle
        {
            primaryColor = primaryColor,
            useOutline = true,
            outlineColor = outlineColor,
            fontStyle = fontStyle
        };
    }

    private DialogueTextStyle CreateChoiceButtonDefaultStyle()
    {
        return new DialogueTextStyle
        {
            primaryColor = new Color(1f, 0.9f, 0.7f),
            useOutline = true,
            outlineColor = new Color(0.4f, 0.2f, 0.1f),
            fontSize = 22f,
            fontStyle = FontStyles.Normal,
            enableTextAnimation = false,
            animationType = TextAnimationType.Pulse,
            animationSpeed = 2f,
            animationIntensity = 0.5f
        };
    }

    private DialogueTextStyle CreateChoiceButtonHoverStyle()
    {
        return new DialogueTextStyle
        {
            primaryColor = new Color(1f, 1f, 0.8f),
            useOutline = true,
            outlineColor = new Color(0.6f, 0.4f, 0.1f),
            fontSize = 24f,
            fontStyle = FontStyles.Bold,
            useGlow = true,
            glowColor = new Color(1f, 1f, 0.6f)
        };
    }

    private void ApplyAnimationSettingsToChoiceButtons()
    {
        if (choiceButtonTextStyle != null)
        {
            choiceButtonTextStyle.enableTextAnimation = enableChoiceButtonTextAnimation;
            choiceButtonTextStyle.animationType = choiceButtonAnimationType;
            choiceButtonTextStyle.animationSpeed = choiceButtonAnimationSpeed;
            choiceButtonTextStyle.animationIntensity = choiceButtonAnimationIntensity;
        }
    }

    private void OnValidate()
    {
        // Font size validation
        choiceButtonFontSizeMax = Mathf.Max(choiceButtonFontSizeMax, choiceButtonFontSizeMin);
        choiceButtonFontSizeMin = Mathf.Max(choiceButtonFontSizeMin, 8f);
        choiceButtonFontSizeMax = Mathf.Max(choiceButtonFontSizeMax, 8f);

        // Apply animation settings when values change in inspector
        if (Application.isPlaying)
        {
            ApplyAnimationSettingsToChoiceButtons();
        }
    }

    private void Start()
    {
        dialoguePanel?.SetActive(false);
        endConversationButton?.onClick.AddListener(EndDialogue);
        FindPlayerController();
        InitializeChoiceButtons();

        ApplyAnimationSettingsToChoiceButtons();
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
        
        if (useTextAnimations)
        {
            UpdateTextAnimations();
        }
        
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

    public void ApplyTextStyling(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
        if (textComponent == null || style == null) return;

        // Apply font and basic styling
        ApplyBasicTextProperties(textComponent, style);
        
        // Apply colors and gradients
        ApplyTextColors(textComponent, style);

        // Apply effects (outline, shadow, glow)
        ApplyTextEffects(textComponent, style);
    }

    private void ApplyBasicTextProperties(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
        if (style.customFont != null)
            textComponent.font = style.customFont;

        textComponent.fontSize = style.fontSize;
        textComponent.fontStyle = style.fontStyle;
        textComponent.characterSpacing = style.characterSpacing;
        textComponent.lineSpacing = style.lineSpacing;
        textComponent.wordSpacing = style.wordSpacing;
    }

    private void ApplyTextColors(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
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
    }

    private void ApplyTextEffects(TextMeshProUGUI textComponent, DialogueTextStyle style)
    {
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
                originalVertex = ApplyAnimationEffect(originalVertex, style, i);
                vertices[charInfo.vertexIndex + j] = originalVertex;
            }
        }
        
        textComponent.UpdateVertexData();
    }

    // Extract animation logic to reduce duplication
    private Vector3 ApplyAnimationEffect(Vector3 vertex, DialogueTextStyle style, int charIndex)
    {
        switch (style.animationType)
        {
            case TextAnimationType.Wave:
                vertex.y += Mathf.Sin(Time.time * style.animationSpeed + charIndex * 0.1f) * style.animationIntensity;
                break;
                
            case TextAnimationType.Bounce:
                vertex.y += Mathf.Abs(Mathf.Sin(Time.time * style.animationSpeed + charIndex * 0.2f)) * style.animationIntensity;
                break;
                
            case TextAnimationType.Shake:
                vertex.x += Random.Range(-style.animationIntensity, style.animationIntensity);
                vertex.y += Random.Range(-style.animationIntensity, style.animationIntensity);
                break;
                
            case TextAnimationType.Pulse:
                float scale = 1f + Mathf.Sin(Time.time * style.animationSpeed) * style.animationIntensity * 0.1f;
                vertex = Vector3.Scale(vertex, Vector3.one * scale);
                break;
        }
        
        return vertex;
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

    // Consolidated preset creation with shared logic
    public DialogueTextStyle CreatePresetStyle(string presetName)
    {
        var presets = new Dictionary<string, System.Func<DialogueTextStyle>>
        {
            ["mysterious"] = () => CreateStyleWithAnimation(new Color(0.6f, 0.4f, 0.8f), new Color(0.8f, 0.6f, 1f), TextAnimationType.Wave, true),
            ["heroic"] = () => CreateStyledText(new Color(1f, 0.8f, 0.2f), new Color(0.8f, 0.6f, 0f), FontStyles.Bold),
            ["villain"] = () => CreateStyleWithAnimation(new Color(0.8f, 0.2f, 0.2f), Color.black, TextAnimationType.Shake, false, FontStyles.Normal),
            ["narrator"] = () => CreateStyledText(new Color(0.7f, 0.7f, 0.7f), Color.black, FontStyles.Italic, 2f)
        };

        return presets.ContainsKey(presetName.ToLower()) ? presets[presetName.ToLower()]() : new DialogueTextStyle();
    }

    public DialogueTextStyle CreateChoiceButtonPresetStyle(string presetName)
    {
        var presets = new Dictionary<string, System.Func<DialogueTextStyle>>
        {
            ["elegant"] = () => CreateStyledText(new Color(0.9f, 0.8f, 0.6f), new Color(0.3f, 0.2f, 0.1f), FontStyles.Italic, 1f),
            ["modern"] = () => CreateStyledText(new Color(0.2f, 0.8f, 1f), Color.clear, FontStyles.Normal, 0f, true),
            ["fantasy"] = () => CreateStyleWithAnimation(new Color(0.8f, 0.6f, 1f), new Color(1f, 0.8f, 1f), TextAnimationType.Pulse, true),
            ["military"] = () => CreateStyledText(new Color(0.6f, 0.8f, 0.4f), new Color(0.2f, 0.3f, 0.1f), FontStyles.Bold, 2f),
            ["retro"] = () => CreateStyleWithAnimation(new Color(1f, 0.4f, 0.6f), new Color(0.8f, 0.2f, 0.4f), TextAnimationType.Wave, false)
        };

        return presets.ContainsKey(presetName.ToLower()) ? presets[presetName.ToLower()]() : new DialogueTextStyle();
    }

    // Helper methods to reduce duplication in preset creation
    private DialogueTextStyle CreateStyledText(Color primaryColor, Color outlineColor, FontStyles fontStyle = FontStyles.Normal, float characterSpacing = 0f, bool useShadow = false)
    {
        var style = new DialogueTextStyle
        {
            primaryColor = primaryColor,
            fontStyle = fontStyle,
            characterSpacing = characterSpacing
        };

        if (outlineColor != Color.clear)
        {
            style.useOutline = true;
            style.outlineColor = outlineColor;
        }

        if (useShadow)
        {
            style.useShadow = true;
            style.shadowColor = new Color(0f, 0f, 0f, 0.5f);
        }

        return style;
    }

    private DialogueTextStyle CreateStyleWithAnimation(Color primaryColor, Color glowColor, TextAnimationType animationType, bool useGlow, FontStyles fontStyle = FontStyles.Normal)
    {
        var style = CreateStyledText(primaryColor, Color.black, fontStyle);
        
        if (useGlow)
        {
            style.useGlow = true;
            style.glowColor = glowColor;
        }
        
        style.enableTextAnimation = true;
        style.animationType = animationType;
        
        return style;
    }

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
        ClearExistingChoiceButtons();
        CreateChoiceButtons();
    }

    private void ClearExistingChoiceButtons()
    {
        foreach (Transform child in choiceButtonParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        choiceButtons.Clear();
    }

    private void CreateChoiceButtons()
    {
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
        Button buttonComponent = EnsureButtonComponent(buttonObj);
        ConfigureButtonLayout(buttonObj);
        ConfigureButtonText(buttonObj);
        SetupButtonComponents(buttonObj);
        
        choiceButtons.Add(buttonComponent);
        int choiceIndex = index;
        buttonComponent.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
    }

    private Button EnsureButtonComponent(GameObject buttonObj)
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
        return buttonComponent;
    }

    private void ConfigureButtonLayout(GameObject buttonObj)
    {
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
    }

    private void ConfigureButtonText(GameObject buttonObj)
    {
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.textWrappingMode = TextWrappingModes.NoWrap;
            buttonText.overflowMode = TextOverflowModes.Ellipsis;
            buttonText.enableAutoSizing = true;
            buttonText.fontSizeMin = choiceButtonFontSizeMin;
            buttonText.fontSizeMax = choiceButtonFontSizeMax;
            buttonText.alignment = TextAlignmentOptions.Left;

            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(15, 5);
                textRect.offsetMax = new Vector2(-15, -5);
            }

            ApplyTextStyling(buttonText, choiceButtonTextStyle);
        }
        else
        {
            Debug.LogError($"Choice button prefab is missing TextMeshProUGUI component! Button: {buttonObj.name}");
        }
    }

    private void SetupButtonComponents(GameObject buttonObj)
    {
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
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

        ResetDialogueUI();
        ApplyNPCTextStyling();
        DisplayStyledDialogue(initialMessage);
        playerController?.SetInDialogue(true);
    }

    private void ResetDialogueUI()
    {
        chatHistoryText.text = "";
        dialogueText.text = "";
        HideAllChoices();
    }

    private void ApplyNPCTextStyling()
    {
        ApplyTextStyling(dialogueText, npcTextStyle);
        ApplyTextStyling(npcNameText, npcTextStyle);
    }

    public void EndDialogue()
    {
        if (!isDialogueActive) return;
        
        Debug.Log($"DialogueManager: Ending dialogue with {currentNPC?.NPCName ?? "unknown NPC"}");
        
        dialoguePanel.SetActive(false);
        
        currentNPC?.OnDialogueEnded(); 
        currentNPC = null;
        
        isDialogueActive = false;
        ResetDialogueUI();
        
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