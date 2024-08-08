using FishNet;
using FishNet.Broadcast;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using UnityEngine.UI;
using static GameServerManager;

public class CardsDisplayer : NetworkBehaviour
{
    public TMP_Text cardsText;
    public Button hitButton;
    public Button checkButton;
    public TMP_Text winText;
    public Button newRoundButton;
    public Transform cardParent; // to do: try to delete

    private bool dealerChecked = false;
    private List<GameObject> spawnedCards = new();
    private int cardIndex = 0;
    private List<string> spawnedCardsNames = new();
    private bool dealerRevealAllCards = false;

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
    private void NewRoundInit()
    {
        DespawnAllCards();
        cardIndex = 0;
        dealerChecked = false;
        dealerRevealAllCards = false;
        newRoundButton.gameObject.SetActive(false);
    }

    private void NewRoundInitAsClient()
    {
        DespawnAllCards();
        cardIndex = 0;
    }

    public void NewRound_OnClick()
    {
        NewRoundInit();
        GameServerManager.NewRoundInit();
    }

    private void OnClientMsgBroadcast(ClientMsgBroadcast msg) 
    {
        if (msg.IsWinMessage)
        {
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
                ShowWinMessage();
               // handleClientTurn();
                hitButton.gameObject.SetActive(false);
                checkButton.gameObject.SetActive(false);
                StartCoroutine(FetchCoinsInDelay());
            }
            else if(InstanceFinder.IsServer)
            {
                UpdateCardsDisplay();
        //        StartCoroutine(UpdateCardsInDelay());
            }
        }
        if (msg.IsNewRoundMessage)
        {
            if (!InstanceFinder.IsServer)
            {
                winText.text = "";
                NewRoundInitAsClient();
                StartCoroutine(ClientTurnInDelay());
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
         //   StartCoroutine(ClientTurnInDelay());
        }
        else if (msg.HostTurn && base.Owner.IsHost && InstanceFinder.IsServer)
        {
            Debug.LogWarning("Dealers turn");
            handleDealerTurn();
        }
    }

    private IEnumerator ClientTurnInDelay()
    {
        yield return new WaitForSeconds(0.65f);
        handleClientTurn();
    }

    private void OnUpdateFromServer(UpdateBroadcast msg) 
    {
        if (msg.NewRound && InstanceFinder.IsServer) 
        {
            handleDealerTurn();
            ClientMsgBroadcast msgForClients = new()
            {
                IsNewRoundMessage = true,
                IsWinMessage = false
            };
            InstanceFinder.ServerManager.Broadcast(msgForClients);
            UpdateCardsDisplay();
        }
        else if (msg.UpdateCards && !InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientTurn();
     //       StartCoroutine(UpdateCardsInDelay()); //Because of the bug that broadcasting reaches before the actual value is changed on network.
        }
        else if (msg.DealerTurn) // only server reaches this part
        {
            handleDealerTurn();
            UpdateCardsDisplay();
       //     StartCoroutine(UpdateCardsInDelay());
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
            else
                handleClientTurn();
            UpdateCardsDisplay();
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
            dealerRevealAllCards = true;
            if (cardsValue < 17)
            {
                HitCard();
            }
            else
            {
                Debug.LogWarning("dealers card value is " + cardsValue);
                ClientMsgBroadcast msg = new()
                {
                    IsNewRoundMessage = false,
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
        if (IsMyTurn(base.Owner) && base.Owner.IsLocalClient)
        {
            hitButton.gameObject.SetActive(true);
            checkButton.gameObject.SetActive(true);
        }
        else
        {
            hitButton.gameObject.SetActive(false);
            checkButton.gameObject.SetActive(false);
        }
    }

   /* private IEnumerator UpdateCardsInDelay() 
    {
        yield return new WaitForSeconds(0.1f);
        UpdateCardsDisplay();
        yield return new WaitForSeconds(1f);
        UpdateCardsDisplay();

        Debug.LogWarning("Updating cards in delay");
    }*/

    private void UpdateCardsDisplay()
    {
        if (base.Owner.IsValid && GameServerManager.IsInitialized())
        {
            if (base.Owner.IsLocalClient) //For no double spawn
            {
                string cards = GameServerManager.GetPlayerHand(base.Owner);
                int i = 0;
                foreach (string card in cards.Split(','))
                {
                    if (InstanceFinder.IsServer && !dealerRevealAllCards && i == 0)
                    {
                        i++;
                        continue; // Hide first card
                    }
                    if (!spawnedCardsNames.Contains(card))
                    {
                        SpawnCardOnBoard(card);
                        spawnedCardsNames.Add(card); 
                    }
                    i++;
                }
            }
        }
        else
        {
            Debug.LogWarning("Cant update, no valid owner");
        }
    }

    void SpawnCardOnBoard(string cards)
    {
        float cardSpacing = 300f;

        string[] cardNames = cards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < cardNames.Length; i++)
        {
            string cardName = cardNames[i].Trim();

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir), cardParent);
            instantiatedCard.transform.localScale = new Vector3(2000f, 1900f, 1f);
            instantiatedCard.transform.rotation = Quaternion.identity;
            instantiatedCard.transform.localPosition = new Vector3((cardIndex * cardSpacing)+537, 288, 15);
            instantiatedCard.transform.rotation = Quaternion.Euler(0f, 181f, 0f);
            if(instantiatedCard == null)
            {
                Debug.LogWarning("No card object found in Resources");
                return;
            }
        //    ServerManager.Spawn(instantiatedCard);
            spawnedCards.Add(instantiatedCard);
            cardIndex++;
        }
    }

    private void DespawnAllCards()
    {
        foreach (GameObject cardObject in spawnedCards)
        {
           Destroy(cardObject);
        }
        Debug.Log("Despawning all cards");
        spawnedCards.Clear();
        spawnedCardsNames.Clear();
    }

    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessage;
    }
}
