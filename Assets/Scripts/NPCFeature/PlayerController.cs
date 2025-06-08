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
    [SerializeField] private float interactionCooldown = 0.5f;

    private CharacterController characterController;
    private NPCController currentInteractable;
    private CustomThridPersonController thirdPersonController;
    private CustomStarterAssetsInputs starterAssetsInputs;
    private PlayerInput playerInput;
    private float lastInteractionTime;
    private bool isInDialogue = false;
    public bool IsInDialogue => isInDialogue;
    bool collectingProcess, thereExistItem;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        thirdPersonController = GetComponent<CustomThridPersonController>();
        starterAssetsInputs = GetComponent<CustomStarterAssetsInputs>();
        playerInput = GetComponent<PlayerInput>();
        interactionPrompt?.SetActive(false);
    }

    private void Update()
    {
       
        if (isInDialogue)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                DialogueManager.Instance?.EndDialogue();
                EnableControls();

            }
            return;
        }

        CheckForInteractables();
        HandleInteractionInput();
    }

    private void CheckForInteractables()
    {
        currentInteractable = null;
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);

        NPCController closestNPC = null;
        float closestDistance = float.MaxValue;

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

        currentInteractable = closestNPC;
        ShowInteractionPrompt(closestNPC != null, closestNPC?.NPCName);
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.E) && Time.time > lastInteractionTime + interactionCooldown)
        {
            lastInteractionTime = Time.time;
            if (currentInteractable != null)
            {
                DisableControls();
                currentInteractable.StartInteraction();
            }
        }
    }

    private void ShowInteractionPrompt(bool show, string npcName = "")
    {
        interactionPrompt?.SetActive(show);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    public void OnCollect() { 
        playerInput.enabled=false;
    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Health")
        {
            
            CallingUIHint();
            if (Input.GetKey(KeyCode.C))
            {
                gameObject.GetComponent<PlayerHealth>().UpdateNumberOfHealthItem();
                other.gameObject.SetActive(false);
                AudioManager.Instance.PlayAudioClip("SFX", "CollectSound", false);
                HideUIHint();
              
            }
        }
        if (other.tag == "Ammo")
        {
           
            CallingUIHint();
            if (Input.GetKey(KeyCode.C))
            {
                gameObject.GetComponent<WeaponManager>().UpdateAmmo(other.gameObject.GetComponent<AmmoItem>().ItemAmount);
                other.gameObject.SetActive(false);
                AudioManager.Instance.PlayAudioClip("SFX", "CollectSound", false);
                HideUIHint();
               
            }
        }
        if (other.tag == "Log") {

           
            CallingUIHint();
            if (Input.GetKey(KeyCode.C))
            {
                gameObject.GetComponent<LogsSystem>().Logs.Add(other.gameObject.GetComponent<LogItem>());
                other.gameObject.SetActive(false);
                AudioManager.Instance.PlayAudioClip("SFX", "CollectSound", false);
                HideUIHint();
               
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        HideUIHint();

    }

    public void CallingUIHint()
    {
        UiManager.Instance.DisplayPickUp();
    }

    public void HideUIHint()
    {
        UiManager.Instance.ClosePickUpPanel();
    }
}