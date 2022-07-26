using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance;

    private SpawnManager spawnManager;
    private UIManager uiManager;
    private AudioManager audioManager;
    private LightingController lightingController;
    private SettingsManager settings;

    private PlayerController player;
    private EndCutscene endCutscene;
    private int score, distance;
    private bool gameStarted;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(this);

        Reset();
        StartCoroutine(WaitForLeaderboard());
    }

    private IEnumerator WaitForLeaderboard()
    {
        while (LeaderboardManager.Instance == null)
            yield return new WaitForEndOfFrame();
        Initialize();
    }

    private async Task Initialize()
    {
        if (LeaderboardManager.Instance.CurrentUser == null)
        {
            await LeaderboardManager.Instance.Initialize();
        }

        audioManager = GameObject.FindObjectOfType<AudioManager>();
        lightingController = GameObject.FindObjectOfType<LightingController>();
        settings = GameObject.FindObjectOfType<SettingsManager>();
        
        audioManager.Initialize();
        lightingController.Initialize();
        settings.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            distance = player.GetDistanceTraveled();
            uiManager.UpdateDistance(distance);
            if (distance > 5f && !gameStarted)
            {
                TutorialManager tutorial = GameObject.FindObjectOfType<TutorialManager>();
                if (tutorial)
                    tutorial.TutorialComplete();
            }
        }
    }

    private void Reset()
    {
        TutorialManager tutorial = GameObject.FindObjectOfType<TutorialManager>();
        tutorial.tutorialComplete = new UnityEvent();
        tutorial.tutorialComplete.AddListener(BeginGame);
        gameStarted = false;

        uiManager = GameObject.FindObjectOfType<UIManager>();
        uiManager.Initialize();
        uiManager.UpdateScore(score);

        player = GameObject.FindObjectOfType<PlayerController>();
        player.Initialize();

        endCutscene = GameObject.FindObjectOfType<EndCutscene>();
    }

    private void BeginGame()
    {
        gameStarted = true;
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        spawnManager.BeginSpawning();
    }

    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance.UpdateScore(score);
    }

    public void GameOver()
    {
        SubmitScore();  // TODO: move this to a method accessible after entering a username
        LeaderboardManager.Instance.UpdatePlayerStats(player.ChildrenKicked, player.DemonsVanquished);
        endCutscene.complete = new UnityEvent();
        endCutscene.complete.AddListener(End);
        endCutscene.Play();
        spawnManager.StopSpawning();
    }

    public void SubmitScore()
    {
        LeaderboardManager.Instance.SubmitScore("vpn", distance, score);
    }

    private void End()
    {
        UIManager.Instance.ShowEndScreen();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("title");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
