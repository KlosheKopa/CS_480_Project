using UnityEngine;

public class HoverBob : MonoBehaviour
{
    [Header("Hover Settings")]
    public float bobHeight = 0.25f;     // How high it moves up and down
    public float bobSpeed = 2f;         // How fast it bobs

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}