using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReturnToLobby : MonoBehaviour
{
    public void Button_OnClick()
    {
        if (GameServerManager.IsInitialized())
        {
            if (InstanceFinder.IsServer)
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                GameServerManager.LeaveGame();
                NetworkObject nob = canvasGame.GetComponent<NetworkObject>();
                LoadSceneAllClientsAndFuture(nob, "CreateRoom");
            }
            else
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                CardsDisplayer blackJackDisplayer = canvasGame.GetComponent<CardsDisplayer>();
                blackJackDisplayer.LeaveGame();
                InstanceFinder.ClientManager.StopConnection();
                UnityEngine.SceneManagement.SceneManager.LoadScene("RoomSelection");
            }
        }
        if (PokerServerManager.IsInitialized()) 
        {
            if (InstanceFinder.IsServer)
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                PokerServerManager.LeaveGame();
                NetworkObject nob = canvasGame.GetComponent<NetworkObject>();
                LoadSceneAllClientsAndFuture(nob, "CreateRoom");
            }
            else
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                PokerDisplayer pokerDisplayer = canvasGame.GetComponent<PokerDisplayer>();
                pokerDisplayer.LeaveGame();
                InstanceFinder.ClientManager.StopConnection();
                UnityEngine.SceneManagement.SceneManager.LoadScene("RoomSelection");
            }
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
