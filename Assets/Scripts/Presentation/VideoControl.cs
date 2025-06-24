using UnityEngine;
using UnityEngine.Video;
using System.Collections.Generic;

public class VideoControl : MonoBehaviour
{
    [SerializeField] private List<VideoPlayer> videoPlayers;

    // Select which video player to control
    private int selectedIndex = 0;

    private void Update()
    {
        // Number keys to select player (0–9)
        for (int i = 0; i < videoPlayers.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                selectedIndex = i;
                Debug.Log($"Selected video player: {i}");
            }
        }

        // Space toggles play/pause on the selected video player
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause(selectedIndex);
        }
    }

    public void TogglePlayPause(int index)
    {
        if (index >= 0 && index < videoPlayers.Count)
        {
            var player = videoPlayers[index];
            if (player.isPlaying)
                player.Pause();
            else
                player.Play();
        }
    }

    public void PlayVideo(int index)
    {
        if (index >= 0 && index < videoPlayers.Count)
            videoPlayers[index].Play();
    }

    public void PauseVideo(int index)
    {
        if (index >= 0 && index < videoPlayers.Count)
            videoPlayers[index].Pause();
    }

    public void SelectVideoPlayer(int index)
    {
        if (index >= 0 && index < videoPlayers.Count)
            selectedIndex = index;
    }
}
