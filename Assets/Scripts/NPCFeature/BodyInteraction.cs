using UnityEngine;
using TMPro;

public class BodyInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private string interactionPrompt = "Press E to examine the body";
    
    private bool playerInRange = false;
    private MurderMysteryManager mysteryManager;

    private void Start()
    {
        mysteryManager = MurderMysteryManager.Instance;
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && mysteryManager != null)
        {
            if (mysteryManager.CanInteractWithBody())
            {
                ShowInteractionPrompt(interactionPrompt);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    InteractWithBody();
                }
            }
            else if (mysteryManager.NPCsTalkedTo < mysteryManager.TotalNPCs)
            {
                string message = $"Talk to all NPCs first ({mysteryManager.NPCsTalkedTo}/{mysteryManager.TotalNPCs})";
                ShowInteractionPrompt(message);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            HideInteractionPrompt();
        }
    }

    private void ShowInteractionPrompt(string message)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            
            TextMeshProUGUI text = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = message;
            }
        }
    }

    private void HideInteractionPrompt()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void InteractWithBody()
    {
        if (mysteryManager != null)
        {
            mysteryManager.InteractWithBody();
            HideInteractionPrompt();
        }
    }
}