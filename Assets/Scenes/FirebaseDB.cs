using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;
using Unity.Services.Authentication.PlayerAccounts;

public class FirebaseDB : MonoBehaviour
{
    private DatabaseReference databaseReference;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    const int MaxServers = 100;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
     //   FirebaseApp app = FirebaseApp.DefaultInstance;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        if (databaseReference == null)
            Debug.LogError("db reference is null");
    }

    public void AddServer(string address, string username)
    {
        DatabaseReference newServerRef = databaseReference.Child("servers").Push();
        newServerRef.Child("address").SetValueAsync(address);
        newServerRef.Child("username").SetValueAsync(username);
    }

    public void AddPlayerGuestToServer(string address)
    {
        // Add "PlayerGuest": LoggedUser.Username to the server node in the database
        databaseReference.Child("servers").OrderByChild("address").EqualTo(address).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error adding PlayerGuest to server: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot serverSnapshot in snapshot.Children)
                {
                    string serverAddress = serverSnapshot.Child("address").Value.ToString();
                    if (serverAddress == address)
                    {
                        // Add logged user as PlayerGuest to the server node
                        databaseReference.Child("servers").Child(serverSnapshot.Key).Child("PlayerGuest").SetValueAsync(LoggedUser.Username)
                            .ContinueWithOnMainThread(addTask =>
                            {
                                if (addTask.IsFaulted)
                                {
                                    Debug.LogError("Error adding PlayerGuest to server: " + addTask.Exception);
                                }
                                else if (addTask.IsCompleted)
                                {
                                    Debug.Log("PlayerGuest added to server successfully.");
                                }
                            });
                        return;
                    }
                }
                Debug.LogError("No matching address found in DB: " + address);
            }
        });
    }

    public void FetchServerUsername(string address, System.Action<string> callback)
    {
        databaseReference.Child("servers").OrderByChild("address").EqualTo(address).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching server username: " + task.Exception);
                callback(null);
                return;
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot serverSnapshot in snapshot.Children)
                {
                    string serverAddress = serverSnapshot.Child("address").Value.ToString();
                    if (serverAddress == address)
                    {
                        // Get the username from the sibling node
                        string username = serverSnapshot.Child("username").Value.ToString();
                        if (string.IsNullOrEmpty(username))
                        {
                            Debug.LogError("Username is null or not found in DB for address " + address);
                            username = "Guest";
                        }
                        callback(username);
                        return;
                    }
                }
                callback(null);
            }
        });
    }

    public void FetchServers(System.Action<List<ServerData>> callback)
    {
        if (databaseReference == null)
        {
            Debug.LogError("Database reference is null. Firebase may not be initialized properly.");
            return;
        }
        databaseReference.Child("Servers").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching servers: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            List<ServerData> servers = new();

            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string json = childSnapshot.GetRawJsonValue();
                ServerData server = JsonUtility.FromJson<ServerData>(json);
                servers.Add(server);
            }

            callback(servers);
        });
    }

    public void RemoveServer(string address)
    {
        databaseReference.Child("Servers").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error removing server: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                ServerData server = JsonUtility.FromJson<ServerData>(childSnapshot.GetRawJsonValue());
                if (server.address == address)
                {
                    databaseReference.Child("Servers").Child(childSnapshot.Key).RemoveValueAsync();
                    break;
                }
            }
        });
    }
    void OnApplicationQuit()
    {
        // Check if the database reference is valid
        if (databaseReference != null)
        {
            // Remove all servers from the database
            databaseReference.Child("Servers").RemoveValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Servers removed from database on application exit.");
                }
                else
                {
                    Debug.LogError("Error removing servers from database on application exit: " + task.Exception);
                }
            });
        }
    }
}


[System.Serializable]
public class ServerData
{
    public string address;
    public string username;

    public ServerData(string address, string username)
    {
        this.address = address;
        this.username = username;
    }
}
