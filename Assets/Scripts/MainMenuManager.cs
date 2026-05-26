using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    // ==============================================
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject introPanel;

    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown resolutionDropdown;

    public TextMeshProUGUI forwardKeyText;
    public TextMeshProUGUI backKeyText;
    public TextMeshProUGUI leftKeyText;
    public TextMeshProUGUI rightKeyText;

    // ==============================================
    // NEW: Background & Visual Polish
    // ==============================================
    [Header("Background")]
    [Tooltip("Background image / raw image for animated effect")]
    public RawImage backgroundImage;

    [Tooltip("Optional particle system behind menu")]
    public ParticleSystem backgroundParticles;

    [Tooltip("Dark overlay panel for contrast")]
    public Image darkOverlay;

    [Tooltip("Overlay opacity (0-1)")]
    [Range(0f, 1f)]
    public float overlayAlpha = 0.6f;

    // ==============================================
    // NEW: Title Styling
    // ==============================================
    [Header("Title")]
    [Tooltip("Main game title text")]
    public TextMeshProUGUI titleText;

    [Tooltip("Subtitle text")]
    public TextMeshProUGUI subtitleText;

    [Tooltip("Title glow color")]
    public Color titleGlowColor = new Color(0f, 1f, 0.8f, 1f);

    [Tooltip("Should title pulse animate?")]
    public bool animateTitle = true;

    // ==============================================
    // NEW: Button Styling
    // ==============================================
    [Header("Menu Buttons")]
    [Tooltip("Start Game button")]
    public Button startButton;

    [Tooltip("Settings button")]
    public Button settingsButton;

    [Tooltip("Introduction button")]
    public Button introductionButton;

    [Tooltip("Quit button")]
    public Button quitButton;

    [Tooltip("Normal button color")]
    public Color buttonNormalColor = new Color(0f, 0.7f, 0.9f, 1f);

    [Tooltip("Button hover/highlight color")]
    public Color buttonHoverColor = new Color(0f, 0.9f, 1f, 1f);

    [Tooltip("Button text color")]
    public Color buttonTextColor = Color.white;

    [Tooltip("Button font size")]
    public float buttonFontSize = 28f;

    [Tooltip("Animate buttons on hover?")]
    public bool animateButtons = true;

    [Tooltip("Button scale on hover")]
    public float buttonHoverScale = 1.08f;

    // ==============================================
    // NEW: Text Outline Settings
    // ==============================================
    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    // ==============================================
    // NEW: Version / Footer
    // ==============================================
    [Header("Footer")]
    [Tooltip("Version text at bottom")]
    public TextMeshProUGUI versionText;

    [Tooltip("Team credit text")]
    public TextMeshProUGUI creditsText;

    // ==============================================
    // NEW: Panel Transitions
    // ==============================================
    [Header("Transitions")]
    [Tooltip("Duration of panel fade in/out")]
    public float panelTransitionDuration = 0.25f;

    // ==============================================
    // INTERNAL
    // ==============================================
    private KeyCode keyWaitingFor = KeyCode.None;
    private Resolution[] resolutions;
    private Dictionary<Button, Vector3> buttonOriginalScales = new Dictionary<Button, Vector3>();

    // ==============================================
    // LIFECYCLE
    // ==============================================

    void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        introPanel.SetActive(false);

        SetupDisplayModeDropdown();
        SetupResolutionDropdown();
        UpdateKeyTexts();

        // Apply visual polish
        ApplyButtonStyles();
        ApplyTitleStyle();
        ApplyBackground();
        SetupFooter();
        ApplyTextOutlines();
    }

    void Update()
    {
        // Key binding input
        if (keyWaitingFor == KeyCode.None)
        {
            return;
        }

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                AssignKey(key);
                keyWaitingFor = KeyCode.None;
                UpdateKeyTexts();
                break;
            }
        }

        // Title animation
        if (animateTitle && titleText != null)
        {
            AnimateTitleGlow();
        }
    }

    // ==============================================
    // ORIGINAL METHODS (preserved)
    // ==============================================

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        StartCoroutine(SwitchPanel(mainMenuPanel, settingsPanel));
    }

    public void OpenIntroduction()
    {
        StartCoroutine(SwitchPanel(mainMenuPanel, introPanel));
    }

    public void ClosePanels()
    {
        StartCoroutine(SwitchPanel(null, mainMenuPanel));
        settingsPanel.SetActive(false);
        introPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }

    public void ChangeForwardKey()
    {
        keyWaitingFor = KeyCode.W;
        forwardKeyText.text = "Press new key...";
    }

    public void ChangeBackKey()
    {
        keyWaitingFor = KeyCode.S;
        backKeyText.text = "Press new key...";
    }

    public void ChangeLeftKey()
    {
        keyWaitingFor = KeyCode.A;
        leftKeyText.text = "Press new key...";
    }

    public void ChangeRightKey()
    {
        keyWaitingFor = KeyCode.D;
        rightKeyText.text = "Press new key...";
    }

    // ==============================================
    // TEXT OUTLINE APPLICATION
    // ==============================================

    /// <summary>
    /// Applies outlines to all text elements for readability on any background.
    /// </summary>
    void ApplyTextOutlines()
    {
        ApplyTextOutline(titleText);
        ApplyTextOutline(subtitleText);
        ApplyTextOutline(forwardKeyText);
        ApplyTextOutline(backKeyText);
        ApplyTextOutline(leftKeyText);
        ApplyTextOutline(rightKeyText);
        ApplyTextOutline(versionText);
        ApplyTextOutline(creditsText);
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

    // ==============================================
    // VISUAL STYLING
    // ==============================================

    void ApplyButtonStyles()
    {
        StyleButton(startButton, "START GAME");
        StyleButton(settingsButton, "SETTINGS");
        StyleButton(introductionButton, "HOW TO PLAY");
        StyleButton(quitButton, "QUIT");
    }

    void StyleButton(Button button, string label)
    {
        if (button == null) return;

        // Store original scale
        buttonOriginalScales[button] = button.transform.localScale;

        // Style colors with smooth transitions
        ColorBlock cb = button.colors;
        cb.normalColor = buttonNormalColor;
        cb.highlightedColor = buttonHoverColor;
        cb.pressedColor = new Color(buttonNormalColor.r * 0.8f, buttonNormalColor.g * 0.8f, buttonNormalColor.b * 0.8f, 1f);
        cb.selectedColor = buttonHoverColor;
        cb.fadeDuration = 0.15f;
        button.colors = cb;

        // Style text with outline for readability
        TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = label;
            tmp.color = buttonTextColor;
            tmp.fontSize = buttonFontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;

            // Apply outline for readability on any background
            tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
            tmp.outlineColor = textOutlineColor;
            tmp.outlineWidth = textOutlineWidth;

            Shadow shadow = tmp.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = tmp.gameObject.AddComponent<Shadow>();
            }
            shadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        // Add hover animation events
        if (animateButtons)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            // Pointer Enter
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => { OnButtonHover(button, true); });
            trigger.triggers.Add(enterEntry);

            // Pointer Exit
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((data) => { OnButtonHover(button, false); });
            trigger.triggers.Add(exitEntry);
        }

        // Style button image (rounded corners feel)
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = buttonNormalColor;
            img.type = Image.Type.Sliced;
        }
    }

    void OnButtonHover(Button button, bool isHovering)
    {
        if (!buttonOriginalScales.ContainsKey(button)) return;

        Vector3 original = buttonOriginalScales[button];
        Vector3 target = isHovering ? original * buttonHoverScale : original;

        StopCoroutine(nameof(ScaleButton));
        StartCoroutine(ScaleButton(button.transform, target, 0.1f));
    }

    IEnumerator ScaleButton(Transform target, Vector3 endScale, float duration)
    {
        Vector3 start = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(start, endScale, elapsed / duration);
            yield return null;
        }

        target.localScale = endScale;
    }

    void ApplyTitleStyle()
    {
        if (titleText != null)
        {
            titleText.color = Color.white;
            titleText.fontSize = 64f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;

            // Enable glow via material
            titleText.fontMaterial.EnableKeyword("GLOW_ON");
            titleText.fontMaterial.SetColor("_GlowColor", titleGlowColor);
            titleText.fontMaterial.SetFloat("_GlowPower", 0.3f);
        }

        if (subtitleText != null)
        {
            subtitleText.color = new Color(0.7f, 0.9f, 1f, 0.9f);
            subtitleText.fontSize = 22f;
            subtitleText.fontStyle = FontStyles.Italic;
            subtitleText.alignment = TextAlignmentOptions.Center;
            subtitleText.text = "Find & Fix. Save Energy.";
        }
    }

    void AnimateTitleGlow()
    {
        if (titleText == null || titleText.fontMaterial == null) return;

        float pulse = 0.3f + Mathf.Sin(Time.time * 2f) * 0.15f;
        titleText.fontMaterial.SetFloat("_GlowPower", pulse);
    }

    void ApplyBackground()
    {
        if (darkOverlay != null)
        {
            darkOverlay.color = new Color(0f, 0f, 0.05f, overlayAlpha);
        }

        if (backgroundParticles != null)
        {
            backgroundParticles.Play();
        }
    }

    void SetupFooter()
    {
        if (versionText != null)
        {
            versionText.text = "v1.0 | 3702ICT XR Development";
            versionText.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            versionText.fontSize = 14f;
        }

        if (creditsText != null)
        {
            creditsText.text = "Made with Unity  |  Team: Saha, Minjae & Team";
            creditsText.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            creditsText.fontSize = 12f;
        }
    }

    // ==============================================
    // PANEL TRANSITION
    // ==============================================

    IEnumerator SwitchPanel(GameObject hidePanel, GameObject showPanel)
    {
        // Fade out current
        if (hidePanel != null)
        {
            CanvasGroup cg = hidePanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = hidePanel.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            while (elapsed < panelTransitionDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / panelTransitionDuration);
                yield return null;
            }

            hidePanel.SetActive(false);
            cg.alpha = 1f;
        }

        // Fade in new
        if (showPanel != null)
        {
            showPanel.SetActive(true);
            CanvasGroup cg = showPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = showPanel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < panelTransitionDuration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = elapsed / panelTransitionDuration;
                yield return null;
            }

            cg.alpha = 1f;
        }
    }

    // ==============================================
    // ORIGINAL INTERNAL METHODS (preserved)
    // ==============================================

    void SetupDisplayModeDropdown()
    {
        displayModeDropdown.ClearOptions();

        List<string> options = new List<string>
        {
            "Fullscreen",
            "Windowed"
        };

        displayModeDropdown.AddOptions(options);
        displayModeDropdown.onValueChanged.RemoveAllListeners();
        displayModeDropdown.onValueChanged.AddListener(ChangeDisplayMode);
    }

    void ChangeDisplayMode(int index)
    {
        Screen.fullScreen = index == 0;
    }

    void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();

        resolutions = Screen.resolutions;
        List<string> options = new List<string>();

        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
    }

    void ChangeResolution(int index)
    {
        Resolution selectedResolution = resolutions[index];
        Screen.SetResolution(
            selectedResolution.width,
            selectedResolution.height,
            Screen.fullScreen
        );
    }

    void AssignKey(KeyCode newKey)
    {
        if (keyWaitingFor == KeyCode.W)
        {
            PlayerKeySettings.ForwardKey = newKey;
        }
        else if (keyWaitingFor == KeyCode.S)
        {
            PlayerKeySettings.BackKey = newKey;
        }
        else if (keyWaitingFor == KeyCode.A)
        {
            PlayerKeySettings.LeftKey = newKey;
        }
        else if (keyWaitingFor == KeyCode.D)
        {
            PlayerKeySettings.RightKey = newKey;
        }
    }

    void UpdateKeyTexts()
    {
        StyleKeyText(forwardKeyText, "Forward", PlayerKeySettings.ForwardKey.ToString());
        StyleKeyText(backKeyText, "Back", PlayerKeySettings.BackKey.ToString());
        StyleKeyText(leftKeyText, "Left", PlayerKeySettings.LeftKey.ToString());
        StyleKeyText(rightKeyText, "Right", PlayerKeySettings.RightKey.ToString());
    }

    void StyleKeyText(TextMeshProUGUI text, string label, string key)
    {
        if (text == null) return;
        text.text = $"<color=#AAAAAA>{label}:</color>  <color=#222222><b>[</b></color><color=#FFD700><b> {key} </b></color><color=#222222><b>]</b></color>";
        text.color = Color.white;
    }
}