using UnityEngine;

public class PickableObject : MonoBehaviour
{
    public bool isBeingCarried = false;
    public float dropHeight = 2f; // altura Y para soltar
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

        // Si el jugador sube la mano por encima de dropHeight, suelta
        if (carrier.position.y > dropHeight)
        {
            Debug.Log("Mano levantada, soltando objeto!");
            Drop();
        }
    }

    public void PickUp(Transform player)
    {
        if (isBeingCarried) return;
        isBeingCarried = true;
        carrier = player;
        transform.SetParent(player);
        transform.localPosition = new Vector3(0f, 1.5f, 0f);
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        if (!isBeingCarried) return;
        isBeingCarried = false;
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