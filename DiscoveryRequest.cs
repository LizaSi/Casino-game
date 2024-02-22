using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryRequest : NetworkMessage
{
    public string language = "en";

    // Add properties for notification data in mobile, user clients
    // in their broadcast messages that servers will consume.
}

public class DiscoveryResponse : NetworkMessage
{
    enum GameMode { PvP, PvE };

    // uri object for clients know how to connect to the server
    public Uri uri;

    public GameMode GameMode;
    public int TotalPlayers;
    public int HostPlayerName;

    // Add properties for notification data for server to return to
    // clients for them to display or consume for establishing a connection.
}