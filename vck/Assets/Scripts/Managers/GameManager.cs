using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance;

    private SpawnManager spawnManager;
    private UIManager uiManager;
    private PlayerController player;
    private int score;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(this);

        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Reset()
    {
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        spawnManager.BeginSpawning();   // TODO: maybe start spawning after a certain distance has been traveled

        uiManager = GameObject.FindObjectOfType<UIManager>();
        uiManager.Initialize();
        uiManager.UpdateScore(score);

        player = GameObject.FindObjectOfType<PlayerController>();
        player.Initialize();
    }

    public void AddScore(int points)
    {
        score += points;
        UIManager.Instance.UpdateScore(score);
    }

    public void GameOver()
    {
        UIManager.Instance.ShowEndScreen();
        spawnManager.StopSpawning();
    }

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
