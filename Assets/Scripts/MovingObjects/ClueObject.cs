using UnityEngine;

public class ClueObject : MonoBehaviour
{
    public bool Highlighted = false; // Indicates if the clue object is highlighted
    [SerializeField] private Material highlightMaterial; // Material to use when highlighted
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void SetHighlighted(bool highlighted)
    {
        Highlighted = highlighted;
        if (highlighted)
        {
            // Apply the highlight material
            GetComponent<Renderer>().material = highlightMaterial;
        }
        else
        {
            // Reset to the original material (assuming it is stored or can be retrieved)
            GetComponent<Renderer>().material = GetComponent<Renderer>().sharedMaterial;
        }
    }
}
