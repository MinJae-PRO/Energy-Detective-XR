using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Displays floating feedback text (e.g., "Energy Fixed!") at world positions or screen center.
/// Attach to a canvas with a TextMeshProUGUI component, or it will auto-create one.
/// </summary>
public class FeedbackTextController : MonoBehaviour
{
    [Header("Feedback Text Prefab")]
    [Tooltip("Prefab with a TextMeshProUGUI component. If null, feedback text will be created dynamically.")]
    public GameObject feedbackTextPrefab;

    [Header("Feedback Settings")]
    [Tooltip("Default color for positive feedback (e.g., 'Energy Fixed!')")]
    public Color positiveColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Tooltip("Default color for negative feedback (e.g., 'Try Again!')")]
    public Color negativeColor = new Color(1f, 0.25f, 0.25f, 1f);

    [Tooltip("Default color for neutral/info feedback")]
    public Color infoColor = new Color(0.3f, 0.7f, 1f, 1f);

    [Tooltip("Font size for feedback text")]
    public float feedbackFontSize = 36f;

    [Tooltip("How long feedback text remains visible")]
    public float displayDuration = 1.5f;

    [Tooltip("How fast feedback text floats upward")]
    public float floatSpeed = 60f;

    [Tooltip("How far feedback text floats upward before fading")]
    public float floatDistance = 80f;

    [Header("Animation Settings")]
    [Tooltip("Scale animation peak (1 = no scale)")]
    public float maxScale = 1.3f;

    [Tooltip("Duration of the scale-in animation")]
    public float scaleInDuration = 0.2f;

    [Tooltip("Duration of the fade-out animation")]
    public float fadeOutDuration = 0.4f;

    [Header("Screen Position")]
    [Tooltip("If true, shows feedback at screen center. If false, uses world position.")]
    public bool showAtScreenCenter = true;

    [Tooltip("Offset from screen center (in pixels). Only used when showAtScreenCenter is true.")]
    public Vector2 screenCenterOffset = new Vector2(0f, 100f);

    private RectTransform canvasRect;
    private Queue<FeedbackData> feedbackQueue = new Queue<FeedbackData>();
    private bool isProcessingQueue = false;

    private struct FeedbackData
    {
        public string message;
        public Color color;
        public Vector3? worldPosition;
    }

    void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogWarning("[FeedbackTextController] No Canvas found in parent. Screen-center feedback will not work correctly.");
        }
    }

    /// <summary>
    /// Shows positive feedback (e.g., "Energy Fixed!").
    /// </summary>
    public void ShowPositiveFeedback(string message, Vector3? worldPos = null)
    {
        EnqueueFeedback(message, positiveColor, worldPos);
    }

    /// <summary>
    /// Shows negative feedback (e.g., "Try Again!").
    /// </summary>
    public void ShowNegativeFeedback(string message, Vector3? worldPos = null)
    {
        EnqueueFeedback(message, negativeColor, worldPos);
    }

    /// <summary>
    /// Shows neutral/info feedback.
    /// </summary>
    public void ShowInfoFeedback(string message, Vector3? worldPos = null)
    {
        EnqueueFeedback(message, infoColor, worldPos);
    }

    /// <summary>
    /// Shows feedback with a specific custom color.
    /// </summary>
    public void ShowFeedback(string message, Color color, Vector3? worldPos = null)
    {
        EnqueueFeedback(message, color, worldPos);
    }

    private void EnqueueFeedback(string message, Color color, Vector3? worldPos)
    {
        feedbackQueue.Enqueue(new FeedbackData
        {
            message = message,
            color = color,
            worldPosition = worldPos
        });

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessFeedbackQueue());
        }
    }

    private IEnumerator ProcessFeedbackQueue()
    {
        isProcessingQueue = true;

        while (feedbackQueue.Count > 0)
        {
            FeedbackData data = feedbackQueue.Dequeue();
            yield return StartCoroutine(AnimateFeedback(data));
            yield return new WaitForSeconds(0.15f);
        }

        isProcessingQueue = false;
    }

    private IEnumerator AnimateFeedback(FeedbackData data)
    {
        GameObject feedbackObj = CreateFeedbackObject(data.message, data.color);
        if (feedbackObj == null) yield break;

        RectTransform rectTransform = feedbackObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = feedbackObj.GetComponent<TextMeshProUGUI>();
        CanvasGroup canvasGroup = feedbackObj.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = feedbackObj.AddComponent<CanvasGroup>();
        }

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = startPos + new Vector2(0f, floatDistance);

        float elapsed = 0f;
        float totalDuration = displayDuration;

        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / totalDuration;

            // Move upward
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, EaseOutQuad(t));

            // Scale animation (pop in, then settle)
            if (elapsed < scaleInDuration)
            {
                float scaleT = elapsed / scaleInDuration;
                float scale = Mathf.Lerp(0.5f, maxScale, EaseOutBack(scaleT));
                rectTransform.localScale = Vector3.one * scale;
            }
            else if (elapsed < scaleInDuration + 0.1f)
            {
                float scaleT = (elapsed - scaleInDuration) / 0.1f;
                float scale = Mathf.Lerp(maxScale, 1f, EaseInOutQuad(scaleT));
                rectTransform.localScale = Vector3.one * scale;
            }
            else
            {
                rectTransform.localScale = Vector3.one;
            }

            // Fade out at the end
            if (t > 1f - (fadeOutDuration / totalDuration))
            {
                float fadeT = (t - (1f - (fadeOutDuration / totalDuration))) / (fadeOutDuration / totalDuration);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeT);
            }
            else
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, Mathf.Min(1f, elapsed / 0.1f));
            }

            yield return null;
        }

        Destroy(feedbackObj);
    }

    private GameObject CreateFeedbackObject(string message, Color color)
    {
        GameObject feedbackObj;

        if (feedbackTextPrefab != null)
        {
            feedbackObj = Instantiate(feedbackTextPrefab, transform);
        }
        else
        {
            feedbackObj = new GameObject("FeedbackText", typeof(RectTransform));
            feedbackObj.transform.SetParent(transform, false);

            TextMeshProUGUI tmp = feedbackObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = feedbackFontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;
        }

        TextMeshProUGUI textComponent = feedbackObj.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
            textComponent.fontSize = feedbackFontSize;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.raycastTarget = false;
        }

        RectTransform rectTransform = feedbackObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(600f, 80f);

        if (showAtScreenCenter && canvasRect != null)
        {
            // Position at screen center with offset
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f) + screenCenterOffset;
            rectTransform.position = screenCenter;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
        else
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        CanvasGroup canvasGroup = feedbackObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = feedbackObj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;

        return feedbackObj;
    }

    // Easing functions for smooth animation
    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private float EaseInOutQuad(float t) => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
