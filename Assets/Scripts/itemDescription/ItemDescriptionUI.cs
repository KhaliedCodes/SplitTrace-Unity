using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDescriptionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;


    private void Awake()
    {
        Hide();
    }
    public void Show(ClueData clue)
    {
        if (clue == null) return;

        panel.SetActive(true);
        nameText.text = clue.clueName;
        descriptionText.text = clue.description;

    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
