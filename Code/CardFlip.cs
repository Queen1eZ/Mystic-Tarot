using UnityEngine;
using TMPro; // For TextMeshPro
using UnityEngine.UI; // Required for CanvasGroup
using System.Collections;

public class CardFlip : MonoBehaviour
{
    public float flipDuration = 0.5f;
    // These fade durations might be used if the card has its own minor UI elements to fade,
    // but the main interpretation panel fade is handled by CardManager.
    public float uiFadeInDuration = 0.5f;
    public float uiFadeOutDuration = 0.3f;

    private bool isFlipped = false;
    private bool isFlipping = false;

    [Header("Audio Settings")]
    public AudioClip flipSound; // Card flipping sound
    private AudioSource audioSource;

    [Header("Unique Card Identifier")]
    public string cardID = "DefaultCardID"; // *** ASSIGN UNIQUE IDs (e.g., "Card1", "Card2") IN INSPECTOR ***

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void OnSelectEntered()
    {
        if (isFlipping || (CardManager.instance != null && CardManager.instance.IsPairInterpretationActive()))
        {
            // Debug.Log($"Card {cardID}: Flipping ({isFlipping}) or interpretation active ({CardManager.instance?.IsPairInterpretationActive()}). Skipping select.", this);
            return;
        }

        // Handling for clicking an already face-up card (part of a selection or to deselect)
        if (isFlipped)
        {
            if (CardManager.instance != null && CardManager.instance.IsCardInSelection(this))
            {
            }
            return;
        }

        // If card is face-down and can be selected
        if (!isFlipped && CardManager.instance != null && CardManager.instance.CanSelectMoreCards())
        {
            // Debug.Log($"Card {cardID}: Attempting to flip to face up.", this);
            StartCoroutine(FlipCardVisuals(true)); // true to indicate flipping to face up
        }
        else if (CardManager.instance != null && !CardManager.instance.CanSelectMoreCards())
        {
            // Debug.Log($"Card {cardID}: Cannot select more cards.", this);
            // Optionally, play a "cannot select" sound or visual feedback
        }
    }

    IEnumerator FlipCardVisuals(bool flipToFaceUp)
    {
        if ((isFlipped && flipToFaceUp) || (!isFlipped && !flipToFaceUp)) // Already in the target state
        {
            // Debug.Log($"Card {cardID}: Already in desired state (isFlipped: {isFlipped}, flipToFaceUp: {flipToFaceUp}). No visual flip needed.", this);
            isFlipping = false; // Ensure this is reset if we bail early
            yield break;
        }

        isFlipping = true;
        // Debug.Log($"Card {cardID}: Starting visual flip. Target face up: {flipToFaceUp}. Current isFlipped: {isFlipped}", this);


        if (flipSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flipSound);
        }

        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        float targetYRotation = transform.eulerAngles.y + 180f; // Always flip 180 degrees

        Quaternion targetRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            targetYRotation,
            transform.eulerAngles.z
        );

        while (elapsedTime < flipDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / flipDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;

        isFlipped = flipToFaceUp; // Update the actual state AFTER animation

        // Debug.Log($"Card {cardID}: Visual flip finished. New isFlipped state: {isFlipped}", this);

        if (isFlipped) // If it was flipped to face-UP
        {
            if (CardManager.instance != null)
            {
                CardManager.instance.CardSelected(this);
            }
        }
        // else: If flipped face-DOWN (e.g., during reset), CardManager handles state.
        // No direct UI display here for paired interpretations.

        isFlipping = false;
    }

    // Called by CardManager to flip the card back during a reset
    public void TriggerFlipBack()
    {
        if (isFlipped && !isFlipping)
        {
            // Debug.Log($"Card {cardID}: TriggerFlipBack called. Current isFlipped: {isFlipped}", this);
            StartCoroutine(FlipCardVisuals(false)); // false to flip face-down
        }
    }

}