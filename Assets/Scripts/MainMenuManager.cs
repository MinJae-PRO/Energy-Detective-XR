using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject introPanel;

    public TMP_Dropdown displayModeDropdown;
    public TMP_Dropdown resolutionDropdown;

    public TextMeshProUGUI forwardKeyText;
    public TextMeshProUGUI backKeyText;
    public TextMeshProUGUI leftKeyText;
    public TextMeshProUGUI rightKeyText;

    private KeyCode keyWaitingFor = KeyCode.None;
    private Resolution[] resolutions;

    void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        introPanel.SetActive(false);

        SetupDisplayModeDropdown();
        SetupResolutionDropdown();
        UpdateKeyTexts();
    }

    void Update()
    {
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
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        introPanel.SetActive(false);
    }

    public void OpenIntroduction()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        introPanel.SetActive(true);
    }

    public void ClosePanels()
    {
        settingsPanel.SetActive(false);
        introPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }

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

    public void ChangeForwardKey()
    {
        keyWaitingFor = KeyCode.W;
        forwardKeyText.text = "Press new key";
    }

    public void ChangeBackKey()
    {
        keyWaitingFor = KeyCode.S;
        backKeyText.text = "Press new key";
    }

    public void ChangeLeftKey()
    {
        keyWaitingFor = KeyCode.A;
        leftKeyText.text = "Press new key";
    }

    public void ChangeRightKey()
    {
        keyWaitingFor = KeyCode.D;
        rightKeyText.text = "Press new key";
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
        forwardKeyText.text = "Forward Key " + PlayerKeySettings.ForwardKey;
        backKeyText.text = "Back Key " + PlayerKeySettings.BackKey;
        leftKeyText.text = "Left Key " + PlayerKeySettings.LeftKey;
        rightKeyText.text = "Right Key " + PlayerKeySettings.RightKey;
    }
}