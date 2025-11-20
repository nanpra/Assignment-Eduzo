using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    public bool loop;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    public AudioSource sfxSource;      //taking only 1 audio source for sfx as have very limited audio clips are being played , othwerwise will use certain sources and pool them
    public AudioSource musicSource;

    [Header("SFX List")]
    public List<Sound> sfxSounds = new();

    [Header("Bg Music")]
    public Sound bgMusic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = sfxSounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("SFX not found: " + name);
            return;
        }

        sfxSource.volume = s.volume;
        sfxSource.loop = false;             // SFX should not loop normally
        sfxSource.PlayOneShot(s.clip, s.volume);
    }

    public void PlayBgMusic(string name)
    {
        Sound s = bgMusic;
        if (s == null)
        {
            Debug.LogWarning("Music not found: " + name);
            return;
        }

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.loop = s.loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}