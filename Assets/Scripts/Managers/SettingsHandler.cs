using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button lowQualityButton;
    [SerializeField] private Button mediumQualityButton;
    [SerializeField] private Button highQualityButton;

    private const string FullscreenPref = "Fullscreen";
    private const string QualityPref = "QualityLevel";

    void Start()
    {
        // Initialize fullscreen toggle
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenPref, 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        // Initialize quality buttons
        if (lowQualityButton != null)
            lowQualityButton.onClick.AddListener(() => SetQuality(0));
        if (mediumQualityButton != null)
            mediumQualityButton.onClick.AddListener(() => SetQuality(2));
        if (highQualityButton != null)
            highQualityButton.onClick.AddListener(() => SetQuality(5));

        // Apply saved quality level
        int savedQuality = PlayerPrefs.GetInt(QualityPref, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(savedQuality, true);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt(FullscreenPref, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetQuality(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        PlayerPrefs.SetInt(QualityPref, level);
        PlayerPrefs.Save();
    }
}
