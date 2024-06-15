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

public class ServerManager : MonoBehaviour
{
    public NetworkDiscovery networkDiscovery;
    [SerializeField] private TMP_Text hostsText;
    private string hostsMessage = "No hosts found";

    // Declare serverFound as a synchronized list
    [SyncObject] private readonly SyncList<bool> serverFound = new();

    private NetworkManager _networkManager;
  //  public GameObject _networkHudCanvas;

    private void Awake()
    {
        // Listening to SyncList callbacks
  //      serverFound.OnChange += ServerFound_OnChange;
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

        //   UnityEngine.SceneManagement.SceneManager.LoadScene("CreateRoom");
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
    }

    private IEnumerator DelayedServerCheck()
    {
        UnityEngine.Debug.Log("Start searching...");
        yield return new WaitForSeconds(1f);


        // Check if any servers were found
        if (serverFound.Count == 0 || !serverFound[0])
        {
            hostsText.text = hostsMessage;
        }
        else
        {
            _networkManager.ClientManager.StartConnection(); // Also activates the canvas prefab!
               // join hosts room
          // UnityEngine.SceneManagement.SceneManager.LoadScene("CreateRoom");
            EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
            Destroy(eventSystems[0].gameObject);

            yield return new WaitForSeconds(1f); //Maybe not needed, tlet the event system time to be deleted

            LoadScene("CreateRoom");
            UnloadScene("RoomSelection");

        }
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
    void LoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        SceneLoadData sld = new(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    void UnloadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
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
