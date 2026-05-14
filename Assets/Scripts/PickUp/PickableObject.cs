using UnityEngine;
using UnityEngine.InputSystem;

public class PickableObject : MonoBehaviour
{
    public static bool objectAlreadyCarried = false;

    // Small cooldown so the object is not instantly picked up again
    // after dropping it while still inside the pickup radius
    public static float globalPickupBlockedUntil = 0f;

    public bool isBeingCarried = false;

    // Height under which the player is considered crouching
    public float crouchHeight = 0.3f;

    // Height over which the player is considered standing after picking up
    public float standHeight = 0.8f;

    // Time in seconds during which pickup is disabled after dropping
    public float pickupCooldownAfterDrop = 2f;

    // Object follows the player in X/Z only, keeping this fixed height
    public float carriedObjectHeight = 0.2f;

    // Offset on the floor relative to the player
    public Vector2 carryOffsetXZ = Vector2.zero;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Transform carrier;

    private Quaternion carriedRotation;
    private bool playerStoodUpAfterPickup = false;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        carriedObjectHeight = transform.position.y;
    }

    void Update()
    {
        if (!isBeingCarried || carrier == null) return;

        Vector3 targetPosition = new Vector3(
            carrier.position.x + carryOffsetXZ.x,
            carriedObjectHeight,
            carrier.position.z + carryOffsetXZ.y
        );

        transform.position = targetPosition;
        transform.rotation = carriedRotation;

        // TEMPORARY DEBUG CONTROL FOR EDITOR TESTING
        // This must be removed in the final VR version.
        // Because final drop mechanic should be based on crouching.
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Soltando objeto con E");
            Drop();
            return;
        }

        // Player has stood up after picking up the object
        if (!playerStoodUpAfterPickup && carrier.position.y > standHeight)
        {
            playerStoodUpAfterPickup = true;
        }

        // Player crouches again to drop the object
        if (playerStoodUpAfterPickup && carrier.position.y < crouchHeight)
        {
            Debug.Log("Jugador agachado por segunda vez, soltando objeto!");
            Drop();
        }
    }

    public void PickUp(Transform player)
    {
        // Player must be crouching to pick up the object
        if (player.position.y > crouchHeight)
            return;

        // Prevent pickup during cooldown
        if (Time.time < globalPickupBlockedUntil)
            return;

        if (isBeingCarried)
            return;

        if (objectAlreadyCarried)
            return;

        isBeingCarried = true;
        objectAlreadyCarried = true;

        carrier = player;

        carriedObjectHeight = transform.position.y;
        carriedRotation = transform.rotation;

        playerStoodUpAfterPickup = false;
    }

    public void Drop()
    {
        if (!isBeingCarried)
            return;

        isBeingCarried = false;
        objectAlreadyCarried = false;

        // Start cooldown after dropping the object
        globalPickupBlockedUntil = Time.time + pickupCooldownAfterDrop;

        transform.SetParent(originalParent, true);

        carrier = null;
        playerStoodUpAfterPickup = false;
    }

    public void ForceDisappear()
    {
        if (isBeingCarried)
        {
            isBeingCarried = false;
            objectAlreadyCarried = false;
            carrier = null;
            playerStoodUpAfterPickup = false;
        }

        transform.SetParent(originalParent, true);

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        gameObject.SetActive(false);
    }

    public void ReturnToOriginalPosition()
    {
        Drop();

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}