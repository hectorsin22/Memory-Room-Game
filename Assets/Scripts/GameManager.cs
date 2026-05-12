using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Memorize,
        Reconstruction,
        RoundFinished
    }

    [Header("State")]
    public GameState currentState;

    [Header("Memory Objects")]
    public GameObject[] memoryObjectPrefabs;
    public Transform originalObjectsParent;
    public Transform[] memorySpawnPoints;

    [Header("Pickup Objects")]
    public Transform pickupObjectsParent;

    [Header("Chest")]
    public ChestController chest;

    [Header("Difficulty")]
    public int currentRound = 1;
    public int startingObjects = 3;
    public int objectsIncreasePerRound = 1;
    public int maxObjects = 8;

    public float startingMemorizeTime = 8f;
    public float memorizeTimeDecreasePerRound = 1f;
    public float minimumMemorizeTime = 4f;

    [Header("Round Flow")]
    public float reconstructionTime = 20f;
    public float timeBetweenRounds = 3f;

    private readonly List<GameObject> spawnedMemoryObjects = new List<GameObject>();
    private readonly List<string> selectedObjectIDs = new List<string>();

    void Start()
    {
        StartCoroutine(RoundLoop());
    }

    IEnumerator RoundLoop()
    {
        while (true)
        {
            yield return StartCoroutine(StartRound());

            currentState = GameState.RoundFinished;

            yield return new WaitForSeconds(timeBetweenRounds);

            currentRound++;
        }
    }

    IEnumerator StartRound()
    {
        currentState = GameState.Memorize;

        ClearRound();
        DisableAllPickupObjects();

        if (chest != null)
            chest.CloseChest();

        int objectsThisRound = GetObjectsForCurrentRound();
        float memorizeTime = GetMemorizeTimeForCurrentRound();

        SelectObjectsForRound(objectsThisRound);
        SpawnMemoryObjects();

        yield return new WaitForSeconds(memorizeTime);

        currentState = GameState.Reconstruction;

        HideMemoryObjects();
        ActivateSelectedPickupObjects();

        if (chest != null)
            chest.OpenChestInstant();

        yield return new WaitForSeconds(reconstructionTime);
    }

    int GetObjectsForCurrentRound()
    {
        return Mathf.Min(
            startingObjects + ((currentRound - 1) * objectsIncreasePerRound),
            maxObjects
        );
    }

    float GetMemorizeTimeForCurrentRound()
    {
        return Mathf.Max(
            startingMemorizeTime - ((currentRound - 1) * memorizeTimeDecreasePerRound),
            minimumMemorizeTime
        );
    }

    void SelectObjectsForRound(int amount)
    {
        selectedObjectIDs.Clear();

        List<GameObject> availablePrefabs = new List<GameObject>(memoryObjectPrefabs);

        for (int i = 0; i < amount && availablePrefabs.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePrefabs.Count);
            GameObject selectedPrefab = availablePrefabs[randomIndex];

            MemoryObjectID id = selectedPrefab.GetComponent<MemoryObjectID>();

            if (id == null)
            {
                Debug.LogWarning(selectedPrefab.name + " has no MemoryObjectID.");
                availablePrefabs.RemoveAt(randomIndex);
                i--;
                continue;
            }

            selectedObjectIDs.Add(id.objectID);
            availablePrefabs.RemoveAt(randomIndex);
        }
    }

    void SpawnMemoryObjects()
    {
        List<Transform> availableSpawns = new List<Transform>(memorySpawnPoints);

        foreach (string objectID in selectedObjectIDs)
        {
            GameObject prefab = FindMemoryPrefabByID(objectID);

            if (prefab == null)
            {
                Debug.LogWarning("No memory prefab found for ID: " + objectID);
                continue;
            }

            if (availableSpawns.Count == 0)
            {
                Debug.LogWarning("Not enough spawn points.");
                return;
            }

            int spawnIndex = Random.Range(0, availableSpawns.Count);
            Transform spawn = availableSpawns[spawnIndex];

            GameObject obj = Instantiate(
                prefab,
                spawn.position,
                spawn.rotation,
                originalObjectsParent
            );

            obj.name = prefab.name;
            spawnedMemoryObjects.Add(obj);

            availableSpawns.RemoveAt(spawnIndex);
        }
    }

    GameObject FindMemoryPrefabByID(string objectID)
    {
        foreach (GameObject prefab in memoryObjectPrefabs)
        {
            MemoryObjectID id = prefab.GetComponent<MemoryObjectID>();

            if (id != null && id.objectID == objectID)
                return prefab;
        }

        return null;
    }

    void ActivateSelectedPickupObjects()
    {
        foreach (Transform child in pickupObjectsParent)
        {
            MemoryObjectID id = child.GetComponent<MemoryObjectID>();

            if (id != null && selectedObjectIDs.Contains(id.objectID))
                child.gameObject.SetActive(true);
        }
    }

    void DisableAllPickupObjects()
    {
        foreach (Transform child in pickupObjectsParent)
        {
            child.gameObject.SetActive(false);
        }
    }

    void HideMemoryObjects()
    {
        foreach (GameObject obj in spawnedMemoryObjects)
        {
            obj.SetActive(false);
        }
    }

    void ClearRound()
    {
        foreach (GameObject obj in spawnedMemoryObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedMemoryObjects.Clear();
        selectedObjectIDs.Clear();
    }
}