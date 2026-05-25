using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Central UI manager that coordinates all UI systems: score, timer, feedback, dashboard, and completion screen.
/// Attach to a Canvas GameObject in a scene and assign all references.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Core UI References")]
    [Tooltip("The main HUD canvas")]
    public Canvas hudCanvas;

    [Header("Score Display")]
    [Tooltip("Text for displaying the current score")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Optional: Score panel background image")]
    public Image scoreBackground;

    [Tooltip("Color for score background")]
    public Color scoreBackgroundColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Timer Display")]
    [Tooltip("Text for displaying the elapsed time")]
    public TextMeshProUGUI timerText;

    [Tooltip("Optional: Timer panel background image")]
    public Image timerBackground;

    [Tooltip("Color for timer background")]
    public Color timerBackgroundColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Progress Display")]
    [Tooltip("Text showing objects fixed (e.g., '3/8 Fixed')")]
    public TextMeshProUGUI progressText;

    [Tooltip("Progress bar fill image")]
    public Image progressFillImage;

    [Tooltip("Color when progress is incomplete")]
    public Color progressIncompleteColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Tooltip("Color when all objects are fixed")]
    public Color progressCompleteColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Header("Objective Reminder")]
    [Tooltip("Text showing the current game objective")]
    public TextMeshProUGUI objectiveText;

    [Tooltip("How long the objective text shows at game start")]
    public float objectiveShowDuration = 6f;

    [Tooltip("Objective message text")]
    [TextArea(2, 4)]
    public string objectiveMessage = "Find and fix all broken energy objects!\nAnswer the quiz questions correctly to repair them.";

    [Header("Sub-Module References")]
    [Tooltip("Feedback text controller for floating messages")]
    public FeedbackTextController feedbackController;

    [Tooltip("Dashboard UI for detailed stats")]
    public DashboardUI dashboardUI;

    [Tooltip("Crosshair controller")]
    public CrosshairController crosshairController;

    [Tooltip("Completion screen controller")]
    public CompletionScreenController completionScreen;

    [Header("Visual Settings")]
    [Tooltip("Color for score text")]
    public Color scoreTextColor = new Color(1f, 0.92f, 0.2f, 1f);

    [Tooltip("Color for timer text")]
    public Color timerTextColor = new Color(0.3f, 0.9f, 1f, 1f);

    [Tooltip("Color for progress text")]
    public Color progressTextColor = Color.white;

    [Tooltip("Score font size")]
    public float scoreFontSize = 28f;

    [Tooltip("Timer font size")]
    public float timerFontSize = 24f;

    [Header("Animation Settings")]
    [Tooltip("Animate score changes with a pulse effect")]
    public bool animateScoreChanges = true;

    [Tooltip("Duration of score pulse animation")]
    public float scorePulseDuration = 0.25f;

    [Tooltip("Scale of score pulse peak")]
    public float scorePulseScale = 1.15f;

    // Internal state
    private int currentScore = 0;
    private float currentTime = 0f;
    private int fixedObjectsCount = 0;
    private int totalObjectsCount = 0;
    private Vector3 scoreTextOriginalScale;
    private Coroutine scorePulseCoroutine;
    private Coroutine objectiveHideCoroutine;

    void Start()
    {
        // Initialize displays
        UpdateScore(0);
        UpdateTimer(0f);
        UpdateProgress(0, 0);

        // Show objective reminder
        ShowObjectiveReminder();

        // Apply visual settings
        ApplyVisualSettings();

        // Find references if not assigned
        FindRequiredReferences();
    }

    /// <summary>
    /// Updates the score display. Optionally shows floating feedback.
    /// </summary>
    public void UpdateScore(int newScore, bool showFeedback = false, string feedbackMessage = null)
    {
        int scoreDelta = newScore - currentScore;
        currentScore = newScore;

        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore:N0}";
        }

        // Update dashboard if available
        if (dashboardUI != null)
        {
            dashboardUI.UpdateScore(currentScore);
        }

        // Animate score pulse
        if (animateScoreChanges && scoreDelta > 0)
        {
            if (scorePulseCoroutine != null)
            {
                StopCoroutine(scorePulseCoroutine);
            }
            scorePulseCoroutine = StartCoroutine(PulseScoreText());
        }

        // Show floating feedback
        if (showFeedback && feedbackController != null && scoreDelta > 0)
        {
            string message = feedbackMessage ?? $"+{scoreDelta} Points!";
            feedbackController.ShowPositiveFeedback(message);
        }
    }

    /// <summary>
    /// Updates the timer display.
    /// </summary>
    public void UpdateTimer(float timeInSeconds)
    {
        currentTime = timeInSeconds;

        if (timerText != null)
        {
            timerText.text = FormatTime(timeInSeconds);
        }

        if (dashboardUI != null)
        {
            dashboardUI.UpdateTimer(timeInSeconds);
        }
    }

    /// <summary>
    /// Updates the progress display (objects fixed).
    /// </summary>
    public void UpdateProgress(int fixedCount, int totalCount)
    {
        fixedObjectsCount = fixedCount;
        totalObjectsCount = totalCount;

        if (progressText != null)
        {
            progressText.text = $"{fixedCount} / {totalCount} Fixed";
        }

        if (progressFillImage != null)
        {
            float fillAmount = totalCount > 0 ? (float)fixedCount / totalCount : 0f;
            progressFillImage.fillAmount = fillAmount;
            progressFillImage.color = fixedCount >= totalCount ? progressCompleteColor : progressIncompleteColor;
        }

        if (dashboardUI != null)
        {
            dashboardUI.UpdateProgress(fixedCount, totalCount);
        }
    }

    /// <summary>
    /// Shows positive feedback text (e.g., "Energy Fixed!").
    /// </summary>
    public void ShowPositiveFeedback(string message)
    {
        if (feedbackController != null)
        {
            feedbackController.ShowPositiveFeedback(message);
        }
    }

    /// <summary>
    /// Shows negative feedback text (e.g., "Try Again!").
    /// </summary>
    public void ShowNegativeFeedback(string message)
    {
        if (feedbackController != null)
        {
            feedbackController.ShowNegativeFeedback(message);
        }
    }

    /// <summary>
    /// Shows info feedback text.
    /// </summary>
    public void ShowInfoFeedback(string message)
    {
        if (feedbackController != null)
        {
            feedbackController.ShowInfoFeedback(message);
        }
    }

    /// <summary>
    /// Shows the old-style complete message (legacy support).
    /// Uses the new completion screen if available.
    /// </summary>
    public void ShowCompleteMessage(float finalTime)
    {
        if (completionScreen != null)
        {
            completionScreen.ShowCompletionScreen(currentScore, finalTime, fixedObjectsCount, totalObjectsCount);
        }
        else
        {
            ShowInfoFeedback($"All Energy Objects Fixed!\nTime: {FormatTime(finalTime)}\nScore: {currentScore}");
        }

        if (crosshairController != null)
        {
            crosshairController.HideCrosshair();
        }
    }

    /// <summary>
    /// Shows the objective reminder text at game start.
    /// </summary>
    public void ShowObjectiveReminder()
    {
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);
            objectiveText.text = objectiveMessage;

            if (objectiveHideCoroutine != null)
            {
                StopCoroutine(objectiveHideCoroutine);
            }
            objectiveHideCoroutine = StartCoroutine(HideObjectiveAfterDelay());
        }
    }

    /// <summary>
    /// Updates the currently targeted object on the dashboard.
    /// </summary>
    public void UpdateTargetedObject(string objectName, bool isFixed)
    {
        if (dashboardUI != null)
        {
            dashboardUI.UpdateCurrentObject(objectName, isFixed);
        }
    }

    /// <summary>
    /// Clears the targeted object display.
    /// </summary>
    public void ClearTargetedObject()
    {
        if (dashboardUI != null)
        {
            dashboardUI.ClearCurrentObject();
        }
    }

    /// <summary>
    /// Shows the dashboard panel.
    /// </summary>
    public void ShowDashboard()
    {
        if (dashboardUI != null)
        {
            dashboardUI.ShowDashboard();
        }
    }

    /// <summary>
    /// Hides the dashboard panel.
    /// </summary>
    public void HideDashboard()
    {
        if (dashboardUI != null)
        {
            dashboardUI.HideDashboard();
        }
    }

    /// <summary>
    /// Toggles the dashboard visibility.
    /// </summary>
    public void ToggleDashboard()
    {
        if (dashboardUI != null)
        {
            dashboardUI.ToggleDashboard();
        }
    }

    /// <summary>
    /// Hides the main HUD elements.
    /// </summary>
    public void HideHUD()
    {
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the main HUD elements.
    /// </summary>
    public void ShowHUD()
    {
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (progressText != null) progressText.gameObject.SetActive(true);
    }

    private void ApplyVisualSettings()
    {
        if (scoreText != null)
        {
            scoreText.color = scoreTextColor;
            scoreText.fontSize = scoreFontSize;
            scoreText.fontStyle = FontStyles.Bold;
            scoreTextOriginalScale = scoreText.transform.localScale;
        }

        if (timerText != null)
        {
            timerText.color = timerTextColor;
            timerText.fontSize = timerFontSize;
        }

        if (progressText != null)
        {
            progressText.color = progressTextColor;
        }

        if (scoreBackground != null)
        {
            scoreBackground.color = scoreBackgroundColor;
        }

        if (timerBackground != null)
        {
            timerBackground.color = timerBackgroundColor;
        }
    }

    private void FindRequiredReferences()
    {
        if (feedbackController == null)
        {
            feedbackController = FindFirstObjectByType<FeedbackTextController>();
        }

        if (dashboardUI == null)
        {
            dashboardUI = FindFirstObjectByType<DashboardUI>();
        }

        if (crosshairController == null)
        {
            crosshairController = FindFirstObjectByType<CrosshairController>();
        }

        if (completionScreen == null)
        {
            completionScreen = FindFirstObjectByType<CompletionScreenController>();
        }

        if (hudCanvas == null)
        {
            hudCanvas = GetComponent<Canvas>();
        }
    }

    private IEnumerator PulseScoreText()
    {
        if (scoreText == null) yield break;

        float elapsed = 0f;
        Vector3 originalScale = scoreTextOriginalScale != Vector3.zero ? scoreTextOriginalScale : scoreText.transform.localScale;

        while (elapsed < scorePulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scorePulseDuration;
            float scale = Mathf.Lerp(scorePulseScale, 1f, EaseOutQuad(t));
            scoreText.transform.localScale = originalScale * scale;
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    private IEnumerator HideObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(objectiveShowDuration);

        if (objectiveText != null)
        {
            // Fade out
            CanvasGroup cg = objectiveText.GetComponent<CanvasGroup>();
            if (cg == null) cg = objectiveText.gameObject.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            float fadeDuration = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }

            objectiveText.gameObject.SetActive(false);
            cg.alpha = 1f; // Reset for next time
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int centiseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{centiseconds:00}";
    }

    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
}
