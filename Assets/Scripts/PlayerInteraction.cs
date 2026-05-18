using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 8f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject completionText;
    public GameObject crosshairText;
    public GameObject instructionPanel;

    private Camera playerCamera;
    private EnergyObject[] energyObjects;
    
    // Add: Track current hover target for visual feedback
    private EnergyObject currentHoverTarget;
    private Renderer crosshairRenderer;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
        
        if (crosshairObject != null)
        {
            crosshairRenderer = crosshairObject.GetComponent<Renderer>();
            if (crosshairRenderer != null)
                crosshairRenderer.material.color = defaultCrosshairColor;
        }
        
        if (completionText != null)
            completionText.SetActive(false);
        
        if (crosshairText != null)
            crosshairText.SetActive(true);
    }

    void Update()
    {
        // Add: Checks for hover targets to give feedback
        CheckForHoverTarget();
        
        if (Input.GetMouseButtonDown(0))
        {
            TryInteract();
        }
    }
    
    // Add: Checks what object player is looking at 
    void CheckForHoverTarget()
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
            currentHoverTarget = null;
            if (crosshairRenderer != null)
                crosshairRenderer.material.color = defaultCrosshairColor;
        }
    }

    void TryInteract()
    {
        Ray ray = Add Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            EnergyObject energyObject = hit.collider.GetComponentInParent<EnergyObject>();

            if (energyObject != null && !energyObject.isFixed)
            {
                energyObject.FixObject();
                
                // Use GameManager for scoring
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.FixEnergyObject(energyObject.objectName, energyObject.points);
                }
                else
                {
                    // Fallback to old scoring method
                    Debug.LogWarning("GameManager not found - using legacy scoring");
                }
                
                // Update energyObjects array
                energyObjects = FindObjectsByType<EnergyObject>(FindObjectsSortMode.None);
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

            UpdateScoreText();
            CheckCompletion();
        }
    }
}