using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Enhanced completion screen with stats display, star rating, and action buttons.
/// 
/// </summary>
public class CompletionScreenController : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("The main completion panel GameObject")]
    public GameObject completionPanel;

    [Tooltip("Background overlay image (darkens the game)")]
    public Image backgroundOverlay;

    [Header("Title")]
    public TextMeshProUGUI titleText;

    [Header("Star Rating")]
    [Tooltip("Container for star images")]
    public Transform starContainer;

    [Tooltip("Star image prefab (should have an Image component)")]
    public Image starPrefab;

    [Tooltip("Sprite for an earned star")]
    public Sprite starEarnedSprite;

    [Tooltip("Sprite for an unearned star")]
    public Sprite starEmptySprite;

    [Tooltip("Color for earned stars")]
    public Color starEarnedColor = new Color(1f, 0.85f, 0.1f, 1f);

    [Tooltip("Color for empty stars")]
    public Color starEmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    [Tooltip("Delay between each star appearing")]
    public float starRevealDelay = 0.4f;

    [Tooltip("Scale animation for star pop-in")]
    public float starPopScale = 1.5f;

    [Header("Stats Display")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI objectsFixedText;
    public TextMeshProUGUI accuracyText;
    public TextMeshProUGUI ratingText;

    [Header("Stat Labels")]
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI objectsLabel;
    public TextMeshProUGUI accuracyLabel;

    [Header("Feedback Messages")]
    [Tooltip("Message shown for 3-star rating")]
    public string message3Star = "Outstanding Detective Work!";

    [Tooltip("Message shown for 2-star rating")]
    public string message2Star = "Great Job!";

    [Tooltip("Message shown for 1-star rating")]
    public string message1Star = "Case Closed!";

    [Tooltip("Message shown for 0-star (incomplete)")]
    public string message0Star = "Investigation Incomplete";

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Animation Settings")]
    [Tooltip("Duration of panel fade-in")]
    public float fadeInDuration = 0.5f;

    [Tooltip("Delay before stats start appearing")]
    public float statsStartDelay = 0.8f;

    [Tooltip("Delay between each stat appearing")]
    public float statRevealDelay = 0.2f;

    [Tooltip("Duration of each stat count-up animation")]
    public float countUpDuration = 0.8f;

    [Header("Colors")]
    public Color titleColor = new Color(0.2f, 1f, 0.5f, 1f);
    public Color scoreColor = new Color(1f, 0.9f, 0.2f, 1f);
    public Color timeColor = new Color(0.3f, 0.9f, 1f, 1f);
    public Color objectsColor = new Color(0.5f, 1f, 0.5f, 1f);

    // Runtime state
    private Image[] starImages;
    private CanvasGroup panelCanvasGroup;
    private bool isShowing = false;

    void Start()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
            panelCanvasGroup = completionPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = completionPanel.AddComponent<CanvasGroup>();
            }
            panelCanvasGroup.alpha = 0f;
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        SetupButtons();
        ApplyLabelDefaults();
    }

    /// <summary>
    /// Shows the completion screen with the given stats.
    /// </summary>
    public void ShowCompletionScreen(int finalScore, float finalTime, int fixedObjects, int totalObjects, int wrongAttempts = 0)
    {
        if (isShowing) return;
        isShowing = true;

        int starCount = CalculateStarRating(finalTime, fixedObjects, totalObjects);
        string message = GetMessageForStars(starCount);
        float accuracy = CalculateAccuracy(fixedObjects, totalObjects, wrongAttempts);

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = message;
            titleText.color = titleColor;
        }

        StartCoroutine(AnimateCompletionScreen(finalScore, finalTime, fixedObjects, totalObjects, starCount, accuracy));
    }

    /// <summary>
    /// Hides the completion screen.
    /// </summary>
    public void HideCompletionScreen()
    {
        isShowing = false;

        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        if (backgroundOverlay != null)
        {
            backgroundOverlay.gameObject.SetActive(false);
        }

        // Clean up stars
        if (starContainer != null)
        {
            foreach (Transform child in starContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Restarts the current scene.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Quits the game.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void ApplyLabelDefaults()
    {
        if (scoreLabel != null && string.IsNullOrEmpty(scoreLabel.text))
            scoreLabel.text = "FINAL SCORE";

        if (timeLabel != null && string.IsNullOrEmpty(timeLabel.text))
            timeLabel.text = "TIME ELAPSED";

        if (objectsLabel != null && string.IsNullOrEmpty(objectsLabel.text))
            objectsLabel.text = "OBJECTS FIXED";

        if (accuracyLabel != null && string.IsNullOrEmpty(accuracyLabel.text))
            accuracyLabel.text = "ACCURACY";
    }

    private IEnumerator AnimateCompletionScreen(int finalScore, float finalTime, int fixedObjects, int totalObjects, int starCount, float accuracy)
    {
        // Fade in background
        if (panelCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeInDuration;
                panelCanvasGroup.alpha = t;
                if (backgroundOverlay != null)
                {
                    backgroundOverlay.color = new Color(0f, 0f, 0f, t * 0.7f);
                }
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSecondsRealtime(statsStartDelay);

        // Animate score counting up
        if (scoreText != null)
        {
            scoreText.color = scoreColor;
            yield return StartCoroutine(CountUpText(scoreText, 0, finalScore, $"{{0:N0}} pts", countUpDuration));
        }

        yield return new WaitForSecondsRealtime(statRevealDelay);

        // Animate time
        if (timeText != null)
        {
            timeText.color = timeColor;
            string formattedTime = FormatTime(finalTime);
            timeText.text = formattedTime;
            yield return StartCoroutine(PopInText(timeText));
        }

        yield return new WaitForSecondsRealtime(statRevealDelay);

        // Animate objects fixed
        if (objectsFixedText != null)
        {
            objectsFixedText.color = objectsColor;
            yield return StartCoroutine(CountUpText(objectsFixedText, 0, fixedObjects, $"{{0}} / {totalObjects}", countUpDuration * 0.5f));
        }

        yield return new WaitForSecondsRealtime(statRevealDelay);

        // Animate accuracy
        if (accuracyText != null)
        {
            accuracyText.text = $"{accuracy:F0}%";
            yield return StartCoroutine(PopInText(accuracyText));
        }

        yield return new WaitForSecondsRealtime(0.3f);

        // Show star rating
        yield return StartCoroutine(RevealStars(starCount));

        // Update rating text
        if (ratingText != null)
        {
            ratingText.text = $"{starCount} Star{(starCount != 1 ? "s" : "")}";
            ratingText.color = GetStarColor(starCount);
            yield return StartCoroutine(PopInText(ratingText));
        }

        // Enable buttons after animation
        EnableButtons(true);
    }

    private IEnumerator RevealStars(int starCount)
    {
        if (starContainer == null || starPrefab == null) yield break;

        // Create stars
        int maxStars = 3;
        for (int i = 0; i < maxStars; i++)
        {
            Image star = Instantiate(starPrefab, starContainer);
            star.sprite = starEmptySprite;
            star.color = starEmptyColor;

            star.transform.localScale = Vector3.zero;

            yield return new WaitForSecondsRealtime(starRevealDelay);

            // Pop in animation
            if (i < starCount)
            {
                star.sprite = starEarnedSprite;
                star.color = starEarnedColor;
            }

            yield return StartCoroutine(PopInTransform(star.transform, starPopScale));
        }
    }

    private IEnumerator CountUpText(TextMeshProUGUI textComponent, int startValue, int endValue, string format, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, EaseOutQuad(t)));
            textComponent.text = string.Format(format, currentValue);
            yield return null;
        }
        textComponent.text = string.Format(format, endValue);
        yield return StartCoroutine(PopInText(textComponent));
    }

    private IEnumerator PopInText(TextMeshProUGUI textComponent)
    {
        if (textComponent == null) yield break;

        Vector3 originalScale = textComponent.transform.localScale;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(1.5f, 1f, EaseOutBack(t));
            textComponent.transform.localScale = originalScale * scale;
            yield return null;
        }

        textComponent.transform.localScale = originalScale;
    }

    private IEnumerator PopInTransform(Transform target, float peakScale)
    {
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0f, 1f, EaseOutBack(t));
            target.localScale = Vector3.one * scale;
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private int CalculateStarRating(float finalTime, int fixedObjects, int totalObjects)
    {
        if (fixedObjects < totalObjects) return 0;
        if (finalTime < 90f) return 3;
        if (finalTime < 180f) return 2;
        return 1;
    }

    private string GetMessageForStars(int stars)
    {
        return stars switch
        {
            3 => message3Star,
            2 => message2Star,
            1 => message1Star,
            _ => message0Star
        };
    }

    private float CalculateAccuracy(int fixedObjects, int totalObjects, int wrongAttempts)
    {
        int totalAttempts = fixedObjects + wrongAttempts;
        if (totalAttempts == 0) return 100f;
        return Mathf.Round((float)fixedObjects / totalAttempts * 100f);
    }

    private Color GetStarColor(int stars)
    {
        return stars switch
        {
            3 => new Color(1f, 0.85f, 0.1f, 1f),
            2 => new Color(0.8f, 0.8f, 0.8f, 1f),
            1 => new Color(0.8f, 0.5f, 0.2f, 1f),
            _ => Color.gray
        };
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }

    private void EnableButtons(bool enabled)
    {
        if (restartButton != null) restartButton.interactable = enabled;
        if (mainMenuButton != null) mainMenuButton.interactable = enabled;
        if (quitButton != null) quitButton.interactable = enabled;
    }

    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
