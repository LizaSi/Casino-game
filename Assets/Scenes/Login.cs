using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Extensions;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Firebase.Database;
using Unity.Services.Authentication.PlayerAccounts;
using Firebase.Auth;
using UnityEngine.UIElements;
//using UnityEditor.MemoryProfiler;


public class Login : MonoBehaviour
{
    protected Firebase.Auth.FirebaseAuth auth;
    protected Firebase.Auth.FirebaseAuth otherAuth;
    private bool signInAndFetchProfile = false;
    protected string displayName = "";
    private bool fetchingToken = false;
    private string m_UserName;
    private DatabaseReference databaseReference;


    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
  new Dictionary<string, Firebase.Auth.FirebaseUser>();

    private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions
    {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };

    [SerializeField]
    TMP_InputField m_UsernameText;
    [SerializeField]
    TMP_InputField m_PasswordText;
    [SerializeField]
    Text m_Label;
    [SerializeField]
    UnityEngine.UI.Button m_SignInButton;

    // Start is called before the first frame update
    void Start()
    {
        m_UsernameText.onEndEdit.AddListener(delegate { OnEndEditUsername(); });

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void OnEndEditUsername()
    {
      //  m_PasswordText.Select();
    }

    protected void InitializeFirebase()
    {
      //  DebugLog("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        // Specify valid options to construct a secondary authentication object.
        if (otherAuthOptions != null &&
            !(string.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
              string.IsNullOrEmpty(otherAuthOptions.AppId) ||
              string.IsNullOrEmpty(otherAuthOptions.ProjectId)))
        {
            try
            {
                otherAuth = FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
                  otherAuthOptions, "Secondary"));
                otherAuth.StateChanged += AuthStateChanged;
                otherAuth.IdTokenChanged += IdTokenChanged;
            }
            catch (Exception)
            {
              //  DebugLog("ERROR: Failed to initialize secondary authentication object.");
            }
        }
        AuthStateChanged(this, null);
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false);
        }
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
               // DebugLog("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
              //  DebugLog("AuthStateChanged Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                DisplayDetailedUserInfo(user, 1);
            }
        }
    }

    // Display a more detailed view of a FirebaseUser.
    protected void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        DisplayUserInfo(user, indentLevel);
       /* DebugLog(String.Format("{0}Anonymous: {1}", indent, user.IsAnonymous));
        DebugLog(String.Format("{0}Email Verified: {1}", indent, user.IsEmailVerified));
        DebugLog(String.Format("{0}Phone Number: {1}", indent, user.PhoneNumber));*/
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        var numberOfProviders = providerDataList.Count;
        if (numberOfProviders > 0)
        {
            for (int i = 0; i < numberOfProviders; ++i)
            {
              //  DebugLog(String.Format("{0}Provider Data: {1}", indent, i));
                DisplayUserInfo(providerDataList[i], indentLevel + 2);
            }
        }
    }

    protected void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel)
    {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
        {"Display Name", userInfo.DisplayName},
        {"Email", userInfo.Email},
        {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
        {"Provider ID", userInfo.ProviderId},
        {"User ID", userInfo.UserId}
      };
        foreach (var property in userProperties)
        {
            if (!String.IsNullOrEmpty(property.Value))
            {
              //  DebugLog(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }

    public void SignUp_ButtonClick()
    {
        SceneManager.LoadScene("RoomSelection");
     //   LoggedUser.SetUser(m_UserName, "");
        /*if (m_UserName != null)
            signInFishnetScript.HandleLogin(m_UserName, "");*/
        // signInFishnet.HandleLogin(m_UserName,"");
    }

    public void Login_ButtonClick()
    {
        SigninWithEmailAsync();
    }

    private Task SigninWithEmailAsync()
    {
        string email, password;
        int i = 0;
        string[] dots = { ".", "..", "..." };

        if (!m_UsernameText.text.Contains('@'))
            email = m_UsernameText.text + "@gmail.com";
        else
            email = m_UsernameText.text;
        if (m_PasswordText == null || string.IsNullOrEmpty(m_PasswordText.text))
            password = "123456";
        else
            password = m_PasswordText.text;


        if (signInAndFetchProfile)
        {
            return auth.SignInAndRetrieveDataWithCredentialAsync(
              EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
                HandleSignInWithAuthResult);
        }
        else
        {
            m_Label.text = "Connecting.";
            Debug.Log("Auth instance: " + (auth != null ? "Valid" : "Null"));
            while(auth == null)
            {
                m_Label.text = "Connecting" + dots[i % 3];
                Task.Delay(100);
                i++;
            }

            return auth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(HandleSignInWithAuthResult);
        }
    }

    private async void HandleSignInWithAuthResult(Task<AuthResult> task)
    {
        string[] dots = {".","..","..."};
        if (LogTaskCompletion(task, "Sign-in"))
        {
            if (task.Result.User != null && task.Result.User.IsValid())
            {
              //  string userName = task.Result.User.DisplayName;
                string email = task.Result.User.Email;
                int atIndex = email.IndexOf('@');
                LoggedUser.SetUser(task.Result.User.Email.Substring(0, atIndex), "TOKEN1");
                m_Label.text = string.Format("{0} signed in", LoggedUser.Username);
                int i = 0;
                while (LoggedUser.Username == null)
                {                
                    m_Label.text = "Connecting" + dots[i%3];
                    await Task.Delay(100); // Wait for a short duration before checking again
                    i++;
                }
                SceneManager.LoadScene("RoomSelection");
            }
            else
            {
                m_Label.text = "Signed in but User is either null or invalid";
            }
        }
        else
        {
            await createUser();
            await SigninWithEmailAsync();
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            m_Label.text = operation + " canceled.";
         //   DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            m_Label.text = operation + " encounted an error.";
           // DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                m_Label.text = authErrorCode + exception.ToString();
           //     DebugLog(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            //  DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    private Task createUser()
    {
        string email = m_UsernameText.text;
        string password = "123456";
        if (!m_UsernameText.text.Contains('@'))
            email = m_UsernameText.text + "@gmail.com";
    //    string newDisplayName = displayName;
        return auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWithOnMainThread((task) => {
              if (LogTaskCompletion(task, "User Creation"))
              {
                  var user = task.Result.User;
                  DisplayDetailedUserInfo(user, 1);
                //  return UpdateUserProfileAsync(newDisplayName: newDisplayName);
              }
              return task;
          }).Unwrap();
    }   
   
    // Update is called once per frame
    void Update()
    {
       /* if (Input.GetKeyDown(KeyCode.Tab) && m_UsernameText.isFocused)
        {
            m_PasswordText.Select();
        }*/

        // Check for Enter key press to activate the button
        if (Input.GetKeyDown(KeyCode.Return))
        {
            m_SignInButton.onClick.Invoke();
        }
    }
}

