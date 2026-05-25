using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enhanced quiz manager with improved feedback, visual polish, and UI integration.
/// Attach to a Canvas with the quiz panel UI elements.
/// </summary>
public class QuizManager : MonoBehaviour
{
    [Header("Quiz Panel")]
    [Tooltip("The main quiz panel GameObject")]
    public GameObject quizPanel;

    [Header("Question Display")]
    [Tooltip("Text component for the quiz question")]
    public TextMeshProUGUI questionText;

    [Tooltip("Text component for the question category/header")]
    public TextMeshProUGUI questionCategoryText;

    [Header("Answer Buttons")]
    [Tooltip("Button for answer option 1 (correct answer)")]
    public Button answerButton1;

    [Tooltip("Button for answer option 2")]
    public Button answerButton2;

    [Tooltip("Button for answer option 3")]
    public Button answerButton3;

    [Header("Answer Texts")]
    public TextMeshProUGUI answerText1;
    public TextMeshProUGUI answerText2;
    public TextMeshProUGUI answerText3;

    [Header("Feedback")]
    [Tooltip("Text shown for wrong answers")]
    public TextMeshProUGUI tryAgainText;

    [Tooltip("Text shown for correct answers")]
    public TextMeshProUGUI correctText;

    [Tooltip("Panel background image")]
    public Image quizBackgroundImage;

