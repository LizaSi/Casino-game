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
        HashSet<string> addresses = AddressList.Addresses;
        if (addresses != null && addresses.Count > 0)
        {
            string addressesText = string.Join(", ", addresses);
            textUserNearby.text = addressesText;
        }
    }

    public void JoinRoom1_OnClick() // Need to create a list of buttons, where each button has a matching address.
    {
        //networkDiscovery.StopSearchingOrAdvertising();
        string address = AddressList.Addresses.First();
        InstanceFinder.ClientManager.StartConnection(address);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
