using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipsSO", menuName = "ScriptableObjects/AudioClipsSO", order = 1)]
public class AudioClipsSO : ScriptableObject
{
    [System.Serializable]
    public class AudioCategoryData
    {
        public string category;
        public List<AudioClipData> audioClips;
    }

    [System.Serializable]
    public class AudioClipData
    {
        public string clipName;
        public AudioClip audioClip;
        //[Range(0f, 1f)]
        //public float volume;
    }

    public List<AudioCategoryData> audioCategories;

    public AudioClip GetAudioClip(string category, string clipName)
    {
        var categoryData = audioCategories.Find(cat => cat.category == category);
        if (categoryData != null)
        {
            var clipData = categoryData.audioClips.Find(clip => clip.clipName == clipName);
            if (clipData != null)
            {
                return clipData.audioClip;
            }
        }
        return null;
    }
}
