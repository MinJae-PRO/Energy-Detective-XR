using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Comprehensive game dashboard that tracks and displays all player progress.
/// Attach to Canvas GameObject and assign the UI references in the Inspector.
/// </summary>
public class DashboardUI : MonoBehaviour
{
    [Header("Dashboard Panel")]
    [Tooltip("The main dashboard panel GameObject. Will auto-show/hide with Tab key if assignableToggleKey is set.")]
    public GameObject dashboardPanel;

    [Tooltip("Key to toggle dashboard visibility. Set to KeyCode.None to disable toggling.")]
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Score Display")]
    public TextMeshProUGUI scoreValueText;
    public TextMeshProUGUI scoreLabelText;
    public Image scoreIcon;

    [Header("Timer Display")]
    public TextMeshProUGUI timerValueText;
    public TextMeshProUGUI timerLabelText;

    [Header("Progress Display")]
    public TextMeshProUGUI progressValueText;
    public TextMeshProUGUI progressLabelText;
    public Slider progressBarSlider;
    public Image progressBarFillImage;

    [Header("Objects Remaining")]
    public TextMeshProUGUI remainingValueText;
    public TextMeshProUGUI remainingLabelText;

    [Header("Current Object Info")]
    public TextMeshProUGUI currentObjectNameText;
    public TextMeshProUGUI currentObjectStatusText;

