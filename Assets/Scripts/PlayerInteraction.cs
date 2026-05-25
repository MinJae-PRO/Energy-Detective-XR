using UnityEngine;
using TMPro;

/// <summary>
/// Enhanced player interaction system with UI feedback integration, object targeting info,
/// and dashboard synchronization.
/// Attach to the Player GameObject.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [Tooltip("Maximum distance the player can interact with objects")]
    public float interactDistance = 8f;

    [Tooltip("Layer mask for interaction raycasts")]
    public LayerMask interactableLayers = ~0; // All layers by default

    [Header("Score & UI")]
    [Tooltip("Current player score (managed internally, use AddScore to modify)")]
    public int score = 0;

    [Tooltip("Score display text")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Timer display text")]
    public TextMeshProUGUI timerText;

    [Tooltip("Legacy completion text GameObject")]
    public GameObject completionText;

    [Tooltip("Legacy crosshair text GameObject")]
    public GameObject crosshairText;

    [Tooltip("Legacy instruction panel")]
    public GameObject instructionPanel;

    [Header("UI Manager")]
    [Tooltip("Main UI Manager for coordinated UI updates")]
    public UIManager uiManager;

    [Header("Feedback Settings")]
    [Tooltip("Should feedback text show on successful fix?")]
    public bool showFixFeedback = true;

    [Tooltip("Feedback message shown when fixing an object")]
    public string fixFeedbackMessage = "Energy Fixed!";

    [Tooltip("Feedback messages (randomly selected)")]
    public string[] fixFeedbackVariants = new string[]
    {
        "Energy Fixed!",
        "Repaired!",
        "Power Restored!",
        "Efficiency Improved!",
        "Energy Saved!",
        "System Optimized!"
    };

    [Tooltip("Feedback for wrong answer")]
    public string wrongAnswerFeedback = "Incorrect! Try Again.";

    [Header("Interaction Prompt")]
    [Tooltip("Text showing interaction prompt (e.g., 'Click to Interact')")]
    public TextMeshProUGUI interactionPromptText;

    [Tooltip("Prompt shown when looking at interactable")]
    public string interactPromptMessage = "Click to Interact";

    [Tooltip("Prompt shown when object is out of range")]
    public string outOfRangePrompt = "Too Far Away";

    [Header("Audio")]
    [Tooltip("Sound played when fixing an object")]
    public AudioClip fixSound;

    [Tooltip("Sound played on wrong answer")]
    public AudioClip wrongAnswerSound;

    [Tooltip("Sound played when targeting an object")]
    public AudioClip hoverSound;

    // Internal state
    private Camera playerCamera;
    private EnergyObject[] energyObjects;
    private int fixedCount = 0;
    private int totalObjectCount = 0;
    private float timer = 0f;
    private bool isGameRunning = true;
    private AudioSource audioSource;
    private EnergyObject currentTarget = null;
    private bool wasHovering = false;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        totalObjectCount = energyObjects.Length;
        fixedCount = 0;
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (fixSound != null || wrongAnswerSound != null || hoverSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

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

        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }

        // Find UIManager if not assigned
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }

        // Initialize UI manager with total objects
        if (uiManager != null)
        {
            uiManager.UpdateProgress(0, totalObjectCount);
        }
    }

    void Update()
    {
        if (isGameRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerText();

            if (uiManager != null)
            {
                uiManager.UpdateTimer(timer);
            }
        }

        // Update targeting
        UpdateTargetDetection();

        // Handle interaction input
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }

        // Dashboard toggle
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (uiManager != null)
            {
                uiManager.ToggleDashboard();
            }
        }
    }

    /// <summary>
    /// Adds score and updates all UI displays.
    /// </summary>
    public void AddScore(int points, bool showFeedback = true)
    {
        score += points;
        UpdateScoreText();

        if (uiManager != null)
        {
            uiManager.UpdateScore(score, showFeedback, $"+{points}");
        }
    }

    /// <summary>
    /// Called by QuizManager when an energy object is successfully fixed.
    /// </summary>
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

            // UI Manager update
            if (uiManager != null)
            {
                uiManager.UpdateScore(score, true, $"+{energyObject.points}");
                uiManager.UpdateProgress(fixedCount, totalObjectCount);

                // Show feedback
                if (showFixFeedback)
                {
                    string feedback = GetRandomFeedbackMessage();
                    uiManager.ShowPositiveFeedback(feedback);
                }
            }

            // Play sound
            if (fixSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(fixSound);
            }

            CheckCompletion();
        }
    }

    /// <summary>
    /// Called by QuizManager when the player answers incorrectly.
    /// </summary>
    public void OnWrongAnswer()
    {
        if (uiManager != null)
        {
            uiManager.ShowNegativeFeedback(wrongAnswerFeedback);
        }

        if (wrongAnswerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongAnswerSound);
        }
    }

    /// <summary>
    /// Gets the current game time.
    /// </summary>
    public float GetCurrentTime()
    {
        return timer;
    }

    /// <summary>
    /// Gets the current score.
    /// </summary>
    public int GetCurrentScore()
    {
        return score;
    }

    /// <summary>
    /// Gets the number of fixed objects.
    /// </summary>
    public int GetFixedCount()
    {
        return fixedCount;
    }

    /// <summary>
    /// Gets the total number of energy objects.
    /// </summary>
    public int GetTotalObjectCount()
    {
        return totalObjectCount;
    }

    private void UpdateTargetDetection()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        EnergyObject previousTarget = currentTarget;
        currentTarget = null;

        if (Physics.Raycast(ray, out hit, interactDistance * 1.5f, interactableLayers))
        {
            float distance = hit.distance;
            bool inRange = distance <= interactDistance;

            // Check for EnergyObject
            EnergyObject energyObj = hit.collider.GetComponentInParent<EnergyObject>();
            if (energyObj != null)
            {
                currentTarget = energyObj;

                if (inRange)
                {
                    ShowPrompt(energyObj.isFixed ? "Already Fixed" : interactPromptMessage);
                }
                else
                {
                    ShowPrompt(outOfRangePrompt);
                }

                // Update UI manager with target info
                if (uiManager != null)
                {
                    uiManager.UpdateTargetedObject(energyObj.objectName, energyObj.isFixed);
                }

                // Hover sound
                if (previousTarget != energyObj && hoverSound != null && audioSource != null && !energyObj.isFixed)
                {
                    audioSource.PlayOneShot(hoverSound);
                }

                wasHovering = true;
                return;
            }

            // Check for InstructionNote
            InstructionNote note = hit.collider.GetComponentInParent<InstructionNote>();
            if (note != null && inRange)
            {
                ShowPrompt(note.HasBeenCollected() ? "Press N to Review" : "Click to Collect Note");
                wasHovering = true;

                if (uiManager != null)
                {
                    uiManager.UpdateTargetedObject("Field Guide", false);
                }
                return;
            }

            // Check for KickDoor
            KickDoor door = hit.collider.GetComponentInParent<KickDoor>();
            if (door != null && inRange)
            {
                ShowPrompt("Click to Open/Close");
                wasHovering = true;

                if (uiManager != null)
                {
                    uiManager.UpdateTargetedObject("Door", false);
                }
                return;
            }
        }

        // No target
        if (wasHovering)
        {
            HidePrompt();

            if (uiManager != null)
            {
                uiManager.ClearTargetedObject();
            }

            wasHovering = false;
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactableLayers))
        {
            // Energy Object
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();
            if (energyObject != null && energyObject.isFixed == false)
            {
                OpenQuizForObject(energyObject);
                return;
            }

            // Kick Door
            KickDoor kickDoor = hit.collider.GetComponentInParent<KickDoor>();
            if (kickDoor != null)
            {
                kickDoor.OpenDoor();
                return;
            }

            // Instruction Note
            InstructionNote instructionNote = hit.collider.GetComponentInParent<InstructionNote>();
            if (instructionNote != null)
            {
                instructionNote.PickUpNote();
                return;
            }
        }
    }

    private void OpenQuizForObject(EnergyObject energyObject)
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

    private void CheckCompletion()
    {
        foreach (EnergyObject obj in energyObjects)
        {
            if (obj.isFixed == false)
            {
                return;
            }
        }

        // All objects fixed!
        EndGame();
    }

    private void EndGame()
    {
        isGameRunning = false;

        // Hide legacy UI
        if (completionText != null)
        {
            completionText.SetActive(true);
        }

        if (crosshairText != null)
        {
            crosshairText.SetActive(false);
        }

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        HidePrompt();

        // Show completion through UI Manager
        if (uiManager != null)
        {
            uiManager.ShowCompleteMessage(timer);
        }

        Debug.Log($"[PlayerInteraction] Game Complete! Score: {score}, Time: {timer:F1}s");
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = $"Time: {timer:F1}s";
        }
    }

    private void ShowPrompt(string message)
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(true);
            interactionPromptText.text = message;
        }
    }

    private void HidePrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    private string GetRandomFeedbackMessage()
    {
        if (fixFeedbackVariants.Length > 0)
        {
            return fixFeedbackVariants[Random.Range(0, fixFeedbackVariants.Length)];
        }
        return fixFeedbackMessage;
    }

    void OnDrawGizmosSelected()
    {
        // Visualize interaction range in editor
        Gizmos.color = Color.yellow;
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * interactDistance);
            Gizmos.DrawWireSphere(cam.transform.position + cam.transform.forward * interactDistance, 0.2f);
        }
    }
}
