using FishNet;
using FishNet.Discovery;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Net;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using System.Diagnostics;
using Unity.Services.Authentication.PlayerAccounts;
using FishNet.Managing;
using UnityEngine.SceneManagement;
using FishNet.Managing.Scened;
using FishNet.Managing.Server;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using FishNet.Object;

public class ServerManager : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    [SerializeField] private Text hostsText;


    // Declare serverFound as a synchronized list
    [SyncObject] private readonly SyncList<bool> serverFound = new();

    private NetworkManager _networkManager;
    private string _address;

    //  public GameObject _networkHudCanvas;

    private void Awake()
    {
        // Listening to SyncList callbacks
        //      serverFound.OnChange += ServerFound_OnChange;
        networkDiscovery.ServerFoundCallback += endPoint => _address = (endPoint.Address.ToString());

    }

    private void Start()
    {
        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        if (_networkManager == null)
            _networkManager = FindObjectOfType<NetworkManager>();

        networkDiscovery.ServerFoundCallback += OnServerAdvertising;
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
            if (serverFound.Count == 0 || !serverFound[0])
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
        // Start the client and server connections
        yield return new WaitForSeconds(1f);

        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Destroy(eventSystems[0].gameObject);

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
        string[] dots = { ".", "..", "..."};
        hostsText.text = "Searching" + dots[i];
        yield return new WaitForSeconds(1f);

        // Check if any servers were found
        while (serverFound.Count == 0 || !serverFound[0])
        {
            i++;
            hostsText.text = "Searching" + dots[i % 3];
            yield return new WaitForSeconds(0.5f);
        }

        InstanceFinder.ClientManager.StartConnection(_address);

        hostsText.text = "Connected!";
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        Destroy(eventSystems[0].gameObject);

        yield return new WaitForSeconds(3f); //Maybe not needed, tlet the event system time to be deleted

        if (!LoadScene("CreateRoom"))
        {
            yield return new WaitForSeconds(3f);
            LoadScene("CreateRoom");
        }
        UnloadScene("RoomSelection");
    }

    private void OnServerAdvertising(EndPoint endPoint)
    {
        // Update the synchronized list serverFound
        if (serverFound.Count == 0)
        {
            serverFound.Add(true);
            string newAddress = endPoint.ToString();
            UnityEngine.Debug.LogWarning("Address published is " + newAddress);
            UnityEngine.Debug.Log($"Server found at: {endPoint}");
        }
    }

    bool LoadScene(string sceneName)
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

    void UnloadScene(string sceneName)
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
