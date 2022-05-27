using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get { return _instance; } }
    private static GameManager _instance;

    private SpawnManager spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        GameManager[] gameManagers = GameObject.FindObjectsOfType<GameManager>();
        if (gameManagers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        spawnManager = GameObject.FindObjectOfType<SpawnManager>();
        spawnManager.BeginSpawning();   // TODO: maybe start spawning after a certain distance has been traveled
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GameOver()
    {
        UIManager.Instance.ShowEndScreen();
    }
}
