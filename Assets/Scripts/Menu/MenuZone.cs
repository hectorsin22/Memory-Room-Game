using UnityEngine;

public class MenuZone : MonoBehaviour
{
    public enum MenuAction
    {
        StartGame,
        PauseGame,
        ResumeGame,
        QuitToMainMenu,
        Done
    }

    public MenuAction action;
    public GameManager gameManager;

    // Size of the invisible area that detects the player
    public Vector2 zoneSize = new Vector2(4f, 2f);
    public string playerTag = "Player";

    [Header("Optional")]
    // If the visual button is hidden, the zone should not work either
    public GameObject linkedOptionRoot;

    [Header("Audio")]
    public AudioClip buttonSound;
    public float buttonVolume = 0.6f;

    private bool playerWasInside = false;

    void Update()
    {
        if (gameManager == null) return;

        // Prevent hidden buttons from being triggered
        if (linkedOptionRoot != null && !linkedOptionRoot.activeInHierarchy)
            return;

        // Ask the GameManager if this button is allowed right now
        if (!gameManager.CanExecuteMenuAction(action))
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        bool someoneInside = false;

        foreach (GameObject player in players)
        {
            // Convert the player's position to the local space of this zone
            Vector3 localPos = transform.InverseTransformPoint(player.transform.position);

            bool insideX = Mathf.Abs(localPos.x) <= zoneSize.x / 2f;
            bool insideZ = Mathf.Abs(localPos.z) <= zoneSize.y / 2f;

            if (insideX && insideZ)
            {
                someoneInside = true;

                // Execute only once when the player enters the zone
                if (!playerWasInside)
                {
                    playerWasInside = true;
                    ExecuteAction();
                }

                return;
            }
        }

        // Allows the zone to be activated again after the player leaves it
        if (!someoneInside)
            playerWasInside = false;
    }

    void ExecuteAction()
    {
        if (gameManager == null) return;
        if (!gameManager.CanExecuteMenuAction(action)) return;

        PlayButtonSound();

        // Each zone calls a different GameManager function depending on its action
        switch (action)
        {
            case MenuAction.StartGame:
                gameManager.StartGameFromMenu();
                break;

            case MenuAction.PauseGame:
                gameManager.PauseGame();
                break;

            case MenuAction.ResumeGame:
                gameManager.ResumeGame();
                break;

            case MenuAction.QuitToMainMenu:
                gameManager.QuitToMainMenu();
                break;

            case MenuAction.Done:
                gameManager.DoneReconstruction();
                break;
        }
    }

    public void ResetZone()
    {
        playerWasInside = false;
    }

    private void PlayButtonSound()
    {
        if (buttonSound == null)
            return;

        AudioSource.PlayClipAtPoint(
            buttonSound,
            transform.position,
            buttonVolume
        );
    }

    private void OnDrawGizmos()
    {
        // Draws the menu zone in the Scene view so we can place it correctly
        Gizmos.color = Color.green;

        Vector3 center = transform.position;
        Vector3 right = transform.right * zoneSize.x / 2f;
        Vector3 forward = transform.forward * zoneSize.y / 2f;

        Vector3 p1 = center - right - forward;
        Vector3 p2 = center + right - forward;
        Vector3 p3 = center + right + forward;
        Vector3 p4 = center - right + forward;

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}