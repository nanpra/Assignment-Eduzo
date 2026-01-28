using System.Collections.Generic;
using UnityEngine;

namespace Eduzo.Games.Archery.Audio
{
    [System.Serializable]
    public class ArcherySound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop;
    }

    public class ArcheryAudioManager : MonoBehaviour
    {
        public static ArcheryAudioManager Instance;

        public AudioSource sfxSource;
        public AudioSource musicSource;

        public List<ArcherySound> sfxSounds = new();
        public ArcherySound bgMusic;

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