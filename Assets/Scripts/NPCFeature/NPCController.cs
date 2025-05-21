using StarterAssets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCController : MonoBehaviour
{
    [Header("NPC Configuration")]
    [SerializeField] private string npcName;
    [SerializeField, TextArea(3, 5)] private string npcDescription;
    [SerializeField] private Transform interactionTrigger;
    [SerializeField] private float interactionRadius = 2f;

    [Header("AI Integration")]
    [SerializeField] private GeminiAPIPersonality geminiAI;
    [SerializeField, TextArea(3, 5)] private string personalityPrompt;

    [Header("UI Components")]
    [SerializeField] private GameObject interactionIndicator;
    [SerializeField, TextArea(2, 4)] private string initialGreeting = "Hello there! What brings you here today?";

    private bool isInteracting;
    private PlayerController nearbyPlayer;
    private DialogueManager dialogueManager;

    private void Awake()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        ConfigureGeminiAI();
    }

    private void Start()
    {
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
    }

    private void Update()
    {
        CheckPlayerProximity();

        if (nearbyPlayer != null && Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting && !nearbyPlayer.IsInDialogue)
            {
                StartInteraction();
            }
        }
    }

    private void ConfigureGeminiAI()
    {
        if (geminiAI == null) return;

        if (string.IsNullOrEmpty(geminiAI._systemInstructions))
        {
            string defaultPrompt = $"You are {npcName}, {npcDescription}. " +
                                   "Respond in character with brief, natural dialogue. " +
                                   "Include emotional state in JSON format at the end of each response: {{\"emotion\": \"happy/sad/angry/neutral\"}}";

            string finalPrompt = string.IsNullOrWhiteSpace(personalityPrompt) ? defaultPrompt : personalityPrompt;

            var field = typeof(GeminiAPIPersonality).GetField("_systemInstructions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            field?.SetValue(geminiAI, finalPrompt);
        }

        geminiAI.ClearChatHistory();
    }

    private void CheckPlayerProximity()
    {
        if (isInteracting) return;

        Collider[] colliders = Physics.OverlapSphere(
            interactionTrigger ? interactionTrigger.position : transform.position,
            interactionRadius);

        PlayerController detectedPlayer = null;

        foreach (Collider col in colliders)
        {
            detectedPlayer = col.GetComponent<PlayerController>();
            if (detectedPlayer != null)
            {
                nearbyPlayer = detectedPlayer;
                nearbyPlayer.SetCurrentInteractable(this);
                break;
            }
        }

        // Player left
        if (nearbyPlayer == null && detectedPlayer == null)
        {
            FindFirstObjectByType<PlayerController>()?.ClearCurrentInteractable();
        }

        // Update indicator
        if (interactionIndicator != null)
            interactionIndicator.SetActive(detectedPlayer != null);
    }

    public void StartInteraction()
    {
        if (isInteracting || dialogueManager == null) return;

        isInteracting = true;
        interactionIndicator?.SetActive(false);

        Debug.Log($"Starting interaction with {npcName}");
        dialogueManager.StartDialogue(this, initialGreeting);
    }

    public void EndInteraction()
    {
        if (!isInteracting) return;

        isInteracting = false;

        Debug.Log($"Ending interaction with {npcName}");
        geminiAI?.ClearChatHistory();

        if (interactionIndicator != null && nearbyPlayer != null)
            interactionIndicator.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != nearbyPlayer) return;

        nearbyPlayer.ClearCurrentInteractable();

        if (isInteracting)
        {
            if (dialogueManager != null)
                dialogueManager.EndDialogue();
            else
                EndInteraction();
        }

        nearbyPlayer = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            interactionTrigger ? interactionTrigger.position : transform.position,
            interactionRadius);
    }
}
