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
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

[System.Serializable]
public class SessionWrapper
{
    string name;
    public List<UserData> userData;
    public SessionWrapper()
    {
        userData = new List<UserData>();
    }
}

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
    private string playerKey = "";
    private string ip = "";

    public UserData CurrentUser { get { return currentUser; } }
    private UserData currentUser;
    private bool initialized;
    private string myUsername;
    private int myScore, myDist;
    private bool sortMethod;

    private string firebaseRoot = "users/";
    //private string firebaseRoot = "https:\/\/vancouverck-3919f-default-rtdb.firebaseio.com\/";
    private string adminUser = "admin@vck.org";
    private string adminKey = "tH!sI$Ahe!n0u$@uthKeYG00DLucKf**k3r$";

    private List<ScoreData> highscores;
    private Transform leaderboardPanel;
    private GameObject leaderboardElementPrefab;
    private List<LeaderboardElement> leaderboardElements;

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            if (!initialized) Initialize();
        }
    }

    public void Initialize()
    {
        initialized = true;
        //_auth = FirebaseAuth.DefaultInstance;
        FirebaseAuth.SignInWithEmailAndPassword(adminUser, adminKey, name, "AuthLoginCallback", "AuthLoginFallback");
        // This should prevent outside users from modifying the database
        // TODO: do something if admin login fails. Just disable all features and don't try to query

        //_database = FirebaseDatabase.DefaultInstance;
       
        ip = Random.Range(0, 1000000000).ToString();
        if (playerKey == "")
            GenerateKey();

        if (currentUser == null)
        {
            //currentUser = GetUserData();
            string path = firebaseRoot + playerKey;
            Debug.Log("Getting from firebase path: " + path);
            FirebaseDatabase.GetJSON(path, name, "InitializeUser", "CreateNewUser");//"DatabaseGetUserFallback");
            // TODO: look through each session and find the users name
        }
        
    }
    #region FirebaseBridgeCalllbacks
    private void InitializeUser(string output)
    {
        Debug.Log("Get User callback: " + output);
        if (output != null && output != "null")
        {
            /*JToken data = JsonConvert.DeserializeObject<JToken>(output);
            foreach (var session in data.Children())
            {
                foreach (var child in session.Children())
                {
                    var user = JsonUtility.FromJson<UserData>(child.ToString());
                    if (user != null)
                    { 
                        foreach (var profile in user.profiles)
                        {
                            if (profile.name == myUsername)
                            {
                                currentUser = user;
                                break;
                            }
                        }
                    }
                    if (currentUser != null)
                        break;
                }
                if (currentUser != null)
                    break;
            }*/
            currentUser = JsonUtility.FromJson<UserData>(output);
        }
        bool newUser = false;
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
            CreateNewUser("");
            //await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        }

        //_auth.SignOut();
    }

    private void CreateNewUser(string output)
    {
        currentUser = new UserData();
        currentUser.authenticated = true;
        FirebaseDatabase.PushJSON(firebaseRoot + playerKey, JsonUtility.ToJson(currentUser), name, "DatabasePostCallback", "DatabasePostFallback");
    }

    private void AuthLoginCallback(string output)
    {
        Debug.Log("Login callback message: " + output);
    }

    private void AuthLoginFallback(string output)
    {
        Debug.Log("Login fallback error:" + output);
    }
    
    private void DatabaseGetUserFallback(string output)
    {
        currentUser = new UserData();
        Debug.Log("Database get fallback error:" + output);
    }

    private void DatabaseGetFallback(string output)
    {
        Debug.Log("Database get fallback error:" + output);
    }

    private void DatabasePostCallback(string output)
    {
        Debug.Log("Post callback: " + output);
    }

    private void DatabasePostFallback(string output)
    {
        Debug.Log("Post fallback error: " + output);
    }

    private void GetScoresCallback(string output)
    {

    }

    private void GetScoresSubmit(string output)
    {
        //var dbData = JsonUtility.FromJson<object>(output);
        /*foreach (var child in dbData.Children)
{
    UserData user = JsonUtility.FromJson<UserData>(child.GetRawJsonValue());
    var json = child.GetRawJsonValue();
    foreach (var score in user.profiles)
        if (score != null)
            highscores.Add(score);
}*/
        Debug.Log("Getting scores to submit a score");
        highscores = PopulateHighScores(output);
        int profileIdx = SetUsername(myUsername);
        Debug.Log($"Set username as: {myUsername} with idx: {profileIdx}");
        if (0 > profileIdx) return;
        //var fireuser = await _auth.SignInWithEmailAndPasswordAsync(adminUser, adminKey);

        ScoreData scoreEntry = currentUser.profiles[profileIdx];
        if (scoreEntry == null) scoreEntry = new ScoreData();
        scoreEntry.name = myUsername;
        if (myDist > scoreEntry.distance) scoreEntry.distance = myDist;
        if (myScore > scoreEntry.score) scoreEntry.score = myScore;
        currentUser.profiles[profileIdx] = scoreEntry;

        SettingsManager settings = SettingsManager.Instance;
        currentUser.settings.master = settings.masterVol.value;
        currentUser.settings.music = settings.musicVol.value;
        currentUser.settings.ambience = settings.ambienceVol.value;
        currentUser.settings.sfx = settings.sfxVol.value;
        currentUser.settings.brightness = settings.brightnessSlider.value;
        Debug.Log($"Submiting score {scoreEntry.name} - dist: {scoreEntry.distance} score: {scoreEntry.score}");
        FirebaseDatabase.UpdateJSON(firebaseRoot + playerKey, JsonUtility.ToJson(currentUser), name, "DatabasePostCallback", "DatabasePostFallback");
        highscores.Add(scoreEntry);
        PopulateLeaderboard();
        //await _database.GetReference(playerKey).SetRawJsonValueAsync(JsonUtility.ToJson(currentUser));
        //_auth.SignOut();
    }

    private void PopulateLeaderboardCallback(string output)
    {
        Debug.Log("Called back to populate leaderboard");
        highscores = PopulateHighScores(output);
        PopulateLeaderboard();
        
    }
    #endregion

    private List<ScoreData> PopulateHighScores(string json)
    {
        Debug.Log($"getting highscores from: {json}");
        var scores = new List<ScoreData>();
        /*var scoresDb = JsonConvert.DeserializeObject<JToken>(json);
        foreach (var session in scoresDb.Children()) 
        {
            JToken info = null;
            foreach (var child in session.Children())
                if (child != null)
                    info = child;

            if (info != null)
            {
                Debug.Log($"Getting score from {info.ToString()}");
                var score = JsonUtility.FromJson<ScoreData>(info.ToString());
                scores.Add(score);
            }
        }*/
        // TODO: have to do this with JTokens. Figure out why Newtonsoft not working
        var allSessions = JsonUtility.FromJson<List<SessionWrapper>>(json);
        foreach (var session in allSessions)
        {
            foreach (var user in session.userData)
            {
                foreach (var score in user.profiles)
                    if (score != null)
                        scores.Add(score);
            }
        }

        return scores;
    }
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

    public int SetUsername(string user)
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

    public void SubmitScore(string name, int dist, int score)
    {
        myUsername = name;
        myDist = dist;
        myScore = score;
        FirebaseDatabase.GetJSON(firebaseRoot, gameObject.name, "GetScoresSubmit", "DatabaseGetFallback");
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

    public void SetLeaderboard(Transform panel, GameObject prefab, bool orderByScore)
    {
        if (panel == null || prefab == null) return;
        sortMethod = orderByScore;
        leaderboardPanel = panel;
        leaderboardElementPrefab = prefab;
        sortMethod = orderByScore;
        FirebaseDatabase.GetJSON(firebaseRoot, name, "PopulateLeaderboardCallback", "DatabaseGetFallback");
    }

    private void PopulateLeaderboard()
    {
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

        int profileIdx = sortMethod ? scoreIdx : distIdx;
        string profileName = currentUser.profiles[profileIdx].name;

        //var highscores = await GetHighscores();
        int top = 5;
        int playerRank = -1;
        highscores = OrderHighScores(highscores, sortMethod);
        for (int i = 0; i < highscores.Count; i++)
        {
            if (highscores[i].name == profileName)
                playerRank = i;
            if (i < top)
            {
                var score = highscores[i];
                var element = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
                element.Initialize(i + 1, score.name, sortMethod ? score.score.ToString() : score.distance.ToString() + "m", playerRank == i);
                leaderboardElements.Add(element);
            }
        }

        if (playerRank >= top)
        {
            int prev = playerRank - 1;
            if (prev < top)
            {
                var elip = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
                elip.Initialize(-1, "...", "");
                leaderboardElements.Add(elip);
            }
            for (int i = playerRank - 1; i <= playerRank + 1; i++)
            {
                if (i < highscores.Count)
                {
                    var score = highscores[i];
                    var element = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
                    element.Initialize(i + 1, score.name, sortMethod ? score.score.ToString() : score.distance.ToString() + "m", i == playerRank);
                    leaderboardElements.Add(element);
                }
            }
        }

        if (playerRank != highscores.Count - 1)
        {
            var elipses = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
            elipses.Initialize(-1, "...", "");
            leaderboardElements.Add(elipses);

            var worstScore = highscores[highscores.Count - 1];
            var worstElement = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
            worstElement.Initialize(highscores.Count, worstScore.name, sortMethod ? worstScore.score.ToString() : worstScore.distance.ToString() + "m");
            leaderboardElements.Add(worstElement);
        }
    }


    private void OnApplicationQuit()
    {
        //_auth.SignOut();
    }
}
