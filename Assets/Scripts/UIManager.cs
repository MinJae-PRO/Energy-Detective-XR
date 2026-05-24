using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;  // SceneManager

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completeText;
    
    // Dashboard elements
    public GameObject dashboardPanel;
    public TextMeshProUGUI missionStatusText;
    public TextMeshProUGUI educationalMessageText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI remainingObjectsText;
    
    // Mission incomplete popup
    public GameObject missionIncompletePanel;
    public TextMeshProUGUI incompleteMessageText;
    
    // Feedback text for fixing objects
    public GameObject feedbackTextPrefab;
    public Transform feedbackParent;
    public Color successFeedbackColor = Color.green;
    public Color warningFeedbackColor = Color.yellow;
    
    // Improved completion screen
    public GameObject improvedCompletionPanel;
    public TextMeshProUGUI completionScoreText;
    public TextMeshProUGUI completionTimeText;
    public TextMeshProUGUI completionGradeText;
    public Button restartButton;
    public Button menuButton;
    
    // Crosshair design improvements
    public GameObject crosshairObject;
    public Image crosshairImage;
    public Sprite defaultCrosshairSprite;
    public Sprite hoverCrosshairSprite;
    public Color defaultCrosshairColor = Color.white;
    public Color hoverCrosshairColor = Color.green;
    public float crosshairPulseSpeed = 2f;
    
    // Educational messages pool
    private string[] educationalMessages = new string[]
    {
        "💡 Did you know? Electronics use power even when 'off' (phantom load)!",
        "🔌 Unplug phone chargers - they draw power even without a phone!",
        "💡 LED bulbs use 75% less energy than traditional bulbs!",
        "📺 A TV left on overnight wastes enough power for 100 phone charges!",
        "❄️ Keep fridge at 3-4°C for optimal efficiency!",
        "🖥️ Enable power save mode on computers - saves $$$!",
        "🔋 Smart power strips cut power to idle devices automatically!",
        "🌡️ Lower thermostat by 1°C saves 8% on heating bills!",
        "🧺 Wash clothes in cold water - 90% less energy usage!",
        "💨 Clean dryer lint filter - improves efficiency by 30%!"
    };
    
    private Coroutine messageRotator;
    private bool isGameRunning = true;
    private float crosshairPulseTimer = 0f;
    private bool crosshairPulsing = false;
    private GameManager cachedGameManager;

    void Start()
    {
        // Use FindFirstObjectByType instead of obsolete FindObjectOfType
        cachedGameManager = FindFirstObjectByType<GameManager>();
        
        if (dashboardPanel != null)
            dashboardPanel.SetActive(true);
        
        if (missionIncompletePanel != null)
            missionIncompletePanel.SetActive(false);
        
        if (improvedCompletionPanel != null)
            improvedCompletionPanel.SetActive(false);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMainMenu);
        
        StartEducationalMessageRotation();
        DisplayImprovedInstructions();
        SetupCrosshair();
        UpdateMissionStatus("IN PROGRESS");
        ShowFloatingFeedback("Mission Started! Find and fix energy waste!", warningFeedbackColor);
    }

    void Update()
    {
        if (crosshairPulsing && crosshairImage != null)
        {
            crosshairPulseTimer += Time.deltaTime * crosshairPulseSpeed;
            float scale = 1f + Mathf.Sin(crosshairPulseTimer) * 0.15f;
            crosshairImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }
        else if (crosshairImage != null && !crosshairPulsing)
        {
            crosshairImage.rectTransform.localScale = Vector3.one;
        }
    }

    void SetupCrosshair()
    {
        if (crosshairImage != null)
        {
            if (defaultCrosshairSprite != null)
                crosshairImage.sprite = defaultCrosshairSprite;
            crosshairImage.color = defaultCrosshairColor;
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "🔋 SCORE: " + score;
        
        if (scoreText != null)
            StartCoroutine(PulseText(scoreText, Color.green));
        
        if (remainingObjectsText != null && cachedGameManager != null)
        {
            int remaining = cachedGameManager.GetRemainingObjects();
            remainingObjectsText.text = $"📦 Remaining: {remaining}";
        }
    }

    public void UpdateTimer(float time, bool isTimeAttackMode = true)
    {
        if (timerText == null) return;
        
        if (isTimeAttackMode)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"⏱️ TIME: {minutes:00}:{seconds:00}";
            
            if (time <= 10f)
            {
                timerText.color = Color.red;
                StartCoroutine(PulseText(timerText, Color.red));
            }
            else if (time <= 30f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
        else
        {
            timerText.text = "🕐 TIME: " + time.ToString("F1") + "s";
        }
    }

    public void ShowFloatingFeedback(string message, Color color)
    {
        if (feedbackTextPrefab == null) return;
        
        Transform parent = feedbackParent != null ? feedbackParent : (dashboardPanel != null ? dashboardPanel.transform : transform);
        GameObject feedback = Instantiate(feedbackTextPrefab, parent);
        TextMeshProUGUI textComponent = feedback.GetComponent<TextMeshProUGUI>();
        
        if (textComponent != null)
        {
            textComponent.text = message;
            textComponent.color = color;
        }
        
        Destroy(feedback, 2f);
    }

    public void ShowFixFeedback(string objectName, int points)
    {
        string feedbackMessage = $"✅ {objectName} FIXED! +{points} points!";
        ShowFloatingFeedback(feedbackMessage, successFeedbackColor);
        StartCoroutine(ShowTemporaryEducationalTip($"Great! You saved energy fixing the {objectName}!", 2.5f));
    }

    public void ShowImprovedCompletionScreen(float finalTime, int finalScore, int totalObjects)
    {
        isGameRunning = false;
        
        if (improvedCompletionPanel != null)
        {
            improvedCompletionPanel.SetActive(true);
            string grade = CalculateGrade(finalTime, finalScore, totalObjects);
            
            if (completionScoreText != null)
                completionScoreText.text = $"🏆 FINAL SCORE: {finalScore}";
            
            if (completionTimeText != null)
                completionTimeText.text = $"⏱️ TIME: {finalTime:F1} seconds";
            
            if (completionGradeText != null)
            {
                completionGradeText.text = $"📊 EFFICIENCY GRADE: {grade}";
                
                if (grade.StartsWith("S") || grade.StartsWith("A"))
                    completionGradeText.color = Color.green;
                else if (grade.StartsWith("B") || grade.StartsWith("C"))
                    completionGradeText.color = Color.yellow;
                else
                    completionGradeText.color = Color.red;
            }
        }
        
        if (dashboardPanel != null)
            dashboardPanel.SetActive(false);
        
        if (crosshairObject != null)
            crosshairObject.SetActive(false);
        
        UpdateMissionStatus("COMPLETE ✓");
        
        if (messageRotator != null)
            StopCoroutine(messageRotator);
    }

    string CalculateGrade(float time, int score, int totalObjects)
    {
        float timeScore = Mathf.Clamp01(60f / time);
        float accuracyScore = (float)score / (totalObjects * 10f);
        float totalScore = (timeScore + accuracyScore) / 2f;
        
        if (totalScore >= 0.9f) return "S - ENERGY HERO! ⭐⭐⭐";
        if (totalScore >= 0.75f) return "A - ENERGY STAR! ⭐⭐";
        if (totalScore >= 0.6f) return "B - GOOD DETECTIVE! ⭐";
        if (totalScore >= 0.45f) return "C - KEEP LEARNING!";
        return "D - NEEDS IMPROVEMENT";
    }

    public void ShowCompleteMessage(float finalTime)
    {
        if (completeText != null)
        {
            completeText.text = "🎉 Mission Complete! 🎉\nTime: " + finalTime.ToString("F1") + "s";
        }
        UpdateMissionStatus("COMPLETE ✓");
    }
    
    public void ShowMissionIncomplete()
    {
        isGameRunning = false;
        
        if (missionIncompletePanel != null)
        {
            missionIncompletePanel.SetActive(true);
            if (incompleteMessageText != null)
            {
                incompleteMessageText.text = "❌ MISSION INCOMPLETE! ❌\n\n" +
                                            "You ran out of time!\n" +
                                            "Not all energy waste was fixed.\n\n" +
                                            "💡 TIPS FOR NEXT TIME:\n" +
                                            "• Move faster between objects\n" +
                                            "• Prioritize high-point items first\n" +
                                            "• Look around more efficiently\n\n" +
                                            "Press R to restart mission";
            }
        }
        
        if (dashboardPanel != null)
            dashboardPanel.SetActive(false);
        
        UpdateMissionStatus("FAILED - Time's Up!");
    }
    
    public void UpdateMissionStatus(string status)
    {
        if (missionStatusText != null)
            missionStatusText.text = $"📋 MISSION STATUS: {status}";
    }
    
    public void UpdateRemainingObjects(int remaining, int total)
    {
        if (remainingObjectsText != null)
            remainingObjectsText.text = $"📦 REMAINING: {remaining}/{total}";
    }

    public void SetCrosshairHover(bool isHovering)
    {
        crosshairPulsing = isHovering;
        
        if (crosshairImage != null)
        {
            if (isHovering)
            {
                crosshairImage.color = hoverCrosshairColor;
                if (hoverCrosshairSprite != null)
                    crosshairImage.sprite = hoverCrosshairSprite;
                crosshairPulseTimer = 0f;
            }
            else
            {
                crosshairImage.color = defaultCrosshairColor;
                if (defaultCrosshairSprite != null)
                    crosshairImage.sprite = defaultCrosshairSprite;
            }
        }
    }

    private void DisplayImprovedInstructions()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "═══════════════════════════════════\n" +
                                   "     🎯 ENERGY DETECTIVE - MISSION BRIEFING\n" +
                                   "═══════════════════════════════════\n\n" +
                                   "🎮 YOUR GOAL:\n" +
                                   "   Find and fix ALL energy-wasting objects\n" +
                                   "   before time runs out!\n\n" +
                                   "🖱️ HOW TO PLAY:\n" +
                                   "   1. LOOK around using your mouse\n" +
                                   "   2. AIM the crosshair at any glowing object\n" +
                                   "   3. CLICK Left Mouse Button to fix it\n" +
                                   "   4. Each fix gives you points and a tip!\n\n" +
                                   "⏱️ TIME ATTACK:\n" +
                                   "   • You have 60 seconds to complete the mission\n" +
                                   "   • Watch the timer - it turns red when low!\n" +
                                   "   • Press R to restart if needed\n\n" +
                                   "🏆 SCORING:\n" +
                                   "   • Each fixed object = 10 points\n" +
                                   "   • Complete all objects to win!\n" +
                                   "   • Faster completion = better grade!\n\n" +
                                   "💡 PRO TIPS:\n" +
                                   "   • Green crosshair = fixable object!\n" +
                                   "   • Read the energy tips to learn!\n" +
                                   "   • Save energy in real life too!\n\n" +
                                   "═══════════════════════════════════\n" +
                                   "         GOOD LUCK, DETECTIVE!\n" +
                                   "═══════════════════════════════════";
        }
    }
    
    private void StartEducationalMessageRotation()
    {
        if (messageRotator != null)
            StopCoroutine(messageRotator);
        
        messageRotator = StartCoroutine(RotateEducationalMessages());
    }
    
    private IEnumerator RotateEducationalMessages()
    {
        while (isGameRunning)
        {
            if (educationalMessageText != null && educationalMessages.Length > 0)
            {
                int randomIndex = Random.Range(0, educationalMessages.Length);
                educationalMessageText.text = "💡 DID YOU KNOW?\n" + educationalMessages[randomIndex];
            }
            yield return new WaitForSeconds(12f);
        }
    }
    
    public void ShowPopupEducationalTip(string tip)
    {
        StartCoroutine(ShowTemporaryEducationalTip(tip, 3f));
    }
    
    private IEnumerator ShowTemporaryEducationalTip(string message, float duration)
    {
        if (educationalMessageText == null) yield break;
        
        string originalMessage = educationalMessageText.text;
        educationalMessageText.text = "💡 " + message;
        yield return new WaitForSeconds(duration);
        educationalMessageText.text = originalMessage;
    }
    
    private IEnumerator PulseText(TextMeshProUGUI text, Color pulseColor)
    {
        Color originalColor = text.color;
        text.color = pulseColor;
        yield return new WaitForSeconds(0.2f);
        text.color = originalColor;
    }
    
    void RestartGame()
    {
        if (cachedGameManager != null)
            cachedGameManager.RestartMission();
        else
        {
            // Fallback - find it again using new API
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
                gm.RestartMission();
        }
    }
    
    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}