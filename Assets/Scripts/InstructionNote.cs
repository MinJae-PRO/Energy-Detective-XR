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

    [Header("Button Styling")]
    [Tooltip("Normal button color")]
    public Color buttonNormalColor = new Color(0.1f, 0.5f, 0.7f, 1f);

    [Tooltip("Button hover/highlight color")]
    public Color buttonHoverColor = new Color(0.15f, 0.65f, 0.9f, 1f);

    [Tooltip("Button text color")]
    public Color buttonTextColor = Color.white;

    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    [Header("Instruction Pages")]
    public string noteTitle = "Energy Detective - Field Guide";

    [TextArea(5, 10)]
    public List<string> pages = new List<string>
    {
        "<color=#00FFCC><size=140%><b>YOUR MISSION</b></size></color>\n\n" +
        "You are an <b>Energy Detective</b>! Find and repair <color=#FF4444><b>ALL</b></color> broken energy objects in the facility.\n\n" +
        "Look for objects with a <color=#FF4444><b>RED</b></color> status light \u2192 those are wasting energy and need fixing!",

        "<color=#00FFCC><size=140%><b>HOW TO PLAY</b></size></color>\n\n" +
        "<b>1.</b> Walk around and look for broken objects (red light)\n" +
        "<b>2.</b> Click on a broken object to start a repair quiz\n" +
        "<b>3.</b> Answer correctly to fix the object\n" +
        "<b>4.</b> Fix <color=#FF4444><b>ALL</b></color> objects to complete the mission!\n\n" +
        "<color=#FFD700><b>Goal:</b></color> Fix every broken energy object as fast as you can.",

        "<color=#00FFCC><size=140%><b>CONTROLS</b></size></color>\n\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b> WASD </b></color><color=#222222><b>]</b></color>          Move around\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b> Mouse </b></color><color=#222222><b>]</b></color>         Look around\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b> Left Click </b></color><color=#222222><b>]</b></color>   Interact with objects\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b>  F  </b></color><color=#222222><b>]</b></color>          Toggle flashlight\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b>  N  </b></color><color=#222222><b>]</b></color>          Open / Close guide\n" +
        "  <color=#222222><b>[</b></color><color=#FFD700><b> Tab </b></color><color=#222222><b>]</b></color>          Toggle Dashboard",

        "<color=#00FFCC><size=140%><b>TIPS</b></size></color>\n\n" +
        "<color=#AAAAAA>Crosshair colors:</color>\n" +
        "  <color=white><b>+</b></color>     Default\n" +
        "  <color=#44FF44><b>[+]</b></color>  Can interact\n" +
        "  <color=#66BBFF><b>o</b></color>    Already fixed\n" +
        "  <color=#FFDD44><b>[?]</b></color>  Note / Guide\n\n" +
        "<color=#FF6666>Wrong answers make you retry</color> \u2192 read carefully!"
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

        // Force all text to white so it shows on dark backgrounds
        ForceTextColors();

        // Apply professional button styling
        StyleNavigationButton(nextPageButton, ">");
        StyleNavigationButton(prevPageButton, "<");
    }

    /// <summary>
    /// Styles the navigation buttons (Next/Previous) with professional colors and transitions.
    /// </summary>
    void StyleNavigationButton(Button button, string label)
    {
        if (button == null) return;

        ColorBlock cb = button.colors;
        cb.normalColor = buttonNormalColor;
        cb.highlightedColor = buttonHoverColor;
        cb.pressedColor = new Color(buttonNormalColor.r * 0.8f, buttonNormalColor.g * 0.8f, buttonNormalColor.b * 0.8f, 1f);
        cb.selectedColor = buttonHoverColor;
        cb.disabledColor = new Color(buttonNormalColor.r, buttonNormalColor.g, buttonNormalColor.b, 0.35f);
        cb.fadeDuration = 0.12f;
        button.colors = cb;

        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = buttonNormalColor;
            img.type = Image.Type.Sliced;
        }

        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = label;
            tmp.color = buttonTextColor;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            // Apply outline for readability
            tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            tmp.outlineColor = textOutlineColor;
            tmp.outlineWidth = textOutlineWidth;

            Shadow shadow = tmp.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = tmp.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    /// <summary>
    /// Forces text color to white for readability on dark backgrounds.
    /// </summary>
    void ForceTextColors()
    {
        if (titleText != null)
        {
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;

            titleText.fontMaterial.EnableKeyword("OUTLINE_ON");
            titleText.outlineColor = textOutlineColor;
            titleText.outlineWidth = textOutlineWidth;

            Shadow shadow = titleText.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = titleText.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        if (bodyText != null)
        {
            bodyText.color = Color.white;
            bodyText.alignment = TextAlignmentOptions.Left;

            bodyText.fontMaterial.EnableKeyword("OUTLINE_ON");
            bodyText.outlineColor = textOutlineColor;
            bodyText.outlineWidth = textOutlineWidth;

            Shadow shadow = bodyText.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = bodyText.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        if (pageIndicatorText != null)
        {
            pageIndicatorText.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            pageIndicatorText.fontMaterial.EnableKeyword("OUTLINE_ON");
            pageIndicatorText.outlineColor = textOutlineColor;
            pageIndicatorText.outlineWidth = textOutlineWidth * 0.8f;

            Shadow shadow = pageIndicatorText.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = pageIndicatorText.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
        }
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
