using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completeText;

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateTimer(float time)
    {
        timerText.text = "Time: " + time.ToString("F1") + "s";
    }

    public void ShowCompleteMessage(float finalTime)
    {
        if (completeText != null)
        {
            completeText.text = "All Fixed!\nTime: " + finalTime.ToString("F1") + "s";
        }
    }
}