using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Required for OrderBy and SequenceEqual

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [Header("Combination Rules")]
    public int numberOfCardsForCombination = 3;

    [Header("Selected Cards")]
    public List<CardFlip2> selectedCards = new List<CardFlip2>();

    [Header("Master Interpretation Texts")]
    public InterpretationTexts masterInterpretationA;
    public InterpretationTexts masterInterpretationB;
    public InterpretationTexts masterInterpretationC;

    [Header("Shared UI Elements")]
    public GameObject sharedInfoPanel;
    public TextMeshProUGUI sharedInfoText;
    public CanvasGroup sharedInfoPanelCanvasGroup;
    public float sharedUIPanelFadeInDuration = 0.5f;
    public float sharedUIPanelFadeOutDuration = 0.3f;

    public GameObject sharedResetButton;
    public CanvasGroup sharedResetButtonCanvasGroup;
    public float sharedResetButtonFadeInDuration = 0.5f;
    public float sharedResetButtonFadeOutDuration = 0.3f;

    private bool pairInterpretationActive = false;

    [Header("Shared Text Animation Settings")]
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float delayBetweenSegments = 0.75f;

    private Coroutine progressiveTextCoroutineManager;
    private enum InterpretationDisplayState { Idle, ShowingOutcome, ShowingStraightTalk, ShowingMantra, Complete }
    private InterpretationDisplayState currentManagerInterpretationState = InterpretationDisplayState.Idle;


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeSharedUI();
        if (numberOfCardsForCombination != 3)
        {
            Debug.LogWarning("CardManager: numberOfCardsForCombination is not set to 3, but current logic expects 3 cards for a combination.", this);
        }
    }

    void InitializeSharedUI()
    {
        if (sharedInfoPanel != null)
        {
            if (sharedInfoPanelCanvasGroup == null) sharedInfoPanelCanvasGroup = sharedInfoPanel.GetComponent<CanvasGroup>();
            if (sharedInfoPanelCanvasGroup == null) sharedInfoPanelCanvasGroup = sharedInfoPanel.AddComponent<CanvasGroup>();
            sharedInfoPanelCanvasGroup.alpha = 0f;
            sharedInfoPanelCanvasGroup.interactable = false;
            sharedInfoPanelCanvasGroup.blocksRaycasts = false;
            sharedInfoPanel.SetActive(false);
            if (sharedInfoText == null) sharedInfoText = sharedInfoPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (sharedInfoText == null) Debug.LogError("CardManager: SharedInfoText (TextMeshProUGUI) not found in SharedInfoPanel children or not assigned!", this);
        }
        else Debug.LogError("CardManager: SharedInfoPanel is not assigned!", this);

        if (sharedResetButton != null)
        {
            if (sharedResetButtonCanvasGroup == null) sharedResetButtonCanvasGroup = sharedResetButton.GetComponent<CanvasGroup>();
            if (sharedResetButtonCanvasGroup == null) sharedResetButtonCanvasGroup = sharedResetButton.AddComponent<CanvasGroup>();
            sharedResetButtonCanvasGroup.alpha = 0f;
            sharedResetButtonCanvasGroup.interactable = false;
            sharedResetButtonCanvasGroup.blocksRaycasts = false;
            sharedResetButton.SetActive(false);

            Button resetButtonComponent = sharedResetButton.GetComponent<Button>();
            if (resetButtonComponent != null)
            {
                resetButtonComponent.onClick.RemoveAllListeners();
                resetButtonComponent.onClick.AddListener(HandleResetButtonPressed);
            }
            else Debug.LogWarning("CardManager: SharedResetButton does not have a UI Button component for OnClick events.", this);
        }
        // else Debug.LogWarning("CardManager: SharedResetButton is not assigned.");
    }

    public bool CanSelectMoreCards()
    {
        return selectedCards.Count < numberOfCardsForCombination && !pairInterpretationActive;
    }

    public bool IsCardInSelection(CardFlip2 card)
    {
        return selectedCards.Contains(card);
    }

    public bool IsPairInterpretationActive()
    {
        return pairInterpretationActive;
    }

    public void CardSelected(CardFlip2 card)
    {
        if (pairInterpretationActive || IsCardInSelection(card))
        {
            return;
        }

        if (selectedCards.Count < numberOfCardsForCombination)
        {
            selectedCards.Add(card);
            if (selectedCards.Count == numberOfCardsForCombination)
            {
                AttemptDisplayCombinationInterpretation(); 
            }
        }
    }

    private static int CompareCardIDsForSort(string x, string y)
    {
        return string.Compare(x, y, System.StringComparison.OrdinalIgnoreCase);
    }

    private void AttemptDisplayCombinationInterpretation()
    {
        if (selectedCards.Count != numberOfCardsForCombination) 
        {
            Debug.LogError("AttemptDisplayCombinationInterpretation called with incorrect number of selected cards.", this);
            return;
        }

        // 1. Obtain the ids of the selected three cards and sort them
        List<string> sortedSelectedCardIDs = selectedCards
            .Select(card => card.cardID)
            .OrderBy(id => id, Comparer<string>.Create(CompareCardIDsForSort))
            .ToList();

        // 2. Combine the sorted ids into a unique string as the signature
        string combinationSignature = string.Join("_", sortedSelectedCardIDs); 

        // 3. Obtain the hash code of this combined string
        int hashCode = combinationSignature.GetHashCode();

        // 4. Take the modulus of the hash code by 3 to obtain 0, 1, or 2
        // Use System.Math.Abs to ensure that the hash value is positive and avoid the problem of taking the modulus of negative numbers
        int interpretationIndex = System.Math.Abs(hashCode) % 3;

        pairInterpretationActive = true; // Set the status and start displaying the parsing

        // 5. Select the main parsed text based on the calculated index
        InterpretationTexts textsToDisplay;
        switch (interpretationIndex)
        {
            case 0:
                textsToDisplay = masterInterpretationA;
                Debug.Log($"Combination '{combinationSignature}' (Hash: {hashCode}) mapped to Interpretation A (Index 0)");
                break;
            case 1:
                textsToDisplay = masterInterpretationB;
                Debug.Log($"Combination '{combinationSignature}' (Hash: {hashCode}) mapped to Interpretation B (Index 1)");
                break;
            case 2:
                textsToDisplay = masterInterpretationC;
                Debug.Log($"Combination '{combinationSignature}' (Hash: {hashCode}) mapped to Interpretation C (Index 2)");
                break;
            default:
                Debug.LogError($"Unexpected interpretationIndex: {interpretationIndex} for combination '{combinationSignature}' (Hash: {hashCode})! Defaulting to A.", this);
                textsToDisplay = masterInterpretationA;
                break;
        }

        // 6. Display the selected parsed text
        if (sharedInfoPanel != null && sharedInfoPanelCanvasGroup != null)
        {
            if (sharedInfoText != null) sharedInfoText.text = "";
            sharedInfoPanel.SetActive(true);

            StartCoroutine(FadeUIElement_Manager(sharedInfoPanelCanvasGroup, true, sharedUIPanelFadeInDuration, () =>
            {
                if (this.gameObject.activeInHierarchy && sharedInfoPanel.activeSelf)
                {
                    if (progressiveTextCoroutineManager != null) StopCoroutine(progressiveTextCoroutineManager);
                    progressiveTextCoroutineManager = StartCoroutine(ShowInterpretationProgressively_Manager(textsToDisplay));
                }
            }));
        }

    }


    private void ResetCardSelectionVisuals(bool triggerFlip)
    {
        foreach (CardFlip2 card in selectedCards)
        {
            if (card != null && triggerFlip)
            {
                card.TriggerFlipBack();
            }
        }
    }

    public void HandleResetButtonPressed() 
    {
        if (progressiveTextCoroutineManager != null)
        {
            StopCoroutine(progressiveTextCoroutineManager);
            progressiveTextCoroutineManager = null;
        }
        currentManagerInterpretationState = InterpretationDisplayState.Idle;

        if (sharedInfoPanel != null && sharedInfoPanel.activeSelf)
        {
            StartCoroutine(FadeUIElement_Manager(sharedInfoPanelCanvasGroup, false, sharedUIPanelFadeOutDuration, () =>
            {
                if (sharedInfoPanel != null) sharedInfoPanel.SetActive(false);
                if (sharedInfoText != null) sharedInfoText.text = "";
            }));
        }

        if (sharedResetButton != null && sharedResetButton.activeSelf)
        {
            StartCoroutine(FadeUIElement_Manager(sharedResetButtonCanvasGroup, false, sharedResetButtonFadeOutDuration, () =>
            {
                if (sharedResetButton != null) sharedResetButton.SetActive(false);
            }));
        }

        ResetCardSelectionVisuals(true);
        selectedCards.Clear();
        pairInterpretationActive = false;
    }

    // Text animation coroutine
    private IEnumerator ShowInterpretationProgressively_Manager(InterpretationTexts textsToShow)
    {
        if (sharedInfoText == null || !sharedInfoPanel.activeSelf)
        {
            currentManagerInterpretationState = InterpretationDisplayState.Complete;
            progressiveTextCoroutineManager = null;
            if (sharedResetButton != null && sharedResetButtonCanvasGroup != null && !sharedResetButton.activeSelf)
            {
                sharedResetButton.SetActive(true);
                StartCoroutine(FadeUIElement_Manager(sharedResetButtonCanvasGroup, true, sharedResetButtonFadeInDuration));
            }
            yield break;
        }

        sharedInfoText.text = "";
        currentManagerInterpretationState = InterpretationDisplayState.ShowingOutcome;

        yield return StartCoroutine(TypewriterText_Manager(sharedInfoText, textsToShow.outcome, charactersPerSecond));
        if (currentManagerInterpretationState == InterpretationDisplayState.Idle) { progressiveTextCoroutineManager = null; yield break; }

        yield return new WaitForSeconds(delayBetweenSegments);
        if (currentManagerInterpretationState == InterpretationDisplayState.Idle) { progressiveTextCoroutineManager = null; yield break; }

        currentManagerInterpretationState = InterpretationDisplayState.ShowingStraightTalk;
        if (!string.IsNullOrEmpty(sharedInfoText.text)) sharedInfoText.text += "\n\n";
        yield return StartCoroutine(TypewriterText_Manager(sharedInfoText, textsToShow.straightTalk, charactersPerSecond, true));
        if (currentManagerInterpretationState == InterpretationDisplayState.Idle) { progressiveTextCoroutineManager = null; yield break; }

        yield return new WaitForSeconds(delayBetweenSegments);
        if (currentManagerInterpretationState == InterpretationDisplayState.Idle) { progressiveTextCoroutineManager = null; yield break; }

        currentManagerInterpretationState = InterpretationDisplayState.ShowingMantra;
        if (!string.IsNullOrEmpty(sharedInfoText.text)) sharedInfoText.text += "\n\n";
        yield return StartCoroutine(TypewriterText_Manager(sharedInfoText, textsToShow.mantra, charactersPerSecond, true));
        if (currentManagerInterpretationState == InterpretationDisplayState.Idle) { progressiveTextCoroutineManager = null; yield break; }

        currentManagerInterpretationState = InterpretationDisplayState.Complete;
        progressiveTextCoroutineManager = null;

        if (sharedResetButton != null && sharedResetButtonCanvasGroup != null)
        {
            sharedResetButton.SetActive(true);
            StartCoroutine(FadeUIElement_Manager(sharedResetButtonCanvasGroup, true, sharedResetButtonFadeInDuration));
        }
    }

    private IEnumerator TypewriterText_Manager(TextMeshProUGUI targetText, string textToType, float charsPerSec, bool append = false)
    {
        if (targetText == null || string.IsNullOrEmpty(textToType)) yield break;
        string baseText = append ? targetText.text : "";
        if (!append) targetText.text = "";
        float delay = (charsPerSec <= 0) ? 0f : 1.0f / charsPerSec;
        string currentDisplay = baseText;
        for (int i = 0; i < textToType.Length; i++)
        {
            if (currentManagerInterpretationState == InterpretationDisplayState.Idle && progressiveTextCoroutineManager == null)
            { yield break; }
            currentDisplay += textToType[i];
            targetText.text = currentDisplay;
            if (delay > 0) yield return new WaitForSeconds(delay);
            else yield return null;
        }
        if (!(currentManagerInterpretationState == InterpretationDisplayState.Idle && progressiveTextCoroutineManager == null))
        { targetText.text = baseText + textToType; }
    }

    private IEnumerator FadeUIElement_Manager(CanvasGroup canvasGroup, bool fadeIn, float duration, System.Action onComplete = null)
    {
        if (canvasGroup == null) { onComplete?.Invoke(); yield break; }
        float startAlpha = canvasGroup.alpha;
        float targetAlpha = fadeIn ? 1.0f : 0.0f;
        if (Mathf.Approximately(startAlpha, targetAlpha))
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = fadeIn;
            canvasGroup.blocksRaycasts = fadeIn;
            onComplete?.Invoke();
            yield break;
        }
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = fadeIn;
        canvasGroup.blocksRaycasts = fadeIn;
        onComplete?.Invoke();
    }
}
