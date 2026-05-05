using UnityEngine;

public class DropZone : MonoBehaviour
{
    public int zoneIndex; // para identificar cada zona
    private PickableObject currentObject = null;

    void OnTriggerEnter(Collider other)
    {
        PickableObject obj = other.GetComponent<PickableObject>();
        if (obj != null && obj.isBeingCarried)
        {
            obj.Drop();
            obj.transform.position = transform.position + Vector3.up * 0.5f;
            currentObject = obj;
            Debug.Log($"Objeto soltado en zona {zoneIndex}");
            // Aquí puedes llamar a tu GameManager para validar la posición
        }
    }

    void OnTriggerExit(Collider other)
    {
        PickableObject obj = other.GetComponent<PickableObject>();
        if (obj != null && obj == currentObject)
            currentObject = null;
    }

    public bool HasObject() => currentObject != null;
    public PickableObject GetCurrentObject() => currentObject;
}
