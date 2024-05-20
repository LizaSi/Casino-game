using System;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public static class LoggedUser
    {
        public static string Username { get; private set; }
        public static string Token { get; private set; }

        // Event to notify when the user data is set
       // public static event Action OnUserLoggedIn;
        public static void SetUser(string username, string token)
        {
            Username = username;
            Token = token;

            // Invoke the event to notify subscribers
           // OnUserLoggedIn?.Invoke();
        }
    }

    public class GetUserName : MonoBehaviour
    {
        [SerializeField]
        TMP_Text userNameText;
        // Start is called before the first frame update

       /* private void OnEnable()
        {
            // Subscribe to the OnUserLoggedIn event
            LoggedUser.OnUserLoggedIn += UpdateUserName;
        }

        private void OnDisable()
        {
            // Unsubscribe from the OnUserLoggedIn event
            LoggedUser.OnUserLoggedIn -= UpdateUserName;
        }*/

        private void Start()
        {
            // Update the text if the user data is already set
            if (!string.IsNullOrEmpty(LoggedUser.Username))
            {
                userNameText.text = LoggedUser.Username;
            }
            else
                userNameText.text = "Guest";
        }

       /* private void UpdateUserName()
        {
            // Update the text when the user data is set
            userNameText.text = LoggedUser.Username;
        }*/
    }
}
