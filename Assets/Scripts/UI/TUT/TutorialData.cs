using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Objects/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string uniqueKey; // "EnemyTUT","WeaponTUT"

    [TextArea(2, 4)]
    public string tutorialText;
}
