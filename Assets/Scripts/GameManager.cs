using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;
    
    public float timeLimit = 60f;
    public bool useTimeAttackMode = true;
    
    private int score = 0;
    private int fixedObjects = 0;
    private int totalObjects = 0;
    
    private float currentTime;
    private bool isGameRunning = true;
    private bool isMissionComplete = false;
    
    public static GameManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        EnergyObject[] objects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        totalObjects = objects.Length;
        
        score = 0;
        fixedObjects = 0;
        
        if (useTimeAttackMode)
        {
            currentTime = timeLimit;
        }
        else
        {
            currentTime = 0f;
        }
        
        isGameRunning = true;
        isMissionComplete = false;
        
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateTimer(currentTime, useTimeAttackMode);
            uiManager.UpdateRemainingObjects(totalObjects - fixedObjects, totalObjects);
            uiManager.UpdateMissionStatus("IN PROGRESS");
        }
    }
    
    void Update()
    {
        if (!isGameRunning || isMissionComplete) return;
        
        if (useTimeAttackMode)
        {
            currentTime -= Time.deltaTime;
            if (uiManager != null)
                uiManager.UpdateTimer(currentTime, true);
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                MissionFailed();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            if (uiManager != null)
                uiManager.UpdateTimer(currentTime, false);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartMission();
        }
    }
    
    public void FixEnergyObject(string objectName, int points)
    {
        if (!isGameRunning || isMissionComplete) return;
        
        score += points;
        fixedObjects++;
        
        if (uiManager != null)
        {
            uiManager.UpdateScore(score);
            uiManager.UpdateRemainingObjects(totalObjects - fixedObjects, totalObjects);
            string tip = GetEnergySavingTip(objectName);
            uiManager.ShowPopupEducationalTip(tip);
        }
        
        if (fixedObjects >= totalObjects)
        {
            MissionComplete();
        }
    }
    
    private void MissionComplete()
    {
        isMissionComplete = true;
        isGameRunning = false;
        
        float finalTime = useTimeAttackMode ? (timeLimit - currentTime) : currentTime;
        
        // Use improved completion screen
        if (uiManager != null)
        {
            uiManager.ShowImprovedCompletionScreen(finalTime, score, totalObjects);
            uiManager.UpdateMissionStatus("COMPLETE ✓");
        }
        
        Debug.Log($"Mission Complete! Time: {finalTime:F1}s, Score: {score}");
    }
    
    private void MissionFailed()
    {
        isGameRunning = false;
        isMissionComplete = false;
        
        if (uiManager != null)
        {
            uiManager.ShowMissionIncomplete();
            uiManager.UpdateMissionStatus("FAILED - Time's Up!");
        }
        
        Debug.Log("Mission Failed - Time's Up!");
    }
    
    public int GetRemainingObjects()
    {
        return totalObjects - fixedObjects;
    }
    
    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    public void RestartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    
    private string GetEnergySavingTip(string objectName)
    {
        string lowerName = objectName.ToLower();
        
        if (lowerName.Contains("light") || lowerName.Contains("bulb"))
            return "Switch to LED bulbs - they use 75% less energy! 💡";
        else if (lowerName.Contains("tv") || lowerName.Contains("television"))
            return "Turn off TVs when not watching - saves $30/year! 📺";
        else if (lowerName.Contains("computer") || lowerName.Contains("pc"))
            return "Enable sleep mode after 15 minutes of idle time! 💻";
        else if (lowerName.Contains("charger") || lowerName.Contains("adapter"))
            return "Unplug chargers when devices reach 100% - phantom power wasted! 🔌";
        else if (lowerName.Contains("fridge") || lowerName.Contains("refrigerator"))
            return "Clean fridge coils yearly - improves efficiency by 30%! ❄️";
        else
            return "Every fixed energy waste saves money AND the planet! 🌍";
    }
}