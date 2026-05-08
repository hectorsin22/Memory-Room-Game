using UnityEngine;

public class ChestController : MonoBehaviour
{
    public Transform lidPivot;

    public Vector3 closedRotation;
    public Vector3 openRotation;

    public AudioSource audioSource;

    public void OpenChestInstant()
    {
        lidPivot.localRotation = Quaternion.Euler(openRotation);

        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    public void CloseChest()
    {
        lidPivot.localRotation = Quaternion.Euler(closedRotation);
    }
}