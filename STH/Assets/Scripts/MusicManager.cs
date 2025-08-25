using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider masterVolSlider;
    [SerializeField] Slider musicVolSlider;
    [SerializeField] Slider effectsVolSlider;

    private List<AudioClip> unplayedClips = new List<AudioClip>();
    private AudioClip currentClip;

    private const string MasterKey = "MasterVolume";
    private const string MusicKey = "MusicVolume";
    private const string EffectsKey = "EffectsVolume";


    void Start()
    {
        // Load saved values (default = 0.75 if not saved yet)
        float masterVol = PlayerPrefs.GetFloat(MasterKey, 0.75f);
        float musicVol = PlayerPrefs.GetFloat(MusicKey, 0.75f);
        float effectsVol = PlayerPrefs.GetFloat(EffectsKey, 0.75f);

        masterVolSlider.value = masterVol;
        musicVolSlider.value = musicVol;
        effectsVolSlider.value = effectsVol;

        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetEffectsVolume(effectsVol);

        // Hook up listeners
        masterVolSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolSlider.onValueChanged.AddListener(SetMusicVolume);
        effectsVolSlider.onValueChanged.AddListener(SetEffectsVolume);


        if (audioClips.Length == 0 || audioSource == null)
        {
            Debug.LogWarning("MusicManager is missing AudioClips or AudioSource.");
            return;
        }

        ResetPlaylist();
        PlayNextSong();
    }

    void Update()
    {
        if (!audioSource.isPlaying && currentClip != null)
        {
            PlayNextSong();
        }
    }

    private void PlayNextSong()
    {
        if (unplayedClips.Count == 0)
            ResetPlaylist();

        int index = Random.Range(0, unplayedClips.Count);
        currentClip = unplayedClips[index];
        unplayedClips.RemoveAt(index);

        audioSource.clip = currentClip;
        audioSource.Play();
    }

    private void ResetPlaylist()
    {
        unplayedClips.Clear();
        unplayedClips.AddRange(audioClips);
    }
    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MasterKey, value);
        PlayerPrefs.Save();
    }
    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();
    }

    public void SetEffectsVolume(float value)
    {
        audioMixer.SetFloat("SoundEffectsVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(EffectsKey, value);
        PlayerPrefs.Save();
    }
}
