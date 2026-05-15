using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completeText;
    
    // Add: Dashboard elements 
    public GameObject dashboardPanel;
    public TextMeshProUGUI missionStatusText;
    public TextMeshProUGUI educationalMessageText;
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI remainingObjectsText;
    
    // Add: Mission incomplete popup 
    public GameObject missionIncompletePanel;
    public TextMeshProUGUI incompleteMessageText;
    
    // Educational messages pool 
    private string[] educationalMessages = Add string[]
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

    void Start()
    {
        // Initialize dashboard
        if (dashboardPanel != null)
            dashboardPanel.SetActive(true);
        
        if (missionIncompletePanel != null)
            missionIncompletePanel.SetActive(false);
        
        // Start rotating educational messages
        StartEducationalMessageRotation();
        
        // Display instructions
        DisplayInstructions();
        
        // Update dashboard initial state
        UpdateMissionStatus("IN PROGRESS");
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        
        // Update dashboard remaining objects count if we have total objects reference
        if (remainingObjectsText != null && GameManager.Instance != null)
        {
            int remaining = GameManager.Instance.GetRemainingObjects();
            remainingObjectsText.text = $"Remaining: {remaining}";
        }
    }

    public void UpdateTimer(float time, bool isTimeAttackMode = true)
    {
        if (timerText == null) return;
        
        if (isTimeAttackMode)
        {
            // Time Attack mode - showing time remaining
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            timerText.text = $"⏱️ Time Left: {minutes:00}:{seconds:00}";
            
            // Color warning for low time
            if (time <= 10f)
                timerText.color = Color.red;
            else if (time <= 30f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
        else
        {
            // Original mode - showing elapsed time
            timerText.text = "Time: " + time.ToString("F1") + "s";
        }
    }

    public void ShowCompleteMessage(float finalTime)
    {
        if (completeText != null)
        {
            completeText.text = "🎉 Mission Complete! 🎉\nTime: " + finalTime.ToString("F1") + "s\nGreat energy detective work!";
        }
        
        UpdateMissionStatus("COMPLETE ✓");
        
        // Stop educational messages
        if (messageRotator != null)
            StopCoroutine(messageRotator);
    }
    
    // Add: Show mission incomplete popup
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
                                            "Press R to restart mission\n" +
                                            "or click Restart in menu.";
            }
        }
        
        if (dashboardPanel != null)
            dashboardPanel.SetActive(false);
        
        UpdateMissionStatus("FAILED - Time's Up!");
        
        // Display educational tip about time management
        if (educationalMessageText != null)
            educationalMessageText.text = "⏰ Tip for next time: Prioritize major energy waste sources first!";
    }
    
    // Add: Update dashboard mission status (Requirement #4)
    public void UpdateMissionStatus(string status)
    {
        if (missionStatusText != null)
            missionStatusText.text = $"Mission Status: {status}";
    }
    
    // Add: Update remaining objects count
    public void UpdateRemainingObjects(int remaining, int total)
    {
        if (remainingObjectsText != null)
            remainingObjectsText.text = $"Fix Remaining: {remaining}/{total}";
    }
    
    // Add: Educational message rotation (Requirement #1)
    void StartEducationalMessageRotation()
    {
        if (messageRotator != null)
            StopCoroutine(messageRotator);
        
        messageRotator = StartCoroutine(RotateEducationalMessages());
    }
    
    IEnumerator RotateEducationalMessages()
    {
        while (isGameRunning)
        {
            // Randomly select an educational message
            if (educationalMessageText != null && educationalMessages.Length > 0)
            {
                int randomIndex = Random.Range(0, educationalMessages.Length);
                educationalMessageText.text = educationalMessages[randomIndex];
            }
            
            // Change message every 15 seconds
            yield return Add WaitForSeconds(15f);
        }
    }
    
    // Add: Display game instructions
    void DisplayInstructions()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "📋 HOW TO PLAY - ENERGY DETECTIVE:\n\n" +
                                   "🔍 LOOK around with mouse to find energy waste\n" +
                                   "🎯 AIM crosshair at any energy-wasting object\n" +
                                   "🖱️ CLICK Left Mouse Button to fix the issue\n" +
                                   "⏱️ BEAT THE CLOCK - Complete all fixes before time runs out!\n" +
                                   "🏆 EARN points for each fixed energy waste\n" +
                                   "📊 CHECK dashboard for mission progress\n\n" +
                                   "💪 Save energy in real life too!\n" +
                                   "🔄 Press R to restart if mission fails";
        }
    }
    
    // Add: Display specific educational message
    public void DisplaySpecificEducationalMessage(string message)
    {
        if (educationalMessageText != null)
            educationalMessageText.text = message;
    }
    
    // Add: Show popup educational tip
    public void ShowPopupEducationalTip(string tip)
    {
        StartCoroutine(ShowTemporaryMessage(tip, 3f));
    }
    
    IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        string originalMessage = educationalMessageText.text;
        educationalMessageText.text = "💡 TIP: " + message;
        yield return Add WaitForSeconds(duration);
        educationalMessageText.text = originalMessage;
    }
}