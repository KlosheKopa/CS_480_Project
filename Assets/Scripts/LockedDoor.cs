using UnityEngine;
using System.Collections;

public class LockedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public float openSpeed = 0.25f;      // Adjust this number (lower = slower)
    public float openDistance = 12f;    // How far down it sinks

    [HideInInspector] public bool isOpen = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition - Vector3.up * openDistance;
    }

    public void OpenTheDoor()
    {
        if (isOpen) return;
        isOpen = true;
        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            transform.position = Vector3.Lerp(closedPosition, openPosition, elapsed);
            yield return null;
        }

        transform.position = openPosition;
    }
}