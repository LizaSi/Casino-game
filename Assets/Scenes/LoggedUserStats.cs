using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class LoggedUserStats : MonoBehaviour
{
    [SerializeField]
    TMP_Text userNameText;
    [SerializeField]
    TMP_Text coinsText;
    [SerializeField]
    GameObject ScriptHolderObject; // Serialize in order to keep between scenes

    private static LoggedUserStats instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoggedUser.OnUserLoggedIn += UpdateNameAndCoins;
            LoggedUser.OnCoinsChange += UpdateNameAndCoins;
            DontDestroyOnLoad(ScriptHolderObject);
            DontDestroyOnLoad(userNameText);
            if (!string.IsNullOrEmpty(LoggedUser.Username))
            {
                UpdateNameAndCoins();
            }
            else
            {
                userNameText.text = "Guest";
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UpdateNameAndCoins();
        }
    }

    private void UpdateNameAndCoins()
    {
        userNameText.text = LoggedUser.Username;
        coinsText.text = LoggedUser.Coins.ToString();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed to prevent memory leaks
        LoggedUser.OnUserLoggedIn -= UpdateNameAndCoins;
    }

}
