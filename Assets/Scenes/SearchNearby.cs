using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Discovery;
using Unity.VisualScripting;

public class Load : MonoBehaviour
{
    public GameObject searchingText;
    public Text resultText;
    private NetworkDiscovery net;

    public void CreateLobby()
    {
        InstanceFinder.ServerManager.StartConnection();
        if (net.IsAdvertising)
        {
            /*if (GUILayout.Button("Stop", buttonHeight))
                networkDiscovery.StopSearchingOrAdvertising();*/
        }
        else
        {
            net.AdvertiseServer();
        }
    }

   /* public void StartSearch()
    {
        searchingText.gameObject.SetActive(true);
        try
        {
            FindObjectOfType<NetworkDiscovery>().SearchForServers();
        }
        catch (System.Exception e)
        {
            net.SearchForServers();
        }
     //   resultText.gameObject.SetActive(true);
        SceneManager.LoadScene("JoinARoom");
       // SearchForUsers();
    }*/

    private void SearchForUsers()
    {
        Invoke("UserFound", 2f);
    }

    private void UserFound()
    {
     //   resultText.text = "User Found!";
        searchingText.gameObject.SetActive(false);	
    }
}
