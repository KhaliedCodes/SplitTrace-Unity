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
    [SerializeField] private float interactionCooldown = 0.5f; // Prevent spam clicking

    // References
    private CharacterController characterController;
    private NPCController currentInteractable;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private PlayerInput playerInput;
    private float lastInteractionTime;

    // Track if player is in dialogue
    private bool isInDialogue = false;
    public bool IsInDialogue => isInDialogue;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (!characterController)
            characterController = gameObject.AddComponent<CharacterController>();

        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();

        if (interactionPrompt)
            interactionPrompt.SetActive(false);

        lastInteractionTime = -interactionCooldown;
    }

    private void Update()
    {

        if (isInDialogue)
        {
            // Allow Escape to end dialogue
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                DialogueManager.Instance?.EndDialogue();
            }

            // Skip everything else while in dialogue
            return;
        }

        CheckForInteractables();
        HandleInteractionInput();
    }

    private void CheckForInteractables()
    {
        currentInteractable = null;

        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);

        if (colliders.Length > 0)
        {
            float closestDistance = float.MaxValue;
            NPCController closestNPC = null;

            foreach (Collider col in colliders)
            {
                NPCController npc = col.GetComponent<NPCController>();
                if (npc != null)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }

            if (closestNPC != null)
            {
                currentInteractable = closestNPC;
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (isInDialogue)
        {
            return;

        }
        else
        {
        if (Input.GetKeyDown(KeyCode.E) && Time.time > lastInteractionTime + interactionCooldown)
        {
            lastInteractionTime = Time.time;

            if (currentInteractable != null)
            {
                DisableControls();
                currentInteractable.StartInteraction();

                ShowInteractionPrompt(true);
            }
        }

        }
    }

    private void ShowInteractionPrompt(bool show, string npcName = "")
    {
        if (interactionPrompt)
            interactionPrompt.SetActive(show);

        if (interactionPromptText && show)
            interactionPromptText.text = $"Press E to talk with {npcName}";
    }

    public void SetCurrentInteractable(NPCController interactable)
    {
        if (!isInDialogue)
        {
            currentInteractable = interactable;
            ShowInteractionPrompt(true, interactable.name);
        }
    }

    public void ClearCurrentInteractable()
    {
        if (!isInDialogue)
        {
            currentInteractable = null;
            ShowInteractionPrompt(false);
        }
    }

    public void DisableControls()
    {
        isInDialogue = true;

        if (thirdPersonController) thirdPersonController.enabled = false;
        if (starterAssetsInputs) starterAssetsInputs.enabled = false;
        if (playerInput) playerInput.enabled = false;
    }

    public void EnableControls()
    {
        if (thirdPersonController) thirdPersonController.enabled = true;
        if (starterAssetsInputs) starterAssetsInputs.enabled = true;
        if (playerInput) playerInput.enabled = true;

        isInDialogue = false;
        ShowInteractionPrompt(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
