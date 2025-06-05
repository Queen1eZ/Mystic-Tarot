using UnityEngine;

public class XRQuadButton : MonoBehaviour
{
    private Material originalMaterial;
    public Material hoverMaterial;

    private MeshRenderer meshRenderer;


    [Header("Logic Controller")]
    [Tooltip("Drag the GameObject with the IntroController script here")]
    public IntroController introController; // Assign this in the Unity Inspector

    void Awake() 
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }
        else
        {
            Debug.LogError("XRQuadButton: MeshRenderer component not found on this GameObject!", this);
        }

        // Optional: Add a check to ensure the introController is assigned
        if (introController == null)
        {
            Debug.LogError("XRQuadButton: The 'Intro Controller' reference is not set in the Inspector! Button click will not start the intro sequence.", this);
        }

        if (ButtonSounds.Instance == null)
        {
            Debug.LogError("XRQuadButton: ButtonSounds instance not found in the scene! Make sure an AudioManager GameObject with the script exists.");
        }

    }

    public void OnHoverEnter()
    {
        if (meshRenderer != null && hoverMaterial != null)
        {
            meshRenderer.material = hoverMaterial;
        }

    }

    public void OnHoverExit()
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
    }

    public void OnSelectEntered() 
    {
        Debug.Log("XRQuadButton clicked (OnSelectEntered called)");

        if (ButtonSounds.Instance != null)
        {
            ButtonSounds.Instance.PlayButtonClickSound();
        }

        gameObject.SetActive(false);

        if (introController != null)
        {
            introController.StartIntroduction();
        }
        else
        {
            Debug.LogError("XRQuadButton: Cannot start intro sequence because 'Intro Controller' reference is not set!", this);
        }
    }

    public void DisableButtonInteraction()
    {
        Debug.Log($"Disabling button visuals/interaction for {gameObject.name}");
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
        }
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

    }
}