using UnityEngine;

public class LevelMusicLoader : MonoBehaviour
{
    [SerializeField] private string MusicName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudioClip("Music", MusicName, true);
        }
    }

}
