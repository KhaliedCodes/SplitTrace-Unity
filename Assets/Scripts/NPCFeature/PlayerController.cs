using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
        if (npc != null)
        {
            currentInteractable = npc;
            ShowInteractionPrompt(true, npc.NPCName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<NPCController>() == currentInteractable)
        {
            currentInteractable = null;
            ShowInteractionPrompt(false);
        }
    }

    private void Update()
    {
        if (isInDialogue && Keyboard.current.escapeKey.wasPressedThisFrame)
            DialogueManager.Instance?.EndDialogue();
        
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            DisableControls();
            currentInteractable.StartInteraction();
        }
    }

    private void ShowInteractionPrompt(bool show, string npcName = "")
    {
        if (interactionPrompt == null) return;
        
        interactionPrompt.SetActive(show);
        if (show) interactionPromptText.text = $"Press E to talk with {npcName}";
    }

    public void SetCurrentInteractable(NPCController interactable)
    {
        if (!isInDialogue) currentInteractable = interactable;
    }

    public void ClearCurrentInteractable()
    {
        if (!isInDialogue) currentInteractable = null;
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
    }

    public void SetInDialogue(bool inDialogue)
    {
        isInDialogue = inDialogue;
        Cursor.lockState = inDialogue ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inDialogue;
    }
}