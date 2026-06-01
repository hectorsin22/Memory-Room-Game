using System.Collections;
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
    public TMP_Text roundText;

    [Header("Instructions")]
    public GameObject instructionsPanel;

    private float currentTimerSeconds;

    void Start()
    {
        SetScoresVisible(false);

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
    }

    void Update()
    {
        if (timerText == null) return;

        if (currentTimerSeconds > 0f && currentTimerSeconds <= 10f)
        {
            float flash = Mathf.PingPong(Time.time * 4f, 1f);
            timerText.color = new Color(1f, flash * 0.2f, flash * 0.2f);
        }
        else
        {
            timerText.color = Color.white;
        }
    }

    public void ShowInstructions(bool visible)
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(visible);
    }

    public void SetScoresVisible(bool visible)
    {
        if (player1ScoreText != null) player1ScoreText.gameObject.SetActive(visible);
        if (player2ScoreText != null) player2ScoreText.gameObject.SetActive(visible);
    }

    public void UpdateScore(int playerIndex, int score)
    {
        TMP_Text target = playerIndex == 0 ? player1ScoreText : player2ScoreText;
        if (target == null) return;
        target.text = score.ToString();
        StartCoroutine(PunchScale(target.transform));
    }

    public void UpdateTimer(float seconds)
    {
        currentTimerSeconds = seconds;
        if (timerText == null) return;
        int s = Mathf.CeilToInt(Mathf.Max(seconds, 0f));
        timerText.text = s > 0 ? s.ToString() : "";
    }

    public void UpdatePhase(string phaseName)
    {
        if (phaseText != null)
            phaseText.text = phaseName;
    }

    public void UpdateRound(int round, int maxRounds)
    {
        if (roundText != null)
            roundText.text = $"Round {round}/{maxRounds}";
    }

    private IEnumerator PunchScale(Transform t)
    {
        Vector3 original = t.localScale;
        float duration = 0.35f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.6f;
            t.localScale = original * scale;
            elapsed += Time.deltaTime;
            yield return null;
        }

        t.localScale = original;
    }
}
