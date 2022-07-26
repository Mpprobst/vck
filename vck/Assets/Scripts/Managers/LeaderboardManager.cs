using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;
using System.Linq;


[System.Serializable]
public class UserData
{
    public UserSettings settings;
    public ScoreData[] profiles;    // max 3
    public int childrenKicked, demonsVanquished;
    public bool authenticated;
    public bool HasEntries()
    {
        return profiles.Length > 0;
    }
    public UserData()
    {
        settings = new UserSettings();
        profiles = new ScoreData[0];
        authenticated = false;
    }
}

[System.Serializable]
public class ScoreData
{
    public string name = "fucker";
    public int distance = 0;
    public int score = -3000;
}

[System.Serializable]
public class UserSettings
{
    public float master, music, ambience, sfx, brightness; // settings values
    public UserSettings()
    {
        master = 1;
        music = 1;
        ambience = 1;
        sfx = 1;
        brightness = 1;
    }
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get { return _instance; } }
    private static LeaderboardManager _instance;
    
    public int maxEntries = 1000;
    public int maxKeyLength = 16;
    public int maxProfiles;

    FirebaseDatabase _database;
    FirebaseAuth _auth;
    private string playerKey = "F**KFACE";
    private string ip = "";
    private string username;

    public UserData CurrentUser { get { return currentUser; } }
    private UserData currentUser;
    private bool initialized;

    private string adminUser = "admin@vck.org";
    private string adminKey = "tH!sI$Ahe!n0u$@uthKeYG00DLucKf**k3r$";

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task Initialize()
    {
        initialized = true;
        _auth = FirebaseAuth.DefaultInstance;
        await _auth.SignInWithEmailAndPasswordAsync(adminUser, adminKey);
        // This should prevent outside users from modifying the database
        // TODO: do something if admin login fails. Just disable all features and don't try to query

        _database = FirebaseDatabase.DefaultInstance;
        string hostName = Dns.GetHostName();
        ip = Dns.GetHostEntry(hostName).AddressList[0].ToString();
        GenerateKey();

        bool newUser = true;
        currentUser = await GetUserData();
        if (currentUser != null )
        {
            if (!currentUser.authenticated)
            {
                newUser = false;
            }
        }

        if (newUser)
        {
            currentUser = new UserData();
            await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        }

        _auth.SignOut();
    }

    private async Task<UserData> GetUserData()
    {
        var userEntry = await _database.GetReference(playerKey).GetValueAsync();
        UserData user = JsonUtility.FromJson<UserData>(userEntry.GetRawJsonValue());
        return user;
    }

    public async Task<List<ScoreData>> GetHighscores()
    {
        await _auth.SignInWithEmailAndPasswordAsync(adminUser, adminKey);
        List<ScoreData> highscores = new List<ScoreData>();
        var dbData = await _database.RootReference.GetValueAsync();
        foreach (var child in dbData.Children)
        {
            UserData user = JsonUtility.FromJson<UserData>(child.GetRawJsonValue());
            var json = child.GetRawJsonValue();
            foreach (var score in user.profiles)
                if (score != null)
                    highscores.Add(score);
        }
        _auth.SignOut();
        return highscores;
    }

    public List<ScoreData> OrderHighScores(List<ScoreData> highscores, bool byScore = true)
    {
        if (byScore) return highscores.OrderBy(h => h.score).ToList();
        else return highscores.OrderBy(h => h.distance).ToList();
    }

    public async void UpdatePlayerStats(int childrenKicked, int demonsVanquished)
    {
        // get lifetime stats and add these values to them
        currentUser.childrenKicked += childrenKicked;
        currentUser.demonsVanquished += demonsVanquished;
        //await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
    }

    public async void SubmitScore(string name, int dist, int score)
    {
        if (!await SetUsername(name)) return;
        var fireuser = await _auth.SignInWithEmailAndPasswordAsync(adminUser, adminKey);

        int profileIdx = currentUser.profiles.Length;
        currentUser.authenticated = true;
        if (currentUser.profiles.Length >= maxProfiles)
        {
            int bestDist = 0;
            int bestScore = 0;
            int distIdx = 0;
            int scoreIdx = 0;
            for (int i = 0; i < currentUser.profiles.Length; i++)
            {
                if (currentUser.profiles[i].distance > bestDist)
                {
                    bestDist = currentUser.profiles[i].distance;
                    distIdx = i;
                }
                if (currentUser.profiles[i].score > bestScore)
                {
                    bestScore = currentUser.profiles[i].score;
                    scoreIdx = i;
                }
            }
            List<int> availableInts = new List<int>();
            availableInts.Add(0);
            availableInts.Add(1);
            availableInts.Add(2);
            availableInts.Remove(distIdx);
            availableInts.Remove(scoreIdx);
            profileIdx = availableInts[0];
        }
        else
        {
            ScoreData[] scores = new ScoreData[currentUser.profiles.Length + 1];
            for (int i = 0; i < currentUser.profiles.Length; i++)
                scores[i] = currentUser.profiles[i];
            currentUser.profiles = scores;
        }

        ScoreData scoreEntry = currentUser.profiles[profileIdx];
        if (scoreEntry == null) scoreEntry = new ScoreData();
        scoreEntry.name = username;
        if (dist > scoreEntry.distance) scoreEntry.distance = dist;
        if (score > scoreEntry.score) scoreEntry.score = score;
        currentUser.profiles[profileIdx] = scoreEntry;

        SettingsManager settings = GameObject.FindObjectOfType<SettingsManager>();
        currentUser.settings.master = settings.masterVol.value;
        currentUser.settings.music = settings.musicVol.value;
        currentUser.settings.ambience = settings.ambienceVol.value;
        currentUser.settings.sfx = settings.sfxVol.value;
        currentUser.settings.brightness = settings.brightnessSlider.value;
        Debug.Log($"Submiting score {scoreEntry.name} - dist: {scoreEntry.distance} score: {scoreEntry.score}");
        await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        _auth.SignOut();
    }

    private static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    private void GenerateKey()
    {
        string hash = GetHashString(ip);
        playerKey = hash;
    }

    public async Task<bool> SetUsername(string user)
    {
        bool valid = true;
        if (user.Contains('/')) valid = false;
        if (user.Contains('\\')) valid = false;
        if (user.Contains('%')) valid = false;
        if (user.Contains('&')) valid = false;

        if (!valid)
        {
            Debug.LogWarning($"{user} has illegal characters");
            return false;
        }

        if (user.Length > maxKeyLength)
        {
            Debug.LogWarning($"{user} exceeds maximum length ({maxKeyLength})");
            return false;
        }

        var highscores = await GetHighscores();
        for (int i = 0; i < highscores.Count; i++)
        {
            bool usersName = false;
            foreach (var profile in currentUser.profiles)
                if (profile.name == user)
                    usersName = true;
            if (user == highscores[i].name && !usersName)
            {
                Debug.LogWarning($"Cannot use {user}. It is already in use");
                return false;
            }
        }
        username = user;
        if (playerKey == "")
            GenerateKey();

        return true;
    }

    private void OnApplicationQuit()
    {
        _auth.SignOut();
    }
}
