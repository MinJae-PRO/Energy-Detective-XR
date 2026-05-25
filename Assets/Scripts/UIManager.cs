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

    private Vector3 feedbackStartPos;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        if (feedbackText != null)
        {
            feedbackText.gameObject.SetActive(false);
            feedbackStartPos = feedbackText.rectTransform.anchoredPosition;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "<b>Score:</b> " + score;
            scoreText.color = new Color(1f, 0.95f, 0.2f, 1f);
        }
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            timerText.text = $"<b>Time:</b> {minutes:00}:{seconds:00}";
            timerText.color = new Color(0.3f, 0.9f, 1f, 1f);
        }
    }

    public void UpdateProgress(int fixedCount, int totalCount)
    {
        if (progressText != null)
        {
            progressText.text = $"<b>Fixed:</b> {fixedCount} / {totalCount}";
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
