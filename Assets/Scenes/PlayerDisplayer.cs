using FishNet.Broadcast;
using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using FishNet.Object;
using UnityEngine.UI;
using System;
using FishNet.Connection;
using System.Linq;
using Unity.VisualScripting;
using static GameServerManager;

public class PlayerDisplayer : NetworkBehaviour
{
    private bool isPlayerDisplayed = false;
    private int playerIndex = -1;
    private GameObject gameServerManager;
    // Start is called before the first frame update
    void Start()
    {
        GameServerManager.OnInitialized += OnGameServerSttarted;
        if (GameServerManager.IsInitialized())
        {
            OnGameServerSttarted();
        }
    }

    public void OnGameServerSttarted()
    {
        InitPlayer();
    }

    public void InitPlayer()
    {
        //InstanceFinder.ClientManager.RegisterBroadcast<ClientIndexSetBroadcast>(DisplayPlayer);
        playerIndex = GameServerManager.GetPlayerIndex(base.Owner);
        DisplayPlayer2();
    }

    /*
    public void DisplayPlayer(ClientIndexSetBroadcast msg)
    {
        if (msg.StartDisplay && msg.client == base.Owner)
        {
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
                handleClientDisplay(msg.playerIndex);
                StartCoroutine(UpdatePlayersInDelay());
            }
        }
    }
    */

    public void DisplayPlayer2()
    {
        if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientDisplay(playerIndex);
            StartCoroutine(UpdatePlayersInDelay());
        }
    }

    private void handleClientDisplay(int playerIndexSetFromServer)
    {
        Debug.LogWarning($"player index = {playerIndexSetFromServer}");
        playerIndex = playerIndexSetFromServer;
        Debug.LogWarning($"player index after init = {playerIndex}");
        updatePlayerDisplayForClient();
    }

    private void updatePlayerDisplayForClient()
    {
        if (base.Owner.IsValid && GameServerManager.IsInitialized())
        {
            if (base.Owner.IsLocalClient && !isPlayerDisplayed)
            {
                //int playerIndex = GameServerManager.GetPlayerIndex(base.Owner);
                //SpawnPlayerToScene(playerIndex);
                SpawnPlayerToScene();
                isPlayerDisplayed = true;

            }

        }
    }

    //void SpawnPlayerToScene(int playerIndex)
    void SpawnPlayerToScene()
    {
        GameObject instantiatedPlayer = Instantiate(Resources.Load<GameObject>("Players/PlayerWithCamera"));
        instantiatedPlayer.transform.localScale = new Vector3(1f, 1f, 1f);
        instantiatedPlayer.transform.rotation = Quaternion.identity;
        if (playerIndex == 2)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(0, 0, 0);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else if (playerIndex == 3)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(27f, 0, 22.31f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -69.77f, 0f);
        }
        else if (playerIndex == 4)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(29.2486f, 0, 40.15456f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -103.669f, 0f);
        }
        else if (playerIndex == 5)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(18.38f, 0, 57.11f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -144.912f, 0f);
        }
        else if (playerIndex == 6)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(-18.76f, 0, 52.46f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, 140.332f, 0f);
        }
        if (instantiatedPlayer == null)
        {
            Debug.LogWarning("No player object found in Resources");
            return;
        }
    }

    private IEnumerator UpdatePlayersInDelay()
    {
        yield return new WaitForSeconds(0.2f);
        updatePlayerDisplayForClient();
        yield return new WaitForSeconds(1f);
        updatePlayerDisplayForClient();

        Debug.LogWarning("Updating players in delay");
    }
}