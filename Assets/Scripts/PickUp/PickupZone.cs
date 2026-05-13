using UnityEngine;

public class PickupZone : MonoBehaviour
{
    private PickableObject pickable;

    public float pickupRadius = 5f;

    // Height under which the player is considered crouching
    public float crouchHeight = 1.0f;

    void Awake()
    {
        pickable = GetComponentInParent<PickableObject>();
    }

    void Update()
    {
        if (pickable == null) return;

        if (pickable.isBeingCarried) return;
        if (PickableObject.objectAlreadyCarried) return;
        if (Time.time < PickableObject.globalPickupBlockedUntil) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            Vector3 playerPos = player.transform.position;
            Vector3 triggerPos = transform.position;

            Vector3 playerXZ = new Vector3(playerPos.x, 0f, playerPos.z);
            Vector3 triggerXZ = new Vector3(triggerPos.x, 0f, triggerPos.z);

            float distance = Vector3.Distance(playerXZ, triggerXZ);

            if (distance <= pickupRadius && player.transform.position.y < crouchHeight)
            {
                pickable.PickUp(player.transform);
                return;
            }
        }
    }
}