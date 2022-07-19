using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] Button startButton, infoButton, quitButton, infoBackButon;
    [SerializeField] GameObject infoPanel;

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
        infoButton.onClick.AddListener(OpenInfo);
        infoBackButon.onClick.AddListener(CloseInfo);
        quitButton.onClick.AddListener(QuitGame);
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

    private void QuitGame()
    {
        Application.Quit();
    }
}
