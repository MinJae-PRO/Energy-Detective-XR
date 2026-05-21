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

    private EnergyObject currentObject;
    private PlayerInteraction playerInteraction;

    void Start()
    {
        quizPanel.SetActive(false);
        tryAgainText.SetActive(false);

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
        if (playerInteraction != null && currentObject != null)
        {
            playerInteraction.FixEnergyObjectFromQuiz(currentObject);
        }

        CloseQuiz();
    }

    void WrongAnswer()
    {
        StartCoroutine(ShowTryAgain());
    }

    IEnumerator ShowTryAgain()
    {
        tryAgainText.SetActive(true);

        yield return new WaitForSeconds(1.5f);

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
    }
}