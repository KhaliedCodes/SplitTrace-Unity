using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class MurderMysteryManager : MonoBehaviour
{
    [Header("Murder Mystery Configuration")]
    [SerializeField] private List<NPCController> allNPCs = new List<NPCController>();
    [SerializeField] private NPCController killerNPC; // Assign this in the inspector
    [SerializeField] private GameObject victimBody; // The body GameObject
    [SerializeField] private float bodyInteractionRadius = 3f;
    
    [Header("UI References")]
    [SerializeField] private GameObject killerSelectionUI;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab; // YOUR BUTTON PREFAB
    [SerializeField] private GameObject congratulationsPanel;
    [SerializeField] private GameObject wrongChoicePanel;
    
    private HashSet<NPCController> talkedToNPCs = new HashSet<NPCController>();
    private bool hasInteractedWithBody = false;
    private bool mysteryResolved = false;
    private SphereCollider bodyTrigger;
    private PlayerController player;
    
    public static MurderMysteryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupBodyInteraction();

        // Find player
        player = FindFirstObjectByType<PlayerController>();

        // Auto-find all NPCs if list is empty
        if (allNPCs.Count == 0)
        {
            allNPCs = FindObjectsByType<NPCController>(FindObjectsSortMode.None).ToList();
        }

        // Ensure UI is hidden at start
        if (killerSelectionUI != null)
            killerSelectionUI.SetActive(false);
        if (congratulationsPanel != null)
            congratulationsPanel.SetActive(false);
        if (wrongChoicePanel != null)
            wrongChoicePanel.SetActive(false);
        
    }

    private void Update()
    {
        // Allow canceling body interaction with ESC
        if (killerSelectionUI != null && killerSelectionUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBodyInteraction();
        }
    }

    public void CancelBodyInteraction()
    {
        Debug.Log("Body interaction canceled by player.");

        if (killerSelectionUI != null)
            killerSelectionUI.SetActive(false);
        
        hasInteractedWithBody = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetupBodyInteraction()
    {
        if (victimBody != null)
        {
            bodyTrigger = victimBody.GetComponent<SphereCollider>();
            if (bodyTrigger == null)
            {
                bodyTrigger = victimBody.AddComponent<SphereCollider>();
            }
            bodyTrigger.radius = bodyInteractionRadius;
            bodyTrigger.isTrigger = true;

            victimBody.tag = "VictimBody";

            BodyInteraction bodyInteraction = victimBody.GetComponent<BodyInteraction>();
            if (bodyInteraction == null)
            {
                bodyInteraction = victimBody.AddComponent<BodyInteraction>();
            }
        }
    }
    
    public void RegisterNPCInteraction(NPCController npc)
    {
        if (npc != null && !talkedToNPCs.Contains(npc))
        {
            talkedToNPCs.Add(npc);
            Debug.Log($"Player talked to {npc.NPCName}. Total NPCs talked to: {talkedToNPCs.Count}/{allNPCs.Count}");
            CheckInvestigationProgress();
        }
    }
    
    private void CheckInvestigationProgress()
    {
        if (talkedToNPCs.Count >= allNPCs.Count)
        {
            Debug.Log("Player has talked to all NPCs. Body interaction is now available.");
        }
    }
    
    public bool CanInteractWithBody()
    {
        return talkedToNPCs.Count >= allNPCs.Count && !hasInteractedWithBody && !mysteryResolved;
    }
    
    public void InteractWithBody()
    {
        if (!CanInteractWithBody())
        {
            if (talkedToNPCs.Count < allNPCs.Count)
            {
                Debug.Log($"You need to talk to all NPCs first. Talked to: {talkedToNPCs.Count}/{allNPCs.Count}");
            }
            return;
        }
        
        hasInteractedWithBody = true;
        ShowKillerSelectionUI();
    }
    
    private void ShowKillerSelectionUI()
    {
        if (killerSelectionUI == null || buttonContainer == null)
        {
            Debug.LogError("UI components not assigned in MurderMysteryManager!");
            return;
        }

        if (buttonPrefab == null)
        {
            Debug.LogError("Button prefab not assigned! Please assign your button prefab in the inspector.");
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create buttons for each NPC using ONLY the prefab
        foreach (NPCController npc in allNPCs)
        {
            if (npc != null)
            {
                Debug.Log($"Creating button for NPC: {npc.NPCName}");
                
                // SIMPLY instantiate your prefab - no custom creation!
                GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
                buttonObj.name = $"Button_{npc.NPCName}";
                
                // Get or add the KillerSelectionButton component
                KillerSelectionButton buttonScript = buttonObj.GetComponent<KillerSelectionButton>();
                if (buttonScript == null)
                {
                    buttonScript = buttonObj.AddComponent<KillerSelectionButton>();
                }
                
                // Setup the button with NPC data
                buttonScript.Setup(npc, this);
            }
        }
        
        // Show UI and setup interaction
        killerSelectionUI.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log("Killer selection UI shown with prefab buttons.");
    }
    
    public void SelectKiller(NPCController selectedNPC)
    {
        if (mysteryResolved) return;
        
        mysteryResolved = true;
        killerSelectionUI.SetActive(false);
        
        if (selectedNPC == killerNPC)
        {
            ShowCongratulations();
            Debug.Log($"Correct! {selectedNPC.NPCName} was indeed the killer!");
        }
        else
        {
            ShowWrongChoice(selectedNPC);
            Debug.Log($"Wrong! {selectedNPC.NPCName} was not the killer. The real killer was {killerNPC.NPCName}.");
        }
    }
    
    private void ShowCongratulations()
    {
        if (congratulationsPanel != null)
        {
            congratulationsPanel.SetActive(true);
            
            UnityEngine.UI.Text congratsText = congratulationsPanel.GetComponentInChildren<UnityEngine.UI.Text>();
            if (congratsText != null)
            {
                congratsText.text = $"Congratulations! You correctly identified {killerNPC.NPCName} as the killer!";
            }
        }
    }
    
    private void ShowWrongChoice(NPCController selectedNPC)
    {
        if (wrongChoicePanel != null)
        {
            wrongChoicePanel.SetActive(true);
            
            UnityEngine.UI.Text wrongText = wrongChoicePanel.GetComponentInChildren<UnityEngine.UI.Text>();
            if (wrongText != null)
            {
                wrongText.text = $"Wrong choice! {selectedNPC.NPCName} was not the killer. The real killer was {killerNPC.NPCName}.";
            }
        }
    }
    
    public void CloseResultPanel()
    {
        Time.timeScale = 1f;
        
        if (congratulationsPanel != null)
            congratulationsPanel.SetActive(false);
        if (wrongChoicePanel != null)
            wrongChoicePanel.SetActive(false);
            
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void RestartInvestigation()
    {
        talkedToNPCs.Clear();
        hasInteractedWithBody = false;
        mysteryResolved = false;
        
        if (killerSelectionUI != null)
            killerSelectionUI.SetActive(false);
        if (congratulationsPanel != null)
            congratulationsPanel.SetActive(false);
        if (wrongChoicePanel != null)
            wrongChoicePanel.SetActive(false);
            
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        Debug.Log("Investigation restarted. Talk to all NPCs again!");
    }
    
    // Public getters
    public int NPCsTalkedTo => talkedToNPCs.Count;
    public int TotalNPCs => allNPCs.Count;
    public bool HasTalkedToAllNPCs => talkedToNPCs.Count >= allNPCs.Count;
    public bool MysteryResolved => mysteryResolved;
    public string KillerName => killerNPC?.NPCName ?? "Unknown";
}