using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class QuizManager : MonoBehaviour
{
    public GameObject quizPanel;

    public TextMeshProUGUI questionText;

    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;

    public TextMeshProUGUI answerText1;
    public TextMeshProUGUI answerText2;
    public TextMeshProUGUI answerText3;

    public GameObject tryAgainText;

    // --- Correct answer feedback ---
    [Header("Correct Feedback")]
    public GameObject correctText;
    public float correctDelay = 0.8f;

    [Header("Button Styling")]
    [Tooltip("Normal button background color")]
    public Color buttonNormalColor = new Color(0.12f, 0.35f, 0.55f, 1f);

    [Tooltip("Button hover/highlight color")]
    public Color buttonHoverColor = new Color(0.2f, 0.5f, 0.75f, 1f);

    [Tooltip("Button pressed color")]
    public Color buttonPressedColor = new Color(0.08f, 0.25f, 0.4f, 1f);

    [Tooltip("Button text color")]
    public Color buttonTextColor = Color.white;

    [Tooltip("Button font size")]
    public float buttonFontSize = 24f;

    [Tooltip("Button hover scale effect")]
    public float buttonHoverScale = 1.05f;

    [Header("Text Outline Settings")]
    [Tooltip("Outline color for text readability on any background")]
    public Color textOutlineColor = new Color(0f, 0f, 0f, 1f);

    [Tooltip("Outline thickness for text readability")]
    public float textOutlineWidth = 0.25f;

    [Header("Question Text Color")]
    public Color questionTextColor = new Color(1f, 1f, 1f, 1f);

    private EnergyObject currentObject;
    private PlayerInteraction playerInteraction;

    void Start()
    {
        quizPanel.SetActive(false);
        if (tryAgainText != null)
            tryAgainText.SetActive(false);
        if (correctText != null)
            correctText.SetActive(false);

        playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        // Apply professional button styling
        StyleAnswerButton(answerButton1, answerText1);
        StyleAnswerButton(answerButton2, answerText2);
        StyleAnswerButton(answerButton3, answerText3);

        // Apply outline to question text
        ApplyTextOutline(questionText);
        if (questionText != null)
        {
            questionText.color = questionTextColor;
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

    /// <summary>
    /// Styles an answer button with professional colors, transitions, and text formatting.
    /// </summary>
    void StyleAnswerButton(Button button, TextMeshProUGUI buttonText)
    {
        if (button == null) return;

        // Configure ColorBlock for smooth transitions
        ColorBlock cb = button.colors;
        cb.normalColor = buttonNormalColor;
        cb.highlightedColor = buttonHoverColor;
        cb.pressedColor = buttonPressedColor;
        cb.selectedColor = buttonHoverColor;
        cb.disabledColor = new Color(buttonNormalColor.r, buttonNormalColor.g, buttonNormalColor.b, 0.4f);
        cb.fadeDuration = 0.12f;
        button.colors = cb;

        // Style the button image
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            img.color = buttonNormalColor;
            img.type = Image.Type.Sliced;
        }

        // Style the button text
        if (buttonText != null)
        {
            buttonText.color = buttonTextColor;
            buttonText.fontSize = buttonFontSize;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;

            // Apply outline for readability
            ApplyTextOutline(buttonText);
        }

        // Add hover animation
        AddHoverAnimation(button);
    }

    /// <summary>
    /// Adds hover scale animation to a button.
    /// </summary>
    void AddHoverAnimation(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(button.transform, buttonHoverScale, 0.1f));
        });
        trigger.triggers.Add(enterEntry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(button.transform, 1f, 0.1f));
        });
        trigger.triggers.Add(exitEntry);
    }

    IEnumerator ScaleButton(Transform target, float endScale, float duration)
    {
        Vector3 start = target.localScale;
        Vector3 end = Vector3.one * endScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        target.localScale = end;
    }

    public void OpenQuiz(
        EnergyObject energyObject,
        string question,
        string correctAnswer,
        string wrongAnswer1,
        string wrongAnswer2
    )
    {
        currentObject = energyObject;

        quizPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        questionText.text = question;

        answerText1.text = correctAnswer;
        answerText2.text = wrongAnswer1;
        answerText3.text = wrongAnswer2;

        // Reset feedback
        if (tryAgainText != null)
            tryAgainText.SetActive(false);
        if (correctText != null)
            correctText.SetActive(false);

        ResetButtonColors();

        answerButton1.onClick.RemoveAllListeners();
        answerButton2.onClick.RemoveAllListeners();
        answerButton3.onClick.RemoveAllListeners();

        answerButton1.onClick.AddListener(CorrectAnswer);
        answerButton2.onClick.AddListener(WrongAnswer);
        answerButton3.onClick.AddListener(WrongAnswer);

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }
    }

    void CorrectAnswer()
    {
        // Highlight correct button green
        SetButtonColor(answerButton1, new Color(0.2f, 0.8f, 0.3f, 1f));

        // Show "Correct!" text
        if (correctText != null)
            correctText.SetActive(true);

        if (playerInteraction != null && currentObject != null)
        {
            playerInteraction.FixEnergyObjectFromQuiz(currentObject);
        }

        // Disable buttons to prevent double-click
        SetButtonsInteractable(false);

        Invoke(nameof(CloseQuiz), correctDelay);
    }

    void WrongAnswer()
    {
        // Highlight the clicked button red and correct answer green
        Button clicked = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();
        if (clicked != null)
            SetButtonColor(clicked, new Color(0.8f, 0.2f, 0.2f, 1f));
        SetButtonColor(answerButton1, new Color(0.2f, 0.8f, 0.3f, 1f));

        if (tryAgainText != null)
            tryAgainText.SetActive(true);

        // Show "Try Again!" feedback via UIManager
        UIManager ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.ShowNegativeFeedback("Try Again!");

        SetButtonsInteractable(false);

        StartCoroutine(ShowTryAgain());
    }

    IEnumerator ShowTryAgain()
    {
        yield return new WaitForSeconds(1.5f);

        if (tryAgainText != null)
            tryAgainText.SetActive(false);

        CloseQuiz();
    }

    void CloseQuiz()
    {
        quizPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reset button scales
        if (answerButton1 != null) answerButton1.transform.localScale = Vector3.one;
        if (answerButton2 != null) answerButton2.transform.localScale = Vector3.one;
        if (answerButton3 != null) answerButton3.transform.localScale = Vector3.one;

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        ResetButtonColors();
    }

    private void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;
        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = color;
    }

    private void ResetButtonColors()
    {
        SetButtonColor(answerButton1, buttonNormalColor);
        SetButtonColor(answerButton2, buttonNormalColor);
        SetButtonColor(answerButton3, buttonNormalColor);
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButton1 != null) answerButton1.interactable = interactable;
        if (answerButton2 != null) answerButton2.interactable = interactable;
        if (answerButton3 != null) answerButton3.interactable = interactable;
    }
}
