using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private NPCPersonality personality;
    [SerializeField] private Transform interactionTrigger;
    [SerializeField] private float interactionRadius = 2f;

    [Header("UI Components")]
    [SerializeField] private GameObject interactionIndicator;

    private bool isInteracting;
    private PlayerController nearbyPlayer;
    private DialogueManager dialogueManager;
    private GeminiAccessor geminiAccessor;

    public string NPCName => personality != null ? personality.npcName : "NPC";

    private void Awake()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // Get or add the GeminiAccessor component
        geminiAccessor = GetComponent<GeminiAccessor>();
        if (geminiAccessor == null)
            geminiAccessor = gameObject.AddComponent<GeminiAccessor>();

        // Configure the system with our personality data - only pass to GeminiAccessor
        if (personality != null)
        {
            geminiAccessor.ConfigureWithPersonality(personality);
        }

        // Subscribe to AI responses
        geminiAccessor.OnResponseProcessed += HandleAIResponse;
    }

    private void Start()
    {
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        if (isInteracting) return;
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
        string greeting = personality != null ? personality.initialGreeting : "Hello there!";
        dialogueManager.StartDialogue(this, greeting);
    }

    public void EndInteraction()
    {
        isInteracting = false;
        geminiAccessor?.ClearChatHistory();
        interactionIndicator?.SetActive(true);
    }

    public void SendPlayerMessage(string message)
    {
        if (geminiAccessor != null)
            geminiAccessor.SendPlayerInput(message);
    }

    private void HandleAIResponse(string responseText, string emotion)
    {
        if (string.IsNullOrEmpty(responseText)) return;

        // If no emotion specified but our personality uses emotions, default to neutral
        if (string.IsNullOrEmpty(emotion) && personality != null && personality.usesEmotions)
        {
            emotion = personality.defaultEmotion;
        }
        else if (string.IsNullOrEmpty(emotion))
        {
            emotion = "neutral";
        }

        string formattedResponse = $"{responseText} {{\"emotion\":\"{emotion}\"}}";
        dialogueManager.DisplayNPCDialogue(formattedResponse);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == nearbyPlayer)
        {
            player.ClearCurrentInteractable();
            if (isInteracting) dialogueManager.EndDialogue();
            nearbyPlayer = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            interactionTrigger?.position ?? transform.position,
            interactionRadius);
    }

    private void OnDestroy() => geminiAccessor.OnResponseProcessed -= HandleAIResponse;
}