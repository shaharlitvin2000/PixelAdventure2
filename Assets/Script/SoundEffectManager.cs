using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    private AudioSource audioSource;
    private SoundEffectLibrary library;

    [Header("UI References")]
    public Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
        library = GetComponent<SoundEffectLibrary>();
    }

    private void Start()
    {
        // טעינת ווליום שנשמר
        float savedVolume = PlayerPrefs.GetFloat("SFX_Volume", 0.5f);

        if (sfxSlider != null)
        {
            sfxSlider.value = savedVolume;
            sfxSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVolume);
    }

    public void Play(string soundName)
    {
        AudioClip clip = library.GetRandomClipByName(soundName);
        if (clip != null)
        {
            // התיקון: אנחנו מנגנים את ה-clip שמצאנו, לא טקסט
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("לא נמצא צליל בשם: " + soundName);
        }
    }


    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("SFX_Volume", volume);
    }
}