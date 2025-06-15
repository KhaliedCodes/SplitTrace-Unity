using UnityEngine;


[CreateAssetMenu(fileName = "ClueData", menuName = "RPG/ClueData")]
public class ClueData : ScriptableObject
{
    public string clueName;
    [TextArea] public string description;

}
