using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
