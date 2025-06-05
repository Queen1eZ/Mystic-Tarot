using UnityEngine;

public class CardFloat : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float floatHeight = 0.05f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Simulate the up and down floating using the sine function
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}