using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Transform lidPivot;

    public Vector3 closedRotation;
    public Vector3 openRotation;

    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    public void OpenChestInstant()
    {
        lidPivot.localRotation = Quaternion.Euler(openRotation);

        if (audioSource != null && openSound != null)
        {
            audioSource.clip = openSound;
            audioSource.Play();
        }
    }

    public void CloseChest()
    {
        lidPivot.localRotation = Quaternion.Euler(closedRotation);

        if (audioSource != null && closeSound != null)
        {
            audioSource.clip = closeSound;
            audioSource.Play();
        }
    }
}