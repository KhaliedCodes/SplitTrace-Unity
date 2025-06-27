using UnityEngine;

public class Stairs : MonoBehaviour
{
    [SerializeField] private int LevelIndex;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (LevelIndex)
            {
                case 0:
                    GameSceneManager.Instance.LoadMainMenu();
                    break;

                case 2:
                    GameSceneManager.Instance.LoadLobby();
                    break;

                case (3):

                    GameSceneManager.Instance.Load3rdLevel();
                    break;
                
            }
        }
    }
}
