using FishNet;
using FishNet.Discovery;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using UnityEngine;
using System.Net;

public class ServerManager : MonoBehaviour
{
    [SerializeField]
    private NetworkDiscovery networkDiscovery;
    [SerializeField]
    private TMP_Text _serversListLabel;

    private readonly HashSet<string> _addresses = new HashSet<string>();

    void Start()
    {
        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        networkDiscovery.ServerFoundCallback += endPoint =>
        {
            _addresses.Add(endPoint.Address.ToString());
            AddressList.Add(endPoint.Address.ToString(), AuthenticationService.Instance.PlayerName);
        };
    }

    public void Advertise_OnClick()
    {
        if (!networkDiscovery.IsAdvertising)
        {
            networkDiscovery.AdvertiseServer();
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
        }

        StartCoroutine(DelayedServerCheck());
    }
    private IEnumerator DelayedServerCheck()
    {
        // Wait for 1 second before checking for available servers
        yield return new WaitForSeconds(1f);

        // Check if any servers were found
        if (_addresses.Count > 0) // found a server
        {
            SceneManager.LoadScene("JoinARoom");
            string addressesText = string.Join(", ", _addresses);
            _serversListLabel.text = addressesText;
        //    AddressList.Addresses = _addresses;

        }
        else
        {
            _serversListLabel.text = "No servers found";
        }
    }

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
