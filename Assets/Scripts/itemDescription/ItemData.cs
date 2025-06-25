using UnityEngine;


[CreateAssetMenu(fileName = "ClueData", menuName = "RPG/ClueData")]
public class ItemData : ScriptableObject
{
    public string clueName;
    [TextArea] public string description;

}
