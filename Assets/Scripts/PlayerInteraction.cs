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
    public GameObject instructionPanel;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;

    private float timer = 0f;
    private bool isGameRunning = true;
    private int fixedCount = 0;
    private int totalObjectCount = 0;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        totalObjectCount = energyObjects.Length;

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

        // Crosshair: detect hover target
        DetectHoverTarget();

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
                QuizManager quizManager = FindFirstObjectByType<QuizManager>();

                if (quizManager != null)
                {
                    quizManager.OpenQuiz(
                        energyObject,
                        energyObject.question,
                        energyObject.correctAnswer,
                        energyObject.wrongAnswer1,
                        energyObject.wrongAnswer2
                    );
                }
            }

            KickDoor kickDoor = hit.collider.GetComponentInParent<KickDoor>();

            if (kickDoor != null)
            {
                kickDoor.OpenDoor();
            }

            InstructionNote instructionNote = hit.collider.GetComponentInParent<InstructionNote>();

            if (instructionNote != null)
            {
                instructionNote.PickUpNote();
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
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
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
            if (instructionPanel != null)
            {
                instructionPanel.SetActive(false);
            }
        }

        if (crosshairText != null)
        {
            crosshairText.SetActive(false);
        }
    }

    public void FixEnergyObjectFromQuiz(EnergyObject energyObject)
    {
        if (energyObject == null)
        {
            return;
        }

        if (energyObject.isFixed == false)
        {
            energyObject.FixObject();
            score += energyObject.points;
            fixedCount++;

            UpdateScoreText();

            // Show "Energy Fixed!" feedback
            UIManager uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowPositiveFeedback("Energy Fixed!");
                uiManager.UpdateProgress(fixedCount, totalObjectCount);
            }

            CheckCompletion();
        }
    }

    // Crosshair color changes based on hover target
    private void DetectHoverTarget()
    {
        if (playerCamera == null || crosshairText == null) return;

        TextMeshProUGUI crosshairTMP = crosshairText.GetComponent<TextMeshProUGUI>();
        if (crosshairTMP == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObj = hit.collider.GetComponentInParent<EnergyObject>();
            if (energyObj != null)
            {
                if (energyObj.isFixed)
                {
                    // Blue for already fixed
                    crosshairTMP.color = new Color(0.4f, 0.7f, 1f, 0.8f);
                    crosshairTMP.text = "o";
                }
                else
                {
                    // Green for fixable
                    crosshairTMP.color = new Color(0.3f, 1f, 0.4f, 1f);
                    crosshairTMP.text = "[+]";
                }
                return;
            }

            InstructionNote note = hit.collider.GetComponentInParent<InstructionNote>();
            if (note != null)
            {
                crosshairTMP.color = new Color(1f, 0.9f, 0.2f, 1f);
                crosshairTMP.text = "[?]";
                return;
            }

            KickDoor door = hit.collider.GetComponentInParent<KickDoor>();
            if (door != null)
            {
                crosshairTMP.color = new Color(0.3f, 1f, 0.4f, 1f);
                crosshairTMP.text = "[+]";
                return;
            }
        }

        // Default white crosshair
        crosshairTMP.color = new Color(1f, 1f, 1f, 0.9f);
        crosshairTMP.text = "+";
    }
}
