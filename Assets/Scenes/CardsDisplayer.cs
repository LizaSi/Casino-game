<<<<<<< HEAD
using FishNet;
using FishNet.Broadcast;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using UnityEngine.UI;
using static GameServerManager;

public class CardsDisplayer : NetworkBehaviour
{
    [SerializeField]
    public TMP_Text cardsText;
    public Button hitButton;
    public Button checkButton;
    public TMP_Text winText;
    public Button newRoundButton;

    [SyncVar] private readonly bool roundFinished = false;
    private bool dealerChecked = false;

    private void OnEnable()
    {        
        InstanceFinder.ClientManager.RegisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.RegisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        InstanceFinder.ClientManager.RegisterBroadcast<ClientMsgBroadcast>(OnClientMsgBroadcast);
    }

    private void OnDisable()
    {
        InstanceFinder.ClientManager.UnregisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.UnregisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        InstanceFinder.ClientManager.UnregisterBroadcast<ClientMsgBroadcast>(OnClientMsgBroadcast);
    }

    private void OnClientMsgBroadcast(ClientMsgBroadcast msg) 
    {
        if (msg.IsWinMessage)
        {
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
                ShowWinMessage();
                handleClientTurn();
            }
        }
        if (msg.IsNewRoundMessge)
        {
            winText.text = "";
            UpdateCardsDisplay();
        }
    }

    private void OnTurnPassBroadcast(TurnPassBroadcast msg)
    {
        if (msg.PlayerId == GameServerManager.HostId && InstanceFinder.IsServer)
        {
            Debug.LogWarning("Dealers turn");
            handleDealerTurn();
        }
        if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientTurn();
            StartCoroutine(ClientTurnInDelay());
        }
    }

    private IEnumerator ClientTurnInDelay()
    {
        yield return new WaitForSeconds(0.8f);
        handleClientTurn();
    }

    private void OnUpdateFromServer(UpdateBroadcast msg) 
    {
        if (msg.NewRound && InstanceFinder.IsServer) 
        {
            handleDealerTurn();
            ClientMsgBroadcast msgForClients = new()
            {
                IsNewRoundMessge = true,
                IsWinMessage = false
            };
            InstanceFinder.ServerManager.Broadcast(msgForClients);
            UpdateCardsDisplay();
        }
        else if (msg.NewCards && !InstanceFinder.IsServer)
        {
            handleClientTurn();
            StartCoroutine(ClientTurnInDelay());
        }
        else if (msg.NewCards) // only server reaches this part
        {
            handleDealerTurn();
            UpdateCardsDisplay();
        }
    }

    public void NewRound_OnClick()
    {
        dealerChecked = false;
        NewRoundInit();
        newRoundButton.gameObject.SetActive(false);
    }

    public void Check_OnClick()
    {
        ClientCheck();
    }

    public void Hit_OnClick()
    {
        HitCard();
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Just in case theres a non update bug
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
        if (IsMyTurn(base.Owner))
        {
            if (!dealerChecked)
            {
                StartCoroutine(StartRoundInDelay());                
                return;
            }
            int cardsValue = Deck.GetHandValue(GetAllPlayerHands(Owner)[0]);
            if (cardsValue < 17)
            {
                HitCard();
            }
            else
            {
                Debug.LogWarning("dealers card value is " + cardsValue);
                ClientMsgBroadcast msg = new()
                {
                    IsNewRoundMessge = false,
                    IsWinMessage = true
                };
                InstanceFinder.ServerManager.Broadcast(msg);

                newRoundButton.gameObject.SetActive(true);
                Debug.Log("Round finished. showing results");
            }
        }
    }

    private IEnumerator StartRoundInDelay()
    {
        Debug.LogWarning("Server checked");
        dealerChecked = true;
        yield return new WaitForSeconds(0.2f); // Wait for client to get up before broadcasting it. 
        ClientCheck();
    }

    private void handleClientTurn()
    {
        UpdateCardsDisplay();
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
        }
        else
        {
            Debug.LogWarning("Cant update, no valid owner");
        }
    }
    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessge;
    }
}
=======
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
>>>>>>> 28492617fd857876dd52d1ae5d9c7e6cf180a49f
