using UnityEngine;

public class NoAudio : MonoBehaviour
{
    void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.DisableAllAudioSources();
        }
        else
        {
            Debug.LogWarning("SoundManager instance not found in scene!");
        }
    }
}
