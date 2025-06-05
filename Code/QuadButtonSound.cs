using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(AudioSource))]
public class QuadButtonSound : MonoBehaviour
{
    public AudioClip selectSound; 

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (selectSound == null)
        {
            selectSound = Resources.Load<AudioClip>("Sounds/ButtonClick");
        }
    }

    public void OnSelectEntered(XRBaseInteractor interactor) => PlaySound();

    public void OnPointerClick() => PlaySound();

    public void PlaySound()
    {
        if (selectSound != null && audioSource != null)
            audioSource.PlayOneShot(selectSound);
    }
}