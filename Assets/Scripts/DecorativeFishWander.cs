using UnityEngine;

public class DecorativeFishWander : MonoBehaviour
{
    [Header("Swim Area")]
    public Transform boundsCenter;
    public Vector3 swimAreaSize = new Vector3(30f, 8f, 30f);
    public bool useWorldBounds;
    public Vector3 worldBoundsMin;
    public Vector3 worldBoundsMax;

    [Header("Movement")]
    public float swimSpeed = 2f;
    public float turnSpeed = 2.5f;
    public float targetReachDistance = 1f;
    public float minTargetTime = 2f;
    public float maxTargetTime = 6f;
    public float boundaryTurnDistance = 8f;

    [Header("Motion Polish")]
    public float verticalSwayHeight = 0.25f;
    public float verticalSwaySpeed = 2f;
    public Vector3 modelRotationOffset;

    private Vector3 centerPoint;
    private Vector3 targetPosition;
    private float targetTimer;
    private float swayOffset;

    void Start()
    {
        centerPoint = boundsCenter != null ? boundsCenter.position : transform.position;
        swayOffset = Random.Range(0f, 100f);
        transform.position = ClampToSwimArea(transform.position);
        PickNewTarget();
    }

    void Update()
    {
        if (!useWorldBounds && boundsCenter != null)
        {
            centerPoint = boundsCenter.position;
        }

        targetTimer -= Time.deltaTime;

        if (targetTimer <= 0f || Vector3.Distance(transform.position, targetPosition) <= targetReachDistance)
        {
            PickNewTarget();
        }

        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.001f) return;

        Vector3 moveDirection = direction.normalized;
        if (IsApproachingBoundary(moveDirection))
        {
            PickNewTarget();
            direction = targetPosition - transform.position;
            if (direction.sqrMagnitude <= 0.001f) return;
            moveDirection = direction.normalized;
        }

        float sway = Mathf.Sin((Time.time + swayOffset) * verticalSwaySpeed) * verticalSwayHeight;
        Vector3 nextPosition = transform.position + moveDirection * swimSpeed * Time.deltaTime;
        nextPosition.y += sway * Time.deltaTime;

        transform.position = ClampToSwimArea(nextPosition);

        Quaternion lookRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(modelRotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
    }

    public void SetWorldBounds(Vector3 min, Vector3 max)
    {
        useWorldBounds = true;
        worldBoundsMin = min;
        worldBoundsMax = max;
        transform.position = ClampToSwimArea(transform.position);
        PickNewTarget();
    }

    private void PickNewTarget()
    {
        GetActiveBounds(out Vector3 min, out Vector3 max);

        targetPosition = new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );

        targetTimer = Random.Range(minTargetTime, maxTargetTime);
    }

    private Vector3 ClampToSwimArea(Vector3 position)
    {
        GetActiveBounds(out Vector3 min, out Vector3 max);

        position.x = Mathf.Clamp(position.x, min.x, max.x);
        position.y = Mathf.Clamp(position.y, min.y, max.y);
        position.z = Mathf.Clamp(position.z, min.z, max.z);

        return position;
    }

    private void GetActiveBounds(out Vector3 min, out Vector3 max)
    {
        if (useWorldBounds)
        {
            min = Vector3.Min(worldBoundsMin, worldBoundsMax);
            max = Vector3.Max(worldBoundsMin, worldBoundsMax);
            return;
        }

        Vector3 halfSize = swimAreaSize * 0.5f;
        min = centerPoint - halfSize;
        max = centerPoint + halfSize;
    }

    private bool IsApproachingBoundary(Vector3 moveDirection)
    {
        GetActiveBounds(out Vector3 min, out Vector3 max);
        Vector3 position = transform.position;

        return (moveDirection.x < -0.1f && position.x - min.x <= boundaryTurnDistance)
            || (moveDirection.x > 0.1f && max.x - position.x <= boundaryTurnDistance)
            || (moveDirection.z < -0.1f && position.z - min.z <= boundaryTurnDistance)
            || (moveDirection.z > 0.1f && max.z - position.z <= boundaryTurnDistance)
            || (moveDirection.y < -0.1f && position.y - min.y <= boundaryTurnDistance)
            || (moveDirection.y > 0.1f && max.y - position.y <= boundaryTurnDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        if (useWorldBounds)
        {
            Vector3 min = Vector3.Min(worldBoundsMin, worldBoundsMax);
            Vector3 max = Vector3.Max(worldBoundsMin, worldBoundsMax);
            Gizmos.DrawWireCube((min + max) * 0.5f, max - min);
        }
        else
        {
            Vector3 gizmoCenter = boundsCenter != null ? boundsCenter.position : transform.position;
            Gizmos.DrawWireCube(gizmoCenter, swimAreaSize);
        }
    }
}
