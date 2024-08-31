using System.Collections;
using System.Collections.Generic;
using TMPro;
using UMA.CharacterSystem;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class AvatarLoadData : MonoBehaviour
{
    [SerializeField]
    DynamicCharacterAvatar Avatar;
    //[SerializeField]
    //string avatarCompressedString;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateAvatar(string avatarCompressedString)
    {
        //if (string.IsNullOrEmpty(LoggedUser.AvatarCompressedString))
        if (string.IsNullOrEmpty(avatarCompressedString))
        {
            return;
        }

        AvatarDefinition adf = AvatarDefinition.FromCompressedString(avatarCompressedString, '|');
        //AvatarDefinition adf = AvatarDefinition.FromCompressedString(LoggedUser.AvatarCompressedString, '|');
        Avatar.LoadAvatarDefinition(adf);
        Avatar.BuildCharacter(false); // don't restore old DNA...
    }
}
