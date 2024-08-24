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
using FishNet.Transporting.Tugboat;
using System.Linq;
using FishNet.Object;
using PlayerData;
using System.Net.Sockets;
using System;
using System.Collections.Generic;

public class ServerManager : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    [SerializeField] private Text hostsText;
    [SerializeField] private List<Button> serverButtons; // Assign these buttons in the Inspector

    [SyncObject] private readonly SyncDictionary<string,string> serversFound = new();
   // [SyncObject] private readonly SyncDictionary<string,string> serverHosts = new();

    private NetworkManager _networkManager;

   // private string _address;

    //  public GameObject _networkHudCanvas;

    private void Awake()
    {
        //      serverFound.OnChange += ServerFound_OnChange;
        // Founder runs it
        networkDiscovery.ServerFoundCallback += (endPoint, username) => serversFound[endPoint.Address.ToString()] = username;
    }

    private void Start()
    {
        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        if (_networkManager == null)
            _networkManager = FindObjectOfType<NetworkManager>();

     //   networkDiscovery.ServerFoundCallback += OnServerAdvertising;
    }

    private void ServerFound_OnChange(SyncListOperation op, int index, bool oldItem, bool newItem, bool asServer)
    {
        // Handle changes in serverFound list
        UpdateHostText();
    }

    private void UpdateHostText()
    {
        if (hostsText != null)
        {
            if (serversFound.Count == 0 /*|| !serversFound[0]*/)
            {
                hostsText.text = "No Hosts found";
            }
        }
    }

    public void Advertise_OnClick()
    {
        if (networkDiscovery != null && _networkManager != null)
        {
            _networkManager.ClientManager.StartConnection();
            StopCoroutine(DelayedServerCheck());
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
            StopCoroutine(DelayedServerCheck());
            StartCoroutine(DelayedSceneLoad());
        }
    }

    private IEnumerator DelayedSceneLoad()
    {
        hostsText.text = "Creating room...";

        //string hostAddress = GetLocalIPAddress();
       // Hosts.AddHost(hostAddress, LoggedUser.Username);
        //serverHosts[hostAddress] = LoggedUser.Username;

        // Start the client and server connections
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
            StartCoroutine(DelayedServerCheck());
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
            StartCoroutine(DelayedServerCheck());
        }
    }

    private IEnumerator DelayedServerCheck()
    {
        int i = 0;

        string[] dots = { ".", "..", "..." };

        hostsText.text = "Searching" + dots[i];
        yield return new WaitForSeconds(1f);

        // Check if any servers were found
        while (serversFound.Count == 0 /*|| !serversFound[0]*/)
        {
            // tugboat.SetClientAddress(_address);
            i++;
            hostsText.text = "Searching" + dots[i % 3];
            yield return new WaitForSeconds(0.5f);
        }

        hostsText.text = "Found!";
        int index = 0; // To track which button to use
        foreach (var address in serversFound)
        {
            if (index < serverButtons.Count)
            {
                var button = serverButtons[index];
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = $"{address.Value}'s Room";

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartCoroutine(ServerFound_OnClick(address.Key)));

                index++; 
            }
        }
    }

    public IEnumerator ServerFound_OnClick(string address)
    {
        InstanceFinder.ClientManager.StartConnection(address);

        hostsText.text = "Connected!";
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Destroy(eventSystems[0].gameObject);

        yield return new WaitForSeconds(2f); //Maybe not needed, to let the event system time to be deleted

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
        // serversFound[endPoint.ToString()] = LoggedUser.Username;
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
