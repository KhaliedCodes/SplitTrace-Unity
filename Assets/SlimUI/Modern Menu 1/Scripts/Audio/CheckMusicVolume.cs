using UnityEngine;
using UnityEngine.UI;

namespace SlimUI.ModernMenu
{
    public class CheckMusicVolume : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;

        private void Awake()
        {
            if (volumeSlider == null)
            {
                Debug.LogError("Volume Slider not assigned.");
                return;
            }

            float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            volumeSlider.value = savedVolume;

            volumeSlider.onValueChanged.AddListener(HandleSliderChanged);
        }

        private void HandleSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UpdateVolume(value);
            }

            PlayerPrefs.SetFloat("MusicVolume", value);

        }

        private void OnDestroy()
        {
            volumeSlider.onValueChanged.RemoveListener(HandleSliderChanged);
        }
    }
}
