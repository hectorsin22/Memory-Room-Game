using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Tiers (XZ distance thresholds)")]
    public float[] scoreTierDistances = { 20.00f, 40.00f, 60.00f, 80.00f };

    [Header("Points per tier")]
    public int[] tierPoints = { 100, 75, 50, 25 };

    public event Action<int, int> OnScoreChanged;

    private readonly int[] scores = new int[PickableObject.MaxPlayers];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int CalculateScore(float xzDistance)
    {
        for (int i = 0; i < scoreTierDistances.Length; i++)
        {
            if (xzDistance < scoreTierDistances[i])
                return tierPoints[i];
        }
        return 0;
    }

    public void AddScore(int playerIndex, int points)
    {
        if (playerIndex < 0 || playerIndex >= PickableObject.MaxPlayers) return;
        scores[playerIndex] += points;
        Debug.Log($"Player {playerIndex + 1} +{points} pts  (total: {scores[playerIndex]})");
        OnScoreChanged?.Invoke(playerIndex, scores[playerIndex]);
    }

    public int GetScore(int playerIndex) => scores[playerIndex];

    public int[] GetAllScores() => (int[])scores.Clone();

    public void ResetScores()
    {
        for (int i = 0; i < scores.Length; i++)
            scores[i] = 0;

        for (int i = 0; i < PickableObject.MaxPlayers; i++)
            OnScoreChanged?.Invoke(i, 0);
    }
}
