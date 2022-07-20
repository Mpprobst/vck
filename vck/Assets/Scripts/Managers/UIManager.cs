using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get { return _instance; } }
    private static UIManager _instance;

    [SerializeField] private Text score;
    [SerializeField] private Text distance;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private Button retryButton, quitButton;
    [SerializeField] private Transform health;  // must have horiz layout
    [SerializeField] private GameObject healthPrefab;
    private List<GameObject> healthIcons;

    private PlayerController player;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(this);

        Reset();
    }

    public void ShowEndScreen()
    {
        endPanel.SetActive(true);
        Animator endScreenAnimator = endPanel.GetComponent<Animator>();
        // TODO: animate end screen
    }

    public void Reset()
    {
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
}
