using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Text;
using UnityEngine;
using PlayerData;
using FishNet.Managing.Scened;
using FishNet;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MemberList : NetworkBehaviour
{
    [SerializeField] private TMP_Text displayText;
    
    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, string> _playerNames = new();
    // Passing network con because need to disconnect a user when leave

    private static MemberList _instance;

    private void Awake()
    {
        _instance = this;
        SetName(LoggedUser.Username);
    }

    public static void SetLobbyList(SyncDictionary<NetworkConnection, string> playerNamesDict)
    {
        StringBuilder sb = new();
        foreach (var user in playerNamesDict)
        {
            _instance._playerNames[user.Key] = user.Value;
        }
        UpdateLobbyList();
    }


    public static void UpdateLobbyList()
    {
        StringBuilder sb = new();
        foreach (string username in _instance._playerNames.Values)
        {
            sb.AppendLine(username);
        }
        _instance.displayText.text = sb.ToString();
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
        if (_instance == null || _instance._playerNames == null)
        {
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
        UpdateLobbyList();
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

        EnableStartButtons();
    }

    private void EnableStartButtons()
    {
        GameObject backgroundPanel = GameObject.Find("Canvas/BackgroundPanel");
        Button[] buttons = backgroundPanel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.gameObject.SetActive(true);
        }
    }

    public void StartBlackjack_OnClick()
    {
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName("Lobby"));
        }

        GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
        if (canvasGame != null)
        {
            if (canvasGame.TryGetComponent<NetworkObject>(out var nob))
            {
               // UpdateLobbyList(false);
                LoadSceneAllClientsAndFuture(nob, "Lobby");
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
    public void StartPoker_OnClick()
    {
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            SceneManager.AddConnectionToScene(conn, UnityEngine.SceneManagement.SceneManager.GetSceneByName("PokerRoom"));
        }

        GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
        if (canvasGame != null)
        {
           // UpdateLobbyList(false); 
            if (canvasGame.TryGetComponent<NetworkObject>(out var nob))
            {
                LoadSceneAllClientsAndFuture(nob, "PokerRoom");
                UnloadScene("CreateRoom");
            }
        }
        else
        {
            Debug.LogWarning("canvas members prefab is null");
        }
    }

    private void LoadSceneAllClientsAndFuture(NetworkObject nob, string sceneName)
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
            UpdateLobbyList();
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
