using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobby : MonoBehaviour
{
    public void Button_OnClick()
    {
        if (InstanceFinder.IsServer)
        {
            if (PokerServerManager.IsInitialized())
            {
                MemberList.SetLobbyList(PokerServerManager.GetPlayersNames());
                MemberList.UpdateLobbyList();
            }
            else if (GameServerManager.IsInitialized())
            {
                // to add
            }
            SceneManager.LoadScene("CreateRoom");

        }
        else
            SceneManager.LoadScene("RoomSelection");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
