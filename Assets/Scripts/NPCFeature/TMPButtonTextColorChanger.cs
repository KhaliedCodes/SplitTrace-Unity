using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TMPButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public TextMeshProUGUI text;
    
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.85f, 0.85f, 1f);     // Light blue
    public Color selectedColor = new Color(0.7f, 0.9f, 1f);    // Cyan-ish

    private bool isSelected = false;

    void Reset()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            SetTextColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            SetTextColor(normalColor);
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        SetTextColor(selectedColor);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        SetTextColor(normalColor);
    }

    private void SetTextColor(Color color)
    {
        if (text != null)
        {
            text.color = color;
        }
    }
}
