using FirebaseWebGL;
using System;
using PlayerData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginWeb : MonoBehaviour
{
    private string m_email;

    public Text outputText;

    private const string password = "123456";

    private void Start()
    {
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            return;
        }

        FirebaseAuthWeb.OnAuthStateChanged(gameObject.name, "DisplayUserInfo", "DisplayInfo");
    }

    public void SignIn(string email)
    {
        m_email = email;
        Debug.LogWarning("loggin in as " + email);
        SignInWithEmailAndPassword();
    }

    public void CreateUserWithEmailAndPassword() =>
        FirebaseAuthWeb.CreateUserWithEmailAndPassword(m_email, password, gameObject.name, "DisplayInfo", "DisplayErrorObject");

    public void SignInWithEmailAndPassword() =>
        FirebaseAuthWeb.SignInWithEmailAndPassword(m_email, password, gameObject.name, "DisplayInfo", "DisplayErrorObject");

    public void DisplayUserInfo(string user)
    {
        var parsedUser = StringSerializationAPI.Deserialize(typeof(FirebaseUser), user) as FirebaseUser;
        DisplayData(parsedUser.email);
    }

    public void DisplayData(string userEmail)
    {
        outputText.color = outputText.color == Color.green ? Color.blue : Color.green;
        outputText.text = userEmail + " logged in";
        m_email = userEmail;
    }

    public void DisplayInfo(string email)
    {
        outputText.color = Color.white;
        outputText.text = email + " logged in";
        HandleSignInWithAuthResult(email);
    }

    public void DisplayErrorObject(string error)
    {
        var parsedError = StringSerializationAPI.Deserialize(typeof(FirebaseError), error) as FirebaseError;
        DisplayError(parsedError.message);
    }

    public void DisplayError(string error)
    {
        outputText.color = Color.red;
        //  outputText.text = error;
        Debug.LogError(error);
        outputText.text = "Creating user";
        //    createUser();
        //   SigninWithEmailAsync();
    }

    private void HandleSignInWithAuthResult(string email)
    {
        int atIndex = email.IndexOf('@');
        if(atIndex < 0)
        {
            outputText.text = email + " is not a valid username";
            return;
        }
        string username = email[..atIndex];
        //int coins = await FetchUserCoinsAsync(username);
        LoggedUser.SetUser(username, 1000);

        SceneManager.LoadScene("RoomSelection");
    }
}
