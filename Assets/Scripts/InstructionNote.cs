using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class InstructionNote : MonoBehaviour
{
    public GameObject instructionPanel;

    // --- Multi-page instruction UI ---
    [Header("Page Navigation")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI pageIndicatorText;
    public Button nextPageButton;
    public Button prevPageButton;

    [Header("Instruction Pages")]
    public string noteTitle = "Energy Detective - Field Guide";

    [TextArea(5, 10)]
    public List<string> pages = new List<string>
    {
        "<b><size=130%>YOUR MISSION</size></b>\n\n" +
        "You are an Energy Detective! Find and repair ALL broken energy objects in the facility.\n\n" +
        "Look for objects with a <color=red>RED</color> status light - those are wasting energy and need fixing!",

        "<b><size=130%>HOW TO PLAY</size></b>\n\n" +
        "1. Walk around and look for broken objects (red light)\n" +
        "2. Click on a broken object to start a repair quiz\n" +
        "3. Answer the question correctly to fix the object\n" +
        "4. Fix ALL objects to complete the mission!\n\n" +
        "<b>Goal:</b> Fix every broken energy object as fast as you can.",

        "<b><size=130%>CONTROLS</size></b>\n\n" +
        "<b>WASD</b>  -  Move around\n" +
        "<b>Mouse</b>  -  Look around\n" +
        "<b>Left Click</b>  -  Interact with objects\n" +
        "<b>F</b>  -  Toggle flashlight\n" +
        "<b>N</b>  -  Open/Close this guide\n" +
        "<b>Tab</b>  -  Toggle Dashboard",

        "<b><size=130%>TIPS</size></b>\n\n" +
        "Crosshair colors:\n" +
        "  <color=white>+</color> = Default\n" +
        "  <color=green>[+]</color> = Can interact\n" +
        "  <color=#66BBFF>o</color> = Already fixed\n" +
        "  <color=yellow>[?]</color> = Note/Guide\n\n" +
        "Wrong answers make you retry - read carefully!"
    };

    private bool hasBeenPickedUp = false;
    private Renderer noteRenderer;
    private Collider noteCollider;
    private int currentPage = 0;

    void Start()
    {
        noteRenderer = GetComponent<Renderer>();
        noteCollider = GetComponent<Collider>();

        // Setup button listeners
        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);
        if (prevPageButton != null)
            prevPageButton.onClick.AddListener(PrevPage);
    }

    void Update()
    {
        if (hasBeenPickedUp && Input.GetKeyDown(KeyCode.N))
        {
            if (instructionPanel != null)
            {
                instructionPanel.SetActive(!instructionPanel.activeSelf);
            }
        }

        // Keyboard page navigation when panel is open
        if (instructionPanel != null && instructionPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                NextPage();
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                PrevPage();
        }
    }

    public void PickUpNote()
    {
        if (hasBeenPickedUp)
        {
            // Already picked up - just toggle
            if (instructionPanel != null)
                instructionPanel.SetActive(!instructionPanel.activeSelf);
            return;
        }

        hasBeenPickedUp = true;
        currentPage = 0;

        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }

        if (noteRenderer != null)
        {
            noteRenderer.enabled = false;
        }

        if (noteCollider != null)
        {
            noteCollider.enabled = false;
        }

        ShowPage(0);
    }

    void ShowPage(int index)
    {
        currentPage = Mathf.Clamp(index, 0, pages.Count - 1);

        if (titleText != null)
            titleText.text = noteTitle;

        if (bodyText != null && pages.Count > 0)
            bodyText.text = pages[currentPage];

        if (pageIndicatorText != null)
            pageIndicatorText.text = $"Page {currentPage + 1} / {pages.Count}";

        // Update button visibility
        if (prevPageButton != null)
            prevPageButton.interactable = currentPage > 0;
        if (nextPageButton != null)
            nextPageButton.interactable = currentPage < pages.Count - 1;
    }

    void NextPage()
    {
        if (currentPage < pages.Count - 1)
            ShowPage(currentPage + 1);
    }

    void PrevPage()
    {
        if (currentPage > 0)
            ShowPage(currentPage - 1);
    }
}
