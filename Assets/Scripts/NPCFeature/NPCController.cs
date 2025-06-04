using UnityEngine;
using System.Collections.Generic;
public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCPersonality personality;
    [SerializeField] private float interactionRadius = 2f;

    [Header("Enemy Conversion")]
    [SerializeField] private HostilityTracker hostilityTracker;
    [SerializeField] private string[] aggressiveResponseTriggers = 
        { "angry", "furious", "hate", "enemy", "kill", "attack", "die" };

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

    private void Update() => hostilityTracker.Update();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            nearbyPlayer = player;
            player.SetCurrentInteractable(this);
        }
    }

    // Fixed OnTriggerExit
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

        isInteracting = true;
        dialogueManager.StartDialogue(this, personality?.initialGreeting ?? "Hello there!");
    }

    public void SendPlayerChoice(string choiceText, int choiceIndex)
    {
        hostilityTracker.AnalyzeText(choiceText);
        CheckForEnemyConversion();
        
        waitingForChoices = false;
        geminiAccessor.SendPlayerInput(choiceText);
    }

    public void RequestDialogueChoices()
    {
        if (!waitingForChoices && geminiAccessor != null)
        {
            waitingForChoices = true;
            geminiAccessor.RequestChoices();
        }
    }

    private void HandleAIResponse(string responseText, string emotion)
    {
        if (string.IsNullOrEmpty(responseText)) return;
        
        AnalyzeAIResponse(responseText); 
        hostilityTracker.AnalyzeText(responseText);
        CheckForEnemyConversion();
        
        dialogueManager.DisplayNPCDialogue($"{responseText} {{\"emotion\":\"{emotion}\"}}");
    }

    private void AnalyzeAIResponse(string responseText)
    {
        if (!hostilityTracker.canBecomeEnemy || hostilityTracker.IsEnemy) return;

        string lowerResponse = responseText.ToLower();
        
        foreach (string aggressiveWord in aggressiveResponseTriggers)
        {
            if (lowerResponse.Contains(aggressiveWord))
            {
                hostilityTracker.AddHostility(0.3f);
                Debug.Log($"[ENEMY CONVERSION] AI responded aggressively: '{responseText}'");
                Debug.Log($"[ENEMY CONVERSION] Hostility increased to: {hostilityTracker.CurrentHostility}");
                break;
            }
        }
    }

    private void HandleChoicesReceived(List<string> choices)
    {
        waitingForChoices = false;
        dialogueManager?.DisplayChoices(choices);
    }

    private void CheckForEnemyConversion()
    {
        if (hostilityTracker.CheckEnemyConversion())
            ConvertToEnemy();
    }

    private void ConvertToEnemy()
    {
        Debug.Log($"[ENEMY CONVERSION] {NPCName} has become hostile!");
        dialogueManager?.DisplayNPCDialogue($"{NPCName} has turned against you!");
        dialogueManager.EndDialogue();
    
    }

    public void OnDialogueEnded()
{
    isInteracting = false;
    waitingForChoices = false;
    geminiAccessor?.ClearChatHistory();
}


public void EndInteraction()
{
    isInteracting = false;
    waitingForChoices = false;
    geminiAccessor?.ClearChatHistory();
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