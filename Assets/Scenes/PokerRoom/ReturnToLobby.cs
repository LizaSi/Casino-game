using FishNet;
using FishNet.Discovery;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ReturnToLobby : MonoBehaviour
{
    public void Button_OnClick()
    {
        if (GameServerManager.IsInitialized())
        {
            if (InstanceFinder.IsServer)
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                CardsDisplayer blackJackDisplayer = canvasGame.GetComponent<CardsDisplayer>();
                blackJackDisplayer.ClientComponentsParent.SetActive(false);
                blackJackDisplayer.newRoundButton.gameObject.SetActive(false);
                NetworkObject nob = canvasGame.GetComponent<NetworkObject>();
                LoadSceneAllClientsAndFuture(nob, "CreateRoom");
            }
            else
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                CardsDisplayer blackJackDisplayer = canvasGame.GetComponent<CardsDisplayer>();
                blackJackDisplayer.ClientComponentsParent.SetActive(false);
                UnityEngine.SceneManagement.SceneManager.LoadScene("RoomSelection");
            }
        }
        else if (PokerServerManager.IsInitialized()) 
        {
            if (InstanceFinder.IsServer)
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                PokerDisplayer pokerDisplayer = canvasGame.GetComponent<PokerDisplayer>();
                pokerDisplayer.PokerComponentsParent.SetActive(false);
                NetworkObject nob = canvasGame.GetComponent<NetworkObject>();
                LoadSceneAllClientsAndFuture(nob, "CreateRoom");
            }
            else
            {
                GameObject canvasGame = GameObject.Find("CanvasGame(Clone)");
                PokerDisplayer pokerDisplayer = canvasGame.GetComponent<PokerDisplayer>();
                pokerDisplayer.PokerComponentsParent.SetActive(false);
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
