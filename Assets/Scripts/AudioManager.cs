using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip clickOkClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip grabClip;
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string lowpassParameterName = "Lowpass";
    [SerializeField] private float muffledLowPassCutoff = 100f;
    [SerializeField] private float normalLowPassCutoff = 5000f;
    
    [Header("Music Settings")]
    [SerializeField] private List<AudioClip> musicTracks = new List<AudioClip>();
    [SerializeField] private int minLoopCount = 1;
    [SerializeField] private int maxLoopCount = 3;
    [SerializeField] private bool playRandomMusicOnStart = true;
    
    [Header("Timer Music Settings")]
    [SerializeField] private float maxMusicPitch = 1.5f;
    [SerializeField] private float normalMusicPitch = 1.0f;
    [SerializeField] private float highTensionTimeThreshold = 15f;
    [SerializeField] private float mediumTensionTimeThreshold = 30f;
    [SerializeField] private float mediumTensionPitch = 1.1f;

    private Dictionary<GlobalSfx, AudioClip> _clipsDictionary = new Dictionary<GlobalSfx, AudioClip>();
    private int _currentTrackIndex = -1;
    private int _remainingLoops = 0;
    private bool _isRandomMusicPlaying = false;
    private float _defaultMusicVolume;
    private float _defaultSfxVolume;
    private float _lastRemainingTime = 0f;
    private bool _isMuffled = false;
    

    public static AudioManager instance;
    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);
        
        _isMuffled = PlayerPrefs.GetInt("SoundMuffled", 0) == 1;

    }
    
    void Start()
    {
        _clipsDictionary[GlobalSfx.Click] = clickOkClip;
        _clipsDictionary[GlobalSfx.Error] = errorClip;
        _clipsDictionary[GlobalSfx.Grab] = grabClip;
        
        _defaultMusicVolume = musicAudioSource.volume;
        _defaultSfxVolume = sfxAudioSource.volume;
        
        if (_isMuffled)
        {
            ApplyMuffledVolume();
        }
        else
        {
            RestoreNormalVolume();
        }
        
        if (playRandomMusicOnStart && musicTracks.Count > 0)
        {
            StartRandomMusicPlayer();
        }
        
        TimerManager.onTimerChanged += AdjustMusicPitchBasedOnTime;
    }
    
    private void OnDestroy()
    {
        TimerManager.onTimerChanged -= AdjustMusicPitchBasedOnTime;
    }
    
    void Update()
    {
        if (_isRandomMusicPlaying && !musicAudioSource.isPlaying)
        {
            OnMusicTrackFinished();
        }
    }
    
    public bool IsMuffled()
    {
        return _isMuffled;
    }
    
    public void ToggleMuffledSound()
    {
        _isMuffled = !_isMuffled;
        
        if (_isMuffled)
        {
            ApplyMuffledVolume();
        }
        else
        {
            RestoreNormalVolume();
        }
        
        PlayerPrefs.SetInt("SoundMuffled", _isMuffled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log("Muffled sound toggled: " + _isMuffled);
    }
    
    private void ApplyMuffledVolume()
    {
        bool success = audioMixer.SetFloat(lowpassParameterName, muffledLowPassCutoff);
        Debug.Log("Applied muffled volume. Success: " + success + ", Value: " + muffledLowPassCutoff);
    }
    
    private void RestoreNormalVolume()
    {
        bool success = audioMixer.SetFloat(lowpassParameterName, normalLowPassCutoff);
        Debug.Log("Restored normal volume. Success: " + success + ", Value: " + normalLowPassCutoff);
    }
    
    private void AdjustMusicPitchBasedOnTime()
    {
        float remainingTime = TimerManager.GetRemainingTime();
        
        if (Mathf.Abs(_lastRemainingTime - remainingTime) < 0.1f)
            return;
            
        _lastRemainingTime = remainingTime;
        
        if (remainingTime <= highTensionTimeThreshold)
        {
            float tensionFactor = 1 - (remainingTime / highTensionTimeThreshold);
            float newPitch = Mathf.Lerp(mediumTensionPitch, maxMusicPitch, tensionFactor);
            musicAudioSource.pitch = Mathf.Clamp(newPitch, mediumTensionPitch, maxMusicPitch + 0.1f);
        }
        else if (remainingTime <= mediumTensionTimeThreshold)
        {
            float tensionFactor = 1 - ((remainingTime - highTensionTimeThreshold) / (mediumTensionTimeThreshold - highTensionTimeThreshold));
            float newPitch = Mathf.Lerp(normalMusicPitch, mediumTensionPitch, tensionFactor);
            musicAudioSource.pitch = newPitch;
        }
        else
        {
            musicAudioSource.pitch = normalMusicPitch;
        }
    }
    
    private Coroutine _pitchResetCoroutine;
    public void ResetMusicPitch()
    {
        if (_pitchResetCoroutine != null)
            StopCoroutine(_pitchResetCoroutine);
        
        _pitchResetCoroutine = StartCoroutine(GraduallyResetPitch());
    }


    private IEnumerator GraduallyResetPitch()
    {
        float startPitch = musicAudioSource.pitch;
        float duration = 1.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            musicAudioSource.pitch = Mathf.Lerp(startPitch, normalMusicPitch, t);
            
            yield return null;
        }
        
        musicAudioSource.pitch = normalMusicPitch;
        _pitchResetCoroutine = null;
    }
    
    private Coroutine _musicCoroutine;

    public void StartRandomMusicPlayer()
    {
        if (musicTracks.Count == 0)
        {
            Debug.LogWarning("No music tracks assigned to AudioManager");
            return;
        }
        
        _isRandomMusicPlaying = true;
        
        if (_musicCoroutine != null)
            StopCoroutine(_musicCoroutine);
        
        _musicCoroutine = StartCoroutine(PlayRandomMusicSequence());
    }
    
    public void StopRandomMusicPlayer()
    {
        _isRandomMusicPlaying = false;
        
        if (_musicCoroutine != null)
            StopCoroutine(_musicCoroutine);
        
        musicAudioSource.Stop();
    }
    
    private IEnumerator PlayRandomMusicSequence()
    {
        while (_isRandomMusicPlaying)
        {
            PlayNextRandomTrack();
        
            float trackDuration = musicAudioSource.clip.length;
        
            for (int i = 0; i < _remainingLoops; i++)
            {
                yield return new WaitForSeconds(trackDuration);
            
                if (!_isRandomMusicPlaying)
                    yield break;
                
                if (i < _remainingLoops - 1)
                {
                    musicAudioSource.Play();
                    Debug.Log($"Replaying music track: {musicAudioSource.clip.name}, loops remaining: {_remainingLoops - i - 1}");
                }
            }
        }
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
        musicAudioSource.pitch = normalMusicPitch;
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