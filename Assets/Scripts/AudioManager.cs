using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    
    [Header("UI Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    
    private Dictionary<string, AudioClip> sfxClips = new();
    private HashSet<string> currentlyPlayingSounds = new HashSet<string>();
    
    private float pitchOriginal;
    
    // Volume keys for PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllSounds();
            InitializeSliders();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void LoadAllSounds()
    {
        pitchOriginal = sfxSource.pitch;
        
        // Load all sounds from Resources/SFX/
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sonidos");
        foreach (AudioClip clip in clips)
        {
            sfxClips[clip.name] = clip;
        }
    }
    
    private void InitializeSliders()
    {
        // Load saved volume values or use default (1.0)
        float savedMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.25f);
        
        // Set audio source volumes
        musicSource.volume = savedMusicVolume;
        sfxSource.volume = savedSFXVolume;
        
        // Initialize sliders if they exist
        if (musicSlider != null)
        {
            musicSlider.value = savedMusicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }
    
    // Method to update sliders if they're assigned after initialization
    public void RefreshSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = musicSource.volume;
            if (musicSlider.onValueChanged.GetPersistentEventCount() == 0)
            {
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
        }
        
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxSource.volume;
            if (sfxSlider.onValueChanged.GetPersistentEventCount() == 0)
            {
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }
    }
    
    public void PlaySFX(string clipName)
    {
        PlaySFX(clipName, true, pitchOriginal);
    }
    
    public void PlaySFX(string clipName, bool playEvenIfPlaying)
    {
        PlaySFX(clipName, playEvenIfPlaying, pitchOriginal);
    }
    
    public void PlaySFX(string clipName, bool playEvenIfPlaying, float speed)
    {
        if (sfxClips.TryGetValue(clipName, out AudioClip clip))
        {
            // Check if sound is currently playing and we don't want to play it again
            if (!playEvenIfPlaying && currentlyPlayingSounds.Contains(clipName))
            {
                return;
            }
            
            // Add to currently playing set
            currentlyPlayingSounds.Add(clipName);
            
            // Create a temporary GameObject with AudioSource for this sound
            GameObject tempAudio = new GameObject("TempAudio_" + clipName);
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.pitch = speed;
            tempSource.volume = sfxSource.volume; // This will now use the current SFX volume
            tempSource.Play();
            
            // Start coroutine to clean up after sound finishes
            StartCoroutine(CleanupSound(tempAudio, clipName, clip.length / speed));
            Debug.Log($"Playing sound {clipName}.");
        }
        else
        {
            Debug.LogWarning($"Sound {clipName} not found in AudioManager.");
        }
    }
    
    private IEnumerator CleanupSound(GameObject tempAudio, string clipName, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        // Remove from currently playing set
        currentlyPlayingSounds.Remove(clipName);
        
        // Destroy the temporary audio object
        if (tempAudio != null)
        {
            Destroy(tempAudio);
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
    
    // Additional utility methods
    public bool IsSoundPlaying(string clipName)
    {
        return currentlyPlayingSounds.Contains(clipName);
    }
    
    public void StopAllSFX()
    {
        // Find all temporary audio objects and destroy them
        GameObject[] tempAudios = GameObject.FindGameObjectsWithTag("TempAudio");
        foreach (GameObject tempAudio in tempAudios)
        {
            Destroy(tempAudio);
        }
        
        // Clear the tracking set
        currentlyPlayingSounds.Clear();
    }
    
    // Getter methods for current volumes
    public float GetMusicVolume()
    {
        return musicSource.volume;
    }
    
    public float GetSFXVolume()
    {
        return sfxSource.volume;
    }
}