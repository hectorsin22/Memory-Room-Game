using UnityEngine;

public class PickupZone : MonoBehaviour
{
    private PickableObject pickable;
    private Collider triggerCollider;

    void Awake()
    {
        pickable = GetComponentInParent<PickableObject>();
        triggerCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (pickable == null) return;
        if (triggerCollider == null) return;

        if (pickable.isBeingCarried) return;
        if (PickableObject.objectAlreadyCarried) return;
        if (Time.time < PickableObject.globalPickupBlockedUntil) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (IsPlayerInsideTriggerXZ(player.transform))
            {
                pickable.PickUp(player.transform);
                return;
            }
        }
    }

    private bool IsPlayerInsideTriggerXZ(Transform player)
    {
        Bounds bounds = triggerCollider.bounds;
        Vector3 playerPos = player.position;

        bool insideX = playerPos.x >= bounds.min.x && playerPos.x <= bounds.max.x;
        bool insideZ = playerPos.z >= bounds.min.z && playerPos.z <= bounds.max.z;

        return insideX && insideZ;
    }
}