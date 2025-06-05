using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class ResetButton : MonoBehaviour
{
    [Header("Button Visuals")]
    public Material originalMaterial;
    public Material hoverMaterial;

    private MeshRenderer meshRenderer;

    void Awake()
    {

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("ResetSceneButton: MeshRenderer component not found on this GameObject.");
            enabled = false; 
            return;
        }

        // During initialization, if originalMaterial is not set in the Inspector, an attempt will be made to obtain the current material as the original material
        if (originalMaterial == null)
        {
            originalMaterial = meshRenderer.material;
        }
        else
        {
            // If originalMaterial is set in Inspector, apply it initially.
            meshRenderer.material = originalMaterial;
        }

        if (hoverMaterial == null)
        {
            Debug.LogWarning("ResetSceneButton: HoverMaterial is not assigned. Hover effect will not be visible.");
        }
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (meshRenderer != null && hoverMaterial != null)
        {
            meshRenderer.material = hoverMaterial;
            Debug.Log("Hover Entered on: " + gameObject.name + " by " + args.interactorObject.transform.name);
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        if (meshRenderer != null && originalMaterial != null)
        {
            meshRenderer.material = originalMaterial;
            Debug.Log("Hover Exited on: " + gameObject.name + " by " + args.interactorObject.transform.name);
        }
    }

    public void OnButtonPressed(SelectEnterEventArgs args)
    {
        Debug.Log("Reset button pressed by: " + args.interactorObject.transform.name);

        if (ButtonSounds.Instance != null)
        {
            ButtonSounds.Instance.PlayButtonClickSound();
        }

        RestartCurrentScene();
    }

    public void RestartCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
        Debug.Log("Scene reloaded: " + SceneManager.GetActiveScene().name);
    }
}
