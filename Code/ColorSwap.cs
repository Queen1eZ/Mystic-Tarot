using UnityEngine;

public class ColorSwap : MonoBehaviour
{
    public Material swapMaterial;
    private Material initMaterial;
    private bool isSwapped;
    void Start()
    {
        initMaterial = GetComponent<Renderer>().material;
        isSwapped = false;
    }

    public void SwapColor()
    {
        GetComponent<Renderer>().material = isSwapped ? initMaterial : swapMaterial;
        isSwapped = !isSwapped;
    }
}
