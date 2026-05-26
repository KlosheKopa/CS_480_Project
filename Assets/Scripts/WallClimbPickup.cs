using UnityEngine;

public class WallClimbPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WallClimb wallClimb = other.GetComponent<WallClimb>();
            if (wallClimb != null)
            {
                wallClimb.hasAbility = true;   // Give the ability
            }

            Destroy(gameObject);
        }
    }
}