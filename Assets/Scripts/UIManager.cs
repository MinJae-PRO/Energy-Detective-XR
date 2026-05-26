using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completeText;

    [Header("Feedback Text")]
    [Tooltip("Text for floating feedback (e.g. 'Energy Fixed!')")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 1.5f;

    [Header("Progress Display")]
    [Tooltip("Text showing 'X / Y Fixed' (optional)")]
    public TextMeshProUGUI progressText;

    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    [Header("Score Text Colors")]
    public Color scoreTextColor = new Color(1f, 0.95f, 0.2f, 1f);

    [Header("Timer Text Colors")]
    public Color timerTextColor = new Color(0.3f, 0.9f, 1f, 1f);

    [Header("Progress Text Colors")]
    public Color progressTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private Vector3 feedbackStartPos;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackStartPos = feedbackText.rectTransform.anchoredPosition;
        }

        // Apply outlines to all text elements for readability
        ApplyTextOutline(scoreText);
        ApplyTextOutline(timerText);
        ApplyTextOutline(completeText);
        ApplyTextOutline(feedbackText);
        ApplyTextOutline(progressText);
    }

    /// <summary>
    /// Applies an outline to a TextMeshProUGUI element so text is readable on any background.
    /// </summary>
    void ApplyTextOutline(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.fontMaterial.EnableKeyword("OUTLINE_ON");
        text.outlineColor = textOutlineColor;
        text.outlineWidth = textOutlineWidth;

        // Also add a shadow component as backup for non-material rendering
        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "<b>Score:</b> " + score;
            scoreText.color = scoreTextColor;
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"<b>Time:</b> {minutes:00}:{seconds:00}";
            timerText.color = timerTextColor;
        }
    }

    public void UpdateProgress(int fixedCount, int totalCount)
    {
        if (progressText != null)
        {
            progressText.text = $"<b>Fixed:</b> {fixedCount} / {totalCount}";
            progressText.color = progressTextColor;
        }
    }

    public void ShowCompleteMessage(float finalTime)
    {
        if (completeText != null)
        {
            int minutes = Mathf.FloorToInt(finalTime / 60f);
            int seconds = Mathf.FloorToInt(finalTime % 60f);

            completeText.text = "<size=150%>\u2605 CASE CLOSED! \u2605</size>\n\n" +
                               "<b>Time:</b> " + $"{minutes:00}:{seconds:00}";
            completeText.color = new Color(1f, 0.85f, 0.2f, 1f);
        }
    }

    public void ShowPositiveFeedback(string message)
    {
        ShowFeedback(message, new Color(0.3f, 1f, 0.4f, 1f));
    }

    public void ShowNegativeFeedback(string message)
    {
        ShowFeedback(message, new Color(1f, 0.3f, 0.3f, 1f));
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackText == null) return;

        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(AnimateFeedback(message, color));
    }

    private IEnumerator AnimateFeedback(string message, Color color)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.rectTransform.anchoredPosition = feedbackStartPos;

        // Pop in
        feedbackText.transform.localScale = Vector3.one * 1.3f;
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            feedbackText.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, t / 0.15f);
            yield return null;
        }
        feedbackText.transform.localScale = Vector3.one;

        // Float up and fade
        float elapsed = 0f;
        Vector2 startPos = feedbackStartPos;

        while (elapsed < feedbackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / feedbackDuration;

            feedbackText.rectTransform.anchoredPosition = startPos + Vector2.up * 50f * progress;

            Color faded = color;
            faded.a = Mathf.Lerp(1f, 0f, progress);
            feedbackText.color = faded;

            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
        feedbackText.rectTransform.anchoredPosition = feedbackStartPos;
    }
}
