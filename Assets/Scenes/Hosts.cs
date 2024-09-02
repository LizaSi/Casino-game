using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*public class Hosts : NetworkBehaviour
{
    [SyncObject] private readonly SyncDictionary<string, string> AddressNameDict = new();
    private static Hosts instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static string GetHostName(string address)
    {
        Debug.LogWarning(instance.AddressNameDict.Count);
        return instance.AddressNameDict[address];
    }

    public static void AddHost(string address, string name)
    {
        instance.AddressNameDict.Add(address, name);
    } 
}*/
