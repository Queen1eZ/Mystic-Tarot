using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    public static ButtonSounds Instance { get; private set; }

    private AudioSource audioSource;

    [Header("UI Sound Effects")]
    public AudioClip defaultButtonClickSound;
    public AudioClip defaultButtonHoverSound;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false; 
    }

    // Play the default click sound effect
    public void PlayButtonClickSound()
    {
        if (defaultButtonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defaultButtonClickSound);
        }
        else
        {
            if (defaultButtonClickSound == null) Debug.LogWarning("AudioManager: DefaultButtonClickSound not assigned.");
            if (audioSource == null) Debug.LogError("AudioManager: AudioSource is missing!");
        }
    }

    // Play the default hover sound effect
    public void PlayButtonHoverSound()
    {
        if (defaultButtonHoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(defaultButtonHoverSound);
        }
        else
        {
            if (defaultButtonHoverSound == null) Debug.LogWarning("AudioManager: DefaultButtonHoverSound not assigned.");
            if (audioSource == null) Debug.LogError("AudioManager: AudioSource is missing!");
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            if (clip == null) Debug.LogWarning("AudioManager: AudioClip to play is null.");
            if (audioSource == null) Debug.LogError("AudioManager: AudioSource is missing!");
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
}