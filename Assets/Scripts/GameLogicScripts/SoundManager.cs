using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private List<AudioSource> allAudioSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RefreshAudioSourceCache();   // find EVERY AudioSource in the scene (active + inactive)
    }

    /// <summary>
    /// Rescans the scene and caches all AudioSource components (including inactive objects).
    /// </summary>
    [ContextMenu("Refresh Audio Source Cache")]
    public void RefreshAudioSourceCache()
    {
        allAudioSources.Clear();
        allAudioSources.AddRange(FindObjectsOfType<AudioSource>(includeInactive: true));
    }

    /// <summary>
    /// Disables every cached AudioSource component.
    /// </summary>
    [ContextMenu("Disable All Audio Sources")]
    public void DisableAllAudioSources()
    {
        foreach (var src in allAudioSources)
        {
            if (!src) continue;
            src.enabled = false;
        }
    }
}
