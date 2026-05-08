using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject originalObjects;
    public GameObject pickupObjects;
    public ChestController chest;

    public float memorizeTime = 6f;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        originalObjects.SetActive(true);
        pickupObjects.SetActive(false);
        chest.CloseChest();

        yield return new WaitForSeconds(memorizeTime);

        originalObjects.SetActive(false);
        pickupObjects.SetActive(true);
        chest.OpenChestInstant();
    }
}