using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerScreenController : MonoBehaviour
{
    [Header("Panel")]
    public GameObject winnerPanel;

    [Header("Text")]
    public TMP_Text winnerText;
    public TMP_Text scoreSummaryText;

    [Header("Button")]
    public Button playAgainButton;

    void Start()
    {
        if (winnerPanel != null) winnerPanel.SetActive(false);
        if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainClicked);
    }

    public void ShowWinner(int winnerIndex, int[] scores)
    {
        if (winnerPanel != null) winnerPanel.SetActive(true);

        if (winnerText != null)
        {
            winnerText.text = winnerIndex switch
            {
                0 => "Player 1 Wins!",
                1 => "Player 2 Wins!",
                _ => "It's a Draw!"
            };
        }

        if (scoreSummaryText != null)
        {
            string p1 = scores.Length > 0 ? scores[0].ToString() : "0";
            string p2 = scores.Length > 1 ? scores[1].ToString() : "0";
            scoreSummaryText.text = $"P1: {p1}  —  P2: {p2}";
        }
    }

    void OnPlayAgainClicked()
    {
        if (winnerPanel != null) winnerPanel.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.RestartMatch();
    }
}
