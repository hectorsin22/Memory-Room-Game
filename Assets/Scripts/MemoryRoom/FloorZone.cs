using UnityEngine;

public class FloorZone : MonoBehaviour
{
    [Header("Colors")]
    public Color idleColor = new Color(1f, 1f, 1f, 0f);
    public Color player1Color = new Color(1f, 0.6f, 0.1f, 0.8f);
    public Color player2Color = new Color(0.2f, 0.5f, 1f, 0.8f);
    public Color correctColor = new Color(0.1f, 0.9f, 0.3f, 0.8f);

    private Renderer zoneRenderer;
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        zoneRenderer = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        SetColor(idleColor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Color c = other.name.Contains("1") ? player1Color : player2Color;
            SetColor(c);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            SetColor(idleColor);
    }

    public void SetCorrect()
    {
        SetColor(correctColor);
    }

    private void SetColor(Color color)
    {
        if (zoneRenderer == null) return;
        zoneRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", color);
        zoneRenderer.SetPropertyBlock(mpb);
    }
}