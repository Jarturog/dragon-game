using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    public Dictionary<string, AudioClip> sfxClips = new();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllSounds()
    {
        // Load all sounds from Resources/SFX/
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sonidos");
        foreach (AudioClip clip in clips)
        {
            sfxClips[clip.name] = clip;
        }
    }

    public void PlaySFX(string clipName)
    {
        if (sfxClips.TryGetValue(clipName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Sound {clipName} not found in AudioManager.");
        }
    }

    public void PlayMusic(string music)
    {
        if (sfxClips.TryGetValue(music, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning($"Music {music} not found in AudioManager.");
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}