using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Firebase.Database;
//using Firebase.Auth;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography;
using System.Linq;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;
using FirebaseWebGL.Scripts.FirebaseBridge;

[System.Serializable]
public class UserData
{
    public UserSettings settings;
    public ScoreData[] profiles;
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

    [SerializeField] private Text totalKickText, totalKillText, maxDistText, maxScoreText;
    [SerializeField] private Transform profilesPanel;
    private List<LeaderboardElement> profileElements;

    //FirebaseDatabase _database;
    //FirebaseAuth _auth;
    private string playerKey = "F**KFACE";
    private string ip = "";

    public UserData CurrentUser { get { return currentUser; } }
    private UserData currentUser;
    private bool initialized;
    private string myUsername;

    private string firebaseRoot = "https://vancouverck-3919f-default-rtdb.firebaseio.com/";
    private string adminUser = "admin@vck.org";
    private string adminKey = "tH!sI$Ahe!n0u$@uthKeYG00DLucKf**k3r$";

    private List<ScoreData> highscores;
    private Transform leaderboardPanel;
    private GameObject leaderboardElementPrefab;
    private List<LeaderboardElement> leaderboardElements;
    private bool sortMethod;

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            if (!initialized) Initialize();
        }
    }

    public async Task Initialize()
    {
        initialized = true;
        //_auth = FirebaseAuth.DefaultInstance;
        FirebaseAuth.SignInWithEmailAndPassword(adminUser, adminKey, name, "AuthLoginCallback", "");
        // This should prevent outside users from modifying the database
        // TODO: do something if admin login fails. Just disable all features and don't try to query

        //_database = FirebaseDatabase.DefaultInstance;
        string hostName = Dns.GetHostName();
        ip = Dns.GetHostEntry(hostName).AddressList[0].ToString();
        GenerateKey();

        bool newUser = false;
        if (currentUser == null)
        {
            //currentUser = GetUserData();
            UpdateUserData();
            while (currentUser == null)
                await Task.Delay(0);
        }
        if (currentUser != null)
        {
            if (!currentUser.authenticated)
            {
                newUser = true;
            }
        }
        else
        {
            newUser = true;
        }

        if (newUser)
        {
            currentUser = new UserData();
            currentUser.authenticated = true;
            FirebaseDatabase.PostJSON(firebaseRoot+playerKey, JsonUtility.ToJson(currentUser), name, "DatabasePostCallback", "");
            //await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        }

        //_auth.SignOut();
    }
    #region FBWebGL
    private void AuthLoginCallback(string output)
    {
        Debug.Log("Login callback message: "+output);
    }

    private void UpdateUserData()
    {
        FirebaseDatabase.GetJSON(playerKey, name, "DatabaseGetCallback", "");
    }
    
    private void DatabaseGetUserCallback(string output)
    {
        UserData user = JsonUtility.FromJson<UserData>(output);
    }

    private void DatabasePostCallback(string output)
    {
        Debug.Log("Post callback: " + output);
    }

    public void GetHighscores()
    {
        FirebaseDatabase.GetJSON(firebaseRoot, name, "DatabaseGetScoresCallback", "");
    }

    private void DatabaseGetScoresCallback(string output)
    {
        var dbData = JsonUtility.FromJson<object>(output);
        var allUsers = JsonUtility.FromJson<List<UserData>>(output);
        highscores = new List<ScoreData>();
        foreach (var user in allUsers)
        {
            foreach (var score in user.profiles)
                if (score != null)
                    highscores.Add(score);
        }
        /*foreach (var child in dbData.Children)
        {
            UserData user = JsonUtility.FromJson<UserData>(child.GetRawJsonValue());
            var json = child.GetRawJsonValue();
            foreach (var score in user.profiles)
                if (score != null)
                    highscores.Add(score);
        }*/
    }
    #endregion
    /*private async Task<UserData> GetUserData()
    {
        var userEntry = await _database.GetReference(playerKey).GetValueAsync();
        UserData user = JsonUtility.FromJson<UserData>(userEntry.GetRawJsonValue());
        return user;
    }*/

    /*public async Task<List<ScoreData>> GetHighscores()
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
        //_auth.SignOut();
        return highscores;
    }*/

    public List<ScoreData> OrderHighScores(List<ScoreData> highscores, bool byScore = true)
    {
        if (byScore) return highscores.OrderByDescending(h => h.score).ToList();
        else return highscores.OrderByDescending(h => h.distance).ToList();
    }

    public void UpdatePlayerStats(int childrenKicked, int demonsVanquished)
    {
        currentUser.childrenKicked += childrenKicked;
        currentUser.demonsVanquished += demonsVanquished;
    }

    public async Task<int> SetUsername(string user)
    {
        int id = -1;

        if (user.Length > maxKeyLength)
        {
            ReportWarning($"name exceeds maximum length ({maxKeyLength})");
            return -1;
        }

        Regex whitespace = new Regex(@"[-_.\s]");
        string cleanedUsername = whitespace.Replace(user, "");
        Regex replaceA = new Regex(@"[4@]");
        cleanedUsername = replaceA.Replace(cleanedUsername, "a");
        Regex replaceE = new Regex(@"[3]");
        cleanedUsername = replaceE.Replace(cleanedUsername, "e");
        Regex replaceI = new Regex(@"[!1]");
        cleanedUsername = replaceI.Replace(cleanedUsername, "i");
        Regex replaceO = new Regex(@"[0]");
        cleanedUsername = replaceO.Replace(cleanedUsername, "o");
        Regex replaceH = new Regex(@"[#]");
        cleanedUsername = replaceH.Replace(cleanedUsername, "h");
        Regex replaceS = new Regex(@"[$5]");
        cleanedUsername = replaceS.Replace(cleanedUsername, "s");
        Regex replaceG = new Regex(@"[96]");
        cleanedUsername = replaceG.Replace(cleanedUsername, "g");

        Debug.Log($"\'{user}\' cleaned: \'{cleanedUsername}\'");

        string invalidChars = @"[?/\|:;]";
        Regex charCheck = new Regex(invalidChars);
        if (charCheck.IsMatch(cleanedUsername))
        {
            ReportWarning($"Illegal characters found in \'{user}\'");
            return -1;
        }

        string[] offensiveWords = { "d(i|y)ke", "fag+(s|ot|y|ier)", "gooks", "kikes", "ni?g+(a|er)", "shemale", "spick", "beaner", "trann(ie|y)", "hooknose", "cunt", "pussy" };
        string pattern = @"(";
        foreach (var slur in offensiveWords) pattern += $"{slur}|";
        pattern = pattern.Substring(0, pattern.Length - 1);
        pattern += ")";

        Regex offensiveRegex = new Regex(pattern, RegexOptions.IgnoreCase);
        if (offensiveRegex.IsMatch(cleanedUsername))
        {
            ReportWarning($"Go fuck yourself");
            return -1;
        }

        Regex fuckface = new Regex(@"f+u+c+k+f+a+c+e");
        //if (cleanedUsername.Contains("fuckface"))
        if (fuckface.IsMatch(cleanedUsername))
        {
            int censorQueue = 0;
            string censoredFuckName = "";
            for (int i = 0; i < user.Length; i++)
            {
                if (censorQueue > 0)
                {
                    censoredFuckName += '*';
                    censorQueue--;
                }
                else
                    censoredFuckName += user[i];

                if (user[i] == 'f' || user[i] == 'F')
                {
                    for (int f = i+1; f < user.Length; f++)
                    {
                        if (user[f] == 'k' || user[f] == 'K')
                        {
                            break;
                        }
                        else if (f == user.Length - 1 || f > 6)
                        {
                            censorQueue = -1;
                        }
                        censorQueue++;
                    }
                }
            }
            user = censoredFuckName;
        }

        highscores = null;
        GetHighscores();
        while (highscores == null)
            await Task.Delay(0);
        //var highscores = await GetHighscores();
        for (int i = 0; i < highscores.Count; i++)
        {
            for (int j = 0; j < currentUser.profiles.Length; j++)
                if (currentUser.profiles[j].name == user)
                    return j;
            if (user == highscores[i].name && id < 0)
            {
                ReportWarning($"Cannot use {user}. It is already in use");
                return -1;
            }
        }

        // username is new, choose next available slot in profiles
        id = currentUser.profiles.Length;
        if (id >= maxProfiles)
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
            id = availableInts[0];
            currentUser.profiles[id] = new ScoreData();
        }
        else // add new profile
        {
            ScoreData[] scores = currentUser.profiles;
            scores = new ScoreData[currentUser.profiles.Length + 1];
            for (int i = 0; i < currentUser.profiles.Length; i++)
                scores[i] = currentUser.profiles[i];

            currentUser.profiles = scores;
        }
        if (playerKey == "")
            GenerateKey();
        myUsername = user;
        return id;
    }

    public async void SubmitScore(string name, int dist, int score)
    {
        int profileIdx = await SetUsername(name);
        if (0 > profileIdx) return;
        //var fireuser = await _auth.SignInWithEmailAndPasswordAsync(adminUser, adminKey);

        ScoreData scoreEntry = currentUser.profiles[profileIdx];
        if (scoreEntry == null) scoreEntry = new ScoreData();
        scoreEntry.name = myUsername;
        if (dist > scoreEntry.distance) scoreEntry.distance = dist;
        if (score > scoreEntry.score) scoreEntry.score = score;
        currentUser.profiles[profileIdx] = scoreEntry;

        SettingsManager settings = SettingsManager.Instance;
        currentUser.settings.master = settings.masterVol.value;
        currentUser.settings.music = settings.musicVol.value;
        currentUser.settings.ambience = settings.ambienceVol.value;
        currentUser.settings.sfx = settings.sfxVol.value;
        currentUser.settings.brightness = settings.brightnessSlider.value;
        Debug.Log($"Submiting score {scoreEntry.name} - dist: {scoreEntry.distance} score: {scoreEntry.score}");
        FirebaseDatabase.UpdateJSON(firebaseRoot + playerKey, JsonUtility.ToJson(currentUser), name, "", "");
        //await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        //_auth.SignOut();

        await PopulateLeaderboard(leaderboardPanel, leaderboardElementPrefab, sortMethod);
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

    private void ReportWarning(string message)
    {
        UIManager.Instance.ShowWarning(message);
        Debug.LogWarning(message);
    }

    public async Task PopulateLeaderboard(Transform panel, GameObject prefab, bool orderByScore)
    {
        Debug.Log("populating leaderboard");
        if (panel == null || prefab == null) return;
        sortMethod = orderByScore;
        leaderboardPanel = panel;
        leaderboardElementPrefab = prefab;

        if (profileElements != null)
            foreach (var prof in profileElements)
                if (prof)
                    Destroy(prof.gameObject);
        profileElements = new List<LeaderboardElement>();

        // Show user stats
        int bestDist = 0;
        int bestScore = 0;
        int distIdx = 0;
        int scoreIdx = 0;
        for (int i = 0; i < currentUser.profiles.Length; i++)
        {
            var element = Instantiate(leaderboardElementPrefab, profilesPanel).GetComponent<LeaderboardElement>();
            var prof = currentUser.profiles[i];
            element.Initialize(i + 1, prof.name, $"<b>{prof.distance}m</b> - <b>{prof.score}</b>");
            profileElements.Add(element);
            if (prof.distance > bestDist)
            {
                distIdx = i;
                bestDist = prof.distance;
            }
            if (prof.score > bestScore)
            {
                scoreIdx = i;
                bestScore = prof.score;
            }
        }
        totalKickText.text = currentUser.childrenKicked.ToString();
        totalKillText.text = currentUser.demonsVanquished.ToString();

        maxDistText.text = bestDist.ToString();
        maxScoreText.text = bestScore.ToString();

        // Populate the leaderboard
        if (leaderboardElements != null)
            foreach (var element in leaderboardElements)
                if (element != null)
                    Destroy(element.gameObject);
        leaderboardElements = new List<LeaderboardElement>();

        int profileIdx = orderByScore ? scoreIdx : distIdx;
        string profileName = currentUser.profiles[profileIdx].name;

        highscores = null;
        GetHighscores();
        while (highscores == null)
            await Task.Delay(0);
        //var highscores = await GetHighscores();
        int top = 5;
        int playerRank = -1;
        highscores = OrderHighScores(highscores, orderByScore);
        for (int i = 0; i < highscores.Count; i++)
        {
            if (highscores[i].name == profileName)
                playerRank = i;
            if (i < top)
            {
                var score = highscores[i];
                var element = Instantiate(prefab, panel).GetComponent<LeaderboardElement>();
                element.Initialize(i + 1, score.name, orderByScore ? score.score.ToString() : score.distance.ToString() + "m", playerRank == i);
                leaderboardElements.Add(element);
            }
        }

        if (playerRank >= top)
        {
            int prev = playerRank - 1;
            if (prev < top)
            {
                var elip = Instantiate(prefab, panel).GetComponent<LeaderboardElement>();
                elip.Initialize(-1, "...", "");
                leaderboardElements.Add(elip);
            }
            for (int i = playerRank - 1; i <= playerRank + 1; i++)
            {
                if (i < highscores.Count)
                {
                    var score = highscores[i];
                    var element = Instantiate(prefab, panel).GetComponent<LeaderboardElement>();
                    element.Initialize(i + 1, score.name, orderByScore ? score.score.ToString() : score.distance.ToString() + "m", i == playerRank);
                    leaderboardElements.Add(element);
                }
            }
        }

        if (playerRank != highscores.Count - 1)
        {
            var elipses = Instantiate(prefab, panel).GetComponent<LeaderboardElement>();
            elipses.Initialize(-1, "...", "");
            leaderboardElements.Add(elipses);

            var worstScore = highscores[highscores.Count - 1];
            var worstElement = Instantiate(prefab, panel).GetComponent<LeaderboardElement>();
            worstElement.Initialize(highscores.Count, worstScore.name, orderByScore ? worstScore.score.ToString() : worstScore.distance.ToString() + "m");
            leaderboardElements.Add(worstElement);
        }
    }

    private void OnApplicationQuit()
    {
        //_auth.SignOut();
    }
}
