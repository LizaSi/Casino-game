using FishNet.Discovery;
using FishNet;
using System;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Unity.Services.Authentication.Samples
{
    //Anonymous sign-in creates a new player for the game session with an input from the player and is a quick way for a player
    //to get started with your game. The following UI sample shows how to set up the ability for players to sign in
    //in your game and get your access token. If a player has already signed in before, the SignInAnonymously() recovers the existing
    //login of a player whether they signed in anonymously or through a social account.
    public class SignInUI : MonoBehaviour
    {
        [SerializeField]
        Button m_SignInButton;
        [SerializeField]
        Text m_SignInButtonText;
        [SerializeField]
        Text m_PlayerIdText;
        [SerializeField]
        TMP_InputField m_PlayerNameInput;
        [SerializeField]
        Text m_ExceptionText;
        [SerializeField]
        Text m_PlayerInfoText;
        [SerializeField]
        private NetworkDiscovery netDiscovery;

        //   InputField m_ProfileNameInput;

        PlayerInfo m_PlayerInfo;
        string m_ExternalIds;

        async void Start()
        {
            //UnityServices.Initialize() will initialize all services that are subscribed to Core
            await UnityServices.InitializeAsync();
            Debug.Log($"Unity services initialization: {UnityServices.State}");

            //Shows if a cached session token exist
            Debug.Log($"Cached Session Token Exist: {AuthenticationService.Instance.SessionTokenExists}");

            // Shows Current profile
            Debug.Log(AuthenticationService.Instance.Profile);
       //     m_ProfileNameInput.text = AuthenticationService.Instance.Profile;

            AuthenticationService.Instance.SignedIn += () =>
            {
                //Shows how to get a playerID
                Debug.Log($"PlayedID: {AuthenticationService.Instance.PlayerId}");

                //Shows how to get an access token
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

                                //Reset sign out button text
                const string successMessage = "Sign in anonymously succeeded!";
                Debug.Log(successMessage);
                m_SignInButtonText.text = successMessage;
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                //Reset sign in button text
                m_SignInButtonText.text = "Sign In Anonymously";

                Debug.Log("Signed Out!");
            };
            //You can listen to events to display custom messages
            AuthenticationService.Instance.SignInFailed += errorResponse =>
            {
                Debug.LogError($"Sign in anonymously failed with error code: {errorResponse.ErrorCode}");
            };

            UpdateUI();
        }

        void UpdateUI()
        {
            var isSignedIn = AuthenticationService.Instance.IsSignedIn;
      //      m_PlayerIdText.text = isSignedIn ? $"PlayerId: {AuthenticationService.Instance.PlayerId}" : "";
            m_PlayerIdText.text = isSignedIn ? $"Player name: {AuthenticationService.Instance.PlayerName}" :"";

            m_SignInButton.interactable = !isSignedIn;
        //    m_ProfileNameInput.interactable = !isSignedIn;
         //   m_ProfileNameInput.text = AuthenticationService.Instance.Profile;
            m_ExceptionText.text = "";

            if (m_PlayerInfo != null)
                m_PlayerInfoText.text = isSignedIn ? GetPlayerInfoText(m_PlayerInfo) : "";
        }

        /// <summary>
        /// Returns Player info string if the player is authorized
        /// </summary>
        /// <param name="playerInfo"></param>
        /// <returns></returns>
        string GetPlayerInfoText(PlayerInfo playerInfo)
        {
            if (playerInfo.CreatedAt == null)
                return string.Empty;

            var localDateTime = playerInfo?.CreatedAt.Value.ToLocalTime();

            //  var playerText = $"CreatedAt: {localDateTime.Value} \n ExternalIds: {m_ExternalIds} \n ";

            var playerText = $"Hello ";
            return playerText;
        }

        public async void OnClickSignIn()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                if(m_PlayerNameInput)
                    await AuthenticationService.Instance.UpdatePlayerNameAsync(m_PlayerNameInput.text);

                //Start advertising a server
                if (netDiscovery == null)
                {
                    netDiscovery = FindObjectOfType<NetworkDiscovery>();
                   // InstanceFinder.ServerManager.StartConnection();
                    //   FindObjectOfType<NetworkDiscovery>().AdvertiseServer();
                }

                SceneManager.LoadScene("RoomSelection");
                UpdateUI();
            }
            catch (RequestFailedException ex)
            {
                Debug.LogError($"Login failed with error code: {ex.ErrorCode}");
                m_ExceptionText.text = $"{ex.GetType().Name}: {ex.Message}";
            }
        }

        public void OnClickSignOut()
        {
            AuthenticationService.Instance.SignOut();
            UpdateUI();
        }

        public void OnClickSwitchProfile()
        {
            try
            {
           //     AuthenticationService.Instance.SwitchProfile(m_ProfileNameInput.text);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
           //     m_ProfileNameInput.text = AuthenticationService.Instance.Profile;
            }
            Debug.Log($"Current Profile: {AuthenticationService.Instance.Profile}");
            PlayerPrefsLog();
        }

        public async void OnClickGetPlayerInfo() => await GetPlayerInfoAsync();

        async Task GetPlayerInfoAsync()
        {
            m_PlayerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            m_ExternalIds = GetExternalIds(m_PlayerInfo);
            UpdateUI();
        }

        string GetExternalIds(PlayerInfo playerInfo)
        {
            var sb = new StringBuilder();
            if (playerInfo.Identities != null)
            {
                foreach (var id in playerInfo.Identities)
                    sb.Append(id.TypeId + " ");

                return sb.ToString();
            }

            return "None";
        }

        public void OnClearSessionToken()
        {
            AuthenticationService.Instance.ClearSessionToken();
            UpdateUI();
        }

        void PlayerPrefsLog()
        {
            var sessionToken = PlayerPrefs.GetString($"{Application.cloudProjectId}.{AuthenticationService.Instance.Profile}.unity.services.authentication.session_token");
            var playerPrefsMessageResult = string.IsNullOrEmpty(sessionToken) ? "No session token for this profile" : $"Session token: {sessionToken}";
            Debug.Log(playerPrefsMessageResult);
        }
    }
}
