using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
        Color normal = new Color(0.15f, 0.25f, 0.4f, 1f);
        SetButtonColor(answerButton1, normal);
        SetButtonColor(answerButton2, normal);
        SetButtonColor(answerButton3, normal);
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (answerButton1 != null) answerButton1.interactable = interactable;
        if (answerButton2 != null) answerButton2.interactable = interactable;
        if (answerButton3 != null) answerButton3.interactable = interactable;
    }
}
