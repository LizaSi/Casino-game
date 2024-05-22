using FishNet;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class JoinAddress : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textUserNearby;
    [SerializeField]
    private GameObject buttonPrefab; // Prefab for the buttons
    [SerializeField]
    private Transform buttonParent; // Parent transform to hold the buttons
    void Start()
    {
        DisplayNearbyUsers();
        CreateServerButtons();
    }

    private void DisplayNearbyUsers()
    {
        if (AddressList.Devices != null && AddressList.Devices.Count > 0)
        {
            List<string> usernames = AddressList.Usernames();
            string usernamesText = string.Join("\n", usernames);
            textUserNearby.text = usernamesText;
        }
        else
        {
            textUserNearby.text = "No nearby users found.";
        }
    }

    private void CreateServerButtons()
    {
        foreach (var device in AddressList.Devices)
        {
            GameObject button = Instantiate(buttonPrefab, buttonParent);
            button.GetComponentInChildren<TMP_Text>().text = device.username; // Display the username on the button
            button.GetComponent<Button>().onClick.AddListener(() => JoinRoomOnClick(device.server));
        }
    }

    private void JoinRoomOnClick(string serverAddress)
    {
        InstanceFinder.ClientManager.StartConnection(serverAddress);
        SceneManager.LoadScene("LobbyRoom");
    }
}
