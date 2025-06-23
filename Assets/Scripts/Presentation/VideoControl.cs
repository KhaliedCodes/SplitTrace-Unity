using UnityEngine;
using UnityEngine.Video;

public class VideoControl : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    private void Update()
    {
        // Toggle Play/Pause with the Space key
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }
    }

    public void TogglePlayPause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }
        else
        {
            videoPlayer.Play();
        }
    }
}
