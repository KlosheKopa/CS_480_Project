using UnityEngine;

public class SlowRotateImage : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // Y axis = vertical spin
    public float rotationSpeed = 45f;                   // Degrees per second

    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}