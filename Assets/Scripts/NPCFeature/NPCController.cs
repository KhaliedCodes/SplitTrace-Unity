using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCPersonality personality;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Enemy Conversion")]
    [SerializeField] private HostilityTracker hostilityTracker;
    
    private SphereCollider interactionTrigger;
    private DialogueManager dialogueManager;
    private GeminiAccessor geminiAccessor;
    private PlayerController nearbyPlayer;
    
    private bool isInteracting;
    private bool waitingForChoices = false;

    public string NPCName => personality?.npcName ?? "NPC";

    private void Awake()
    {
        interactionTrigger = gameObject.AddComponent<SphereCollider>();
        interactionTrigger.radius = interactionRadius;
        interactionTrigger.isTrigger = true;
        
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        geminiAccessor = GetComponent<GeminiAccessor>() ?? gameObject.AddComponent<GeminiAccessor>();
        
        if (personality != null)
            geminiAccessor.ConfigureWithPersonality(personality);

        geminiAccessor.OnResponseProcessed += HandleAIResponse;
        geminiAccessor.OnChoicesReceived += HandleChoicesReceived;
        
        hostilityTracker.Initialize();
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

    private string ModifyResponseBasedOnHostility(string originalResponse, string emotion)
    {
        float hostilityLevel = hostilityTracker.CurrentHostility;
        
        // If hostility is building up, add subtle tension to responses
        if (hostilityLevel > hostilityTracker.hostilityThreshold * 0.5f && !hostilityTracker.IsEnemy)
        {
            // Add some tension indicators
            string[] tensionPrefixes = 
            {
                "Look, ",
                "Listen here, ",
                "I'm starting to get annoyed... ",
                "You're really pushing it... "
            };
            
            if (Random.Range(0f, 1f) < 0.3f) // 30% chance to add tension
            {
                string prefix = tensionPrefixes[Random.Range(0, tensionPrefixes.Length)];
                return prefix + originalResponse;
            }
        }
        
        return originalResponse;
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
        
        // End dialogue after a short delay
        Invoke(nameof(EndHostileDialogue), 3f);
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