using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UIManager uiManager;

    private int score = 0;
    private int fixedObjects = 0;
    private int totalObjects = 0;

    private float timer = 0f;
    private bool isGameRunning = true;

    void Start()
    {
        EnergyObject[] objects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        totalObjects = objects.Length;

        score = 0;
        fixedObjects = 0;
        timer = 0f;
        isGameRunning = true;

        uiManager.UpdateScore(score);
        uiManager.UpdateTimer(timer);
        uiManager.UpdateProgress(0, totalObjects);
    }

    void Update()
    {
        if (isGameRunning)
        {
            timer += Time.deltaTime;
            uiManager.UpdateTimer(timer);
        }
    }

    public void FixEnergyObject(string objectName)
    {
        score += 10;
        fixedObjects++;

        uiManager.UpdateScore(score);
        uiManager.UpdateProgress(fixedObjects, totalObjects);
        uiManager.ShowPositiveFeedback($"{objectName} Fixed!");

        if (fixedObjects >= totalObjects)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        isGameRunning = false;
        uiManager.ShowCompleteMessage(timer);
    }
}
