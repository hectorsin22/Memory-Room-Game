using UnityEngine;

public class DropZone : MonoBehaviour
{
    public int zoneIndex;
    private PickableObject currentObject = null;

    void OnTriggerEnter(Collider other)
    {
        // Reject if zone already occupied
        if (currentObject != null) return;

        PickableObject obj = other.GetComponent<PickableObject>();
        if (obj == null || !obj.isBeingCarried) return;

        obj.Drop();
        obj.transform.position = transform.position + Vector3.up * 0.5f;
        currentObject = obj;
        obj.currentDropZone = this;

        Debug.Log($"Object placed in zone {zoneIndex} by Player {obj.owningPlayerIndex + 1}");
    }

    void OnTriggerExit(Collider other)
    {
        // Only clear if the object physically left while NOT being carried
        // (pickup path is handled by PickableObject.PickUp calling ClearZone)
        PickableObject obj = other.GetComponent<PickableObject>();
        if (obj != null && obj == currentObject && !obj.isBeingCarried)
            currentObject = null;
    }

    public bool HasObject() => currentObject != null;
    public PickableObject GetCurrentObject() => currentObject;

    // Called by GameManager at round start and by PickableObject when picked back up
    public void ClearZone()
    {
        currentObject = null;
    }
}
