using Firebase.Database;
using Firebase.Extensions;
using System;
using TMPro;
using UnityEngine;

namespace PlayerData
{
    public static class LoggedUser
    {
        public static string Username { get; private set; }
        public static int Coins { get; private set; }

        public static event Action OnUserLoggedIn;
        public static event Action OnCoinsChange;

        public static void SetUser(string username, int coins)
        {
            Username = username;
            Coins = coins;

            OnUserLoggedIn?.Invoke();
        }

        public static void FetchCoins()
        {
            var userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(Username).Child("coins");
            userRef.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error fetching user coins: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    int coins = int.Parse(snapshot.Value.ToString());
                    Coins = coins;
                    OnCoinsChange?.Invoke();
                }
                else
                {
                    Debug.LogError("User not found in database: " + Username);
                }
            });
        }
    }

    public class GetUserName : MonoBehaviour
    {
        [SerializeField]
        TMP_Text userNameText;

        private void Start()
        {
            LoggedUser.OnUserLoggedIn += UpdateNameAndCoins;
            LoggedUser.OnCoinsChange += UpdateNameAndCoins;

            // Update the text if the user data is already set
            if (!string.IsNullOrEmpty(LoggedUser.Username))
            {
                UpdateNameAndCoins();
            }
            else
            {
                userNameText.text = "Guest";
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from the event when the object is destroyed to prevent memory leaks
            LoggedUser.OnUserLoggedIn -= UpdateNameAndCoins;
        }

        private void UpdateNameAndCoins()
        {
            userNameText.text = LoggedUser.Username + "               " + LoggedUser.Coins;
        }
    }
}
