using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA.CharacterSystem;
using UMA;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;

public class AvatarData : MonoBehaviour
{
    public GameObject creatingTheAvatarObjects;
    public GameObject loginCanvasAndCameraObjects;

    public DynamicCharacterAvatar Avatar;

    public bool useAvatarDefinition;
    public bool useCompressedString;
    public UMARandomAvatar Randomizer;
    public Button LoadButton;

    public string saveString;
    public string avatarString;
    public string compressedString;
    public int saveStringSize;
    public int avatarStringSize;
    public int compressedStringSize;
    public int asciiStringSize;
    public int binarySize;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateANewAvatar()
    {
        Randomizer.Randomize(Avatar);
        Avatar.BuildCharacter(false);
    }

    public void OnSaveClicked()
    {
        avatarString = Avatar.GetAvatarDefinitionString(true);
        saveString = Avatar.GetCurrentRecipe();
        compressedString = Avatar.GetAvatarDefinition(true).ToCompressedString("|");
        asciiStringSize = Avatar.GetAvatarDefinition(true).ToASCIIString().Length;

        binarySize = BinaryDefinition.ToBinary(new BinaryFormatter(), Avatar.GetAvatarDefinition(true)).Length;
        saveStringSize = saveString.Length * 2;
        avatarStringSize = avatarString.Length * 2;
        compressedStringSize = compressedString.Length * 2; // utf-16

        LoadButton.interactable = true;
    }

    public void OnLoadClicked()
    {
        if (string.IsNullOrEmpty(saveString))
        {
            return;
        }

        if (useCompressedString)
        {
            AvatarDefinition adf = AvatarDefinition.FromCompressedString(compressedString, '|');
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
            compressedString = Avatar.GetAvatarDefinition(true).ToCompressedString("|");
            asciiStringSize = Avatar.GetAvatarDefinition(true).ToASCIIString().Length;

            binarySize = BinaryDefinition.ToBinary(new BinaryFormatter(), Avatar.GetAvatarDefinition(true)).Length;
            saveStringSize = saveString.Length * 2;
            avatarStringSize = avatarString.Length * 2;
            compressedStringSize = compressedString.Length * 2; // utf-16

            LoadButton.interactable = true;
        }


        creatingTheAvatarObjects.SetActive(false);
        loginCanvasAndCameraObjects.SetActive(true);
    }
}
