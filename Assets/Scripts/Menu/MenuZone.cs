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

    public Vector2 zoneSize = new Vector2(4f, 2f);
    public string playerTag = "Player";

    private bool playerWasInside = false;

    [Header("Audio")]
    public AudioClip buttonSound;
    public float buttonVolume = 0.5f;

    void Update()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        bool someoneInside = false;

        foreach (GameObject player in players)
        {
            Vector3 localPos = transform.InverseTransformPoint(player.transform.position);

            bool insideX = Mathf.Abs(localPos.x) <= zoneSize.x / 2f;
            bool insideZ = Mathf.Abs(localPos.z) <= zoneSize.y / 2f;

            if (insideX && insideZ)
            {
                someoneInside = true;

                if (!playerWasInside)
                {
                    playerWasInside = true;
                    ExecuteAction();
                }

                return;
            }
        }

        if (!someoneInside)
            playerWasInside = false;
    }

    void ExecuteAction()
    {
        
        if (gameManager == null) return;

        PlayButtonSound();

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

    private void OnDrawGizmos()
    {
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
}