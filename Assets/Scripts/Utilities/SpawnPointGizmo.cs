using UnityEngine;

public class SpawnPointGizmo : MonoBehaviour
{
    public float radius = 0.35f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);

        Gizmos.DrawLine(
            transform.position + Vector3.left * radius,
            transform.position + Vector3.right * radius
        );

        Gizmos.DrawLine(
            transform.position + Vector3.forward * radius,
            transform.position + Vector3.back * radius
        );
    }
}