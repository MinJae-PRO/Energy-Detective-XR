using UnityEngine;

/// <summary>
/// Enhanced game manager that coordinates game state, scoring, timer, and UI updates.
/// Attach to a dedicated GameObject in a scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("UI Manager")]
    [Tooltip("Main UI Manager for all UI updates")]
    public UIManager uiManager;

    [Header("Dashboard Reference")]
    [Tooltip("Dashboard UI for detailed stats")]
    public DashboardUI dashboardUI;

    [Header("Game State")]
    [Tooltip("Is the game currently running?")]
    public bool IsGameRunning { get; private set; } = false;

    [Tooltip("Has the game been completed?")]
    public bool IsGameComplete { get; private set; } = false;

    [Header("Score Settings")]
    [Tooltip("Points awarded per fixed object")]
    public int pointsPerObject = 10;

    [Tooltip("Current player score")]
    public int CurrentScore { get; private set; } = 0;

    [Header("Timer Settings")]
    [Tooltip("Current elapsed time")]
    public float CurrentTime { get; private set; } = 0f;

    [Header("Progress Tracking")]
    [Tooltip("Number of objects fixed")]
    public int FixedObjectCount { get; private set; } = 0;

    [Tooltip("Total number of energy objects in the scene")]
    public int TotalObjectCount { get; private set; } = 0;

    [Tooltip("Number of wrong quiz attempts")]
    public int WrongAttemptCount { get; private set; } = 0;

    // Events
    public delegate void GameEvent();
    public event GameEvent OnGameStarted;
    public event GameEvent OnGameCompleted;
    public event GameEvent OnObjectFixed;

    public delegate void ScoreEvent(int newScore, int scoreDelta);
    public event ScoreEvent OnScoreChanged;

    void Start()
    {
        InitializeGame();
    }

    void Update()
    {
        if (IsGameRunning && !IsGameComplete)
        {
            UpdateTimer();
        }
    }

    /// <summary>
    /// Initializes or resets the game state.
    /// </summary>
    public void InitializeGame()
    {
        EnergyObject[] objects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        TotalObjectCount = objects.Length;
        FixedObjectCount = 0;
        CurrentScore = 0;
        CurrentTime = 0f;
        WrongAttemptCount = 0;
        IsGameRunning = true;
        IsGameComplete = false;

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(CurrentScore);
            uiManager.UpdateTimer(CurrentTime);
            uiManager.UpdateProgress(0, TotalObjectCount);
        }

        if (dashboardUI != null)
        {
            dashboardUI.UpdateProgress(0, TotalObjectCount);
        }

        OnGameStarted?.Invoke();

        Debug.Log($"[GameManager] Game initialized. {TotalObjectCount} energy objects to fix.");
    }

    /// <summary>
    /// Called when an energy object is successfully fixed.
    /// </summary>
    public void FixEnergyObject(string objectName)
    {
        if (IsGameComplete) return;

        int scoreDelta = pointsPerObject;
        CurrentScore += scoreDelta;
        FixedObjectCount++;

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateScore(CurrentScore, true, $"+{scoreDelta}");
            uiManager.UpdateProgress(FixedObjectCount, TotalObjectCount);
            uiManager.ShowPositiveFeedback($"{objectName} Fixed!");
        }

        if (dashboardUI != null)
        {
            dashboardUI.UpdateScore(CurrentScore);
            dashboardUI.UpdateProgress(FixedObjectCount, TotalObjectCount);
        }

        OnScoreChanged?.Invoke(CurrentScore, scoreDelta);
        OnObjectFixed?.Invoke();

        Debug.Log($"[GameManager] Fixed: {objectName} | Score: {CurrentScore} | Progress: {FixedObjectCount}/{TotalObjectCount}");

        // Check for completion
        if (FixedObjectCount >= TotalObjectCount)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Called when the player answers a quiz question incorrectly.
    /// </summary>
    public void RegisterWrongAttempt()
    {
        WrongAttemptCount++;

        if (uiManager != null)
        {
            uiManager.ShowNegativeFeedback("Incorrect! Try Again.");
        }

        Debug.Log($"[GameManager] Wrong attempt #{WrongAttemptCount}");
    }

    /// <summary>
    /// Gets the current completion percentage (0-1).
    /// </summary>
    public float GetCompletionPercent()
    {
        return TotalObjectCount > 0 ? (float)FixedObjectCount / TotalObjectCount : 0f;
    }

    /// <summary>
    /// Gets the calculated star rating (0-3).
    /// </summary>
    public int GetStarRating()
    {
        if (FixedObjectCount < TotalObjectCount) return 0;
        if (CurrentTime < 90f) return 3;
        if (CurrentTime < 180f) return 2;
        return 1;
    }

    /// <summary>
    /// Gets the accuracy percentage.
    /// </summary>
    public float GetAccuracy()
    {
        int totalAttempts = FixedObjectCount + WrongAttemptCount;
        if (totalAttempts == 0) return 100f;
        return (float)FixedObjectCount / totalAttempts * 100f;
    }

    private void UpdateTimer()
    {
        CurrentTime += Time.deltaTime;

        if (uiManager != null)
        {
            uiManager.UpdateTimer(CurrentTime);
        }
    }

    private void EndGame()
    {
        IsGameComplete = true;
        IsGameRunning = false;

        int starRating = GetStarRating();
        float accuracy = GetAccuracy();

        // Show completion UI
        if (uiManager != null)
        {
            uiManager.UpdateProgress(FixedObjectCount, TotalObjectCount);
            uiManager.ShowCompleteMessage(CurrentTime);
        }

        OnGameCompleted?.Invoke();

        Debug.Log($"[GameManager] Game Complete! Score: {CurrentScore} | Time: {CurrentTime:F1}s | Stars: {starRating} | Accuracy: {accuracy:F0}%");
    }
}
