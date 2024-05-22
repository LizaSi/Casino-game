using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

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
        /*    databaseReference.Child("Servers").OrderByChild("address").EqualTo(address).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    // Server address doesn't exist in the database, add it
                    string key = databaseReference.Child("Servers").Push().Key;
                    ServerData serverData = new ServerData(address, username);
                    string json = JsonUtility.ToJson(serverData);

                    databaseReference.Child("Servers").Child(key).SetRawJsonValueAsync(json).ContinueWithOnMainThread(setTask =>
                    {
                        if (setTask.IsFaulted)
                        {
                            Debug.LogError("Error adding server: " + setTask.Exception);
                        }
                        else
                        {
                            Debug.Log("Server added successfully.");
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Error checking for existing server: " + task.Exception);
            }
        });*/
    }

    public void FetchServerUsername(string address, System.Action<string> callback)
    {
        databaseReference.Child("servers").OrderByChild("address").EqualTo(address).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching server username: " + task.Exception);
                callback(null);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot serverSnapshot in snapshot.Children)
                {
                    string username = serverSnapshot.Child("username").Value.ToString();
                    callback(username);
                    return;
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
