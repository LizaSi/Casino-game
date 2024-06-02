using FishNet;
using FishNet.Object;
using FishNet.Managing.Scened;

public class NetworkManager : MonoBehaviour
{
    private void Start()
    {
        // Automatically start the server.
        if (InstanceFinder.IsServer)
            InstanceFinder.ServerManager.StartConnection();
        else
            InstanceFinder.ClientManager.StartConnection();
    }
}
