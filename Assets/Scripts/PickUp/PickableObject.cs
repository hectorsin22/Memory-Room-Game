using UnityEngine;
using UnityEngine.InputSystem;

public class PickableObject : MonoBehaviour
{
    public static bool objectAlreadyCarried = false;

    // Small cooldown so the object is not instantly picked up again
    // after dropping it while still inside the pickup radius
    public static float globalPickupBlockedUntil = 0f;

    public bool isBeingCarried = false;

    public float dropHeight = 2f;

    // Time in seconds during which pickup is disabled after dropping
    public float pickupCooldownAfterDrop = 1f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform carrier;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (!isBeingCarried || carrier == null) return;

        // TEMPORARY DEBUG CONTROL FOR EDITOR TESTING
        // We have to remove this in the final VR version.
        // Because final drop mechanic should be based on tracker height.
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Soltando objeto con E");
            Drop();
            return;
        }

        // Final VR drop mechanic using tracker height
        if (carrier.position.y > dropHeight)
        {
            Debug.Log("Mano levantada, soltando objeto!");
            Drop();
        }
    }

    public void PickUp(Transform player)
    {
        // Prevent pickup during cooldown
        if (Time.time < globalPickupBlockedUntil) return;

        if (isBeingCarried) return;
        if (objectAlreadyCarried) return;

        isBeingCarried = true;
        objectAlreadyCarried = true;

        carrier = player;

        transform.SetParent(player);
        transform.localPosition = new Vector3(0f, 1.5f, 0f);
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (!isBeingCarried) return;

        isBeingCarried = false;
        objectAlreadyCarried = false;

        // Start cooldown after dropping the object
        globalPickupBlockedUntil = Time.time + pickupCooldownAfterDrop;

        carrier = null;

        transform.SetParent(null);
    }

    public void ReturnToOriginalPosition()
    {
        Drop();

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}