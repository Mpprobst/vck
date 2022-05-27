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

    // Start is called before the first frame update
    void Start()
    {
        UIManager[] uimanagers = GameObject.FindObjectsOfType<UIManager>();
        if (uimanagers.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        _instance = this;

        endPanel.SetActive(false);
        player = GameObject.FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDistance(player.GetDistanceTraveled());
    }

    public void ShowEndScreen()
    {
        endPanel.SetActive(true);
        Animator endScreenAnimator = endPanel.GetComponent<Animator>();
        // TODO: animate end screen
    }

    public void UpdateScore(int points)
    {
        score.text = "score: " + points.ToString();
    }

    public void UpdateDistance(int dist)
    {
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
