using FishNet;
using FishNet.Broadcast;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Observing;
using IO.Swagger.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Transform cardParent;

    private bool dealerChecked = false;
    private List<GameObject> spawnedCards = new();

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
                StartCoroutine(FetchCoinsInDelay());
            }
        }
        if (msg.IsNewRoundMessge)
        {
            if (!InstanceFinder.IsServer)
            {
                DespawnAllCards();
                winText.text = "";
                handleClientTurn();
            }

            UpdateCardsDisplay();            
        }
    }

    private IEnumerator FetchCoinsInDelay()
    {
        yield return new WaitForSeconds(0.7f);
        LoggedUser.FetchCoins();
    }

    private void OnTurnPassBroadcast(TurnPassBroadcast msg)
    {
        if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientTurn();
            StartCoroutine(ClientTurnInDelay());
        }
        else if (msg.HostTurn && base.Owner.IsHost && InstanceFinder.IsServer)
        {
            Debug.LogWarning("Dealers turn");
            handleDealerTurn();
        }
    }

    private IEnumerator ClientTurnInDelay()
    {
        yield return new WaitForSeconds(0.8f);
        handleClientTurn();
        yield return new WaitForSeconds(1.8f);
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
        else if (msg.UpdateCards && !InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientTurn();
            StartCoroutine(ClientTurnInDelay());
        }
        else if (msg.UpdateCards) // only server reaches this part
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

    private async void ShowWinMessage()
    {
        if (InstanceFinder.IsServer || !base.Owner.IsLocalClient)
        {
            winText.text = "";
            return;
        }

        GameResult result = await DidIWin(base.Owner, LoggedUser.Username);

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
        if (IsMyTurn(base.Owner) && InstanceFinder.IsServer)
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

      /*  if (roundFinished)
        {
            ShowWinMessage();
        }*/
    }

    private void UpdateCardsDisplay()
    {
        if (base.Owner.IsValid && GameServerManager.IsInitialized())
        {
            if (InstanceFinder.IsServer)
            {
                List<string> cards = GetAllPlayerHands(base.Owner);
                cardsText.text = "Players cards:\n" + string.Join("\n", cards);
            //    DisplayCardsOnBoard(cards[0]); // only his own..
            }
            else if (base.Owner.IsLocalClient)
            {
                string cards = GameServerManager.GetPlayerHand(base.Owner);
                //   int playerIndex = GameServerManager.GetPlayerIndex(base.Owner);
                DisplayCardsOnBoard(cards);
            }
        }
        else
        {
            Debug.LogWarning("Cant update, no valid owner");
        }
    }
    void DisplayCardsOnBoard(string cards)
    {
        float cardSpacing = 300f;

        string[] cardNames = cards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);              

        for (int i = 0; i < cardNames.Length; i++)
        {
            // Trim any extra whitespace from the card name
            string cardName = cardNames[i].Trim();

            // Load the card prefab from the Resources/Cards folder
            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir), cardParent);
            instantiatedCard.transform.localScale = new Vector3(2000f, 1900f, 1f);
            instantiatedCard.transform.rotation = Quaternion.identity;
            instantiatedCard.transform.localPosition = new Vector3((i * cardSpacing)+537, 288, 15);
            instantiatedCard.transform.rotation = Quaternion.Euler(0f, 181f, 0f);
            if(instantiatedCard == null)
            {
                Debug.LogWarning("No card object found in Resources");
                return;
            }
            ServerManager.Spawn(instantiatedCard);
            spawnedCards.Add(instantiatedCard);
        }
    }

    private void DespawnAllCards()
    {
        foreach (GameObject cardObject in spawnedCards)
        {
         //   ServerManager.Despawn(cardObject);
            Debug.LogWarning("Despawning card");
           Destroy(cardObject);
        }
        spawnedCards.Clear();
    }

    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessge;
    }
}
