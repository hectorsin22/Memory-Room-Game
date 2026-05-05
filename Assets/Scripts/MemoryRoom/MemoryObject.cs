using UnityEngine;

public class MemoryObject : MonoBehaviour
{
    [Header("Settings")]
    public float placementRadius = 1.5f;
    public string playerTag = "Player";

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Renderer[] renderers;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            SetColor(highlightColor);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            SetColor(normalColor);
    }

    private void SetColor(Color color)
    {
        foreach (var r in renderers)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", color);
            r.SetPropertyBlock(mpb);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, placementRadius);
    }
}
