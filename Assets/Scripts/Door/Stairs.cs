using UnityEngine;

public class Stairs : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameSceneManager.Instance.LoadLobby();
        }
    }
}
