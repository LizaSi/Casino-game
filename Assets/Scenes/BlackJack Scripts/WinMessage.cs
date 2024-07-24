using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinMessage : NetworkBehaviour
{
    [SerializeField]
    private GameObject canvasToSpawn;
  //  private TMP_Text winText;

    [HideInInspector] private GameObject spawnedObject;
    // Start is called before the first frame update
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
            GetComponent<WinMessage>().enabled = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5) && spawnedObject == null)
        {
            SpawnText(canvasToSpawn, transform ,this);
        }
        if(Input.GetKeyDown(KeyCode.Alpha6) && spawnedObject != null) 
        {
            DespawnObject(spawnedObject);
        }
    }

    [ServerRpc]
    private void SpawnText(GameObject canvas,Transform t ,WinMessage script)
    {
       // string winMessage = GameServerManager.DidIWin(base.Owner) ? "You win!" : "You lost...";
   //     canvas.GetComponentInChildren<TMP_Text>().text = winMessage;

        GameObject toSpawn = Instantiate(canvas, t, script);
        ServerManager.Spawn(toSpawn);

        SetSpawnedObjec(toSpawn, script);
    }

    [ObserversRpc]
    private void SetSpawnedObjec(GameObject canvas, WinMessage script)
    {
        script.spawnedObject = canvas;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnObject(GameObject obj)
    {
        ServerManager.Despawn(obj);
    } 

}
