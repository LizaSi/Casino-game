using FishNet;
using FishNet.Discovery;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Authentication;
using System.Linq;
using UnityEngine.UI;
using Firebase.Database;

public class ServerManager : MonoBehaviour
{
    [SerializeField]
    private NetworkDiscovery networkDiscovery;
    [SerializeField]
    private TMP_Text _serversListLabel;

    private const int port = 7772;  // Assuming 7772 is the port used in FishNet object

    public GameObject serverEntryPrefab; // Reference to the server entry prefab
    public Transform serversListContainer; // Parent container for server entries
    private FirebaseDB firebaseManager;

    //private readonly HashSet<string> _addresses = new();
    private readonly Dictionary<string, string> serversFound = new();

    void Start()
    {
        firebaseManager = FindObjectOfType<FirebaseDB>();

        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
        networkDiscovery.ServerFoundCallback += OnServerAdvertising;
    }

    void OnServerAdvertising(EndPoint endPoint)
    {
        string newAddress = endPoint.ToString();
        Debug.LogWarning("Address published is " + newAddress); // not realy a warning, for debugging 
        
        firebaseManager.FetchServerUsername(newAddress, (username) =>
        {
            if (!string.IsNullOrEmpty(username))
            {
                serversFound[newAddress] = username;
            }
            else
            {
                Debug.LogError("Server's username was empty");
            }
        });
    }

    private void AddServerToUI(string address, string username)
    {
        _serversListLabel.text += username + "\n";
        GameObject newJoinButton = Instantiate(serverEntryPrefab, serversListContainer);
        newJoinButton.GetComponentInChildren<Text>().text = "Join " + username;
        newJoinButton.SetActive(true);

        newJoinButton.GetComponent<Button>().onClick.AddListener(() => JoinServer_ButtonClick(address));

        // Fix position of the new button
        if (newJoinButton.TryGetComponent<RectTransform>(out var rectTransform))
        {
            rectTransform.anchoredPosition = Vector2.zero; // Adjust this if needed
            rectTransform.localScale = Vector3.one;
        }
    }

    public void JoinServer_ButtonClick(string address)
    {
        Debug.Log("Joining server at address: " + address);
        Debug.LogWarning("Not implemented");
        // Add code here to join the server
    }


    public void Advertise_OnClick()
    {
        if (!networkDiscovery.IsAdvertising)
        {
           // LoggedUser.ServerAddress = GetLocalIPAddress();
            networkDiscovery.AdvertiseServer();

            string localIPAddress = GetLocalIPAddress();
            firebaseManager.AddServer(localIPAddress, LoggedUser.Username);
            SceneManager.LoadScene("CreateRoom");
        }
    }

    public void CreateServer_OnClick()
    {
        InstanceFinder.ServerManager.StartConnection();
        _serversListLabel.text = "Created successfuly";
    }

    public void SearchServer_OnClick()
    {
        if (!networkDiscovery.IsSearching)
        {
            networkDiscovery.SearchForServers();
            StartCoroutine(DelayedServerCheck());
        }
       // StartCoroutine(DelayedServerCheck());
    }

    private IEnumerator DelayedServerCheck()
    {
        // Wait for 1 second before checking for available servers
        yield return new WaitForSeconds(1f);

        // Check if any servers were found
        if (serversFound.Count > 0) // found a server
        {
            _serversListLabel.text = ""; // Clear existing text
            foreach (var serverFound in serversFound)
            {
                AddServerToUI(serverFound.Key, serverFound.Value);
                firebaseManager.AddPlayerGuestToServer(serverFound.Key);
            }
        }
        else
        {
            _serversListLabel.text = "No servers found";
        }
    }

    
    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString() + ":" +port;
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
    /*private void UpdateServerListUI(List<ServerData> servers)
    {
        // Clear previous buttons
        *//*foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var server in servers)
        {
            GameObject button = Instantiate(buttonPrefab, buttonParent);
            button.GetComponentInChildren<TMP_Text>().text = server.username; // Display the username on the button
            button.GetComponent<Button>().onClick.AddListener(() => JoinRoomOnClick(server.address));
        }*//*

        if (servers.Count > 0) // The own logged in doesnt count 
        {
            List<string> usernames = new();
            foreach (var server in servers)
            {
                usernames.Add(server.username);
            }
            string serversUsername = string.Join("\n", usernames);
            _serversListLabel.text = serversUsername;
        }
        else
        {
            _serversListLabel.text = "No servers found";
        }
    }*/

    // Update is called once per frame
    void Update()
    {
        

      /*  foreach (string address in _addresses)
        {
            if (GUILayout.Button(address))
            {
                networkDiscovery.StopSearchingOrAdvertising();

                InstanceFinder.ClientManager.StartConnection(address);
            }
        }*/
    }
}
