using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject completionText;
    public GameObject crosshairText;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;

    private float timer = 0f;
    private bool isGameRunning = true;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);

        UpdateScoreText();
        UpdateTimerText();

        if (completionText != null)
        {
            completionText.SetActive(false);
        }

        if (crosshairText != null)
        {
            crosshairText.SetActive(true);
        }
    }

    void Update()
    {
        if (isGameRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerText();
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();

            if (energyObject != null && energyObject.isFixed == false)
            {
                energyObject.FixObject();
                score += energyObject.points;

                UpdateScoreText();
                CheckCompletion();
            }
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + timer.ToString("F1") + "s";
        }
    }

    void CheckCompletion()
    {
        foreach (EnergyObject obj in energyObjects)
        {
            if (obj.isFixed == false)
            {
                return;
            }
        }

        isGameRunning = false;

        if (completionText != null)
        {
            completionText.SetActive(true);
        }

        if (crosshairText != null)
        {
            crosshairText.SetActive(false);
        }
    }
}