using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class ScoreData
{
    public string name;
    public int distance;
    public int score = -3000;
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get { return _instance; } }
    private static LeaderboardManager _instance;
    
    public int maxEntries = 1000;
    public int maxKeyLength = 16;

    FirebaseDatabase _database;
    private string playerKey = "F**KFACE";
    private List<string> sessionKeys;

    // Start is called before the first frame update
    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _database = FirebaseDatabase.DefaultInstance;
            sessionKeys = new List<string>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void SubmitScore(string name, int dist, int score)
    {
        if (!await SetUsername(name)) return;

        // get the score with that name
        ScoreData highscore = new ScoreData();
        highscore.name = playerKey;
        var highscoreEntry = await _database.GetReference(playerKey).GetValueAsync();
        if (highscoreEntry.Exists)
        {
            highscore = JsonUtility.FromJson<ScoreData>(highscoreEntry.GetRawJsonValue());
        }
        if (dist > highscore.distance) highscore.distance = dist;
        if (score > highscore.score) highscore.score = score;

        await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(highscore));
    }

    public async Task<bool> SetUsername(string username)
    {
        Regex r = new Regex("[^A-Z0-9.$ ]$");
        if (!r.IsMatch(username))
        {
            Debug.LogWarning($"{username} has illegal characters");
            return false;
        }

        if (username.Length > maxKeyLength) 
        {
            Debug.LogWarning($"{username} exceeds maximum length ({maxKeyLength})");
            return false;
        }

        if (sessionKeys.Contains(username))
        {
            playerKey = username;
        }
        else
        {
            if (!await KeyExists(username))
            {
                playerKey = username;
                sessionKeys.Add(username);
            }
            else
            {
                Debug.LogWarning($"Cannot use {username}. It is already in use");
                return false;
            }
        }
        return true;
    }

    private async Task<bool> KeyExists(string key)
    {
        var dbSnapshot = await _database.GetReference(key).GetValueAsync();
        return dbSnapshot.Exists;
    }
}
