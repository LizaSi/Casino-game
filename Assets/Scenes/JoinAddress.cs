using FishNet.Discovery;
using FishNet;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using System.Linq;

public class JoinAddress : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private TMP_Text textUserNearby;
    void Start()
    {
        
        if (AddressList.Devices != null && AddressList.Devices.Count > 0)
        {
            textUserNearby.text = AddressList.Usernames().ToString();
        }
    }

    public void JoinRoom1_OnClick() // Need to create a list of buttons, where each button has a matching address.
    {
        //networkDiscovery.StopSearchingOrAdvertising();
        string serverAddress = AddressList.Devices.First().server;
        InstanceFinder.ClientManager.StartConnection(serverAddress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
