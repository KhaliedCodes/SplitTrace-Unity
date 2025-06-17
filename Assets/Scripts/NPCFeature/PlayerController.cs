using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 3f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private GameObject interactionPrompt;
    
    private CharacterController characterController;
    private CustomThridPersonController thirdPersonController;
    private CustomStarterAssetsInputs starterAssetsInputs;
    private PlayerInput playerInput;
    private SphereCollider interactionCollider;
    
    // Track all nearby NPCs and find the closest one
    private List<NPCController> nearbyNPCs = new List<NPCController>();
    private NPCController currentInteractable;
    private bool isInDialogue;

    public bool IsInDialogue => isInDialogue;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<CustomThridPersonController>();
        starterAssetsInputs = GetComponent<CustomStarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();
        
        interactionCollider = gameObject.AddComponent<SphereCollider>();
        interactionCollider.radius = interactionRadius;
        interactionCollider.isTrigger = true;
        interactionCollider.center = Vector3.up * 0.5f;
        
        interactionPrompt?.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & interactionLayer) == 0) return;
        
        NPCController npc = other.GetComponent<NPCController>();
        if (npc != null && !nearbyNPCs.Contains(npc))
        {
            nearbyNPCs.Add(npc);
            UpdateCurrentInteractable();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NPCController npc = other.GetComponent<NPCController>();
        if (npc != null && nearbyNPCs.Contains(npc))
        {
            nearbyNPCs.Remove(npc);
            UpdateCurrentInteractable();
        }
    }

    private void UpdateCurrentInteractable()
    {
        if (isInDialogue) return; // Don't change interactable during dialogue
        
        // Clean up any destroyed NPCs
        nearbyNPCs.RemoveAll(npc => npc == null);
        
        NPCController closestNPC = null;
        float closestDistance = float.MaxValue;
        
        // Find the closest NPC that is within interaction radius
        foreach (NPCController npc in nearbyNPCs)
        {
            if (npc == null) continue;
            
            float distance = Vector3.Distance(transform.position, npc.transform.position);
            
            // Only consider NPCs within interaction radius
            if (distance <= interactionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestNPC = npc;
            }
        }
        
        // Update current interactable
        if (closestNPC != currentInteractable)
        {
            currentInteractable = closestNPC;
            
            if (currentInteractable != null)
            {
                ShowInteractionPrompt(true, currentInteractable.NPCName);
                Debug.Log($"Current interactable set to: {currentInteractable.NPCName}");
            }
            else
            {
                ShowInteractionPrompt(false);
                Debug.Log("No current interactable");
            }
        }
    }

    private void Update()
    {
        // Update closest NPC every frame (in case NPCs move)
        if (!isInDialogue && nearbyNPCs.Count > 0)
        {
            UpdateCurrentInteractable();
        }
        
        if (isInDialogue && Keyboard.current.escapeKey.wasPressedThisFrame)
            DialogueManager.Instance?.EndDialogue();
        
        // Add distance check before allowing interaction
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null && !isInDialogue)
        {
            float distanceToNPC = Vector3.Distance(transform.position, currentInteractable.transform.position);
            if (distanceToNPC <= interactionRadius)
            {
                Debug.Log($"Starting interaction with: {currentInteractable.NPCName}");
                DisableControls();
                currentInteractable.StartInteraction();
            }
            else
            {
                Debug.Log($"Too far from {currentInteractable.NPCName} to interact. Distance: {distanceToNPC:F2}, Required: {interactionRadius}");
            }
        }
    }

    private void ShowInteractionPrompt(bool show, string npcName = "")
    {
        if (interactionPrompt == null) return;
        
        interactionPrompt.SetActive(show);
        if (show && !string.IsNullOrEmpty(npcName)) 
        {
            interactionPromptText.text = $"Press E to talk with {npcName}";
        }
    }

    public void SetCurrentInteractable(NPCController interactable)
    {
        if (!isInDialogue) 
        {
            // Verify the interactable is within range before setting it
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, interactable.transform.position);
                if (distance <= interactionRadius)
                {
                    currentInteractable = interactable;
                    Debug.Log($"Manually set current interactable to: {interactable.NPCName}");
                }
                else
                {
                    Debug.Log($"Cannot set interactable - {interactable.NPCName} is too far away. Distance: {distance:F2}");
                }
            }
            else
            {
                currentInteractable = null;
                Debug.Log("Cleared current interactable");
            }
        }
    }

    public void ClearCurrentInteractable()
    {
        if (!isInDialogue) 
        {
            currentInteractable = null;
            nearbyNPCs.Clear();
            ShowInteractionPrompt(false);
            Debug.Log("Cleared current interactable");
        }
    }

    public void DisableControls()
    {
        isInDialogue = true;
        thirdPersonController.enabled = false;
        starterAssetsInputs.enabled = false;
        playerInput.enabled = false;
    }

    public void EnableControls()
    {
        thirdPersonController.enabled = true;
        starterAssetsInputs.enabled = true;
        playerInput.enabled = true;
        isInDialogue = false;
        
        // Refresh interactable after dialogue ends
        UpdateCurrentInteractable();
    }

    public void SetInDialogue(bool inDialogue)
    {
        isInDialogue = inDialogue;
        Cursor.lockState = inDialogue ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inDialogue;
    }

    // Public method to get current interactable (for debugging)
    public NPCController GetCurrentInteractable()
    {
        return currentInteractable;
    }
}