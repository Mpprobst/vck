using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get { return _instance; } }
    private static DifficultyManager _instance;

    public float Difficulty { get { return _diff; } }
    private float _diff = 1f;

    public float maxDiff = 3f;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance == null)
            _instance = this;
        if (_instance != this)
            Destroy(gameObject);

        player = GameObject.FindObjectOfType<PlayerController>();
        InvokeRepeating("UpdateDifficulty", 2.5f, 2.5f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateDifficulty()
    {
        if (player == null) return;
        int distance = player.GetDistanceTraveled() / 10;
        // make it harder every 10m
        _diff = 1f + (float)distance / 10f;
        if (_diff > maxDiff) _diff = maxDiff;
    }


}
