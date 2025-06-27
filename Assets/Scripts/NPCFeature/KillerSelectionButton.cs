using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillerSelectionButton : MonoBehaviour
{
    private NPCController associatedNPC;
    private MurderMysteryManager mysteryManager;

    public void Setup(NPCController npc, MurderMysteryManager manager)
    {
        associatedNPC = npc;
        mysteryManager = manager;

        // Setup button component
        Button button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Button component missing from prefab!");
            return;
        }

        // Setup text
        TextMeshProUGUI buttonText = GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = npc.NPCName;
            buttonText.raycastTarget = false;
        }
        else
        {
            Text regularText = GetComponentInChildren<Text>();
            if (regularText != null)
            {
                regularText.text = npc.NPCName;
            }
        }

        // Setup click listener
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);
        button.interactable = true;
    }

    private void OnButtonClicked()
    {
        if (mysteryManager != null && associatedNPC != null)
        {
            mysteryManager.SelectKiller(associatedNPC);
        }
    }
}
