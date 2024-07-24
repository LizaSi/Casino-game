using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    void Start()
    {
        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // Firebase is initialized successfully
                FirebaseApp app = FirebaseApp.DefaultInstance;
                Debug.Log("Firebase Initialized");

                // Now you can start using Firebase services
                // Example: Authenticate user, access Firestore, etc.
            }
            else
            {
                // Handle initialization failure
                Debug.LogError("Failed to initialize Firebase: " + task.Exception);
            }
        });
    }
}
