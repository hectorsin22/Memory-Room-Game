using UnityEngine;

public class PickupZone : MonoBehaviour
{
    private PickableObject pickable;
    public float pickupRadius = 5f;

    void Awake()
    {
        pickable = GetComponentInParent<PickableObject>();
    }

    void Update()
    {
        if (pickable == null || pickable.isBeingCarried) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            Vector3 playerPos = p.transform.position;
            Vector3 objectPos = transform.position;
            playerPos.y = 0;
            objectPos.y = 0;
            float dist = Vector3.Distance(playerPos, objectPos);
            Debug.Log("Distancia XZ a " + p.name + ": " + dist);
            if (dist < minDist)
            {
                minDist = dist;
                closest = p.transform;
            }
        }

        if (closest != null && minDist < pickupRadius)
        {
            Debug.Log("Cogiendo objeto!");
            pickable.PickUp(closest);
        }
    }
}