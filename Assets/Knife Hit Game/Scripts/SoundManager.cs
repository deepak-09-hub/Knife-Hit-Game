using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class NamedClip
    {
        public string id;
        public AudioClip clip;
    }

    [Header("Clips")]
    [SerializeField] private NamedClip[] sfxClips;
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource[] sfxSources = new AudioSource[4];

    [Header("UI")]
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private Image musicButtonImage;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private readonly Dictionary<string, AudioClip> clipMap = new();

    private bool soundEnabled;
    private bool musicEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var c in sfxClips)
            if (c.clip != null)
                clipMap[c.id] = c.clip;

        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
    }

    private void Start()
    {
        musicSource.loop = true;
        musicSource.clip = backgroundMusic;

        if (backgroundMusic != null)
            musicSource.Play();

        ApplySettings();
    }

    public void PlaySFX(string id)
    {
        if (!soundEnabled || !clipMap.TryGetValue(id, out var clip))
            return;

        foreach (var src in sfxSources)
        {
            if (!src.isPlaying)
            {
                src.PlayOneShot(clip);
                return;
            }
        }

        sfxSources[0].PlayOneShot(clip);
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        ApplySettings();
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        ApplySettings();
    }

    private void ApplySettings()
    {
        foreach (var src in sfxSources)
            src.mute = !soundEnabled;

        musicSource.mute = !musicEnabled;

        if (soundButtonImage)
            soundButtonImage.sprite = soundEnabled ? soundOnSprite : soundOffSprite;

        if (musicButtonImage)
            musicButtonImage.sprite = musicEnabled ? musicOnSprite : musicOffSprite;
    }
}