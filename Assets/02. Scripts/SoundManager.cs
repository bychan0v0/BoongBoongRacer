using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Idle,
    LowOff,
    LowOn,
    MidOff,
    MidOn,
    HighOff,
    HighOn,
    MaxRPM
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Engine Sounds")]
    public AudioClip idle;
    public AudioClip lowOff;
    public AudioClip lowOn;
    public AudioClip midOff;
    public AudioClip midOn;
    public AudioClip highOff;
    public AudioClip highOn;
    public AudioClip maxRPM;
    
    private AudioSource idleSource;
    private AudioSource lowOffSource;
    private AudioSource lowOnSource;
    private AudioSource midOffSource;
    private AudioSource midOnSource;
    private AudioSource highOffSource;
    private AudioSource highOnSource;
    private AudioSource maxRPMSource;

    private AudioSource sfxSource;
    private AudioSource bgmSource;

    public float audioVolume = 1.0f;
    private Dictionary<SoundType, AudioClip> audioClips;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        idleSource = gameObject.AddComponent<AudioSource>();
        lowOnSource = gameObject.AddComponent<AudioSource>();
        lowOffSource = gameObject.AddComponent<AudioSource>();
        midOnSource = gameObject.AddComponent<AudioSource>();
        midOffSource = gameObject.AddComponent<AudioSource>();
        highOnSource = gameObject.AddComponent<AudioSource>();
        highOffSource = gameObject.AddComponent<AudioSource>();
        maxRPMSource = gameObject.AddComponent<AudioSource>();
        
        sfxSource = gameObject.AddComponent<AudioSource>();
        bgmSource = gameObject.AddComponent<AudioSource>();

        sfxSource.volume = audioVolume;
        bgmSource.volume = audioVolume;
        bgmSource.loop = true;
    }

    public void PlaySound(SoundType soundType)
    {
        if (audioClips.ContainsKey(soundType))
        {
            sfxSource.PlayOneShot(audioClips[soundType]);
        }
        else
        {
            Debug.LogWarning($"[SFX] Sound not found for {soundType}");
        }
    }

    public void PlayBGM(SoundType soundType)
    {
        if (!audioClips.ContainsKey(soundType))
        {
            Debug.LogWarning($"[BGM] Sound not found for {soundType}");
            return;
        }

        AudioClip newClip = audioClips[soundType];

        if (bgmSource.clip == newClip) return; // 같은 배경음이면 재생하지 않음

        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
