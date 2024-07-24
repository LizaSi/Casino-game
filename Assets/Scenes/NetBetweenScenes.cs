using FishNet;
using FishNet.Managing.Scened;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetBetweenScenes : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            LoadScene("Lobby");
            UnloadScene("CreateRoom");
        }
    }

    void LoadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        SceneLoadData sld = new SceneLoadData(sceneName);
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    void UnloadScene(string sceneName)
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        SceneUnloadData sld = new SceneUnloadData(sceneName);
        InstanceFinder.SceneManager.UnloadGlobalScenes(sld);
    }
}
