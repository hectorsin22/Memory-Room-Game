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
    public int currentRound = 1;

    [Header("Memory Objects")]
    public GameObject[] memoryObjectPrefabs;
    public Transform originalObjectsParent;
    public Transform[] memorySpawnPoints;

    [Header("Pickup Objects")]
    public Transform pickupObjectsParent;

    [Header("Chest")]
    public ChestController chest;

    [Header("Difficulty")]
    public int maxObjects = 12;
    public int maxImpostors = 5;

    [Header("Round Flow")]
    public float timeBetweenRounds = 3f;

    private readonly List<GameObject> spawnedMemoryObjects = new List<GameObject>();
    private readonly List<string> selectedObjectIDs = new List<string>();

    // Saves where each object appeared during the memorize phase
    private readonly Dictionary<string, Transform> correctSpawnByObjectID = new Dictionary<string, Transform>();

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
        float reconstructionTime = GetReconstructionTimeForCurrentRound();
        int impostorsThisRound = GetImpostorsForCurrentRound();

        Debug.Log(
            "Round " + currentRound +
            " | Objects: " + objectsThisRound +
            " | Memorize: " + memorizeTime +
            " | Reconstruction: " + reconstructionTime +
            " | Impostors: " + impostorsThisRound
        );

        SelectObjectsForRound(objectsThisRound);
        SpawnMemoryObjects();

        yield return new WaitForSeconds(memorizeTime);

        currentState = GameState.Reconstruction;

        HideMemoryObjects();
        ActivateSelectedPickupObjects();
        ActivateImpostorPickupObjects(impostorsThisRound);

        if (chest != null)
            chest.OpenChestInstant();

        yield return new WaitForSeconds(reconstructionTime);
    }

    int GetObjectsForCurrentRound()
    {
        if (currentRound <= 5)
            return Mathf.Min(2 + currentRound, maxObjects);

        return Mathf.Min(7 + (currentRound - 5), maxObjects);
    }

    float GetMemorizeTimeForCurrentRound()
    {
        if (currentRound <= 5)
            return 7f + currentRound;

        return Mathf.Min(12f + (currentRound - 5), 15f);
    }

    float GetReconstructionTimeForCurrentRound()
    {
        if (currentRound <= 5)
            return 20f + (currentRound * 5f);

        return Mathf.Min(45f + ((currentRound - 5) * 5f), 60f);
    }

    int GetImpostorsForCurrentRound()
    {
        if (currentRound < 6)
            return 0;

        return Mathf.Min(currentRound - 5, maxImpostors);
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

            correctSpawnByObjectID[objectID] = spawn;

            Debug.Log("Object " + objectID + " appeared at spawn " + spawn.name);

            Transform pickupReference = FindPickupObjectByID(objectID);

            if (pickupReference != null)
            {
                obj.transform.rotation = pickupReference.rotation;
                obj.transform.localScale = pickupReference.localScale;
            }

            spawnedMemoryObjects.Add(obj);
            availableSpawns.RemoveAt(spawnIndex);
        }
    }

    public Transform GetCorrectSpawnForObject(string objectID)
    {
        if (correctSpawnByObjectID.TryGetValue(objectID, out Transform spawn))
            return spawn;

        return null;
    }

    public bool WasObjectSelectedThisRound(string objectID)
    {
        return selectedObjectIDs.Contains(objectID);
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

    Transform FindPickupObjectByID(string objectID)
    {
        foreach (Transform child in pickupObjectsParent)
        {
            MemoryObjectID id = child.GetComponent<MemoryObjectID>();

            if (id != null && id.objectID == objectID)
                return child;
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

    void ActivateImpostorPickupObjects(int amount)
    {
        if (amount <= 0) return;

        List<Transform> availableImpostors = new List<Transform>();

        foreach (Transform child in pickupObjectsParent)
        {
            MemoryObjectID id = child.GetComponent<MemoryObjectID>();

            if (id != null && !selectedObjectIDs.Contains(id.objectID))
            {
                availableImpostors.Add(child);
            }
        }

        for (int i = 0; i < amount && availableImpostors.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableImpostors.Count);

            availableImpostors[randomIndex].gameObject.SetActive(true);
            availableImpostors.RemoveAt(randomIndex);
        }
    }

    void DisableAllPickupObjects()
    {
        PickableObject[] allPickables = Object.FindObjectsByType<PickableObject>(
            FindObjectsSortMode.None
        );

        foreach (PickableObject pickable in allPickables)
        {
            pickable.ForceDisappear();
        }

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
        correctSpawnByObjectID.Clear();
    }
}