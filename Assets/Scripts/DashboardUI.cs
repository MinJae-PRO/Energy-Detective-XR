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

    private bool isVisible = false;

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
