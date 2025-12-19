using System.Collections.Generic;
using UnityEngine;

namespace Eduzo.Games.Patterns.Audio
{
    [System.Serializable]
    public class PatternsSound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
    }

    public class PatternsAudioManager : MonoBehaviour
    {
        public static PatternsAudioManager Instance;

        public AudioSource sfxSource;
        public AudioSource musicSource;

        public List<PatternsSound> sfxSounds = new();
        public PatternsSound bgMusic;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlaySFX(string name)
        {
            var s = sfxSounds.Find(x => x.name == name);
            if (s == null)
            {
                Debug.LogWarning("SFX not found: " + name);
                return;
            }

            sfxSource.PlayOneShot(s.clip, s.volume);
        }

        public void PlayBgMusic(string name)
        {
            if (bgMusic == null) return;
            musicSource.clip = bgMusic.clip;
            musicSource.loop = bgMusic.loop;
            musicSource.volume = bgMusic.volume;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();
    }
}