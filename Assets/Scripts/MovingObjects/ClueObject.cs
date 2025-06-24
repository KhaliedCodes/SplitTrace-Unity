using UnityEngine;

public class ClueObject : MonoBehaviour
{
    [Header("Highlighting")]
    public bool Highlighted = false;
    [SerializeField] private Material highlightMaterial;
    private Material originalMaterial;

    [Header("Clue Data")]
    public ClueData clueData;

    [Header("UI References")]
    [SerializeField] private ItemDescriptionUI itemUI;  // Shows full description
    [SerializeField] private GameObject pressEPromptUI; // "Press E" UI

    private bool playerInTrigger = false;
    private bool descriptionVisible = false;

    private void Start()
    {
        originalMaterial = GetComponent<Renderer>().material;

        if (itemUI == null)
            itemUI = FindFirstObjectByType<ItemDescriptionUI>();

        if (pressEPromptUI != null)
            pressEPromptUI.SetActive(false);
    }

    private void Update()
    {
        if (playerInTrigger && Highlighted && Input.GetKeyDown(KeyCode.E))
        {
            ShowDescription();
        }
    }

    private void ShowDescription()
    {
        descriptionVisible = true;
        pressEPromptUI?.SetActive(false);
        itemUI?.Show(clueData);
    }

    private void HideDescription()
    {
        descriptionVisible = false;
        itemUI?.Hide();
    }

    public void SetHighlighted(bool highlighted)
    {
        Highlighted = highlighted;

        if (highlighted)
        {
            GetComponent<Renderer>().material = highlightMaterial;

            if (playerInTrigger && !descriptionVisible)
                pressEPromptUI?.SetActive(true);
        }
        else
        {
            GetComponent<Renderer>().material = originalMaterial;
            HideDescription();
            pressEPromptUI?.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;

            if (Highlighted && !descriptionVisible)
                pressEPromptUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            pressEPromptUI?.SetActive(false);
            HideDescription();
        }
    }
}
