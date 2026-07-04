using UnityEngine;

namespace PuzzlePals.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameLevelMusic;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip victoryChimeClip;
        [SerializeField] private AudioClip puzzleActivateClip;
        [SerializeField] private AudioClip playerJumpClip;
        [SerializeField] private AudioClip playerThrowClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create AudioSources dynamically if not pre-assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            LoadVolumes();
        }

        public void PlayMusic(bool inGame)
        {
            AudioClip clipToPlay = inGame ? gameLevelMusic : mainMenuMusic;
            
            if (musicSource.clip == clipToPlay && musicSource.isPlaying) return;

            musicSource.Stop();
            musicSource.clip = clipToPlay;
            if (clipToPlay != null)
            {
                musicSource.Play();
            }
        }

        public void PlaySFX(string sfxName)
        {
            AudioClip clip = null;
            switch (sfxName.ToLower())
            {
                case "click":
                    clip = buttonClickClip;
                    break;
                case "victory":
                    clip = victoryChimeClip;
                    break;
                case "puzzle_activate":
                    clip = puzzleActivateClip;
                    break;
                case "jump":
                    clip = playerJumpClip;
                    break;
                case "throw":
                    clip = playerThrowClip;
                    break;
            }

            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxSource.volume);
            PlayerPrefs.Save();
        }

        private void LoadVolumes()
        {
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
    }
}
