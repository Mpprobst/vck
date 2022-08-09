using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Text = TMPro.TextMeshProUGUI;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] Button startButton, infoButton, quitButton, infoBackButon, signInButton;
    [SerializeField] GameObject infoPanel;
    [SerializeField] TMP_InputField usernameInput, passwordInput;
    [SerializeField] Text loginMessage;


    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        infoButton.onClick.AddListener(OpenInfo);
        infoBackButon.onClick.AddListener(CloseInfo);
        quitButton.onClick.AddListener(QuitGame);
        PlayerDetails.Instance.loginAttemptEvent = new UnityEvent<bool>();
        PlayerDetails.Instance.loginAttemptEvent.AddListener(SignInAttempt);
        usernameInput.onEndEdit.AddListener(PlayerDetails.Instance.SetUsername);
        passwordInput.onEndEdit.AddListener(PlayerDetails.Instance.SetPassword);
        signInButton.onClick.AddListener(PlayerDetails.Instance.SignIn);
        CloseMessage();
        CloseInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartGame()
    {
        SceneManager.LoadScene("testing");
    }

    private void OpenInfo()
    {
        infoPanel.gameObject.SetActive(true);
    }

    private void CloseInfo()
    {
        infoPanel.SetActive(false);
    }

    private void SignInAttempt(bool success)
    {
        loginMessage.text = "Login " + (success ? "successful" : "invalid");
        loginMessage.color = success ? Color.green : Color.red;
        Invoke("CloseMessage", 2f);
    }

    private void CloseMessage()
    {
        loginMessage.text = "";
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
