using UnityEngine;
using UnityEngine.InputSystem;

public class PickableObject : MonoBehaviour
{
    public const int MaxPlayers = 2;

    // Per-player carrying state (replaces the old single global flag)
    public static bool[] playerCarrying = new bool[MaxPlayers];
    public static float[] playerPickupBlockedUntil = new float[MaxPlayers];

    public bool isBeingCarried = false;

    public float crouchHeight = 0.3f;
    public float standHeight = 0.8f;
    public float pickupCooldownAfterDrop = 2f;
    public float carriedObjectHeight = 0.2f;
    public Vector2 carryOffsetXZ = Vector2.zero;

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip dropSound;

    // -1 = unclaimed. Set permanently on first pickup for this round.
    public int owningPlayerIndex = -1;

    public bool scoredThisRound = false;
    public bool wasDropped = false;
    public Vector3 droppedPosition;

    // Which DropZone currently holds this object (null if not placed)
    public DropZone currentDropZone = null;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Transform carrier;

    private Quaternion carriedRotation;
    private bool playerStoodUpAfterPickup = false;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        carriedObjectHeight = transform.position.y;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isBeingCarried || carrier == null) return;

        Vector3 targetPosition = new Vector3(
            carrier.position.x + carryOffsetXZ.x,
            carriedObjectHeight,
            carrier.position.z + carryOffsetXZ.y
        );

        transform.position = targetPosition;
        transform.rotation = carriedRotation;

        // TEMPORARY DEBUG CONTROL — remove before final VR build
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Soltando objeto con E");
            Drop();
            return;
        }

        if (!playerStoodUpAfterPickup && carrier.position.y > standHeight)
            playerStoodUpAfterPickup = true;

        if (playerStoodUpAfterPickup && carrier.position.y < crouchHeight)
        {
            Debug.Log("Jugador agachado por segunda vez, soltando objeto!");
            Drop();
        }
    }

    public void PickUp(Transform player)
    {
        // Only allow pickup during reconstruction phase
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.currentState != GameManager.GameState.Reconstruction)
            return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();

        if (pm == null)
            return;

        int playerIndex = pm.playerIndex;

        if (playerIndex < 0 || playerIndex >= MaxPlayers)
            return;

        // Player must be crouching
        if (player.position.y > crouchHeight)
            return;

        // Per-player cooldown
        if (Time.time < playerPickupBlockedUntil[playerIndex])
            return;

        if (isBeingCarried)
            return;

        // Per-player carry limit (one object per player at a time)
        if (playerCarrying[playerIndex])
            return;

        // Ownership check: unclaimed or already owned by this player
        if (owningPlayerIndex != -1 && owningPlayerIndex != playerIndex)
            return;

        // Claim ownership permanently for this round
        if (owningPlayerIndex == -1)
            owningPlayerIndex = playerIndex;

        // If object was sitting in a drop zone, clear it so the zone is free
        if (currentDropZone != null)
        {
            currentDropZone.ClearZone();
            currentDropZone = null;
        }

        isBeingCarried = true;
        playerCarrying[playerIndex] = true;

        carrier = player;

        carriedObjectHeight = transform.position.y;
        carriedRotation = transform.rotation;

        playerStoodUpAfterPickup = false;

        PlaySound(pickupSound, 2f);
    }

    public void Drop()
    {
        if (!isBeingCarried)
            return;

        isBeingCarried = false;

        if (owningPlayerIndex >= 0 && owningPlayerIndex < MaxPlayers)
        {
            playerCarrying[owningPlayerIndex] = false;

            playerPickupBlockedUntil[owningPlayerIndex] =
                Time.time + pickupCooldownAfterDrop;

            // Record where this object landed for scoring at round end
            droppedPosition = transform.position;
            wasDropped = true;
        }

        transform.SetParent(originalParent, true);

        carrier = null;
        playerStoodUpAfterPickup = false;

        PlaySound(dropSound, 0.8f);
    }

    // Called by GameManager at the start of each round to reset all per-round state
    public void ResetRoundState()
    {
        owningPlayerIndex = -1;
        scoredThisRound = false;
        wasDropped = false;
        droppedPosition = Vector3.zero;
        currentDropZone = null;
    }

    public void ForceDisappear()
    {
        if (isBeingCarried)
        {
            if (owningPlayerIndex >= 0 && owningPlayerIndex < MaxPlayers)
                playerCarrying[owningPlayerIndex] = false;

            isBeingCarried = false;

            carrier = null;
            playerStoodUpAfterPickup = false;
        }

        transform.SetParent(originalParent, true);

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        gameObject.SetActive(false);
    }

    public void ReturnToOriginalPosition()
    {
        Drop();

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }
}