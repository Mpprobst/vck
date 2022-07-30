using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get { return _instance; } }
    private static UIManager _instance;

    [SerializeField] private Text score, distance, endScore, endDist, warningText, distOrScoreText;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Button retryButton, quitButton, submitScoreButton;
    [SerializeField] private Transform health, leaderboardContent;  // must have horiz layout
    [SerializeField] private GameObject healthPrefab, leaderboardElementPrefab;
    [SerializeField] private TMPro.TMP_InputField usernameInput;
    [SerializeField] private Toggle scoreDistToggle;
    private List<GameObject> healthIcons;

    private PlayerController player;

    private void Start()
    {
        //Initialize();
    }

    public void Initialize()
    {
        if (_instance == null)
            _instance = this;

        submitScoreButton.onClick = new Button.ButtonClickedEvent();
        submitScoreButton.onClick.AddListener(SubmitScorePressed);
        scoreDistToggle.onValueChanged = new Toggle.ToggleEvent();
        scoreDistToggle.onValueChanged.AddListener(LoadLeaderboard);

        Reset();
    }

    public void ShowEndScreen()
    {
        endPanel.SetActive(true);
        Animator endScreenAnimator = endPanel.GetComponent<Animator>();
        // TODO: animate end screen
        endScore.text = score.text;
        endDist.text = distance.text;
        LoadLeaderboard(false);
    }

    public void LoadLeaderboard(bool orderByScore)
    {
        LeaderboardManager.Instance.PopulateLeaderboard(leaderboardContent, leaderboardElementPrefab, orderByScore);
        distOrScoreText.text = orderByScore ? "SCORE" : "DIST";
    }

    public void Reset()
    {
        ShowWarning("");
        endPanel.SetActive(false);
        player = GameObject.FindObjectOfType<PlayerController>();
    }

    public void UpdateScore(int points)
    {
        // TODO: animate score in a fun way
        score.text = "score: " + points.ToString();
    }

    public void UpdateDistance(int dist)
    {
        // TODO: every 10 or so meters, flash the distance in a fun way
        distance.text = "distance: " + dist.ToString() + "m";
    }

    public void SetHealth(int hp)
    {
        if (healthIcons != null)
        {
            for (int i = 0; i < healthIcons.Count; i++)
                RemoveHP();
        }

        healthIcons = new List<GameObject>();
        for (int i = 0; i < hp; i++)
        {
            GameObject icon = Instantiate(healthPrefab, health);
            healthIcons.Add(icon);
        }
    }

    public void RemoveHP()
    {
        if (healthIcons.Count == 0) return;
        Destroy(healthIcons[healthIcons.Count - 1]);
        healthIcons.RemoveAt(healthIcons.Count - 1);
    }

    private void SubmitScorePressed()
    {
        if (usernameInput.text != "")
            GameManager.Instance.SubmitScore(usernameInput.text);
    }

    public void ShowWarning(string message)
    {
        StartCoroutine(WarningFlash(message));
    }

    private IEnumerator WarningFlash(string message)
    {
        warningText.text = message;
        warningText.color = Color.red;
        yield return new WaitForSecondsRealtime(2f);
        warningText.text = "";
        warningText.color = Color.black;
    }
}
