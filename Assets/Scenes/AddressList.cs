using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AddressList
{
    public struct Device
    {
        public string server;
        public string username;
    }
    public static HashSet<Device> Devices { get; set; }
    public static List<string> Usernames()
    {
        List<string> usernames = new();
        foreach (var device in Devices)
        {
            usernames.Add(device.username);
        }
        return usernames;
    }
    public static void Add(string address ,string username)
    {
        Device device = new()
        {
            server = address,
            username = username
        };
        Devices.Add(device);
    }
    
}
