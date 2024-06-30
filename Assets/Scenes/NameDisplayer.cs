using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class NameDisplayer : NetworkBehaviour
{
    [SerializeField]
    public TMP_Text displayText;
    private Coroutine memberListCoroutine;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetName();
        memberListCoroutine = StartCoroutine(UpdateNamePeriodically(0.5f)); // Update every 0.5 second
        GameServerManager.OnInitialized += OnGameServerManagerInitialized;
    }

    private void OnGameServerManagerInitialized()
    {
        StopCoroutine(memberListCoroutine); // If the game began, stop showing the member list
        displayText.text = "";
        return;
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        SetName();
    }
    
    private IEnumerator UpdateNamePeriodically(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            SetName();
        }
    }

    public void SetName()
    {
        string members = null;
        if (base.Owner.IsValid)
        {
            members = MemberList.GetMembers(base.Owner);
        }
        if(InstanceFinder.IsServer)
            displayText.text = members;
        else
        {
            displayText.fontSize = 19f;
            displayText.text = "Lobby members:\n" + members;
        }
    }
}