    [Header("Visual Settings")]
    [Tooltip("Color when all objects are fixed")]
    public Color completionColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Tooltip("Color when objects remain")]
    public Color inProgressColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Tooltip("Color for timer text")]
    public Color timerColor = new Color(0.3f, 0.9f, 1f, 1f);

    [Tooltip("Color for score text")]
    public Color scoreColor = new Color(1f, 0.9f, 0.2f, 1f);

    [Header("Animation Settings")]
    [Tooltip("Should score changes animate with a punch effect?")]
    public bool animateScoreChanges = true;

    [Tooltip("Duration of score punch animation")]
    public float scorePunchDuration = 0.3f;

    [Tooltip("Scale multiplier for score punch")]
    public float scorePunchScale = 1.2f;

    [Header("Mini Dashboard (Always Visible)")]
    [Tooltip("A compact always-visible panel showing key stats")]
    public GameObject miniDashboardPanel;

    public TextMeshProUGUI miniScoreText;
    public TextMeshProUGUI miniTimerText;
    public TextMeshProUGUI miniProgressText;

    // Internal state
    private int currentScore = 0;
    private float currentTime = 0f;
    private int fixedCount = 0;
    private int totalObjectCount = 0;
    private int remainingCount = 0;
    private string currentObjectName = "";
    private bool isDashboardVisible = false;
    private Vector3 scoreOriginalScale;
    private Coroutine scorePunchCoroutine;

    void Start()
    {
        if (dashboardPanel != null)
        {
            dashboardPanel.SetActive(false);
            isDashboardVisible = false;
        }

        if (miniDashboardPanel != null)
        {
            miniDashboardPanel.SetActive(true);
        }

        if (scoreValueText != null)
        {
            scoreOriginalScale = scoreValueText.transform.localScale;
            scoreValueText.color = scoreColor;
        }

        if (timerValueText != null)
        {
            timerValueText.color = timerColor;
        }

        ApplyLabelDefaults();
        UpdateAllDisplays();
    }

    void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
        {
            ToggleDashboard();
        }
    }

    /// <summary>
    /// Toggles the full dashboard panel visibility.
    /// </summary>
    public void ToggleDashboard()
    {
        if (dashboardPanel == null) return;

        isDashboardVisible = !isDashboardVisible;
        dashboardPanel.SetActive(isDashboardVisible);

        if (isDashboardVisible)
        {
            RefreshDashboard();
        }
    }

    /// <summary>
    /// Shows the dashboard panel.
    /// </summary>
    public void ShowDashboard()
    {
        if (dashboardPanel == null) return;
        dashboardPanel.SetActive(true);
        isDashboardVisible = true;
        RefreshDashboard();
    }

    /// <summary>
    /// Hides the dashboard panel.
    /// </summary>
    public void HideDashboard()
    {
        if (dashboardPanel == null) return;
        dashboardPanel.SetActive(false);
        isDashboardVisible = false;
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int newScore)
    {
        int scoreDelta = newScore - currentScore;
        currentScore = newScore;

        if (scoreValueText != null)
        {
            scoreValueText.text = currentScore.ToString("N0");
        }

        if (miniScoreText != null)
        {
            miniScoreText.text = $"Score: {currentScore:N0}";
        }

        if (animateScoreChanges && scoreDelta > 0)
        {
            if (scorePunchCoroutine != null)
            {
                StopCoroutine(scorePunchCoroutine);
            }
            scorePunchCoroutine = StartCoroutine(PunchScoreAnimation());
        }
    }

    /// <summary>
    /// Updates the timer display.
    /// </summary>
    public void UpdateTimer(float timeInSeconds)
    {
        currentTime = timeInSeconds;

        string formattedTime = FormatTime(timeInSeconds);

        if (timerValueText != null)
        {
            timerValueText.text = formattedTime;
        }

        if (miniTimerText != null)
        {
            miniTimerText.text = formattedTime;
        }
    }

    /// <summary>
    /// Updates the progress tracking displays.
    /// </summary>
    public void UpdateProgress(int fixedAmount, int totalAmount)
    {
        fixedCount = fixedAmount;
        totalObjectCount = totalAmount;
        remainingCount = totalAmount - fixedAmount;

        float progressPercent = totalAmount > 0 ? (float)fixedAmount / totalAmount : 0f;

        if (progressValueText != null)
        {
            progressValueText.text = $"{fixedAmount} / {totalAmount}";
        }

        if (progressBarSlider != null)
        {
            progressBarSlider.value = progressPercent;
        }

        if (remainingValueText != null)
        {
            remainingValueText.text = remainingCount.ToString();
        }

        if (miniProgressText != null)
        {
            miniProgressText.text = $"{fixedAmount}/{totalAmount}";
        }

        // Update progress bar color based on completion
        if (progressBarFillImage != null)
        {
            progressBarFillImage.color = fixedAmount >= totalAmount ? completionColor : inProgressColor;
        }
    }

    /// <summary>
    /// Updates the currently targeted object info.
    /// </summary>
    public void UpdateCurrentObject(string objectName, bool isFixed)
    {
        currentObjectName = objectName;

        if (currentObjectNameText != null)
        {
            currentObjectNameText.text = string.IsNullOrEmpty(objectName) ? "None" : objectName;
        }

        if (currentObjectStatusText != null)
        {
            if (string.IsNullOrEmpty(objectName))
            {
                currentObjectStatusText.text = "Look at an object";
                currentObjectStatusText.color = Color.gray;
            }
            else if (isFixed)
            {
                currentObjectStatusText.text = "Fixed";
                currentObjectStatusText.color = completionColor;
            }
            else
            {
                currentObjectStatusText.text = "Needs Repair";
                currentObjectStatusText.color = new Color(1f, 0.5f, 0.2f, 1f);
            }
        }
    }

    /// <summary>
    /// Clears the current object info display.
    /// </summary>
    public void ClearCurrentObject()
    {
        UpdateCurrentObject("", false);
    }

    /// <summary>
    /// Refreshes all dashboard values. Call this when opening the dashboard.
    /// </summary>
    public void RefreshDashboard()
    {
        UpdateAllDisplays();
    }

    /// <summary>
    /// Gets the current stats as a struct for the completion screen.
    /// </summary>
    public GameStats GetCurrentStats()
    {
        return new GameStats
        {
            finalScore = currentScore,
            finalTime = currentTime,
            fixedObjects = fixedCount,
            totalObjects = totalObjectCount
        };
    }

    private void UpdateAllDisplays()
    {
        UpdateScore(currentScore);
        UpdateTimer(currentTime);
        UpdateProgress(fixedCount, totalObjectCount);
    }

    private void ApplyLabelDefaults()
    {
        if (scoreLabelText != null && string.IsNullOrEmpty(scoreLabelText.text))
        {
            scoreLabelText.text = "SCORE";
        }

        if (timerLabelText != null && string.IsNullOrEmpty(timerLabelText.text))
        {
            timerLabelText.text = "TIME";
        }

        if (progressLabelText != null && string.IsNullOrEmpty(progressLabelText.text))
        {
            progressLabelText.text = "PROGRESS";
        }

        if (remainingLabelText != null && string.IsNullOrEmpty(remainingLabelText.text))
        {
            remainingLabelText.text = "REMAINING";
        }
    }

    private IEnumerator PunchScoreAnimation()
    {
        if (scoreValueText == null) yield break;

        float elapsed = 0f;
        while (elapsed < scorePunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scorePunchDuration;
            float scale = Mathf.Lerp(scorePunchScale, 1f, EaseOutElastic(t));
            scoreValueText.transform.localScale = scoreOriginalScale * scale;
            yield return null;
        }

        scoreValueText.transform.localScale = scoreOriginalScale;
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }

    private float EaseOutElastic(float t)
    {
        const float c4 = (2f * Mathf.PI) / 3f;
        return t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    /// <summary>
    /// Container for end-game statistics.
    /// </summary>
    public struct GameStats
    {
        public int finalScore;
        public float finalTime;
        public int fixedObjects;
        public int totalObjects;

        public int RemainingObjects => totalObjects - fixedObjects;
        public float CompletionPercent => totalObjects > 0 ? (float)fixedObjects / totalObjects * 100f : 0f;
        public int StarRating
        {
            get
            {
                if (fixedObjects < totalObjects) return 0;
                if (finalTime < 60f) return 3;
                if (finalTime < 120f) return 2;
                return 1;
            }
        }
    }
}
