using UnityEngine;

public class MenuZone : MonoBehaviour
{
    public enum MenuAction
    {
        StartGame,
        QuitGame
    }

    public MenuAction action;
    public GameManager gameManager;

    public Vector2 zoneSize = new Vector2(4f, 2f);
    public string playerTag = "Player";

    private bool alreadyActivated = false;

    void Update()
    {
        if (alreadyActivated) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);

        foreach (GameObject player in players)
        {
            Vector3 localPos = transform.InverseTransformPoint(player.transform.position);

            bool insideX = Mathf.Abs(localPos.x) <= zoneSize.x / 2f;
            bool insideZ = Mathf.Abs(localPos.z) <= zoneSize.y / 2f;

            if (insideX && insideZ)
            {
                alreadyActivated = true;

                if (action == MenuAction.StartGame)
                    gameManager.StartGameFromMenu();

                if (action == MenuAction.QuitGame)
                    gameManager.QuitGame();

                return;
            }
        }
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
}