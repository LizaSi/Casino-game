using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using PlayerData;

//using Unity.Services.Authentication.PlayerAccounts;
#if !UNITY_WEBGL
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine.UIElements;
using UMA;
using UMA.CharacterSystem;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
//using UnityEditor.MemoryProfiler;
#endif


public class Login : MonoBehaviour
{
    #if !UNITY_WEBGL
    protected Firebase.Auth.FirebaseAuth auth;
    protected Firebase.Auth.FirebaseAuth otherAuth;
    private bool saveButtonClicked = false;

    //public GameObject creatingTheAvatarObjects;
    //public GameObject loginCanvasAndCameraObjects;
    //public string avatarAsCompressedString;

    public GameObject creatingTheAvatarObjects;
    public GameObject loginCanvasAndCameraObjects;

    public DynamicCharacterAvatar Avatar;

    public bool useAvatarDefinition;
    public bool useCompressedString;
    public UMARandomAvatar Randomizer;
    public UnityEngine.UI.Button LoadButton;

    public string saveString;
    public string avatarString;
    public string compressedAvatarString;
    public int saveStringSize;
    public int avatarStringSize;
    public int compressedStringSize;
    public int asciiStringSize;
    public int binarySize;

    //   private DatabaseReference databaseReference;


    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth = new();
    private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions
    {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };
    #endif

    private bool signInAndFetchProfile = false;
    protected string displayName = "";
    private bool fetchingToken = false;
    private string m_UserName;
 //   private DatabaseReference databaseReference;

    [SerializeField]
    TMP_InputField m_UsernameText;
    [SerializeField]
    Text m_Label;
    [SerializeField]
    UnityEngine.UI.Button m_SignInButton;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return;
        }
        #if !UNITY_WEBGL
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
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
        //GenerateANewAvatar();
        #endif
    }
    public async void Login_ButtonClick()
    {
        await SigninWithEmailAsync();
    }

    private async Task SigninWithEmailAsync()
    {
        string email, password;

        if (!m_UsernameText.text.Contains('@'))
            email = m_UsernameText.text + "@gmail.com";
        else
            email = m_UsernameText.text;

        password = "123456";

#if UNITY_WEBGL
        LoginWeb loginWeb = GetComponent<LoginWeb>();
        loginWeb.SignIn(email);
#else
        if (signInAndFetchProfile)
        {
            try
            {
                await auth.SignInAndRetrieveDataWithCredentialAsync(
                              EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(HandleSignInWithAuthResult);
            }
            catch (Exception e)
            {
                m_Label.text = "Login error " + e.Message;
            }
        }
        else
        {
            try
            {
                await auth.SignInWithEmailAndPasswordAsync(email, password)
                    .ContinueWithOnMainThread(HandleSignInWithAuthResult);
            }
            catch (Exception e)
            {
                m_Label.text = "Error login " + e.Message;
            }
        }
#endif
    }

    /*
    private async void HandleSignInWithAuthResult(Task<AuthResult> task)
    {
        if (LogTaskCompletion(task, "Sign-in"))
        {
            if (task.Result.User != null && task.Result.User.IsValid())
            {
                string email = task.Result.User.Email;
                int atIndex = email.IndexOf('@');
                string username = task.Result.User.Email[..atIndex];

                int coins = await FetchUserCoinsAsync(username);
                string avatarCompressedString = await FetchUserAvatarAsync(username);
                LoggedUser.SetUser(username, coins);

                m_Label.text = string.Format("{0} signed in", LoggedUser.Username);

                SceneManager.LoadScene("RoomSelection");
            }
            else
            {
                m_Label.text = "Signed in but User is either null or invalid";
            }
        }
        else
        {
            m_Label.text = "Creating user";
            await createUser();
            await SigninWithEmailAsync();
        }
    }
    */

    private async void HandleSignInWithAuthResult(Task<AuthResult> task)
    {
        if (LogTaskCompletion(task, "Sign-in"))
        {
            if (task.Result.User != null && task.Result.User.IsValid())
            {
                string email = task.Result.User.Email;
                int atIndex = email.IndexOf('@');
                string username = task.Result.User.Email[..atIndex];

                int coins = await FetchUserCoinsAsync(username);
                /*
                if(compressedAvatarString == null || compressedAvatarString == "")
                {
                    GenerateANewAvatar();
                }
                */
                string avatarCompressedString = await FetchUserAvatarAsync(username);
                LoggedUser.SetUser(username, coins, avatarCompressedString);

                m_Label.text = string.Format("{0} signed in", LoggedUser.Username);

                SceneManager.LoadScene("RoomSelection");
            }
            else
            {
                m_Label.text = "Signed in but User is either null or invalid";
            }
        }
        else
        {
            m_Label.text = "Creating user";
            await createUser();
            await SigninWithEmailAsync();
        }
    }


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
                if (exception is Firebase.FirebaseException firebaseEx)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                m_Label.text = authErrorCode + exception.ToString();
                Debug.LogError(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            complete = true;
        }
        else
        {
            Debug.LogWarning($"{operation} {task.Status} {task.AsyncState}");
        }
        return complete;
    }