    [Tooltip("Color for correct answer feedback")]
    public Color correctColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Tooltip("Color for wrong answer feedback")]
    public Color wrongColor = new Color(1f, 0.25f, 0.25f, 1f);

    [Tooltip("Colors for answer button backgrounds")]
    public Color normalButtonColor = new Color(0.15f, 0.25f, 0.35f, 1f);

    public Color hoverButtonColor = new Color(0.25f, 0.4f, 0.55f, 1f);
    public Color correctButtonColor = new Color(0.15f, 0.6f, 0.25f, 1f);
    public Color wrongButtonColor = new Color(0.6f, 0.15f, 0.15f, 1f);

    [Header("Animation Settings")]
    [Tooltip("Duration of panel open/close animation")]
    public float panelAnimationDuration = 0.25f;

    [Tooltip("Scale of panel when opening")]
    public float panelOpenScale = 1f;

    [Tooltip("Scale of panel when closing")]
    public float panelCloseScale = 0.8f;

    [Tooltip("Duration of answer button color transition")]
    public float buttonColorDuration = 0.2f;

    [Tooltip("Delay before closing after correct answer")]
    public float correctAnswerDelay = 0.8f;

    [Tooltip("Delay before closing after wrong answer")]
    public float wrongAnswerDelay = 1.5f;

    [Header("Audio")]
    [Tooltip("Sound played when quiz opens")]
    public AudioClip quizOpenSound;

    [Tooltip("Sound played on correct answer")]
    public AudioClip correctSound;

    [Tooltip("Sound played on wrong answer")]
    public AudioClip wrongSound;

    [Header("Shuffle Answers")]
    [Tooltip("Randomize the order of answer buttons")]
    public bool shuffleAnswers = true;

    // Internal state
    private EnergyObject currentObject;
    private PlayerInteraction playerInteraction;
    private UIManager uiManager;
    private AudioSource audioSource;
    private RectTransform quizPanelRect;
    private CanvasGroup quizPanelCanvasGroup;
    private bool isQuizOpen = false;
    private Coroutine currentAnimationCoroutine;

    // Answer tracking for shuffling
    private class AnswerData
    {
        public string text;
        public bool isCorrect;
        public int originalIndex;
    }

    void Start()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
            quizPanelRect = quizPanel.GetComponent<RectTransform>();
            quizPanelCanvasGroup = quizPanel.GetComponent<CanvasGroup>();
            if (quizPanelCanvasGroup == null)
            {
                quizPanelCanvasGroup = quizPanel.AddComponent<CanvasGroup>();
            }
        }

        if (tryAgainText != null)
        {
            tryAgainText.gameObject.SetActive(false);
        }

        if (correctText != null)
        {
            correctText.gameObject.SetActive(false);
        }

        playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        uiManager = FindFirstObjectByType<UIManager>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null && (quizOpenSound != null || correctSound != null || wrongSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupButtonColors();
    }

    /// <summary>
    /// Opens the quiz panel for the given energy object.
    /// </summary>
    public void OpenQuiz(
        EnergyObject energyObject,
        string question,
        string correctAnswer,
        string wrongAnswer1,
        string wrongAnswer2
    )
    {
        if (isQuizOpen) return;

        currentObject = energyObject;
        isQuizOpen = true;

        // Reset UI state
        ResetQuizUI();

        // Set question
        questionText.text = question;

        // Set category from object name
        if (questionCategoryText != null)
        {
            questionCategoryText.text = $"Repair: {energyObject.objectName}";
        }

        // Prepare and assign answers
        List<AnswerData> answers = new List<AnswerData>
        {
            new AnswerData { text = correctAnswer, isCorrect = true, originalIndex = 0 },
            new AnswerData { text = wrongAnswer1, isCorrect = false, originalIndex = 1 },
            new AnswerData { text = wrongAnswer2, isCorrect = false, originalIndex = 2 }
        };

        // Shuffle if enabled
        if (shuffleAnswers)
        {
            ShuffleList(answers);
        }

        // Assign to buttons
        answerText1.text = answers[0].text;
        answerText2.text = answers[1].text;
        answerText3.text = answers[2].text;

        // Set up button listeners based on shuffled answers
        answerButton1.onClick.RemoveAllListeners();
        answerButton2.onClick.RemoveAllListeners();
        answerButton3.onClick.RemoveAllListeners();

        answerButton1.onClick.AddListener(() => OnAnswerSelected(answers[0].isCorrect, answerButton1));
        answerButton2.onClick.AddListener(() => OnAnswerSelected(answers[1].isCorrect, answerButton2));
        answerButton3.onClick.AddListener(() => OnAnswerSelected(answers[2].isCorrect, answerButton3));

        // Show panel
        quizPanel.SetActive(true);

        // Play open sound
        if (quizOpenSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(quizOpenSound);
        }

        // Animate panel in
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimatePanelOpen());

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player interaction
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }
    }

    /// <summary>
    /// Closes the quiz panel and restores player control.
    /// </summary>
    public void CloseQuiz()
    {
        if (!isQuizOpen) return;

        isQuizOpen = false;

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimatePanelClose());
    }

    private void OnAnswerSelected(bool isCorrect, Button selectedButton)
    {
        // Disable all buttons to prevent double-clicking
        SetButtonsInteractable(false);

        if (isCorrect)
        {
            HandleCorrectAnswer(selectedButton);
        }
        else
        {
            HandleWrongAnswer(selectedButton);
        }
    }

    private void HandleCorrectAnswer(Button selectedButton)
    {
        // Highlight correct button
        SetButtonColor(selectedButton, correctButtonColor);

        // Show correct text
        if (correctText != null)
        {
            correctText.gameObject.SetActive(true);
            correctText.color = correctColor;
        }

        // Play correct sound
        if (correctSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(correctSound);
        }

        // Fix the object
        if (playerInteraction != null && currentObject != null)
        {
            playerInteraction.FixEnergyObjectFromQuiz(currentObject);
        }

        // Close after delay
        Invoke(nameof(CloseQuiz), correctAnswerDelay);
    }

    private void HandleWrongAnswer(Button selectedButton)
    {
        // Highlight wrong button
        SetButtonColor(selectedButton, wrongButtonColor);

        // Show try again text
        if (tryAgainText != null)
        {
            tryAgainText.gameObject.SetActive(true);
            tryAgainText.color = wrongColor;
        }

        // Play wrong sound
        if (wrongSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        // Notify player interaction
        if (playerInteraction != null)
        {
            playerInteraction.OnWrongAnswer();
        }

        // Close after delay
        Invoke(nameof(CloseQuiz), wrongAnswerDelay);
    }

    private void ResetQuizUI()
    {
        // Hide feedback texts
        if (tryAgainText != null)
        {
            tryAgainText.gameObject.SetActive(false);
        }

        if (correctText != null)
        {
            correctText.gameObject.SetActive(false);
        }

        // Reset button colors
        SetButtonColor(answerButton1, normalButtonColor);
        SetButtonColor(answerButton2, normalButtonColor);
        SetButtonColor(answerButton3, normalButtonColor);

        // Enable buttons
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButton1 != null) answerButton1.interactable = interactable;
        if (answerButton2 != null) answerButton2.interactable = interactable;
        if (answerButton3 != null) answerButton3.interactable = interactable;
    }

    private void SetupButtonColors()
    {
        ColorBlock cb = new ColorBlock
        {
            normalColor = normalButtonColor,
            highlightedColor = hoverButtonColor,
            pressedColor = correctButtonColor,
            selectedColor = hoverButtonColor,
            disabledColor = new Color(normalButtonColor.r, normalButtonColor.g, normalButtonColor.b, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = buttonColorDuration
        };

        if (answerButton1 != null) answerButton1.colors = cb;
        if (answerButton2 != null) answerButton2.colors = cb;
        if (answerButton3 != null) answerButton3.colors = cb;
    }

    private void SetButtonColor(Button button, Color color)
    {
        if (button == null) return;

        ColorBlock cb = button.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color;
        cb.selectedColor = color;
        button.colors = cb;

        // Also update the image directly for immediate feedback
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = color;
        }
    }

    private IEnumerator AnimatePanelOpen()
    {
        if (quizPanelRect == null) yield break;

        float elapsed = 0f;
        Vector3 targetScale = Vector3.one * panelOpenScale;

        quizPanelRect.localScale = Vector3.one * panelCloseScale;

        if (quizPanelCanvasGroup != null)
        {
            quizPanelCanvasGroup.alpha = 0f;
        }

        while (elapsed < panelAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / panelAnimationDuration;
            float smoothT = EaseOutBack(t);

            quizPanelRect.localScale = Vector3.Lerp(Vector3.one * panelCloseScale, targetScale, smoothT);

            if (quizPanelCanvasGroup != null)
            {
                quizPanelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            yield return null;
        }

        quizPanelRect.localScale = targetScale;

        if (quizPanelCanvasGroup != null)
        {
            quizPanelCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator AnimatePanelClose()
    {
        if (quizPanelRect == null)
        {
            FinishClose();
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = quizPanelRect.localScale;
        Vector3 targetScale = Vector3.one * panelCloseScale;

        while (elapsed < panelAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / panelAnimationDuration;
            float smoothT = EaseInQuad(t);

            quizPanelRect.localScale = Vector3.Lerp(startScale, targetScale, smoothT);

            if (quizPanelCanvasGroup != null)
            {
                quizPanelCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        FinishClose();
    }

    private void FinishClose()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player interaction
        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        currentObject = null;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseInQuad(float t) => t * t;
}
