using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Main states of the game
    public enum GameState
    {
        Menu,
        Memorize,
        Reconstruction,
        RoundFinished
    }

    [Header("State")]
    public GameState currentState;
    public int currentRound = 1;

    [Header("Match Settings")]
    public int maxRounds = 3;

    [Header("Menu")]
    public GameObject menuRoot;
    public GameObject pauseMenuRoot;
    public GameObject environmentTitle;

    [Header("Gameplay Buttons")]
    public GameObject pauseButtonRoot;
    public GameObject doneButtonRoot;

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

    [Header("Menu Zones")]
    public MenuZone[] menuZones;

    // Objects spawned only during the memorization phase
    private readonly List<GameObject> spawnedMemoryObjects = new List<GameObject>();

    // IDs of the objects selected for the current round
    private readonly List<string> selectedObjectIDs = new List<string>();

    // Stores where each object originally appeared
    private readonly Dictionary<string, Transform> correctSpawnByObjectID = new Dictionary<string, Transform>();

    private bool memorizeWarningPlayed = false;
    private bool reconstructionWarningPlayed = false;
    private bool roundWarningAudioPlaying = false;

    private bool isPaused = false;
    private bool finishRoundRequested = false;
    private bool doneAlreadyPressedThisRound = false;
    private bool roundIsEnding = false;
    private float reconstructionTimeLeft = 0f;

    private GameState stateBeforePause;

    void Awake()
    {
        // Singleton so other scripts can access the GameManager easily
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
        // Reset everything and show the initial menu
        Time.timeScale = 1f;

        currentState = GameState.Menu;
        isPaused = false;
        finishRoundRequested = false;
        doneAlreadyPressedThisRound = false;
        roundIsEnding = false;
        reconstructionTimeLeft = 0f;

        StopRoundAudio();
        ResetMenuZones();

        if (menuRoot != null) menuRoot.SetActive(true);
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
        if (doneButtonRoot != null) doneButtonRoot.SetActive(false);

        if (environmentTitle != null) environmentTitle.SetActive(false);
        if (mainLight != null) mainLight.intensity = menuLightIntensity;

        ClearAllDropZones();
        ResetAllPickableObjectStates();
        ClearRound();
        ShowAllPickupObjects();

        if (chest != null) chest.CloseChest();

        if (hud != null)
        {
            hud.UpdatePhase("");
            hud.UpdateTimer(0);
            hud.SetScoresVisible(false);
        }
    }

    public void StartGameFromMenu()
    {
        // Starts a new match from the main menu
        if (currentState != GameState.Menu) return;

        Time.timeScale = 1f;

        isPaused = false;
        finishRoundRequested = false;
        doneAlreadyPressedThisRound = false;
        roundIsEnding = false;
        reconstructionTimeLeft = 0f;
        currentRound = 1;

        StopRoundAudio();
        ResetMenuZones();

        if (menuRoot != null) menuRoot.SetActive(false);
        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (environmentTitle != null) environmentTitle.SetActive(true);
        if (mainLight != null) mainLight.intensity = gameplayLightIntensity;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScores();

        if (hud != null)
        {
            hud.SetScoresVisible(true);
            hud.UpdateScore(0, 0);
            hud.UpdateScore(1, 0);
        }

        if (chest != null)
            chest.EnableCloseSound();

        DisableAllPickupObjects();
        UpdateGameplayButtons();

        StartCoroutine(InstructionsThenGame());
    }

    IEnumerator InstructionsThenGame()
    {
        if (hud != null) hud.ShowInstructions(true);
        yield return new WaitForSeconds(8f);
        if (hud != null) hud.ShowInstructions(false);
        StartCoroutine(RoundLoop());
    }

    public void PauseGame()
    {
        // Pause the game and show the pause menu
        if (currentState == GameState.Menu) return;
        if (isPaused) return;
        if (roundIsEnding) return;

        isPaused = true;
        stateBeforePause = currentState;

        Time.timeScale = 0f;

        if (roundAudioSource != null && roundAudioSource.isPlaying)
            roundAudioSource.Pause();

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
        if (pauseButtonRoot != null) pauseButtonRoot.SetActive(false);
        if (doneButtonRoot != null) doneButtonRoot.SetActive(false);

        if (mainLight != null) mainLight.intensity = menuLightIntensity;

        ResetMenuZones();
    }

    public void ResumeGame()
    {
        // Continue from where the game was paused
        if (!isPaused) return;

        isPaused = false;
        currentState = stateBeforePause;

        Time.timeScale = 1f;

        if (roundAudioSource != null && roundWarningAudioPlaying)
            roundAudioSource.UnPause();

        if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
        if (mainLight != null) mainLight.intensity = gameplayLightIntensity;

        ResetMenuZones();
        UpdateGameplayButtons();
    }

    public void QuitToMainMenu()
    {
        // Stop the current match and return to the first screen
        Time.timeScale = 1f;

        StopAllCoroutines();
        StopRoundAudio();

        isPaused = false;
        finishRoundRequested = false;
        doneAlreadyPressedThisRound = false;
        roundIsEnding = false;
        reconstructionTimeLeft = 0f;
        currentRound = 1;

        ShowMenu();
    }

    public void DoneReconstruction()
    {
        // Allows players to finish the reconstruction phase early
        if (isPaused) return;
        if (currentState != GameState.Reconstruction) return;
        if (doneAlreadyPressedThisRound) return;
        if (roundIsEnding) return;

        if (reconstructionTimeLeft <= 1f) return;

        doneAlreadyPressedThisRound = true;
        finishRoundRequested = true;

        if (doneButtonRoot != null)
            doneButtonRoot.SetActive(false);

        ResetMenuZones();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartMatch()
    {
        QuitToMainMenu();
    }

    IEnumerator RoundLoop()
    {
        // Main loop: keeps creating rounds until the match ends
        while (currentRound <= GetMaxRounds())
        {
            yield return StartCoroutine(StartRound());

            currentState = GameState.RoundFinished;
            UpdateGameplayButtons();
            if (hud != null) hud.UpdatePhase("ROUND FINISHED");
            yield return new WaitForSeconds(2f);
            if (hud != null) hud.UpdatePhase("");

            yield return new WaitForSeconds(Mathf.Max(0f, timeBetweenRounds - 2f));

            currentRound++;
        }

        EndMatch();
    }

    IEnumerator StartRound()
    {
        // Controls the full flow of one round
        finishRoundRequested = false;
        doneAlreadyPressedThisRound = false;
        roundIsEnding = false;
        reconstructionTimeLeft = 0f;

        StopRoundAudio();

        ClearAllDropZones();
        ResetAllPickableObjectStates();
        ClearRound();
        DisableAllPickupObjects();

        if (chest != null) chest.CloseChest();

        int objectsThisRound = GetObjectsForCurrentRound();
        float memorizeTime = GetMemorizeTimeForCurrentRound();
        float reconstructionTime = GetReconstructionTimeForCurrentRound();
        int impostorsThisRound = GetImpostorsForCurrentRound();

        memorizeWarningPlayed = false;
        reconstructionWarningPlayed = false;

        SelectObjectsForRound(objectsThisRound);
        SpawnMemoryObjects();

        // Memorization phase
        currentState = GameState.Memorize;
        UpdateGameplayButtons();

        if (hud != null)
        {
            hud.UpdatePhase($"ROUND {currentRound}");
            hud.UpdateRound(currentRound, GetMaxRounds());
            hud.UpdateTimer(0);
        }

        yield return new WaitForSeconds(2f);

        if (hud != null) hud.UpdatePhase("MEMORIZE");

        yield return new WaitForSeconds(2f);

        if (hud != null) hud.UpdatePhase("");

        float timeLeft = memorizeTime;

        // Countdown while players memorize the objects
        while (timeLeft > 0f)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            if (!memorizeWarningPlayed && timeLeft <= 3f)
            {
                memorizeWarningPlayed = true;
                PlayRoundSound(memorizeLastSecondsSound);
            }

            if (hud != null) hud.UpdateTimer(timeLeft);

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        StopRoundAudio();

        // Reconstruction phase
        currentState = GameState.Reconstruction;
        UpdateGameplayButtons();

        HideMemoryObjects();
        ActivateSelectedPickupObjects();
        ActivateImpostorPickupObjects(impostorsThisRound);

        if (chest != null) chest.OpenChestInstant();

        if (hud != null) hud.UpdatePhase("PLACE THEM BACK");

        yield return new WaitForSeconds(2f);

        if (hud != null) hud.UpdatePhase("");

        timeLeft = reconstructionTime;

        // Countdown while players place the objects
        while (timeLeft > 0f && !finishRoundRequested)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            reconstructionTimeLeft = timeLeft;

            if (!reconstructionWarningPlayed && timeLeft <= 10f)
            {
                reconstructionWarningPlayed = true;
                PlayRoundSound(reconstructionLastSecondsSound);
            }

            if (hud != null) hud.UpdateTimer(timeLeft);

            timeLeft -= Time.deltaTime;
            yield return null;
        }

        roundIsEnding = true;
        finishRoundRequested = false;

        StopRoundAudio();

        if (hud != null) hud.UpdateTimer(0);

        if (doneButtonRoot != null)
            doneButtonRoot.SetActive(false);

        ResetMenuZones();

        ForceDisappearCarriedObjects();

        ValidateRoundResults();

        foreach (GameObject obj in spawnedMemoryObjects)
            if (obj != null) obj.SetActive(true);

        if (hud != null) hud.UpdatePhase("HOW CLOSE?");
        yield return new WaitForSeconds(3f);

        foreach (GameObject obj in spawnedMemoryObjects)
            if (obj != null) obj.SetActive(false);

        if (hud != null)
        {
            if (doneAlreadyPressedThisRound)
                hud.UpdatePhase("DONE");
            else
                hud.UpdatePhase("TIME UP");
        }

        yield return new WaitForSeconds(2f);

        if (hud != null) hud.UpdatePhase("");

        roundIsEnding = false;
    }

    void ValidateRoundResults()
    {
        // Compare placed objects with their correct positions and add points
        PickableObject[] all = Object.FindObjectsByType<PickableObject>(FindObjectsSortMode.None);

        foreach (PickableObject obj in all)
        {
            if (!obj.wasDropped) continue;
            if (obj.scoredThisRound) continue;
            if (obj.owningPlayerIndex < 0) continue;

            MemoryObjectID id = obj.GetComponent<MemoryObjectID>();
            if (id == null) continue;

            Transform correctSpawn = GetCorrectSpawnForObject(id.objectID);
            if (correctSpawn == null) continue;

            float xzDist = Vector2.Distance(
                new Vector2(obj.droppedPosition.x, obj.droppedPosition.z),
                new Vector2(correctSpawn.position.x, correctSpawn.position.z)
            );

            int points = ScoreManager.Instance != null ? ScoreManager.Instance.CalculateScore(xzDist) : 0;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(obj.owningPlayerIndex, points);

            obj.scoredThisRound = true;
        }
    }

    void ForceDisappearCarriedObjects()
    {
        // Safety check: removes objects still being carried before changing round
        PickableObject[] all = Object.FindObjectsByType<PickableObject>(FindObjectsSortMode.None);

        foreach (PickableObject obj in all)
        {
            if (obj != null && obj.isBeingCarried)
                obj.ForceDisappear();
        }

        for (int i = 0; i < PickableObject.MaxPlayers; i++)
        {
            PickableObject.playerCarrying[i] = false;
            PickableObject.playerPickupBlockedUntil[i] = 0f;
        }
    }

    void EndMatch()
    {
        // Calculate the winner and show the final screen
        currentState = GameState.RoundFinished;
        UpdateGameplayButtons();

        StopRoundAudio();

        if (ScoreManager.Instance == null || winnerScreen == null)
        {
            Debug.LogWarning("Winner screen or ScoreManager missing. Returning to main menu.");
            QuitToMainMenu();
            return;
        }

        int[] scores = ScoreManager.Instance.GetAllScores();

        int winner = -1;

        if (scores[0] > scores[1]) winner = 0;
        else if (scores[1] > scores[0]) winner = 1;

        winnerScreen.ShowWinner(winner, scores);

        Time.timeScale = 0f;
    }

    int GetMaxRounds()
    {
        return maxRounds;
    }

    void UpdateGameplayButtons()
    {
        // Shows Pause during the game and Done only during reconstruction
        bool inGame = currentState != GameState.Menu && !isPaused && !roundIsEnding;

        if (pauseButtonRoot != null)
            pauseButtonRoot.SetActive(inGame);

        if (doneButtonRoot != null)
            doneButtonRoot.SetActive(currentState == GameState.Reconstruction && !isPaused && !roundIsEnding);
    }

    void ResetMenuZones()
    {
        if (menuZones == null) return;

        foreach (MenuZone zone in menuZones)
            if (zone != null) zone.ResetZone();
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
        // More objects appear as rounds progress
        if (currentRound <= 5)
            return Mathf.Min(2 + currentRound, maxObjects);

        return Mathf.Min(7 + (currentRound - 5), maxObjects);
    }

    float GetMemorizeTimeForCurrentRound()
    {
        // More objects require more memorization time, but with a limit
        int objectsThisRound = GetObjectsForCurrentRound();

        return Mathf.Min(
            5f + (objectsThisRound * 1.2f),
            14f
        );
    }

    float GetReconstructionTimeForCurrentRound()
    {
        // More objects require more reconstruction time, but with a limit
        int objectsThisRound = GetObjectsForCurrentRound();

        return Mathf.Min(
            10f + (objectsThisRound * 3f),
            45f
        );
    }

    int GetImpostorsForCurrentRound()
    {
        // Fake objects start appearing in later rounds
        if (currentRound < 6) return 0;

        return Mathf.Min(currentRound - 5, maxImpostors);
    }

    void SelectObjectsForRound(int amount)
    {
        // Randomly chooses which objects will be used this round
        selectedObjectIDs.Clear();

        List<GameObject> availablePrefabs = new List<GameObject>(memoryObjectPrefabs);

        for (int i = 0; i < amount && availablePrefabs.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePrefabs.Count);
            GameObject selectedPrefab = availablePrefabs[randomIndex];
            MemoryObjectID id = selectedPrefab.GetComponent<MemoryObjectID>();

            if (id == null)
            {
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
        // Spawns the selected objects in random positions for memorization
        List<Transform> availableSpawns = new List<Transform>(memorySpawnPoints);

        foreach (string objectID in selectedObjectIDs)
        {
            GameObject prefab = FindMemoryPrefabByID(objectID);

            if (prefab == null)
                continue;

            if (availableSpawns.Count == 0)
                return;

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

    public bool CanExecuteMenuAction(MenuZone.MenuAction action)
    {
        // Prevents invisible buttons or wrong-state buttons from working
        switch (action)
        {
            case MenuZone.MenuAction.StartGame:
                return currentState == GameState.Menu && !isPaused;

            case MenuZone.MenuAction.PauseGame:
                return currentState != GameState.Menu && !isPaused && !roundIsEnding;

            case MenuZone.MenuAction.ResumeGame:
                return isPaused;

            case MenuZone.MenuAction.QuitToMainMenu:
                return isPaused;

            case MenuZone.MenuAction.Done:
                return currentState == GameState.Reconstruction &&
                       !isPaused &&
                       !roundIsEnding &&
                       !doneAlreadyPressedThisRound &&
                       reconstructionTimeLeft > 1f;

            default:
                return false;
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
        // Enables only the pickup objects selected for this round
        foreach (Transform child in pickupObjectsParent)
        {
            MemoryObjectID id = child.GetComponent<MemoryObjectID>();

            if (id != null && selectedObjectIDs.Contains(id.objectID))
                child.gameObject.SetActive(true);
        }
    }

    void ActivateImpostorPickupObjects(int amount)
    {
        // Adds fake objects to make the game harder
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
        // Removes temporary memory objects from the previous round
        foreach (GameObject obj in spawnedMemoryObjects)
            if (obj != null) Destroy(obj);

        spawnedMemoryObjects.Clear();
        selectedObjectIDs.Clear();
        correctSpawnByObjectID.Clear();
    }

    private void PlayRoundSound(AudioClip clip)
    {
        // Plays countdown warning sounds
        if (roundAudioSource == null || clip == null)
            return;

        roundAudioSource.Stop();
        roundAudioSource.clip = clip;
        roundAudioSource.loop = false;
        roundAudioSource.Play();

        roundWarningAudioPlaying = true;
    }

    private void StopRoundAudio()
    {
        // Stops countdown audio when changing phase or pausing the match
        if (roundAudioSource == null)
            return;

        roundAudioSource.Stop();
        roundWarningAudioPlaying = false;
    }
}