#if !UNITY_WEBGL
    protected void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
     //   databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

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
                  Debug.LogWarning("ERROR: Failed to initialize secondary authentication object.");
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

            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                //  DebugLog("AuthStateChanged Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
               // DisplayDetailedUserInfo(user, 1);
            }
        }
    }  
    
    private async Task<int> FetchUserCoinsAsync(string username)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username).Child("coins");
        try
        {
            DataSnapshot snapshot = await userRef.GetValueAsync();
            if (snapshot.Exists)
            {
                int coins = int.Parse(snapshot.Value.ToString());
                return coins;
            }
            else
            {
                Debug.LogWarning("User coins not found in database.");
                return 0; 
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error fetching user coins: " + e);
            return 0;
        }
    }

    private async Task<string> FetchUserAvatarAsync(string username)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username).Child("avatar");

        if (saveButtonClicked)
        {
            DatabaseReference userRefUserName = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);
            Dictionary<string, object> userUpdates = new()
                      {
                        ////////////
                        {"avatar", compressedAvatarString }
                        ////////////

                        // Add more fields as needed
                    };
            await userRefUserName.UpdateChildrenAsync(userUpdates).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Failed to write user data: " + task.Exception);
                }
                else if (task.IsCompleted)
                {
                    Debug.Log("User data added to database successfully.");
                }
            });
        }
        try
        {
            DataSnapshot snapshot = await userRef.GetValueAsync();
            if (snapshot.Exists)
            {
                string avatarCompressedString = snapshot.Value.ToString();
                return avatarCompressedString;
            }
            else
            {
                Debug.LogWarning("User avatar not found in database.");
                if (string.IsNullOrEmpty(compressedAvatarString))// compressedAvatarString == null || compressedAvatarString == "")
                {
                    GenerateANewAvatar();
                }
                DatabaseReference userRefUserName = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);
                Dictionary<string, object> userUpdates = new()
                      {
                        ////////////
                        {"avatar", compressedAvatarString }
                        ////////////

                        // Add more fields as needed
                    };
                await userRefUserName.UpdateChildrenAsync(userUpdates).ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError("Failed to write user data: " + task.Exception);
                    }
                    else if (task.IsCompleted)
                    {
                        Debug.Log("User data added to database successfully.");
                    }
                });
                return compressedAvatarString;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error fetching user avatar: " + e);
            return "";
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    

    private Task createUser()
    {
        string username = m_UsernameText.text;
        string email = username;
        string password = "123456";
        if (!username.Contains('@'))
            email = username + "@gmail.com";
        //    string newDisplayName = displayName;
        return auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWithOnMainThread((task) => {
              if (LogTaskCompletion(task, "User Creation"))
              {
                  var user = task.Result.User;
              //    DisplayDetailedUserInfo(user, 1);

                  if (user != null)
                  {
                      if(string.IsNullOrEmpty(compressedAvatarString))// compressedAvatarString == null || compressedAvatarString == "")
                      {
                          GenerateANewAvatar();
                      }
                      DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);
                      Dictionary<string, object> userUpdates = new()
                      {
                        { "coins", 1000 },
                        ////////////
                        {"avatar", compressedAvatarString }
                        ////////////

                        // Add more fields as needed
                    };
                      userRef.UpdateChildrenAsync(userUpdates).ContinueWithOnMainThread(task =>
                      {
                          if (task.IsFaulted)
                          {
                              Debug.LogError("Failed to write user data: " + task.Exception);
                          }
                          else if (task.IsCompleted)
                          {
                              Debug.Log("User data added to database successfully.");
                          }
                      });
                  }
              }
              return task;
          }).Unwrap();
    }
