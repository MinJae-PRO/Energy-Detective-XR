using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced instruction note system with clearer goals, multi-page instructions,
/// and better visual presentation.
/// Attach to note collectible objects in the scene.
/// </summary>
public class InstructionNote : MonoBehaviour
{
    [Header("Instruction Panel")]
    [Tooltip("The panel that displays instructions when the note is picked up")]
    public GameObject instructionPanel;

    [Header("UI References")]
    [Tooltip("Text component for the instruction title")]
    public TextMeshProUGUI titleText;

    [Tooltip("Text component for the instruction body content")]
    public TextMeshProUGUI bodyText;

    [Tooltip("Text component for page navigation (e.g., 'Page 1/3')")]
    public TextMeshProUGUI pageIndicatorText;

    [Tooltip("Button to go to previous page")]
    public Button previousPageButton;

    [Tooltip("Button to go to next page")]
    public Button nextPageButton;

    [Tooltip("Button to close the instruction panel")]
    public Button closeButton;

    [Header("Note Settings")]
    [Tooltip("Title shown at the top of the instruction panel")]
    public string noteTitle = "Energy Detective Field Guide";

    [Tooltip("Should the note be destroyed after pickup?")]
    public bool destroyOnPickup = false;

    [Tooltip("Sound played when note is picked up")]
    public AudioClip pickupSound;

    [Header("Instruction Pages")]
    [Tooltip("Multi-page instructions. Each entry is a page of content.")]
    [TextArea(5, 10)]
    public List<string> instructionPages = new List<string>
    {
        "Welcome, Energy Detective!\n\n" +
        "Your mission is to find and repair all broken energy objects in the facility. " +
        "These wasteful objects are draining power and must be fixed to complete your assignment.",

        "How to Play:\n\n" +
        "1. Explore the environment using WASD to move and mouse to look around.\n" +
        "2. Find objects marked with a red indicator - these need repair.\n" +
        "3. Click on a broken object to start the repair quiz.\n" +
        "4. Answer the quiz question correctly to fix the object.\n" +
        "5. Fix ALL objects to complete the mission!",

        "Controls:\n\n" +
        "WASD / Arrow Keys - Move around\n" +
        "Mouse - Look around\n" +
        "Left Click - Interact with objects and doors\n" +
        "F - Toggle flashlight\n" +
        "N - Toggle this instruction panel\n" +
        "Tab - Open/Close Dashboard",

        "Tips:\n\n" +
        "- Pay attention to the crosshair color! Green means you can interact.\n" +
        "- Read each quiz question carefully before answering.\n" +
        "- Wrong answers will cost you time - think before you click!\n" +
        "- Check your progress on the dashboard (Tab key).\n" +
        "- The timer tracks your speed - can you get 3 stars?"
    };

    [Header("Visual Effects")]
    [Tooltip("Particle effect played when note is picked up")]
    public ParticleSystem pickupEffect;

    [Tooltip("Should the note float/bob in place?")]
    public bool floatingAnimation = true;

    [Tooltip("Height of floating bob")]
    public float floatAmplitude = 0.1f;

    [Tooltip("Speed of floating bob")]
    public float floatSpeed = 2f;

    [Tooltip("Should the note rotate slowly?")]
    public bool rotateAnimation = true;

    [Tooltip("Rotation speed")]
    public float rotationSpeed = 45f;

    // Internal state
    private bool hasBeenPickedUp = false;
    private Renderer noteRenderer;
    private Collider noteCollider;
    private AudioSource audioSource;
    private int currentPageIndex = 0;
    private Vector3 startPosition;
    private CanvasGroup panelCanvasGroup;
    private bool isPanelVisible = false;

    void Start()
    {
        noteRenderer = GetComponent<Renderer>();
        noteCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        startPosition = transform.position;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
            panelCanvasGroup = instructionPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = instructionPanel.AddComponent<CanvasGroup>();
            }
        }

        SetupButtons();
    }

    void Update()
    {
        // Floating animation for the note
        if (!hasBeenPickedUp)
        {
            AnimateNote();
        }

        // Toggle panel with N key after pickup
        if (hasBeenPickedUp && Input.GetKeyDown(KeyCode.N))
        {
            TogglePanel();
        }

        // Page navigation with arrow keys
        if (isPanelVisible)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                PreviousPage();
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                NextPage();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePanel();
            }
        }
    }

    /// <summary>
    /// Called when player interacts with the note. Shows the instruction panel.
    /// </summary>
    public void PickUpNote()
    {
        if (hasBeenPickedUp)
        {
            TogglePanel();
            return;
        }

        hasBeenPickedUp = true;
        currentPageIndex = 0;

        // Play effects
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        if (pickupEffect != null)
        {
            pickupEffect.Play();
        }

        // Hide note visual
        if (noteRenderer != null)
        {
            noteRenderer.enabled = false;
        }

        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }

        // Show instruction panel
        ShowPanel();
        UpdatePageContent();

        // Optional: destroy the physical note
        if (destroyOnPickup)
        {
            StartCoroutine(DestroyNoteAfterDelay(2f));
        }
    }

    /// <summary>
    /// Shows a specific page of instructions programmatically.
    /// </summary>
    public void ShowPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < instructionPages.Count)
        {
            currentPageIndex = pageIndex;
            UpdatePageContent();
            ShowPanel();
        }
    }

    /// <summary>
    /// Adds a custom instruction page at runtime.
    /// </summary>
    public void AddInstructionPage(string content)
    {
        instructionPages.Add(content);
        UpdatePageContent();
    }

    /// <summary>
    /// Checks if the note has been collected.
    /// </summary>
    public bool HasBeenCollected()
    {
        return hasBeenPickedUp;
    }

    private void ShowPanel()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
            isPanelVisible = true;

            // Unlock cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Pause game time
            Time.timeScale = 0f;
        }
    }

    private void ClosePanel()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
            isPanelVisible = false;

            // Re-lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Resume game time
            Time.timeScale = 1f;
        }
    }

    private void TogglePanel()
    {
        if (isPanelVisible)
        {
            ClosePanel();
        }
        else
        {
            ShowPanel();
            UpdatePageContent();
        }
    }

    private void NextPage()
    {
        if (currentPageIndex < instructionPages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageContent();
        }
    }

    private void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageContent();
        }
    }

    private void UpdatePageContent()
    {
        if (titleText != null)
        {
            titleText.text = noteTitle;
        }

        if (bodyText != null && currentPageIndex < instructionPages.Count)
        {
            bodyText.text = instructionPages[currentPageIndex];
        }

        if (pageIndicatorText != null)
        {
            pageIndicatorText.text = $"Page {currentPageIndex + 1} / {instructionPages.Count}";
        }

        // Update button states
        if (previousPageButton != null)
        {
            previousPageButton.interactable = currentPageIndex > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPageIndex < instructionPages.Count - 1;
        }
    }

    private void SetupButtons()
    {
        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(PreviousPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(NextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
    }

    private void AnimateNote()
    {
        if (floatingAnimation)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = startPosition + Vector3.up * yOffset;
        }

        if (rotateAnimation)
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private IEnumerator DestroyNoteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
