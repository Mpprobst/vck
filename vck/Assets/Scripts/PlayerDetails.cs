using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Net;
using System.Security.Cryptography;
using FirebaseWebGL.Scripts.FirebaseBridge;

public class PlayerDetails : MonoBehaviour
{
    public static PlayerDetails Instance { get { return _instance; } }
    private static PlayerDetails _instance;

    public UnityEvent<bool> loginAttemptEvent;

    public string Username { get { return _username; } }
    public bool UserLoggedIn { get { return _validUser; } }

    private string firebaseRoot = "users/";
    private string _username = "";
    private string _password;
    private string emailExtension = "@vck.org";
    private bool _validUser;

    // Start is called before the first frame update
    void Start()
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

    public void SetUsername(string username)
    {
        _username = username;
    }

    public void SetPassword(string password)
    {
        _password = GetHashString(password);
    }

    public void SignIn()
    {
        //Debug.Log("Signing in: " + _username + emailExtension);
        FirebaseAuth.SignInWithEmailAndPassword(_username + emailExtension, _password, name, "AuthLoginCallback", "AuthLoginFallback");
    }

    public void RegisterAccount()
    {
        Debug.Log($"Registering user: {_username}");
        FirebaseAuth.CreateUserWithEmailAndPassword(_username + emailExtension, _password, name, "AuthLoginCallback", "AuthLoginFallback");
    }

    private void AuthLoginCallback(string output)
    {
        _validUser = true;
        Debug.Log("Login successful!");
        if (loginAttemptEvent != null)
            loginAttemptEvent.Invoke(true);
    }

    private void AuthLoginFallback(string output)
    {
        _validUser = false;
        Debug.Log("Login error");
        if (loginAttemptEvent != null)
            loginAttemptEvent.Invoke(false);
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

    public void PostUserData(UserData user)
    {
        FirebaseDatabase.PostJSON(firebaseRoot + _username, JsonUtility.ToJson(user), name, "CreateAccountSuccess", "CreateAccountFailure");
    }

    private void CreateAccountSuccess(string output)
    {
        Debug.Log("Account created successfully");
    }

    private void CreateAccountFailure(string output)
    {
        Debug.Log("Error creating account");
    }



}
