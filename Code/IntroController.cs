using UnityEngine;
using TMPro;
using System.Collections;

public class IntroController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI instructionText1;
    [SerializeField] private TextMeshProUGUI instructionText2;
    [SerializeField] private GameObject introCanvasObject;
    private CanvasGroup canvasGroup;

    [Header("Interaction Elements")]
    [Tooltip("Assign the Quad button with the XRNextQuadButton script")]
    [SerializeField] private XRNextQuadButton nextQuadButton; // A reference to the "Next Page" Quad button

    [Header("Animation Timings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.75f;
    [SerializeField] private float canvasFadeOutDuration = 1.0f;

    private string text1Content = "Quiet your mind...\nSilently ask your a 'Yes or No' question.\nFocus on the clear answer you seek.";
    private string text2Content = "Seven cards are presented before you.\nLet your intuition guide you now...\nFeel which three call to you.";

    private bool introRunning = false;

    private enum IntroState
    {
        Idle,
        ShowingInstruction1, // Instruction 1 has been displayed. Wait for the click on "Next Page"
        TransitioningToInstruction2,
        ShowingInstruction2, // Instruction 2 has been displayed. Wait for the click on "Next Page"
        Finalizing
    }
    private IntroState currentState = IntroState.Idle;

    void Start()
    {
        if (introCanvasObject != null)
        {
            canvasGroup = introCanvasObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = introCanvasObject.AddComponent<CanvasGroup>();
        }
        else Debug.LogError("IntroController: Intro Canvas Object not assigned!");

        if (nextQuadButton == null) Debug.LogError("IntroController: Next Quad Button not assigned!", this);

        SetupInitialState();
    }

    void SetupInitialState()
    {
        introRunning = false;
        currentState = IntroState.Idle;

        if (instructionText1 != null) { instructionText1.text = ""; instructionText1.alpha = 0f; }
        else Debug.LogError("IntroController: InstructionText1 not assigned!");

        if (instructionText2 != null) { instructionText2.text = ""; instructionText2.alpha = 0f; }
        else Debug.LogError("IntroController: InstructionText2 not assigned!");

        if (nextQuadButton != null) nextQuadButton.Hide(); // Initially hide the "Next Page"

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else if (introCanvasObject == null) Debug.LogError("IntroController: Intro Canvas Object not assigned, cannot init CanvasGroup.");
        else Debug.LogError("IntroController: CanvasGroup could not be initialized on Intro Canvas!");
    }

    // Called by the initial "Start" Quad button
    public void StartIntroduction()
    {
        if (introRunning)
        {
            Debug.LogWarning("IntroController: StartIntroduction called but intro is already running.");
            return;
        }
        introRunning = true;
        Debug.Log("IntroController: StartIntroduction sequence initiated.");

        if (introCanvasObject != null) introCanvasObject.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        currentState = IntroState.ShowingInstruction1;
        StartCoroutine(ShowInstruction1_Coroutine());
    }

    private IEnumerator ShowInstruction1_Coroutine()
    {
        yield return StartCoroutine(FadeText(instructionText1, text1Content, true, fadeInDuration));
        if (nextQuadButton != null)
        {
            nextQuadButton.Show();
            if (nextQuadButton.introController == null) nextQuadButton.introController = this;
        }
        // currentState is ShowingInstruction1
    }

    // When XRNextQuadButton is clicked, this method is called by it
    public void HandleNextQuadClick()
    {
        if (!introRunning) return;
        // Prevent repeated clicks during the transition period
        if (currentState == IntroState.TransitioningToInstruction2 || currentState == IntroState.Finalizing) return;

        if (nextQuadButton != null)
        {
            nextQuadButton.DisableInteractionTemporarily(); // Click to disable the interaction immediately, and it may still be displayed visually
        }

        switch (currentState)
        {
            case IntroState.ShowingInstruction1:
                currentState = IntroState.TransitioningToInstruction2;
                StartCoroutine(TransitionToInstruction2_Coroutine());
                break;
            case IntroState.ShowingInstruction2:
                currentState = IntroState.Finalizing;
                StartCoroutine(FinalizeIntro_Coroutine());
                break;
            default:
                Debug.LogWarning($"IntroController: NextQuadClick called in unexpected state: {currentState}");
                break;
        }
    }

    private IEnumerator TransitionToInstruction2_Coroutine()
    {
        if (nextQuadButton != null) nextQuadButton.Hide(); // Hide the button during the transition

        StartCoroutine(FadeText(instructionText1, null, false, fadeOutDuration));
        yield return StartCoroutine(FadeText(instructionText2, text2Content, true, fadeInDuration));
        Debug.Log("IntroController: Instruction 2 displayed.");

        currentState = IntroState.ShowingInstruction2;
        if (nextQuadButton != null)
        {
            nextQuadButton.Show(); // Display the button for the next step
        }
    }

    private IEnumerator FinalizeIntro_Coroutine()
    {
        if (nextQuadButton != null) nextQuadButton.Hide();

        StartCoroutine(FadeText(instructionText2, null, false, fadeOutDuration));

        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, false, canvasFadeOutDuration));
            if (introCanvasObject != null) introCanvasObject.SetActive(false);
            Debug.Log("IntroController: Canvas faded out and disabled.");
        }
        else
        {
            Debug.LogError("IntroController: Cannot fade out Canvas because CanvasGroup is not available!");
            if (introCanvasObject != null) introCanvasObject.SetActive(false);
        }

        introRunning = false;
        currentState = IntroState.Idle;
        Debug.Log("Intro Sequence Complete.");
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, float duration)
    {
        float startAlpha = cg.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
        cg.interactable = fadeIn;
        cg.blocksRaycasts = fadeIn;
    }

    private IEnumerator FadeText(TextMeshProUGUI textElement, string newText, bool fadeIn, float duration)
    {
        if (textElement == null) { Debug.LogError("FadeText: Null TextMeshProUGUI!"); yield break; }
        float startAlpha = textElement.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        if (fadeIn)
        {
            textElement.text = newText;
            if (!Mathf.Approximately(startAlpha, 0f)) { textElement.alpha = 0f; startAlpha = 0f; }
        }
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            textElement.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }
        textElement.alpha = targetAlpha;
        if (!fadeIn && targetAlpha == 0f) { textElement.text = ""; }
    }
}