using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 12f;
    public float lifetime = 3.5f;
    public float riseSpeedNearEnd = 1.5f;
    public float riseStartTime = 2.5f;

    [Header("Audio")]
    public AudioClip bubblePopClip;
    [Range(0f, 1f)] public float bubblePopVolume = 1f;

    private Vector3 moveDirection = Vector3.forward;
    private float timeAlive = 0f;
    private Collider bubbleCollider;
    private Rigidbody bubbleRigidbody;

    void Awake()
    {
        bubbleCollider = GetComponent<Collider>();
        bubbleRigidbody = GetComponent<Rigidbody>();

        if (bubbleRigidbody != null)
        {
            bubbleRigidbody.useGravity = false;
            bubbleRigidbody.isKinematic = true;
        }
    }

    public void Initialize(Vector3 aimDirection, Collider[] ignoredColliders)
    {
        if (aimDirection.sqrMagnitude > 0.0001f)
        {
            moveDirection = aimDirection.normalized;
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }

        if (bubbleCollider == null || ignoredColliders == null)
        {
            return;
        }

        foreach (Collider ignoredCollider in ignoredColliders)
        {
            if (ignoredCollider != null)
            {
                Physics.IgnoreCollision(bubbleCollider, ignoredCollider, true);
            }
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        Vector3 frameMove = moveDirection * forwardSpeed;

        if (timeAlive >= riseStartTime)
        {
            frameMove += Vector3.up * riseSpeedNearEnd;
        }

        transform.position += frameMove * Time.deltaTime;

        if (timeAlive >= lifetime)
        {
            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            Destroy(gameObject);
        }
    }
}
