using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Tiers (XZ distance thresholds)")]
    public float[] scoreTierDistances = { 20.00f, 40.00f, 60.00f, 80.00f };

    [Header("Points per tier")]
    public int[] tierPoints = { 100, 75, 50, 25 };

    // This event is used to update the HUD whenever a player's score changes
    public event Action<int, int> OnScoreChanged;

    // One score value per player
    private readonly int[] scores = new int[PickableObject.MaxPlayers];

    void Awake()
    {
        // Singleton pattern so there is only one ScoreManager in the scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int CalculateScore(float xzDistance)
    {
        // The closer the object is to its correct position, the more points it gives
        for (int i = 0; i < scoreTierDistances.Length; i++)
        {
            if (xzDistance < scoreTierDistances[i])
                return tierPoints[i];
        }

        // If the object is too far away, it gives no points
        return 0;
    }

    public void AddScore(int playerIndex, int points)
    {
        if (playerIndex < 0 || playerIndex >= PickableObject.MaxPlayers) return;

        scores[playerIndex] += points;

        Debug.Log($"Player {playerIndex + 1} +{points} pts  (total: {scores[playerIndex]})");

        // Notify the UI that this player's score has changed
        OnScoreChanged?.Invoke(playerIndex, scores[playerIndex]);
    }

    public int GetScore(int playerIndex) => scores[playerIndex];

    public int[] GetAllScores() => (int[])scores.Clone();

    public void ResetScores()
    {
        // Reset all player scores at the start of a new game
        for (int i = 0; i < scores.Length; i++)
            scores[i] = 0;

        // Also update the HUD so it displays 0 for everyone
        for (int i = 0; i < PickableObject.MaxPlayers; i++)
            OnScoreChanged?.Invoke(i, 0);
    }
}