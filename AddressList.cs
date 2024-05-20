using System.Collections.Generic;

public static class AddressList
{
    public struct Device
    {
        public string server;
        public string username;

        public override bool Equals(object obj)
        {
            return obj is Device device && server == device.server;
        }

        public override int GetHashCode()
        {
            return server.GetHashCode();
        }
    }

    static AddressList()
    {
        Devices = new HashSet<Device>();
    }

    public static HashSet<Device> Devices { get; private set; }

    public static List<string> Usernames()
    {
        List<string> usernames = new();
        foreach (var device in Devices)
        {
            usernames.Add(device.username);
        }
        return usernames;
    }

    public static void Add(string address, string username)
    {
        Device device = new()
        {
            server = address,
            username = username
        };
        Devices.Add(device);
    }
}