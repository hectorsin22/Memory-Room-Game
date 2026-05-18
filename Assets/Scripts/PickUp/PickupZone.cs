using UnityEngine;

public class PickupZone : MonoBehaviour
{
    private PickableObject pickable;
    private Collider triggerCollider;
    private PlayerMovement[] players;

    void Awake()
    {
        pickable = GetComponentInParent<PickableObject>();
        triggerCollider = GetComponent<Collider>();
    }

    void Start()
    {
        // Cache once — avoids FindGameObjectsWithTag every frame
        players = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (pickable == null) return;
        if (triggerCollider == null) return;
        if (pickable.isBeingCarried) return;

        foreach (PlayerMovement pm in players)
        {
            if (pm == null || !pm.gameObject.activeSelf) continue;

            int playerIndex = pm.playerIndex;
            if (playerIndex < 0 || playerIndex >= PickableObject.MaxPlayers) continue;

            // Per-player gates
            if (PickableObject.playerCarrying[playerIndex]) continue;
            if (Time.time < PickableObject.playerPickupBlockedUntil[playerIndex]) continue;

            // Ownership gate: unclaimed or already owned by this player
            if (pickable.owningPlayerIndex != -1 && pickable.owningPlayerIndex != playerIndex) continue;

            if (IsPlayerInsideTriggerXZ(pm.transform))
            {
                pickable.PickUp(pm.transform);
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
