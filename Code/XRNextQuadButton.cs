using UnityEngine;

public class XRNextQuadButton : MonoBehaviour
{
    [Header("Visual Materials")]
    public Material normalMaterial;
    public Material hoverMaterial;

    private MeshRenderer meshRenderer;
    private Collider buttonCollider; 

    [Header("Logic Connection")]
    [Tooltip("IntroController")]
    public IntroController introController; 

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        buttonCollider = GetComponent<Collider>();

        if (meshRenderer == null) Debug.LogError("XRNextQuadButton: MeshRenderer component not found!", this);
        if (buttonCollider == null) Debug.LogError("XRNextQuadButton: Collider component not found!", this);
        if (normalMaterial == null) Debug.LogWarning("XRNextQuadButton: Normal Material not assigned.", this);


        SetMaterial(normalMaterial);

        if (introController == null)
        {
            introController = FindObjectOfType<IntroController>();
            if (introController == null)
                Debug.LogError("XRNextQuadButton: IntroController not assigned and couldn't be found in scene!", this);
            else
                Debug.LogWarning("XRNextQuadButton: IntroController was auto-assigned. Recommended to set in Inspector.", this);
        }
    }

    private void SetMaterial(Material mat)
    {
        if (meshRenderer != null && mat != null)
        {
            meshRenderer.material = mat;
        }
    }

    public void OnHoverEnter()
    {
        SetMaterial(hoverMaterial);
    }

    public void OnHoverExit()
    {
        SetMaterial(normalMaterial);
    }

    public void OnSelectEntered()
    {
        Debug.Log($"XRNextQuadButton ({gameObject.name}): OnSelectEntered called.");

        if (ButtonSounds.Instance != null)
        {
            ButtonSounds.Instance.PlayButtonClickSound();
        }

        if (introController != null)
        {
            introController.HandleNextQuadClick(); 
        }
        else
        {
            Debug.LogError("XRNextQuadButton: IntroController reference is null. Cannot proceed.", this);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetMaterial(normalMaterial); 
        if (buttonCollider != null) buttonCollider.enabled = true;

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null) interactable.enabled = true;

        this.enabled = true;
        Debug.Log($"XRNextQuadButton ({gameObject.name}): Shown and Enabled.");
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log($"XRNextQuadButton ({gameObject.name}): Hidden.");
    }

    public void DisableInteractionTemporarily()
    {
        if (buttonCollider != null) buttonCollider.enabled = false;
        Debug.Log($"XRNextQuadButton ({gameObject.name}): Interaction disabled temporarily.");
    }
}