#endif

    // Update is called once per frame
    void Update()
    {
        // Check for Enter key press to activate the button
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            m_SignInButton.onClick.Invoke();
        }
    }

    public void OnCreateYourAvatarClicked()
    {
        loginCanvasAndCameraObjects.SetActive(false);
        creatingTheAvatarObjects.SetActive(true);
        OnLoadClicked();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    public void GenerateANewAvatar()
    {
        Randomizer.Randomize(Avatar);
        Avatar.BuildCharacter(false);
        OnSaveClicked();
        OnLoadClicked();
    }

    public void OnSaveClicked()
    {
        avatarString = Avatar.GetAvatarDefinitionString(true);
        saveString = Avatar.GetCurrentRecipe();
        compressedAvatarString = Avatar.GetAvatarDefinition(true).ToCompressedString("|");
        asciiStringSize = Avatar.GetAvatarDefinition(true).ToASCIIString().Length;

        binarySize = BinaryDefinition.ToBinary(new BinaryFormatter(), Avatar.GetAvatarDefinition(true)).Length;
        saveStringSize = saveString.Length * 2;
        avatarStringSize = avatarString.Length * 2;
        compressedStringSize = compressedAvatarString.Length * 2; // utf-16

        LoadButton.interactable = true;

        creatingTheAvatarObjects.SetActive(false);
        loginCanvasAndCameraObjects.SetActive(true);

        saveButtonClicked = true;
    }

    public void OnLoadClicked()
    {
        if (string.IsNullOrEmpty(saveString))
        {
            return;
        }

        if (useCompressedString)
        {
            AvatarDefinition adf = AvatarDefinition.FromCompressedString(compressedAvatarString, '|');
            Avatar.LoadAvatarDefinition(adf);
            Avatar.BuildCharacter(false); // don't restore old DNA...
        }
        else if (useAvatarDefinition)
        {
            Avatar.LoadAvatarDefinition(avatarString);
            Avatar.BuildCharacter(false); // We must not restore the old DNA
        }
        else
        {
            Avatar.LoadFromRecipeString(saveString);
        }
    }

    public void OnBackClicked()
    {
        if (string.IsNullOrEmpty(saveString))
        {
            GenerateANewAvatar();

            avatarString = Avatar.GetAvatarDefinitionString(true);
            saveString = Avatar.GetCurrentRecipe();
            compressedAvatarString = Avatar.GetAvatarDefinition(true).ToCompressedString("|");
            asciiStringSize = Avatar.GetAvatarDefinition(true).ToASCIIString().Length;

            binarySize = BinaryDefinition.ToBinary(new BinaryFormatter(), Avatar.GetAvatarDefinition(true)).Length;
            saveStringSize = saveString.Length * 2;
            avatarStringSize = avatarString.Length * 2;
            compressedStringSize = compressedAvatarString.Length * 2; // utf-16

            LoadButton.interactable = true;
        }


        creatingTheAvatarObjects.SetActive(false);
        loginCanvasAndCameraObjects.SetActive(true);
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    
}
