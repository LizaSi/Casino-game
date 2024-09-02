using FishNet;
using FishNet.Discovery;
using System.Collections;
using UnityEngine;
using System.Net;
using FishNet.Broadcast;
using FishNet.Object.Synchronizing;
using FishNet.Managing;
using FishNet.Managing.Scened;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FishNet.Object;
using PlayerData;
using System.Collections.Generic;
using FishNet.Transporting;

public class ServerManager : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    [SerializeField] private Text hostsText;
    [SerializeField] private List<Button> serverButtons;

    [SyncObject] private readonly SyncDictionary<string,string> serversFound = new();
    private Dictionary<string, float> serverLastSeenTimes = new Dictionary<string, float>();


    private NetworkManager _networkManager;

    private void Awake()
    {
        serversFound.OnChange += ServerFound_OnChange;
        networkDiscovery.ServerFoundCallback += OnServerFound;
    }

    private void OnServerFound(IPEndPoint endPoint, string username)
    {
        serversFound[endPoint.Address.ToString()] = username;
        DisplayServerButtons();
        serverLastSeenTimes[endPoint.Address.ToString()] = Time.time; // Update the last seen time
    }

    private void OnServerConnectionStateChanged(ServerConnectionStateArgs stateArgs)
    {
        if (stateArgs.ConnectionState == LocalConnectionState.Stopped)
        {
            Debug.LogWarning("A server stopped! ");
        }
    }

    private void Start()
    {
        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        if (_networkManager == null)
        {
            _networkManager = FindObjectOfType<NetworkManager>();
        }
        _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionStateChanged;
    }

    private void CheckServerStatus()
    {
        List<string> serversToRemove = new();

        foreach (var server in serversFound)
        {
            string address = server.Key;

            if (Time.time - serverLastSeenTimes[address] > 2f) 
            {
                serversToRemove.Add(address);
            }
        }

        foreach (var address in serversToRemove)
        {
            serversFound.Remove(address);
            serverLastSeenTimes.Remove(address);
            RemoveAllButtons();
            DisplayServerButtons();
        }
    }

    private void ServerFound_OnChange(SyncDictionaryOperation op, string key, string value, bool asServer)
    {
            DisplayServerButtons();
    }

    public void Advertise_OnClick()
    {
        if (networkDiscovery != null && _networkManager != null)
        {
            _networkManager.ClientManager.StartConnection();
            StartCoroutine(DelayedSceneLoad());
        }
        else
        {
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            _networkManager = FindObjectOfType<NetworkManager>();
            networkDiscovery.StopAllCoroutines();
            _networkManager.StopAllCoroutines();
            networkDiscovery.StopSearchingOrAdvertising();
            _networkManager.ClientManager.StartConnection();
            StartCoroutine(DelayedSceneLoad());
        }
    }

    private IEnumerator DelayedSceneLoad()
    {
        hostsText.text = "Creating room...";

        yield return new WaitForSeconds(1f);

        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        if (eventSystems.Length > 0)
        {
            Destroy(eventSystems[0].gameObject);
        }

        _networkManager.ServerManager.StartConnection();
        networkDiscovery.AdvertiseServer();

        yield return new WaitForSeconds(1f);

        LoadScene("CreateRoom");
        UnloadScene("RoomSelection");
    }

    public void SearchServer_OnClick()
    {
        if (!networkDiscovery.IsSearching)
        {
            networkDiscovery.SearchForServers();
            InvokeRepeating(nameof(CheckServerStatus), 8f, 2f);
            if (serversFound.Count > 0)
            {
                DisplayServerButtons();
            }
        }
        else if (networkDiscovery == null || _networkManager == null)
        {
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            _networkManager = FindObjectOfType<NetworkManager>();
            networkDiscovery.StopAllCoroutines();
           _networkManager.StopAllCoroutines();
            _networkManager.ClientManager.StopConnection();
            _networkManager.ServerManager.StopConnection(false);
            _networkManager.ClientManager.StartConnection();
            networkDiscovery.SearchForServers();
            if (serversFound.Count > 0)
            {
                DisplayServerButtons();
            }
        }
    }

    private void DisplayServerButtons()
    {
       // hostsText.text = "Found!";
        int index = 0;
        foreach (var server in serversFound)
        {
            var button = serverButtons[index];
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<Text>().text = $"{server.Value}'s Room";            

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => StartCoroutine(ServerFound_OnClick(server.Key)));
            index++;
        }
    }

    private void RemoveAllButtons()
    {
        for (int i = 0; i < serverButtons.Count; i++)
        {
            serverButtons[i].gameObject.SetActive(false);
        }

    }

    public IEnumerator ServerFound_OnClick(string address)
    {
        InstanceFinder.ClientManager.StartConnection(address);
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Destroy(eventSystems[0].gameObject);

        yield return new WaitForSeconds(2f); //Maybe not needed, to let the event system time to be deleted

        hostsText.text = "Connected!";
        if (!LoadScene("CreateRoom"))
        {
            yield return new WaitForSeconds(3f);
            LoadScene("CreateRoom");
        }
        UnloadScene("RoomSelection");
    }

    [Server]
    private void OnServerAdvertising(EndPoint endPoint, string username)
    {        
        string newAddress = endPoint.ToString();
        Debug.LogWarning($"{LoggedUser.Username}'s Address published is " + newAddress);
        Debug.Log($"Server found at: {endPoint}");
    }

    private bool LoadScene(string sceneName)
    {
       /* if (!InstanceFinder.IsServer)
        {
            //hostsText.text = "Error connecting to host";
            return false;
        }*/
        SceneLoadData sld = new(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        return true;
    }

    private void UnloadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
           // UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            return;
        }
        SceneUnloadData sld = new(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
    }

    private void OnDestroy()
    {
        networkDiscovery.ServerFoundCallback -= OnServerAdvertising;
        networkDiscovery.ServerFoundCallback -= OnServerFound;

        MemberList.UpdateLobbyList();
   //     UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}

public struct TeamMember : IBroadcast
{
    public string username;
    public string coins;

    public TeamMember(string _user, string _coins)
    {
        this.username = _user;
        this.coins = _coins;
    }

    public void Serialize(FishNet.Serializing.Writer writer)
    {
        writer.WriteString(username);
        writer.WriteString(coins);
    }

    public void Deserialize(FishNet.Serializing.Reader reader)
    {
        username = reader.ReadString();
        coins = reader.ReadString();
    }
}
