using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("Player Scores")]
    public TMP_Text player1ScoreText;
    public TMP_Text player2ScoreText;

    [Header("Center")]
    public TMP_Text timerText;
    public TMP_Text phaseText;

    void Start()
    {
        UpdateScore(0, 0);
        UpdateScore(1, 0);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
            UpdateScore(0, ScoreManager.Instance.GetScore(0));
            UpdateScore(1, ScoreManager.Instance.GetScore(1));
        }
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
    }

    public void UpdateScore(int playerIndex, int score)
    {
        if (playerIndex == 0 && player1ScoreText != null)
            player1ScoreText.text = score.ToString();
        else if (playerIndex == 1 && player2ScoreText != null)
            player2ScoreText.text = score.ToString();
    }

    public void UpdateTimer(float seconds)
    {
        if (timerText == null) return;

        int s = Mathf.CeilToInt(Mathf.Max(seconds, 0f));
        timerText.text = s > 0 ? s + "s" : "";
    }

    public void UpdatePhase(string phaseName)
    {
        if (phaseText != null)
            phaseText.text = phaseName;
    }
}
