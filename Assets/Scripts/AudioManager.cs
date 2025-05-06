using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip clickOkClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip grabClip;
    
    [Header("Music Settings")]
    [SerializeField] private List<AudioClip> musicTracks = new List<AudioClip>();
    [SerializeField] private int minLoopCount = 1;
    [SerializeField] private int maxLoopCount = 3;
    [SerializeField] private bool playRandomMusicOnStart = true;

    private Dictionary<GlobalSfx, AudioClip> _clipsDictionary = new Dictionary<GlobalSfx, AudioClip>();
    private int _currentTrackIndex = -1;
    private int _remainingLoops = 0;
    private bool _isRandomMusicPlaying = false;
    

    public static AudioManager instance;
    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    
    void Start()
    {
        _clipsDictionary[GlobalSfx.Click] = clickOkClip;
        _clipsDictionary[GlobalSfx.Error] = errorClip;
        _clipsDictionary[GlobalSfx.Grab] = grabClip;
        
        if (playRandomMusicOnStart && musicTracks.Count > 0)
        {
            StartRandomMusicPlayer();
        }
    }
    
    void Update()
    {
        if (_isRandomMusicPlaying && !musicAudioSource.isPlaying)
        {
            OnMusicTrackFinished();
        }
    }
    
    public void StartRandomMusicPlayer()
    {
        if (musicTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned to AudioManager");
            return;
        }
        
        _isRandomMusicPlaying = true;
        PlayNextRandomTrack();
    }
    
    public void StopRandomMusicPlayer()
    {
        _isRandomMusicPlaying = false;
        musicAudioSource.Stop();
    }
    
    private void PlayNextRandomTrack()
    {
        int newTrackIndex;
        if (musicTracks.Count == 1)
        {
            newTrackIndex = 0;
        }
        else
        {
            do
            {
                newTrackIndex = Random.Range(0, musicTracks.Count);
            } while (newTrackIndex == _currentTrackIndex);
        }
        
        _currentTrackIndex = newTrackIndex;
        _remainingLoops = Random.Range(minLoopCount, maxLoopCount + 1);
        
        AudioClip selectedTrack = musicTracks[_currentTrackIndex];
        musicAudioSource.clip = selectedTrack;
        musicAudioSource.Play();
        
        Debug.Log($"Playing music track: {selectedTrack.name}, loops remaining: {_remainingLoops}");
    }
    
    private void OnMusicTrackFinished()
    {
        _remainingLoops--;
        
        if (_remainingLoops <= 0)
        {
            PlayNextRandomTrack();
        }
        else
        {
            musicAudioSource.Play();
            Debug.Log($"Replaying music track: {musicAudioSource.clip.name}, loops remaining: {_remainingLoops}");
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        sfxAudioSource.PlayOneShot(clip);
    }

    public void ChangeMusic(AudioClip clip)
    {
        _isRandomMusicPlaying = false;
        musicAudioSource.clip = clip;
        musicAudioSource.Play();
    }

    public void PlaySfx(GlobalSfx clipKey)
    {
        sfxAudioSource.pitch = 1f;
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySfxWithPitch(GlobalSfx clipKey, float pitch)
    {
        sfxAudioSource.pitch = pitch;
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySfxRandomPitch(AudioClip clip)
    {
        sfxAudioSource.PlayOneShot(clip);
    }

    public void PlaySoundAtPosition(GlobalSfx clipKey, Vector3 pos)
    {
        _clipsDictionary.TryGetValue(clipKey, out AudioClip clip);
        AudioSource.PlayClipAtPoint(clip, pos);
    }

    public AudioSource GetSfxSource(GlobalSfx clipKey)
    {
        return sfxAudioSource;
    }
}

public enum GlobalSfx
{
    Click,
    Error,
    Grab
}