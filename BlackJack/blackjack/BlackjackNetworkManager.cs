using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using UnityEngine;

public class BlackjackNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        // Ensure this script persists across scene changes.
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // Automatically connect server and clients upon starting.
        if (InstanceFinder.IsServer && InstanceFinder.IsClient)
            InstanceFinder.ServerManager.StartConnection();
        else
            InstanceFinder.ClientManager.StartConnection();
    }
}
