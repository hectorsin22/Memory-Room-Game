using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Transform lidPivot;

    public Vector3 closedRotation;
    public Vector3 openRotation;

    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Range(0f, 1f)]
    public float openVolume = 1f;

    [Range(0f, 1f)]
    public float closeVolume = 0.25f;

    private bool enableCloseSound = false;

    public void EnableCloseSound()
    {
        enableCloseSound = true;
    }

    public void OpenChestInstant()
    {
        lidPivot.localRotation = Quaternion.Euler(openRotation);

        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound, openVolume);
        }
    }

    public void CloseChest()
    {
        lidPivot.localRotation = Quaternion.Euler(closedRotation);

        if (!enableCloseSound)
            return;

        if (audioSource != null && closeSound != null)
        {
            audioSource.PlayOneShot(closeSound, closeVolume);
        }
    }
}