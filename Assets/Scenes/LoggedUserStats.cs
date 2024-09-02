using System.Collections;
using System.Collections.Generic;
using TMPro;
using UMA.CharacterSystem;
using Unity.Services.Authentication.PlayerAccounts;
using PlayerData;
using UnityEngine;

public class LoggedUserStats : MonoBehaviour
{
    [SerializeField]
    TMP_Text userNameText;
    [SerializeField]
    TMP_Text coinsText;
    [SerializeField]
    TMP_Text avatarText;
    [SerializeField]
    DynamicCharacterAvatar Avatar;
    [SerializeField]
    GameObject ScriptHolderObject; // Serialize in order to keep between scenes

    private static LoggedUserStats instance;
    public string avatarString;
    //public DynamicCharacterAvatar Avatar;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoggedUser.OnUserLoggedIn += UpdateNameAndCoins;
            LoggedUser.OnCoinsChange += UpdateNameAndCoins;
            DontDestroyOnLoad(ScriptHolderObject);
           // DontDestroyOnLoad(userNameText);
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
        avatarText.text = LoggedUser.AvatarCompressedString;
        avatarString = LoggedUser.AvatarCompressedString;
        LoadAvatar();
    }

    public void LoadAvatar()
    {
        if (string.IsNullOrEmpty(LoggedUser.AvatarCompressedString))
        {
            return;
        }

        AvatarDefinition adf = AvatarDefinition.FromCompressedString(LoggedUser.AvatarCompressedString, '|');
        Avatar.LoadAvatarDefinition(adf);
        Avatar.BuildCharacter(false); // don't restore old DNA...
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event when the object is destroyed to prevent memory leaks
        LoggedUser.OnUserLoggedIn -= UpdateNameAndCoins;
    }

}
