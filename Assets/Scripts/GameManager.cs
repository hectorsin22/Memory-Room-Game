using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        Menu,
        Memorize,
        Reconstruction,
        RoundFinished
    }

    public enum GameMode
    {
        QuickMatch,
        NormalMatch
    }

    [Header("State")]
    public GameState currentState;
    public GameMode currentMode;
    public int currentRound = 1;

    [Header("Menu")]
    public GameObject menuRoot;
    public GameObject environmentTitle;

    [Header("Lighting")]
    public Light mainLight;
    public float menuLightIntensity = 0.15f;
    public float gameplayLightIntensity = 1f;

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

    [Header("Round Audio")]
    public AudioSource roundAudioSource;
    public AudioClip memorizeLastSecondsSound;
    public AudioClip reconstructionLastSecondsSound;

    [Header("Drop Zones")]
    public DropZone[] allDropZones;

    [Header("UI")]
    public HUDController hud;
    public WinnerScreenController winnerScreen;

    [Header("Menu Zones (for reset on restart)")]
    public MenuZone[] menuZones;

    private readonly List<GameObject> spawnedMemoryObjects = new List<GameObject>();
    private readonly List<string> selectedObjectIDs = new List<string>();
    private readonly Dictionary<string, Transform> correctSpawnByObjectID = new Dictionary<string, Transform>();

    private bool memorizeWarningPlayed = false;
    private bool reconstructionWarningPlayed = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (roundAudioSource == null)
            roundAudioSource = GetComponent<AudioSource>();

        ShowMenu();
    }

    void ShowMenu()
    {
        currentState = GameState.Menu;

        if (menuRoot != null) menuRoot.SetActive(true);
        if (environmentTitle != null) environmentTitle.SetActive(false);
        if (mainLight != null) mainLight.intensity = menuLightIntensity;

        ClearRound();
        ShowAllPickupObjects();

        if (chest != null) chest.CloseChest();

        if (hud != null)
        {
            hud.UpdatePhase("");
            hud.UpdateTimer(0);
            hud.UpdateScore(0, 0);
            hud.UpdateScore(1, 0);
        }
    }

    public void StartGameFromMenu(GameMode mode)
    {
        if (currentState != GameState.Menu) return;

        currentMode = mode;

        if (menuRoot != null) menuRoot.SetActive(false);
        if (environmentTitle != null) environmentTitle.SetActive(true);
        if (mainLight != null) mainLight.intensity = gameplayLightIntensity;

        currentRound = 1;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScores();

        if (hud != null)
        {
            hud.UpdateScore(0, 0);
            hud.UpdateScore(1, 0);
        }

        DisableAllPickupObjects();
        StartCoroutine(RoundLoop());
    }

    public void StartGameFromMenu()
    {
        StartGameFromMenu(GameMode.QuickMatch);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartMatch()
    {
        Time.timeScale = 1f;
        StopAllCoroutines();
        currentRound = 1;

        foreach (MenuZone zone in menuZones)
            if (zone != null) zone.ResetZone();

        ShowMenu();
    }

    IEnumerator RoundLoop()
    {
        while (currentRound <= GetMaxRounds())
        {
            yield return StartCoroutine(StartRound());

            currentState = GameState.RoundFinished;
            if (hud != null) hud.UpdatePhase("Round finished!");

            yield return new WaitForSeconds(timeBetweenRounds);

            currentRound++;
        }

        EndMatch();
    }

    IEnumerator StartRound()
    {
        ClearAllDropZones();
        ResetAllPickableObjectStates();

        currentState = GameState.Memorize;
        if (hud != null) hud.UpdatePhase("Memorize the objects!");

        ClearRound();
        DisableAllPickupObjects();

        if (chest != null) chest.CloseChest();

        int objectsThisRound = GetObjectsForCurrentRound();
        float memorizeTime = GetMemorizeTimeForCurrentRound();
        float reconstructionTime = GetReconstructionTimeForCurrentRound();
        int impostorsThisRound = GetImpostorsForCurrentRound();

        memorizeWarningPlayed = false;
        reconstructionWarningPlayed = false;

        Debug.Log(
            "Round " + currentRound +
            " | Objects: " + objectsThisRound +
            " | Memorize: " + memorizeTime +
            " | Reconstruction: " + reconstructionTime +
            " | Impostors: " + impostorsThisRound
        );

        SelectObjectsForRound(objectsThisRound);
        SpawnMemoryObjects();

        float timeLeft = memorizeTime;
        while (timeLeft > 0f)
        {
            if (!memorizeWarningPlayed && timeLeft <= 3f)
            {
                memorizeWarningPlayed = true;
                PlayRoundSound(memorizeLastSecondsSound);
            }

            if (hud != null) hud.UpdateTimer(timeLeft);

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        currentState = GameState.Reconstruction;
        HideMemoryObjects();
        ActivateSelectedPickupObjects();
        ActivateImpostorPickupObjects(impostorsThisRound);
        if (chest != null) chest.OpenChestInstant();

        if (hud != null) hud.UpdatePhase("PLACE THEM BACK");
        yield return new WaitForSeconds(2f);
        if (hud != null) hud.UpdatePhase("");
        timeLeft = reconstructionTime;
        while (timeLeft > 0f)
        {
            if (!reconstructionWarningPlayed && timeLeft <= 10f)
            {
                reconstructionWarningPlayed = true;
                PlayRoundSound(reconstructionLastSecondsSound);
            }

            if (hud != null) hud.UpdateTimer(timeLeft);

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if (hud != null) hud.UpdateTimer(0);

        ValidateRoundResults();

        // Reveal correct positions
        foreach (GameObject obj in spawnedMemoryObjects)
            if (obj != null) obj.SetActive(true);

        if (hud != null) hud.UpdatePhase("HOW CLOSE?");
    }

    void ValidateRoundResults()
    {
        PickableObject[] all = Object.FindObjectsByType<PickableObject>(FindObjectsSortMode.None);
        Debug.Log($"[Score] ValidateRoundResults — checking {all.Length} objects");

        foreach (PickableObject obj in all)
        {
            Debug.Log($"[Score] {obj.name} | wasDropped={obj.wasDropped} | owningPlayer={obj.owningPlayerIndex} | scored={obj.scoredThisRound}");

            if (!obj.wasDropped) continue;
            if (obj.scoredThisRound) continue;
            if (obj.owningPlayerIndex < 0) continue;

            MemoryObjectID id = obj.GetComponent<MemoryObjectID>();
            if (id == null)
            {
                Debug.Log($"[Score] {obj.name} has no MemoryObjectID — skipped");
                continue;
            }

            Transform correctSpawn = GetCorrectSpawnForObject(id.objectID);
            if (correctSpawn == null)
            {
                Debug.Log($"[Score] {id.objectID} has no correct spawn (impostor?) — skipped");
                continue;
            }

            float xzDist = Vector2.Distance(
                new Vector2(obj.droppedPosition.x, obj.droppedPosition.z),
                new Vector2(correctSpawn.position.x, correctSpawn.position.z)
            );

            int points = ScoreManager.Instance != null ? ScoreManager.Instance.CalculateScore(xzDist) : 0;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(obj.owningPlayerIndex, points);

            obj.scoredThisRound = true;

            Debug.Log($"[Score] {id.objectID} | dist {xzDist:F2}m | {points} pts → P{obj.owningPlayerIndex + 1}");
        }
    }

    void EndMatch()
    {
        currentState = GameState.RoundFinished;
        Time.timeScale = 0f;

        if (ScoreManager.Instance == null || winnerScreen == null) return;

        int[] scores = ScoreManager.Instance.GetAllScores();

        int winner = -1;
        if (scores[0] > scores[1]) winner = 0;
        else if (scores[1] > scores[0]) winner = 1;

        winnerScreen.ShowWinner(winner, scores);
    }

    int GetMaxRounds()
    {
        return currentMode == GameMode.QuickMatch ? 3 : 10;
    }

    void ClearAllDropZones()
    {
        if (allDropZones == null) return;

        foreach (DropZone zone in allDropZones)
            if (zone != null) zone.ClearZone();
    }

    void ResetAllPickableObjectStates()
    {
        PickableObject[] all = Object.FindObjectsByType<PickableObject>(FindObjectsSortMode.None);

        foreach (PickableObject obj in all)
            obj.ResetRoundState();

        for (int i = 0; i < PickableObject.MaxPlayers; i++)
        {
            PickableObject.playerCarrying[i] = false;
            PickableObject.playerPickupBlockedUntil[i] = 0f;
        }
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
        if (currentRound < 6) return 0;

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

            GameObject obj = Instantiate(prefab, Vector3.zero, spawn.rotation, originalObjectsParent);
            obj.name = prefab.name;

            Transform pickupReference = FindPickupObjectByID(objectID);

            if (pickupReference != null)
            {
                obj.transform.rotation = pickupReference.rotation;
                obj.transform.localScale = pickupReference.localScale;
            }

            GroundPoint groundPoint = obj.GetComponentInChildren<GroundPoint>();

            if (groundPoint != null)
            {
                Vector3 correction = spawn.position - groundPoint.transform.position;
                obj.transform.position += correction;
            }
            else
            {
                obj.transform.position = spawn.position;
            }

            correctSpawnByObjectID[objectID] = spawn;
            Debug.Log("Object " + objectID + " appeared at spawn " + spawn.name);

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
                availableImpostors.Add(child);
        }

        for (int i = 0; i < amount && availableImpostors.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableImpostors.Count);
            availableImpostors[randomIndex].gameObject.SetActive(true);
            availableImpostors.RemoveAt(randomIndex);
        }
    }

    void ShowAllPickupObjects()
    {
        for (int i = 0; i < PickableObject.MaxPlayers; i++)
        {
            PickableObject.playerCarrying[i] = false;
            PickableObject.playerPickupBlockedUntil[i] = 0f;
        }

        foreach (Transform child in pickupObjectsParent)
            child.gameObject.SetActive(true);
    }

    void DisableAllPickupObjects()
    {
        PickableObject[] allPickables = Object.FindObjectsByType<PickableObject>(FindObjectsSortMode.None);

        foreach (PickableObject pickable in allPickables)
            pickable.ForceDisappear();

        foreach (Transform child in pickupObjectsParent)
            child.gameObject.SetActive(false);
    }

    void HideMemoryObjects()
    {
        foreach (GameObject obj in spawnedMemoryObjects)
            obj.SetActive(false);
    }

    void ClearRound()
    {
        foreach (GameObject obj in spawnedMemoryObjects)
            if (obj != null) Destroy(obj);

        spawnedMemoryObjects.Clear();
        selectedObjectIDs.Clear();
        correctSpawnByObjectID.Clear();
    }

    private void PlayRoundSound(AudioClip clip)
    {
        if (roundAudioSource == null || clip == null)
            return;

        roundAudioSource.PlayOneShot(clip);
    }
}