using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine.UI;
using FishNet.Managing.Scened;
using FishNet;
using UnityEngine.EventSystems;
using UnityEngine.AdaptivePerformance.VisualScripting;

public class MemberList : NetworkBehaviour
{
    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, string> _playerNames = new();
    // Passing network con because need to disconnect a user when leave


    private static MemberList _instance;

    private void Awake()
    {
        _instance = this;
        SetName(LoggedUser.Username);
        var eventSystems = FindObjectsOfType<EventSystem>();
        //      _playerNames.OnChange += _playerNames_OnChange;
    }

    public void HitTest_OnClick()
    {
        SetName("Set new name worked");
    }

    private void Update()
    {
        //   if(!GetMembers(base.Owner).Contains(LoggedUser.Username))
        if (Input.GetKeyDown(KeyCode.F))
        {
            SetName(LoggedUser.Username);
        }
    }

    public static string GetMembers(NetworkConnection conn)
    {
        StringBuilder sb = new();

        if (_instance == null || _instance._playerNames == null) 
        {
          //  Debug.LogWarning("Member list not yet initialized");
            return "";
        }

        foreach (string username in _instance._playerNames.Values)
        {
            sb.AppendLine(username);
        }

        return sb.ToString();
    }

    public static string GetMember(NetworkConnection conn)
    {
        StringBuilder sb = new();

        if (_instance == null || _instance._playerNames == null)
        {
            //  Debug.LogWarning("Member list not yet initialized");
            return "";
        }
        return _instance._playerNames[conn];
    }

    [Client]
    public static void SetName(string name)
    {
        _instance.ServerSetName(name);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSetName(string name, NetworkConnection sender = null)
    {
        _playerNames[sender] = name;
        Debug.Log(name + " is set");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetName(LoggedUser.Username);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        EnableStartButton();
    }

    private void EnableStartButton()
    {
        GameObject startButtonObject = GameObject.Find("Canvas/BackgroundPanel/StartButton");
        if (startButtonObject != null)
        {
            startButtonObject.SetActive(true);
        }
        else
        {
            Debug.LogError("StartButton not found in the scene.");
        }

    }
    public void StartButton_OnClick()
    {
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName("BlackJackRoom"));
            //SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName("Lobby"));
        }

        GameObject playerPrefab = GameObject.Find("BasicPlayerPrefab(Clone)");
        //GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
        if (playerPrefab != null)
        //if (canvasGame != null)
        {
            // Retrieve the NetworkObject component attached to prefab
            NetworkObject nob = playerPrefab.GetComponent<NetworkObject>();
            //NetworkObject nob = canvasGame.GetComponent<NetworkObject>();
            if (nob != null)
            {
                LoadScene2(nob, "BlackJackRoom");
                //LoadScene2(nob, "Lobby");
                UnloadScene("CreateRoom");
            }
        }
        else
        {
            Debug.LogWarning("canvas members prefab is null");
        }
      //  UnloadScene("CreateRoom");
     //      UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    private void LoadScene2(NetworkObject nob, string sceneName)
    {
        if (!nob.Owner.IsActive)
        {
            Debug.LogWarning("Net obj is not active");
            return;
        }

        SceneLoadData sld = new(sceneName)
        {
            MovedNetworkObjects = new NetworkObject[] { nob },
            ReplaceScenes = ReplaceOption.All
        };
      //  InstanceFinder.SceneManager.LoadConnectionScenes(nob.Owner, sld);
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);

    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        base.NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
    {
        if(arg2.ConnectionState != RemoteConnectionState.Started)
        {
            _playerNames.Remove(arg1);
        }
    }

    void LoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        SceneLoadData sld = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadConnectionScenes(sld);
    }

    void UnloadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);

        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Destroy(eventSystems[0].gameObject);        
    }
}
