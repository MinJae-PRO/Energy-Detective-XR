using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public int score = 0;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject completionText;
    public GameObject crosshairText;
    public GameObject instructionPanel;

    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    [Header("Score Text Color")]
    public Color scoreTextColor = new Color(1f, 0.95f, 0.2f, 1f);

    [Header("Timer Text Color")]
    public Color timerTextColor = new Color(0.3f, 0.9f, 1f, 1f);

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

        // Apply outlines for readability on any background
        ApplyTextOutline(scoreText);
        ApplyTextOutline(timerText);

        // Apply outlines to crosshair text if present
        if (crosshairText != null)
        {
            TextMeshProUGUI crosshairTMP = crosshairText.GetComponent<TextMeshProUGUI>();
            ApplyTextOutline(crosshairTMP);
        }

        UpdateScoreText();
        UpdateTimerText();

        if (completionText != null)
        {
            completionText.SetActive(false);

            // Apply outline to completion text
            TextMeshProUGUI completionTMP = completionText.GetComponent<TextMeshProUGUI>();
            ApplyTextOutline(completionTMP);
        }

        if (crosshairText != null)
        {
            crosshairText.SetActive(true);
        }
    }

    /// <summary>
    /// Applies an outline to a TextMeshProUGUI element for readability on any background.
    /// </summary>
    void ApplyTextOutline(TextMeshProUGUI text)
    {
        if (text == null) return;

        text.fontMaterial.EnableKeyword("OUTLINE_ON");
        text.outlineColor = textOutlineColor;
        text.outlineWidth = textOutlineWidth;

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
            scoreText.color = scoreTextColor;
        }
    }

    void UpdateTimerText()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            timerText.color = timerTextColor;
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
