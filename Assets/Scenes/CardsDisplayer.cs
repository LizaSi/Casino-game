using FishNet;
using FishNet.Broadcast;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using PlayerData;
using UnityEngine;
using UnityEngine.UI;
using static GameServerManager;

public class CardsDisplayer : NetworkBehaviour
{
    public TMP_Text cardsText;
    public GameObject ButtonsParent;
    public TMP_Text winText;
    public Button newRoundButton;

    private bool dealerChecked = false;
    private int cardIndex = 0;
    private List<string> spawnedCardsNames = new();
    private List<string> spawnedCardsServer = new();
    private bool dealerRevealAllCards = false;
    
    private void OnEnable()
    {        
        GameServerManager.OnInitialized += GameServerManager_OnInitialized;
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

    private void GameServerManager_OnInitialized()
    {
        if (base.Owner.IsLocalClient && !InstanceFinder.IsServer)
        {
            PlayerDisplayer.SetCameraBlackJack(GetPlayerIndex(base.Owner) - 1); // -1 cuz index 1 is the host
            StartCoroutine(ClientTurnInDelay());
        }
    }

    private void NewRoundInit()
    {
        DespawnAllCards();
        cardIndex = 0;
        dealerChecked = false;
        dealerRevealAllCards = false;
        newRoundButton.gameObject.SetActive(false);
        CountdownTimer.StartCountdown(this);
    }

    private void NewRoundInitAsClient()
    {
        DespawnAllCards();
        cardIndex = 0;
        CountdownTimer.StartCountdown(this);
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
                ButtonsParent.SetActive(false);
                StartCoroutine(FetchCoinsInDelay());
            }
            else if(InstanceFinder.IsServer)
            {
                UpdateCardsDisplay();
            }
        }
        
