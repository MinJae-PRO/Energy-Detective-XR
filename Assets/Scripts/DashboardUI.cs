using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DashboardUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject dashboardPanel;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Stats")]
    public TextMeshProUGUI scoreValueText;
    public TextMeshProUGUI timerValueText;
    public TextMeshProUGUI progressText;
    public Slider progressSlider;

    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    [Header("Score Text Color")]
    public Color scoreTextColor = new Color(1f, 0.95f, 0.2f, 1f);

    [Header("Timer Text Color")]
    public Color timerTextColor = new Color(0.3f, 0.9f, 1f, 1f);

    [Header("Progress Text Color")]
    public Color progressTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private bool isVisible = false;

    void Start()
    {
        // Apply outlines and initial colors to all text elements
        StyleTextElement(scoreValueText, scoreTextColor);
        StyleTextElement(timerValueText, timerTextColor);
        StyleTextElement(progressText, progressTextColor);
    }

    /// <summary>
    /// Applies outline and initial color to a TextMeshProUGUI element.
    /// </summary>
    void StyleTextElement(TextMeshProUGUI text, Color color)
    {
        if (text == null) return;

        text.color = color;

        text.fontMaterial.EnableKeyword("OUTLINE_ON");
        text.outlineColor = textOutlineColor;
        text.outlineWidth = textOutlineWidth;

        // Add shadow component for extra readability
        Shadow shadow = text.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
        shadow.effectDistance = new Vector2(2f, -2f);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDashboard();
        }
    }

    public void ToggleDashboard()
    {
        if (dashboardPanel == null) return;
        isVisible = !isVisible;
        dashboardPanel.SetActive(isVisible);
    }

    public void UpdateScore(int score)
    {
        if (scoreValueText != null)
            scoreValueText.text = score.ToString();
    }

    public void UpdateTimer(float time)
    {
        if (timerValueText != null)
        {
            int m = Mathf.FloorToInt(time / 60f);
            int s = Mathf.FloorToInt(time % 60f);
            timerValueText.text = $"{m:00}:{s:00}";
        }
    }

    public void UpdateProgress(int fixedCount, int totalCount)
    {
        if (progressText != null)
            progressText.text = $"{fixedCount} / {totalCount}";

        if (progressSlider != null)
            progressSlider.value = totalCount > 0 ? (float)fixedCount / totalCount : 0f;
    }
}
