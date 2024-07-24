using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class ClientOperations : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private TMP_Text cardsText;

    void Start()
    {
       // cardsText.text = GameServerManager.GetPlayerHand()
    }

    public void Hit_OnClick()
    {
        GameServerManager.HitCard();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.F))
        {
            GameServerManager.HitCard();
        }*/
    }
}
