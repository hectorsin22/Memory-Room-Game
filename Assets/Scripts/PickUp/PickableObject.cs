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

    public Vector3 carryLocalPosition = new Vector3(0f, 1.5f, 0f);
    public Vector3 carryLocalRotation = Vector3.zero;

    public bool centerObjectOnPlayer = true;

    public bool keepOriginalRotationWhenCarried = true;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Transform carrier;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    void Update()
    {
        if (!isBeingCarried || carrier == null) return;

        //transform.localRotation = Quaternion.Euler(carryLocalRotation);

        if (!keepOriginalRotationWhenCarried)
        {
            transform.localRotation = Quaternion.Euler(carryLocalRotation);
        }

        if (centerObjectOnPlayer)
        {
            CenterObjectOnCarrier();
        }
        else
        {
            transform.localPosition = carryLocalPosition;
        }

        // TEMPORARY DEBUG CONTROL FOR EDITOR TESTING
        // This must be removed in the final VR version.
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

        transform.SetParent(player, true);

        if (!keepOriginalRotationWhenCarried)
        {
            transform.localRotation = Quaternion.Euler(carryLocalRotation);
        }
        //transform.localRotation = Quaternion.Euler(carryLocalRotation);

        if (centerObjectOnPlayer)
            CenterObjectOnCarrier();
        else
            transform.localPosition = carryLocalPosition;
    }

    private void CenterObjectOnCarrier()
    {
        Vector3 targetWorldPosition = carrier.TransformPoint(carryLocalPosition);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            transform.localPosition = carryLocalPosition;
            return;
        }

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 visualCenter = bounds.center;
        Vector3 correction = targetWorldPosition - visualCenter;

        transform.position += correction;
    }

    public void Drop()
    {
        if (!isBeingCarried) return;

        isBeingCarried = false;
        objectAlreadyCarried = false;

        // Start cooldown after dropping the object
        globalPickupBlockedUntil = Time.time + pickupCooldownAfterDrop;

        transform.SetParent(originalParent, true);

        carrier = null;
    }

    public void ForceDisappear()
    {
        if (isBeingCarried)
        {
            isBeingCarried = false;
            objectAlreadyCarried = false;
            carrier = null;
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