        if (msg.IsNewRoundMessage)
        {
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
                winText.text = "";
                NewRoundInitAsClient();
                StartCoroutine(ClientTurnInDelay());
            }
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
        }
        else if (msg.UpdateCards && !InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
            handleClientTurn();
        }
        else if (msg.DealerTurn) // only server reaches this part
        {
            handleDealerTurn();
            UpdateCardsDisplay();
        }
        if (msg.NewRound && base.Owner.IsLocalClient)
        {
            winText.text = "";
            NewRoundInitAsClient();
          //  StartCoroutine(ClientTurnInDelay());
        }
        if (msg.UpdateCards)
            UpdateCardsDisplay();
    }

    public void Check_OnClick()
    {
        ClientCheck();
    }

    public void Hit_OnClick()
    {
        HitCard();
    }

    public void Double_OnClick()
    {
        HitCard();
        ClientCheck();
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
            {
                handleClientTurn();
            }
            UpdateCardsDisplay();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Just in case theres a non update bug
        {
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
        yield return new WaitForSeconds(0.5f); // Wait for client to get up before broadcasting it. 
        ClientCheck();
    }

    private void handleClientTurn()
    {
        UpdateCardsDisplay();
        if (IsMyTurn(base.Owner) && base.Owner.IsLocalClient)
        {
            ButtonsParent.SetActive(true);
        }
        else
        {
            ButtonsParent.SetActive(false);
        }
    }

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
                    if (InstanceFinder.IsServer && base.Owner.IsLocalClient && !dealerRevealAllCards && i == 0)
                    {
                        i++;
                        continue; // Hide first card
                    }
                    if (!spawnedCardsNames.Contains(card))
                    {
                        Debug.LogWarning($"Spawning {card} as client");
                        SpawnCardOnBoard(card);
                        spawnedCardsNames.Add(card);
                    }
                    else
                    {
                        cardsText.text = card + " Is already displayed";
                    }
                    i++;
                }
            }
            else if (InstanceFinder.IsServer) // To spawn clients cards.
            {
                string cards = GameServerManager.GetPlayerHand(base.Owner);
                int i = 0;
                foreach (string card in cards.Split(','))
                {
                    if (!spawnedCardsServer.Contains(card))
                    {
                        SpawnCardOnBoard(card);
                        spawnedCardsServer.Add(card);
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
        int playerIndex = GameServerManager.GetPlayerIndex(base.Owner);
        CardInitialPosition[] cardInitialPositions = new CardInitialPosition[6];
        cardInitialPositions[0] = new()
        {
            xDealedCardPosition = 0.37f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 31.81f,
            xHitCardPosition = 0.57f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 31.27f,
            xRotation = 270,
            yRotation = 0,
            zRotation = 0
        };
        cardInitialPositions[1] = new()
        {
            xDealedCardPosition = -4.05f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 31.42f,
            xHitCardPosition = -3.47f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 31.45f,
            xRotation = 270,
            yRotation = 0,
            zRotation = -69.864f
        };
        cardInitialPositions[2] = new()
        {
            xDealedCardPosition = -3.84f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 29.39f,
            xHitCardPosition = -3.58f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 29.9f,
            xRotation = 270,
            yRotation = 0,
            zRotation = 224.227f
        };
        cardInitialPositions[3] = new()
        {
            xDealedCardPosition = -0.3f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 28.83f,
            xHitCardPosition = -0.49f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 29.37f,
            xRotation = 270,
            yRotation = 0,
            zRotation = 180

        };
        cardInitialPositions[4] = new()
        {
            xDealedCardPosition = 3.63f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 29.07f,
            xHitCardPosition = 3.13f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 29.37f,
            xRotation = 270,
            yRotation = 0,
            zRotation = 138.62f

        };
        cardInitialPositions[5] = new()
        {
            xDealedCardPosition = 4.35f,
            yDealedCardPosition = 2.48f,
            zDealedCardPosition = 30.91f,
            xHitCardPosition = 3.9f,
            yHitCardPosition = 2.48f,
            zHitCardPosition = 30.58f,
            xRotation = 270,
            yRotation = 0,
            zRotation = 72.893f

        };
        float card2Spacing_X = 0.07f;
        float card2Spacing_Y = 0.03f;
        float card2Spacing_Z = -0.1f;
        float cardSpacing_X = -0.03f;
        float cardSpacing_Y = 0.02f;
        float cardSpacing_Z = -0.1f;

        string[] cardNames = cards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < cardNames.Length; i++)
        {
            string cardName = cardNames[i].Trim();

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
            instantiatedCard.transform.localScale = new Vector3(2.2816f, 2.2816f, 2.2816f);
            instantiatedCard.transform.rotation = Quaternion.identity;
            //instantiatedCard.transform.localPosition = new Vector3((cardIndex * cardSpacing)+537, 288, 15);
            //instantiatedCard.transform.rotation = Quaternion.Euler(0f, 181f, 0f);
            Debug.LogWarning($"Dealing card no. {cardIndex} : {cardName}");
            if (cardIndex < 2)
            {
                //instantiatedCard.transform.localPosition = new Vector3((cardIndex * card2Spacing_X) - 4.05f, (cardIndex * card2Spacing_Y) + 2.48f, (cardIndex * card2Spacing_Z) + 31.42f);
                instantiatedCard.transform.localPosition = new Vector3((cardIndex * card2Spacing_X) + cardInitialPositions[playerIndex - 1].xDealedCardPosition, (cardIndex * card2Spacing_Y) + cardInitialPositions[playerIndex - 1].yDealedCardPosition, (cardIndex * card2Spacing_Z) + cardInitialPositions[playerIndex - 1].zDealedCardPosition);

            }
            else
            {
                //instantiatedCard.transform.localPosition = new Vector3(((cardIndex - 2) * cardSpacing_X) - 3.47f, ((cardIndex - 2) * cardSpacing_Y) + 2.48f, ((cardIndex - 2) * cardSpacing_Z) + 31.45f);
                instantiatedCard.transform.localPosition = new Vector3(((cardIndex - 2) * cardSpacing_X) + cardInitialPositions[playerIndex - 1].xHitCardPosition, ((cardIndex - 2) * cardSpacing_Y) + cardInitialPositions[playerIndex - 1].yHitCardPosition, ((cardIndex - 2) * cardSpacing_Z) + cardInitialPositions[playerIndex - 1].zHitCardPosition);
            }
            //instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -69.864f);
            instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, cardInitialPositions[playerIndex - 1].zRotation);

            if (instantiatedCard == null)
            {
                Debug.LogWarning("No card object found in Resources");
                return;
            }
        //    ServerManager.Spawn(instantiatedCard);
          //  spawnedCards.Add(instantiatedCard);
            cardIndex++;
        }
    }

    private void DespawnAllCards()
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            Destroy(card);
            Debug.LogWarning("Despawning " + card.name);
        }

        spawnedCardsNames.Clear();
        spawnedCardsServer.Clear();
    }

    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessage;
    }

    public struct CardInitialPosition
    {
        public float xDealedCardPosition;
        public float yDealedCardPosition;
        public float zDealedCardPosition;
        public float xHitCardPosition;
        public float yHitCardPosition;
        public float zHitCardPosition;
        public float xRotation;
        public float yRotation;
        public float zRotation;
    }
}
