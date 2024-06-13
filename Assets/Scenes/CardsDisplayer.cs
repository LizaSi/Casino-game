using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GameServerManager;

public class CardsDisplayer : NetworkBehaviour
{
    [SerializeField]
    public TMP_Text membersText;
    public TMP_Text cardsText;
    public Button hitButton;
    public Button checkButton;
    public TMP_Text winText;

    //   private bool cardsInitialized = false;
    [SyncVar] private bool roundFinished = false;
    private bool dealerChecked = false;

    private void OnEnable()
    {
        hitButton.gameObject.SetActive(false);
        GameServerManager.OnInitialized += OnGameServerManagerInitialized;
        GameServerManager.OnTurnPass += OnTurnPass;
    }

    private void OnDisable()
    {
        GameServerManager.OnInitialized -= OnGameServerManagerInitialized;
        GameServerManager.OnTurnPass -= OnTurnPass;
    }

    private void OnGameServerManagerInitialized()
    {
        UpdateCardsDisplay();
        StartCoroutine(UpdateCardsPeriodically(3f)); // Update every 1 second
    }

    private void OnTurnPass()
    {
        if (InstanceFinder.IsServer)
            handleDealerTurn();
        else if (base.Owner.IsLocalClient)
            handleClientTurn();
    }

    private IEnumerator UpdateCardsPeriodically(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            UpdateCardsDisplay();

            if (InstanceFinder.IsServer)
                handleDealerTurn();
            else if (base.Owner.IsLocalClient)
                handleClientTurn();
        }
    }

    public void Check_OnClick()
    {
        ClientCheck();
    }

    public void Hit_OnClick()
    {
        HitCard();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        //    GameServerManager.OnHandChanged -= HandleHandChanged;
    }

    private void ShowWinMessage()
    {
        if (InstanceFinder.IsServer || !base.Owner.IsLocalClient)
        {
            winText.text = "";
            return;
        }

        GameResult result = DidIWin(base.Owner);

        switch (result)
        {
            case GameResult.Win:
                winText.text = "You win!";
                break;
            case GameResult.Lose:
                winText.text = "You lost...";
                break;
            case GameResult.Tie:
                winText.text = "Tie!";
                break;
        }
    }

    private void HandleHandChanged(NetworkConnection conn, List<string> hand)
    {
        if (conn != base.Owner)
            return;
        UpdateCardsDisplay();
    }

      
    void Update()
    {
        /*if(IsMyTurn(base.Owner))
            handleClientTurn();*/

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (InstanceFinder.IsServer)
                handleDealerTurn();
            else if (base.Owner.IsLocalClient)
                handleClientTurn();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ShowWinMessage();
        }
    }    

    private void handleDealerTurn()
    {
        if (IsMyTurn(base.Owner)) // If reached to this and was true after false, it means round finishes, need to reveal your cards
        {
            if (!dealerChecked)
            {
                Debug.LogWarning("Server checked");
                ClientCheck();
                dealerChecked = true;
                return;
            }
            List<string> cards = GetAllPlayerHands(base.Owner);
            if (Deck.GetHandValue(cards[0]) < 17)
                HitCard();
            else
            {
              //  roundFinished = true;
                Debug.LogWarning("Round finished. showing results");
            }
        }
    }

    private void handleClientTurn()
    {
        if (IsMyTurn(base.Owner))
        {
            hitButton.gameObject.SetActive(true);
            checkButton.gameObject.SetActive(true);
        }
        else
        {
            hitButton.gameObject.SetActive(false);
            checkButton.gameObject.SetActive(false);
        }

        if (roundFinished)
        {
            ShowWinMessage();
        }
    }

    private void UpdateCardsDisplay()
    {
        if (base.Owner.IsValid)
        {
            if (InstanceFinder.IsServer)
            {
                List<string> cards = GetAllPlayerHands(base.Owner);
                cardsText.text = "Players cards:\n" + string.Join("\n", cards);                
            }
            else if (base.Owner.IsLocalClient)
            {
                string cards = GameServerManager.GetPlayerHand(base.Owner);
                int playerIndex = GameServerManager.GetPlayerIndex(base.Owner);

                cardsText.text = $"{LoggedUser.Username}'s ({playerIndex}) Cards: " + string.Join(", ", cards);
            }
      //      cardsInitialized = true;
        }
        else
        {
            Debug.LogWarning("Cant update, no valid owner");
        }
    }
}
