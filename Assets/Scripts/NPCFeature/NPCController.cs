using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCPersonality personality;
    [SerializeField] private Transform interactionTrigger;
    [SerializeField] private float interactionRadius = 2f;

    [Header("UI Components")]
    private GameObject interactionIndicator;
    private bool isInteracting;
    private PlayerController nearbyPlayer;
    private DialogueManager dialogueManager;
    private GeminiAccessor geminiAccessor;
    private bool waitingForChoices = false;


    [Header("Enemy Conversion System")]
    [SerializeField] private bool canBecomeEnemy = true;
    [SerializeField] private int hostilityThreshold = 3; // Number of hostile interactions before becoming enemy
    [SerializeField] private float hostilityDecayRate = 0.1f; // How fast hostility decreases over time
    [SerializeField] private string[] hostileTriggerWords = { "threaten", "attack", "kill", "hurt", "fight", "enemy", "hate" };
    [SerializeField] private string[] peacefulTriggerWords = { "sorry", "peace", "friend", "help", "apologize", "calm" };

    private float currentHostility = 0f;
    private int hostileInteractionCount = 0;
    private float lastInteractionTime;
    private bool hasBeenMarkedAsEnemy = false;


    public string NPCName => personality != null ? personality.npcName : "NPC";

    private void Awake()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        if (dialogueManager != null)
        {
            interactionIndicator = dialogueManager.gameObject;
            Debug.Log("DialogueManager found in scene.");
        }
        else
        {
            Debug.LogWarning("DialogueManager not found in scene.");
        }

        // Get or add the GeminiAccessor component
        geminiAccessor = GetComponent<GeminiAccessor>();
        if (geminiAccessor == null)
            geminiAccessor = gameObject.AddComponent<GeminiAccessor>();

        // Configure Gemini with personality
        if (personality != null)
        {
            geminiAccessor.ConfigureWithPersonality(personality);
        }

        // Subscribe to events
        geminiAccessor.OnResponseProcessed += HandleAIResponse;
        geminiAccessor.OnChoicesReceived += HandleChoicesReceived;
    }

    private void Start()
    {
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        if (isInteracting) return;

        // Decay hostility over time
        if (Time.time - lastInteractionTime > 10f && currentHostility > 0)
        {
            float decay = hostilityDecayRate * Time.deltaTime;
            currentHostility = Mathf.Max(0, currentHostility - decay);

            if (currentHostility == 0 && hostileInteractionCount > 0)
            {
                Debug.Log($"[ENEMY CONVERSION] Hostility fully decayed for {NPCName}");
            }
        }

        CheckPlayerProximity();
    }

    private void CheckPlayerProximity()
    {
        Collider[] colliders = Physics.OverlapSphere(
            interactionTrigger?.position ?? transform.position,
            interactionRadius);

        foreach (Collider col in colliders)
        {
            PlayerController player = col.GetComponent<PlayerController>();
            if (player != null)
            {
                nearbyPlayer = player;
                nearbyPlayer.SetCurrentInteractable(this);
                interactionIndicator?.SetActive(true);
                return;
            }
        }

        if (nearbyPlayer != null)
        {
            nearbyPlayer.ClearCurrentInteractable();
            nearbyPlayer = null;
            interactionIndicator?.SetActive(false);
        }
    }

    public void StartInteraction()
    {
        if (isInteracting || dialogueManager == null)
        {
            Debug.LogWarning("StartInteraction failed: isInteracting=" + isInteracting + ", dialogueManager=" + dialogueManager);
            return;
        }

        isInteracting = true;
        interactionIndicator?.SetActive(false);

        // Show and unlock the cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        string greeting = personality != null ? personality.initialGreeting : "Hello there!";
        dialogueManager.StartDialogue(this, greeting);
    }

    public void EndInteraction()
    {
        isInteracting = false;
        waitingForChoices = false;
        geminiAccessor?.ClearChatHistory();
        interactionIndicator?.SetActive(true);

        // Hide and lock the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void SendPlayerChoice(string choiceText, int choiceIndex)
    {
        if (geminiAccessor != null)
        {
            waitingForChoices = false;
            geminiAccessor.SendPlayerInput(choiceText);
        }
    }

    public void RequestDialogueChoices()
    {
        if (waitingForChoices || geminiAccessor == null) return;

        waitingForChoices = true;
        geminiAccessor.RequestChoices();
    }

    private void HandleAIResponse(string responseText, string emotion)
    {
        if (string.IsNullOrEmpty(responseText)) return;

        // Analyze AI response for hostility
        AnalyzeAIResponse(responseText);

        // Check emotion for hostility
        if (emotion == "angry" && !hasBeenMarkedAsEnemy)
        {
            currentHostility += 0.2f;
            Debug.Log($"[ENEMY CONVERSION] NPC is angry! Hostility: {currentHostility}");
            CheckEnemyConversion();
        }

        if (string.IsNullOrEmpty(emotion))
        {
            if (personality != null && personality.usesEmotions)
                emotion = personality.defaultEmotion;
            else
                emotion = "neutral";
        }

        string formattedResponse = $"{responseText} {{\"emotion\":\"{emotion}\"}}";
        dialogueManager.DisplayNPCDialogue(formattedResponse);
    }

    // Add this method for manual testing
    [System.Obsolete("For debugging only")]
    public void DEBUG_ForceEnemyConversion()
    {
        Debug.Log($"[ENEMY CONVERSION] FORCED CONVERSION for {NPCName}");
        hasBeenMarkedAsEnemy = true;
        ConvertToEnemy();
    }

    // Add this method to get current hostility status
    public void DEBUG_ShowHostilityStatus()
    {
        Debug.Log($"[ENEMY CONVERSION] === Hostility Status for {NPCName} ===");
        Debug.Log($"[ENEMY CONVERSION] Current Hostility: {currentHostility}/{hostilityThreshold}");
        Debug.Log($"[ENEMY CONVERSION] Hostile Interactions: {hostileInteractionCount}");
        Debug.Log($"[ENEMY CONVERSION] Can Become Enemy: {canBecomeEnemy}");
        Debug.Log($"[ENEMY CONVERSION] Has Been Marked As Enemy: {hasBeenMarkedAsEnemy}");
        Debug.Log($"[ENEMY CONVERSION] Time Since Last Interaction: {Time.time - lastInteractionTime}");
    }

    private void HandleChoicesReceived(List<string> choices)
    {
        waitingForChoices = false;
        dialogueManager?.DisplayChoices(choices);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == nearbyPlayer)
        {
            player.ClearCurrentInteractable();
            if (isInteracting)
            {
                dialogueManager.EndDialogue();
                EndInteraction(); // Call EndInteraction to reset state
            }
            nearbyPlayer = null;
        }
    }
    public void ProcessPlayerChoice(string choiceText, int choiceIndex)
    {
        // Analyze choice for enemy conversion
        AnalyzePlayerChoice(choiceText);

        // Continue with normal choice processing
        SendPlayerChoice(choiceText, choiceIndex);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            interactionTrigger?.position ?? transform.position,
            interactionRadius);
    }

    private void OnDestroy()
    {
        if (geminiAccessor != null)
        {
            geminiAccessor.OnResponseProcessed -= HandleAIResponse;
            geminiAccessor.OnChoicesReceived -= HandleChoicesReceived;
        }
    }

    #region  trun npc into enemy

    private void AnalyzePlayerChoice(string choiceText)
    {
        if (!canBecomeEnemy || hasBeenMarkedAsEnemy) return;

        string lowerChoice = choiceText.ToLower();
        bool isHostile = false;
        bool isPeaceful = false;

        // Check for hostile words
        foreach (string hostileWord in hostileTriggerWords)
        {
            if (lowerChoice.Contains(hostileWord))
            {
                isHostile = true;
                break;
            }
        }

        // Check for peaceful words
        foreach (string peacefulWord in peacefulTriggerWords)
        {
            if (lowerChoice.Contains(peacefulWord))
            {
                isPeaceful = true;
                break;
            }
        }

        // Update hostility based on choice
        if (isHostile)
        {
            currentHostility += 1f;
            hostileInteractionCount++;
            Debug.Log($"[ENEMY CONVERSION] Hostile choice detected: '{choiceText}'");
            Debug.Log($"[ENEMY CONVERSION] Hostility increased to: {currentHostility}");
            Debug.Log($"[ENEMY CONVERSION] Hostile interaction count: {hostileInteractionCount}");
        }
        else if (isPeaceful)
        {
            currentHostility = Mathf.Max(0, currentHostility - 0.5f);
            Debug.Log($"[ENEMY CONVERSION] Peaceful choice detected: '{choiceText}'");
            Debug.Log($"[ENEMY CONVERSION] Hostility decreased to: {currentHostility}");
        }

        // Check if NPC should become enemy
        CheckEnemyConversion();

        lastInteractionTime = Time.time;
    }

    // Add this method to NPCController.cs
    private void CheckEnemyConversion()
    {
        if (hasBeenMarkedAsEnemy) return;

        bool shouldBecomeEnemy = false;
        string conversionReason = "";

        // Check hostility threshold
        if (currentHostility >= hostilityThreshold)
        {
            shouldBecomeEnemy = true;
            conversionReason = $"Hostility threshold reached ({currentHostility}/{hostilityThreshold})";
        }

        // Check consecutive hostile interactions
        if (hostileInteractionCount >= 2)
        {
            shouldBecomeEnemy = true;
            conversionReason = $"Multiple hostile interactions ({hostileInteractionCount})";
        }

        if (shouldBecomeEnemy)
        {
            hasBeenMarkedAsEnemy = true;
            Debug.Log($"[ENEMY CONVERSION] *** NPC '{NPCName}' SHOULD BECOME ENEMY! ***");
            Debug.Log($"[ENEMY CONVERSION] Reason: {conversionReason}");
            Debug.Log($"[ENEMY CONVERSION] Current Hostility: {currentHostility}");
            Debug.Log($"[ENEMY CONVERSION] Hostile Interactions: {hostileInteractionCount}");
            Debug.Log($"[ENEMY CONVERSION] Time of Conversion: {Time.time}");

            // This is where you would call your enemy conversion logic
            ConvertToEnemy();
        }
    }

    // Add this method to NPCController.cs
    private void ConvertToEnemy()
    {
        Debug.Log($"[ENEMY CONVERSION] Converting {NPCName} to enemy...");

        // End dialogue immediately
        if (dialogueManager != null)
        {
            dialogueManager.EndDialogue();
        }

        // Debug information for enemy conversion
        Debug.Log($"[ENEMY CONVERSION] NPC Position: {transform.position}");
        Debug.Log($"[ENEMY CONVERSION] Player Position: {nearbyPlayer?.transform.position}");
        Debug.Log($"[ENEMY CONVERSION] Distance to Player: {Vector3.Distance(transform.position, nearbyPlayer?.transform.position ?? Vector3.zero)}");

        // Here you would:
        // 1. Disable NPC dialogue components
        // 2. Enable enemy AI components
        // 3. Change NPC appearance/materials
        // 4. Add enemy health/combat stats
        // 5. Switch to enemy behavior state machine

        Debug.Log($"[ENEMY CONVERSION] {NPCName} is now an enemy!");
    }
    
    private void AnalyzeAIResponse(string responseText)
{
    if (!canBecomeEnemy || hasBeenMarkedAsEnemy) return;

    string lowerResponse = responseText.ToLower();
    
    // Check if AI is responding aggressively
    string[] aggressiveResponses = { "angry", "furious", "hate", "enemy", "kill", "attack", "die" };
    
    foreach (string aggressiveWord in aggressiveResponses)
    {
        if (lowerResponse.Contains(aggressiveWord))
        {
            currentHostility += 0.3f;
            Debug.Log($"[ENEMY CONVERSION] AI responded aggressively: '{responseText}'");
            Debug.Log($"[ENEMY CONVERSION] Hostility increased to: {currentHostility}");
            CheckEnemyConversion();
            break;
        }
    }
}
    #endregion
}