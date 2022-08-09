using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;
using Text = TMPro.TextMeshProUGUI;
using FirebaseWebGL.Scripts.FirebaseBridge;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class UserData
{
    public UserSettings settings;
    public ScoreData profile;
    public int childrenKicked, demonsVanquished;
    public bool authenticated;

    public UserData()
    {
        settings = new UserSettings();
        profile = new ScoreData();
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

    [SerializeField] private Text totalKickText, totalKillText;
    [SerializeField] private Transform profilesPanel;
    private LeaderboardElement profileElement;

    public UserData CurrentUser { get { return currentUser; } }
    private UserData currentUser;
    private bool initialized;
    private int myScore, myDist;
    private bool sortMethod;

    private string firebaseRoot = "users/";
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
        }
    }

    public void Initialize()
    {
        initialized = true;
        FirebaseAuth.SignInWithEmailAndPassword(adminUser, adminKey, name, PlayerDetails.Instance.UserLoggedIn ? "InitializeAsAdmin" : "AuthLoginCallback", "AuthLoginFallback");
    }

    #region FirebaseBridgeCalllbacks
    private void InitializeAsAdmin(string output)
    {
        FirebaseDatabase.GetJSON(firebaseRoot + "/" + PlayerDetails.Instance.Username, name, "InitializeUser", "CreateNewUser");
    }

    private void InitializeUser(string output)
    {
        if (output != null && output != "null")
        {
            currentUser = JsonConvert.DeserializeObject<UserData>(output);
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
            return;
        }
        GameManager.Instance.Initialize();
    }

    private void CreateNewUser(string output)
    {
        currentUser = new UserData();
        currentUser.authenticated = true;
        currentUser.profile.name = PlayerDetails.Instance.Username;
        FirebaseDatabase.PushJSON(firebaseRoot + PlayerDetails.Instance.Username, JsonUtility.ToJson(currentUser), name, "DatabasePostCallback", "DatabasePostFallback");
        GameManager.Instance.Initialize();
    }

    private void AuthLoginCallback(string output)
    {
        Debug.Log("Login leaderboard success");
    }

    private void AuthLoginFallback(string output)
    {
        Debug.Log("Login leaderboard error");
    }

    private void DatabaseGetUserFallback(string output)
    {
        Debug.LogWarning("Error getting user from database:" + output);
    }

    private void DatabaseGetFallback(string output)
    {
        Debug.LogWarning("Error getting from database:" + output);
    }

    private void DatabasePostCallback(string output)
    {
        //Debug.Log("Post callback");
    }

    private void DatabasePostFallback(string output)
    {
        Debug.Log("Error posting to database" + output);
    }

    private void GetScoresCallback(string output)
    {

    }

    private void GetScoresSubmit(string output)
    {
        //Debug.Log("Getting scores to submit a score");
        highscores = PopulateHighScores(output);
        //Debug.Log("Got highscores");
        if (currentUser == null) currentUser = new UserData();
        currentUser.profile.name = PlayerDetails.Instance.Username;
        if (myDist > currentUser.profile.distance) currentUser.profile.distance = myDist;
        if (myScore > currentUser.profile.score) currentUser.profile.score = myScore;

        if (SettingsManager.Instance != null)
        {
            currentUser.settings.master = SettingsManager.Instance.masterVol.value;
            currentUser.settings.music = SettingsManager.Instance.musicVol.value;
            currentUser.settings.ambience = SettingsManager.Instance.ambienceVol.value;
            currentUser.settings.sfx = SettingsManager.Instance.sfxVol.value;
            currentUser.settings.brightness = SettingsManager.Instance.brightnessSlider.value;
        }
        currentUser.authenticated = true;
        
        Debug.Log($"Submiting score {currentUser.profile.name} - dist: {currentUser.profile.distance} score: {currentUser.profile.score}");
        if (!PlayerDetails.Instance.UserLoggedIn)
        {
            PlayerDetails.Instance.PostUserData(currentUser);
        }
        else
        {
            FirebaseDatabase.UpdateJSON(firebaseRoot + PlayerDetails.Instance.Username, JsonUtility.ToJson(currentUser), name, "DatabasePostCallback", "DatabasePostFallback");
        }
        foreach (var score in highscores)
        {
            if (score.name == currentUser.profile.name)
            {
                highscores.Remove(score);
                break;
            }
        }
        highscores.Add(currentUser.profile);
        // successful post, dont allow any more posts from this time.
        UIManager.Instance.ScoreSubmitted();
        PopulateLeaderboard();
    }

    private void PopulateLeaderboardCallback(string output)
    {
        highscores = PopulateHighScores(output);
        PopulateLeaderboard();
        
    }
    #endregion

    private List<ScoreData> PopulateHighScores(string json)
    {
        var scores = new List<ScoreData>();
        var scoresDb = JsonConvert.DeserializeObject<JToken>(json);
        foreach (var session in scoresDb.Children()) 
        {
            if (session == null) continue;
            JToken info = null;
            foreach (var child in session.Children())
                if (child != null)
                    info = child;

            if (info != null)
            {
                var userData = JsonUtility.FromJson<UserData>(info.ToString());
                scores.Add(userData.profile);
            }
        }
        return scores;
    }

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
        int id = 1;
        if (PlayerDetails.Instance.UserLoggedIn)
        {
            if (user != PlayerDetails.Instance.Username)
            {
                ReportWarning("That is not your username! (this session)");
                return -1;
            }
            else
            {
                return 1;
            }
        }

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

        if (currentUser.profile.name == user)
            return 1;

        //var highscores = await GetHighscores();
        for (int i = 0; i < highscores.Count; i++)
        {
            if (user == highscores[i].name)
            {
                ReportWarning($"Cannot use {user}. It is already in use");
                return -1;
            }
        }
      
        PlayerDetails.Instance.SetUsername(user);
        return id;
    }

    public void SubmitScore(string username, string password, int dist, int score)
    {
        if (!PlayerDetails.Instance.UserLoggedIn)
        {
            PlayerDetails.Instance.SetPassword(password);
            int profileIdx = SetUsername(username);
            if (0 > profileIdx) return;
        }

        myDist = dist;
        myScore = score;
        PlayerDetails.Instance.loginAttemptEvent = new UnityEngine.Events.UnityEvent<bool>();
        PlayerDetails.Instance.loginAttemptEvent.AddListener(ValidateUser);
        PlayerDetails.Instance.SignIn();
    }

    private void ValidateUser(bool valid)
    {
        if (valid)
        {
            // must sign in as admin before I can get these details
            FirebaseAuth.SignInWithEmailAndPassword(adminUser, adminKey, name, "QueryDatabaseForScores", "AuthLoginFallback");
        }
        else
        {
            // create user.
            // if user cannot be created, then it is invalid login
            PlayerDetails.Instance.loginAttemptEvent = new UnityEngine.Events.UnityEvent<bool>();
            PlayerDetails.Instance.loginAttemptEvent.AddListener(OnAccountRegister);
            PlayerDetails.Instance.RegisterAccount();
        }
    }

    private void OnAccountRegister(bool success)
    {
        if (!success)
        {
            ReportWarning("Invalid username/password");
            return;
        }
        ValidateUser(true);
    }

    private void QueryDatabaseForScores(string output)
    {
        FirebaseDatabase.GetJSON(firebaseRoot, gameObject.name, "GetScoresSubmit", "DatabaseGetFallback");
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
        if (profileElement != null)
            Destroy(profileElement.gameObject);

        if (currentUser.profile.name != "")
        {
            profileElement = Instantiate(leaderboardElementPrefab, profilesPanel).GetComponent<LeaderboardElement>();
            var prof = currentUser.profile;
            profileElement.Initialize(1, prof.name, $"<b>{prof.distance}m</b> - <b>{prof.score}</b>");

            totalKickText.text = currentUser.childrenKicked.ToString();
            totalKillText.text = currentUser.demonsVanquished.ToString();
        }

        // Populate the leaderboard
        if (leaderboardElements != null)
            foreach (var leaderboardElement in leaderboardElements)
                if (leaderboardElement != null)
                    Destroy(leaderboardElement.gameObject);
        leaderboardElements = new List<LeaderboardElement>();

        int top = 5;
        int playerRank = -1;
        highscores = OrderHighScores(highscores, sortMethod);
        for (int i = 0; i < highscores.Count; i++)
        {
            if (highscores[i].name == currentUser.profile.name)
                playerRank = i;
            if (i < top)
            {
                var score = highscores[i];
                var leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
                leaderboardElement.Initialize(i + 1, score.name, sortMethod ? score.score.ToString() : score.distance.ToString() + "m", playerRank == i);
                leaderboardElements.Add(leaderboardElement);
            }
        }

        if (playerRank >= top)
        {
            int prev = playerRank - 1;
            if (prev > top)
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
                    var leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardPanel).GetComponent<LeaderboardElement>();
                    leaderboardElement.Initialize(i + 1, score.name, sortMethod ? score.score.ToString() : score.distance.ToString() + "m", i == playerRank);
                    leaderboardElements.Add(leaderboardElement);
                }
            }
        }

        if (playerRank != highscores.Count - 1)
        {
            if (playerRank != highscores.Count - 2)
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
    }
}
