using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    public float forwardSpeed = 12f;   // Slow and visible
    public float lifetime = 2.5f;

    private float timeAlive = 0f;

    void Update()
    {
        timeAlive += Time.deltaTime;

        // Move straight forward using transform (ignores physics completely)
        if (timeAlive < 1.2f)
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
        }

        if (timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}