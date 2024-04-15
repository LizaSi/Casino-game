using UnityEngine;
using FishNet;
using FishNet.Discovery;

public class ServerAdvertiser : MonoBehaviour
{
    [SerializeField]
    private NetworkDiscovery networkDiscovery;

    void Start()
    {
        if (networkDiscovery == null)
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
        StartServerConnection();
        AdvertiseServer();
    }

    void StartServerConnection()
    {
        // Call InstanceFinder.ServerManager.StartConnection() here
        InstanceFinder.ServerManager.StartConnection();
    }

    void AdvertiseServer()
    {
        // Find the NetworkDiscovery component and advertise the server
        //NetworkDiscovery networkDiscovery = FindObjectOfType<NetworkDiscovery>().AdvertiseServer();
        if (networkDiscovery != null)
        {
            networkDiscovery.AdvertiseServer();
        }
        else
        {
            Debug.LogError("NetworkDiscovery component not found.");
        }
    }
}
