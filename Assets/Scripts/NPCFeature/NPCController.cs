using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCPersonality personality;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Enemy Conversion")]
    [SerializeField] private HostilityTracker hostilityTracker;

    [Header("Enemy trun into hostile NPC")]
    [SerializeField] private RuntimeAnimatorController hostileAnimatorController;
    private RangedEnemy rangedEnemy;

    private SphereCollider interactionTrigger;
    private DialogueManager dialogueManager;
    private GeminiAPIClient geminiAPI; // Optional, can be null if not using Gemini
    private NPCController npcController; // Reference to this NPC's controller for interactions
    private GeminiAccessor geminiAccessor; // This will be this NPC's own instance
    private NavMeshAgent npcNavMeshAgent; // Optional, can be null if not using NavMesh
    private PlayerController nearbyPlayer;
    private Animator npcAnimator;
    private Rigidbody npcRigidbody;
    private bool isInteracting;
    private bool waitingForChoices = false;

    public string NPCName => personality?.npcName ?? "NPC";

    private void Awake()
    {
        interactionTrigger = gameObject.AddComponent<SphereCollider>();
        interactionTrigger.radius = interactionRadius;
        interactionTrigger.isTrigger = true;

        dialogueManager = FindFirstObjectByType<DialogueManager>();

        //Create a dedicated GeminiAccessor for this NPC instead of finding a shared one
        geminiAccessor = gameObject.GetComponent<GeminiAccessor>();
        if (geminiAccessor == null)
        {
            geminiAccessor = gameObject.AddComponent<GeminiAccessor>();
        }

        if (personality != null)
            geminiAccessor.ConfigureWithPersonality(personality);

        geminiAccessor.OnResponseProcessed += HandleAIResponse;
        geminiAccessor.OnChoicesReceived += HandleChoicesReceived;

        hostilityTracker.Initialize();

        if (npcAnimator == null)
        {
            npcAnimator = GetComponent<Animator>();
        }
        else
        {
            Debug.Log($"[{NPCName}] No Animator found on NPC. Dialogue animations will not work.");
        }

        rangedEnemy = GetComponent<RangedEnemy>();
        if (rangedEnemy == null)
        {
            Debug.LogWarning($"[{NPCName}] No RangedEnemy component found. This NPC will not become hostile.");
        }
        geminiAPI = GetComponent<GeminiAPIClient>();
        if (geminiAPI == null)
        {
            Debug.LogWarning($"[{NPCName}] No GeminiAPIClient component found. This NPC will not use Gemini for dialogue.");
        }
        geminiAccessor = GetComponent<GeminiAccessor>();
        if (geminiAccessor == null)
        {
            geminiAccessor = gameObject.AddComponent<GeminiAccessor>();
        }
        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            Debug.LogWarning($"[{NPCName}] No NPCController component found. This NPC will not handle interactions properly.");
        }
        npcRigidbody = GetComponent<Rigidbody>();
        if (npcRigidbody == null)
        {
            Debug.LogWarning($"[{NPCName}] No Rigidbody found. This NPC may not interact with physics correctly.");
        }
        npcNavMeshAgent = GetComponent<NavMeshAgent>();
        if (npcNavMeshAgent == null)
        {
            Debug.LogWarning($"[{NPCName}] No NavMeshAgent found. This NPC will not navigate the environment.");
        }

    }
    private void Start()
    {
        npcRigidbody.isKinematic = true; // Prevent physics interactions during dialogue
        npcNavMeshAgent.enabled = false; // Disable navigation during dialogue
       
    }
    private void Update() 
    {
        // Optional: Display hostility status in debug mode
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log($"[{NPCName}] {hostilityTracker.GetHostilityStatus()}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            nearbyPlayer = player;
            player.SetCurrentInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == nearbyPlayer)
        {
            if (nearbyPlayer != null) 
            {
                nearbyPlayer.ClearCurrentInteractable();
            }
            if (isInteracting)
            {
                dialogueManager?.EndDialogue();
            }
            nearbyPlayer = null;
        }
    }

    public void StartInteraction()
    {
        if (isInteracting || dialogueManager == null) return;

        // Check if NPC is already hostile
        if (hostilityTracker.IsEnemy)
        {
            dialogueManager.StartDialogue(this, "I don't want to talk to you!");
            return;
        }

        isInteracting = true;
        dialogueManager.StartDialogue(this, personality?.initialGreeting ?? "Hello there!");
        npcAnimator?.SetBool("inDialogue", true);
    }

    public void SendPlayerChoice(string choiceText, int choiceIndex)
    {
        if (hostilityTracker.IsEnemy)
        {
            dialogueManager?.DisplayNPCDialogue("I'm done talking to you!");
            return;
        }
        
        // Still analyze player input, but with less impact (optional)
        // hostilityTracker.AnalyzePlayerText(choiceText);
        
        waitingForChoices = false;
        geminiAccessor.SendPlayerInput(choiceText);
        
        // Note: Enemy conversion will be checked when AI responds, not here
    }

    public void RequestDialogueChoices()
    {
        if (!waitingForChoices && geminiAccessor != null && !hostilityTracker.IsEnemy)
        {
            waitingForChoices = true;
            geminiAccessor.RequestChoices();
        }
    }

    private void HandleAIResponse(string responseText, string emotion)
    {
        if (string.IsNullOrEmpty(responseText)) return;
        
        // CRITICAL: Analyze AI response for aggressive content BEFORE displaying
        hostilityTracker.AnalyzeAIResponse(responseText);
        
        // Check if this aggressive response pushed the NPC over the edge
        if (CheckForEnemyConversion())
        {
            // If NPC just became hostile due to AI aggression, show conversion message
            return; // ConvertToEnemy() handles the response display
        }
        
        // If not converted to enemy, display the response normally
        // (but the hostility has still been tracked)
        dialogueManager.DisplayNPCDialogue($"{responseText} {{\"emotion\":\"{emotion}\"}}");
        
        // Optional: Show hostility warning if getting close to threshold
        if (hostilityTracker.CurrentHostility > hostilityTracker.hostilityThreshold * 0.8f && 
            !hostilityTracker.IsEnemy)
        {
            Debug.Log($"[WARNING] {NPCName} is getting very hostile! ({hostilityTracker.CurrentHostility:F1}/{hostilityTracker.hostilityThreshold})");
        }
    }

    private void HandleChoicesReceived(List<string> choices)
    {
        waitingForChoices = false;
        
        // Filter out aggressive choices if NPC is getting hostile
        if (hostilityTracker.CurrentHostility > hostilityTracker.hostilityThreshold * 0.7f)
        {
            choices = FilterAggressiveChoices(choices);
        }
        
        dialogueManager?.DisplayChoices(choices);
    }

    private List<string> FilterAggressiveChoices(List<string> originalChoices)
    {
        List<string> filteredChoices = new List<string>();
        
        foreach (string choice in originalChoices)
        {
            // Use the hostility tracker to check if choice would be aggressive
            // Create a temporary copy to test without affecting the real tracker
            bool isAggressive = choice.ToLower().Contains("attack") || 
                              choice.ToLower().Contains("fight") || 
                              choice.ToLower().Contains("threaten") ||
                              choice.ToLower().Contains("kill");
            
            if (!isAggressive)
            {
                filteredChoices.Add(choice);
            }
        }
        
        // Always provide at least one peaceful option
        if (filteredChoices.Count == 0)
        {
            filteredChoices.Add("Maybe we should calm down...");
            filteredChoices.Add("I think there's been a misunderstanding.");
        }
        
        return filteredChoices;
    }

    private bool CheckForEnemyConversion()
    {
        if (hostilityTracker.CheckEnemyConversion())
        {
            ConvertToEnemy();
            return true;
        }
        return false;
    }
    private void ConvertToEnemy()
    {
        Debug.Log($"[ENEMY CONVERSION] {NPCName} has become hostile due to aggressive AI responses!");

        // Generate context-appropriate hostile response
        string[] conversionMessages =
        {
        $"The way {NPCName} just spoke shows their true hostile nature!",
        $"{NPCName}'s aggressive response reveals they are not to be trusted!",
        $"That hostile outburst from {NPCName} shows they've become an enemy!",
        $"{NPCName} has shown their true colors with that aggressive response!"
    };

        string message = conversionMessages[Random.Range(0, conversionMessages.Length)];
        dialogueManager?.DisplayNPCDialogue($"{message} {{\"emotion\":\"angry\"}}");

        // Schedule end of hostile dialogue
        Invoke(nameof(EndHostileDialogue), 3f);

        // Switch behaviors and appearance
        if (rangedEnemy != null) rangedEnemy.enabled = true;
        if (npcAnimator != null && hostileAnimatorController != null)
            npcAnimator.runtimeAnimatorController = hostileAnimatorController;

        // Disable interaction systems
        if (geminiAccessor != null) geminiAccessor.enabled = false;
        if (geminiAPI != null) geminiAPI.enabled = false;
        if (npcController != null) npcController.enabled = false;

        // Change layer to "Enemy" (make sure it exists in Unity's layer settings)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
            gameObject.layer = enemyLayer;
        else
            Debug.LogWarning("Layer 'Enemy' not found. Please define it in Project Settings > Tags and Layers.");

        npcRigidbody.isKinematic = false; // Allow physics interactions now that NPC is hostile
        npcNavMeshAgent.enabled = true; // Enable navigation for hostile NPC
        interactionTrigger.enabled = false; // Disable interaction trigger to prevent further dialogue
    
}
    private void EndHostileDialogue()
    {
        dialogueManager?.EndDialogue();
    }

    public void OnDialogueEnded()
    {
        isInteracting = false;
        waitingForChoices = false;
        // Don't clear chat history if NPC is hostile - they should remember
        if (!hostilityTracker.IsEnemy)
        {
            geminiAccessor?.ClearChatHistory();
        }
         npcAnimator?.SetBool("inDialogue", false);
    }

    public void EndInteraction()
    {
        isInteracting = false;
        waitingForChoices = false;
        
        // Don't clear chat history if NPC is hostile - they should remember
        if (!hostilityTracker.IsEnemy)
        {
            geminiAccessor?.ClearChatHistory();
        }
    }

    // Public method for external systems to add hostility (e.g., if player does something aggressive in gameplay)
    public void AddExternalHostility(float amount, string reason)
    {
        hostilityTracker.AddHostility(amount, reason);
        CheckForEnemyConversion();
    }

    // Public method to check if NPC is getting hostile (for UI indicators, etc.)
    public bool IsGettingHostile()
    {
        return hostilityTracker.CurrentHostility > hostilityTracker.hostilityThreshold * 0.5f;
    }

    private void OnDestroy()
    {
        if (geminiAccessor != null)
        {
            geminiAccessor.OnResponseProcessed -= HandleAIResponse;
            geminiAccessor.OnChoicesReceived -= HandleChoicesReceived;
        }
    }
}