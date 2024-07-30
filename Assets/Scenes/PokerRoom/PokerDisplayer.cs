using FishNet.Broadcast;
using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;
using FishNet.Object;
using UnityEngine.UI;
using static PokerServerManager;
using System;
using FishNet.Connection;
using System.Linq;

public class PokerDisplayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button newRoundButton;
    [SerializeField] private GameObject pokerComponentsParent;
    [SerializeField] private Transform CardTransform;
    [SerializeField] private Transform TableCardTransform;

    private float cardSpacing = 2.8f;
    private float tableCardSpacing = 1.5f;
    private int tableSpaceIndex = 0;
    private List<GameObject> spawnedCards = new();

    private void Start()
    {
        PokerServerManager.OnInitialized += OnPokerServerStarted;
        if (PokerServerManager.IsInitialized())
        {
            OnPokerServerStarted();
        }
    }

    private void OnPokerServerStarted()
    {
        InitGame();
    }

    public void InitGame()
    {        
        StartCoroutine(ClientTurnInDelay());
        InstanceFinder.ClientManager.RegisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.RegisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        InstanceFinder.ClientManager.RegisterBroadcast<ClientMsgBroadcast>(OnClientMsgBroadcast);
    }

    private void OnDisable()
    {
        pokerComponentsParent.SetActive(false);
        InstanceFinder.ClientManager.UnregisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.UnregisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        InstanceFinder.ClientManager.UnregisterBroadcast<ClientMsgBroadcast>(OnClientMsgBroadcast);
    }

    private void DisplayCardsClient()
    {
        GameObject CardViewer = GameObject.Find("CardViewer");
        int spaceIndex = 0;
        string hand = GetMyHand(base.Owner);
        string[] cardNames = hand.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int j = 0; j < cardNames.Length; j++)
        {
            string cardName = cardNames[j].Trim();
            Debug.LogWarning("Displaying card " + cardName);

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
            Vector3 newPosition = CardTransform.position + new Vector3(spaceIndex * cardSpacing, 0, 0);

            instantiatedCard.transform.SetPositionAndRotation(newPosition, CardTransform.rotation);
            instantiatedCard.transform.localScale = CardTransform.localScale;

            // Set the parent of the instantiated card to CardViewer
            instantiatedCard.transform.SetParent(CardViewer.transform, false);

            spawnedCards.Add(instantiatedCard);
            spaceIndex++;
        }
    }

    private void OnUpdateFromServer(UpdateBroadcast msg)
    {
        if (msg.UpdateCards && InstanceFinder.IsServer && !base.Owner.IsLocalClient)
        {
            DisplayNewCardOnTable();
            
        }
        if (msg.UpdateCards && !InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
           // handleClientTurn();
        //    StartCoroutine(ClientTurnInDelay());
        }
    }

    private void DisplayNewCardOnTable()
    {
        GameObject CardViewer = GameObject.Find("CardViewer");

        List<string> tableCards = PokerServerManager.GetTableCards();

        string cardName = tableCards.Last();
        Debug.LogWarning("Displaying card " + cardName);

        string cardDir = "Cards/" + cardName;
        GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
        Vector3 newPosition = TableCardTransform.position + new Vector3(tableSpaceIndex * tableCardSpacing, 0, 0);

        instantiatedCard.transform.SetPositionAndRotation(newPosition, TableCardTransform.rotation);
        instantiatedCard.transform.localScale = TableCardTransform.localScale;

        instantiatedCard.transform.SetParent(CardViewer.transform, false);

        spawnedCards.Add(instantiatedCard);
        tableSpaceIndex++;
    }

    public void NewRound_OnClick()
    {
        PokerServerManager.NewRoundInit();
        newRoundButton.gameObject.SetActive(false);
    }

    public void Fold_OnClick()
    {
        PokerServerManager.ClientCheck();
        PokerServerManager.ClientFold(base.Owner);
    }

    public void Check_OnClick()
    {
        PokerServerManager.ClientCheck();
    }

    public void Bet_OnClick()
    {
        int amountFromUI = 10;
        PokerServerManager.ClientBet(amountFromUI);
    }

    private async void ShowWinMessage()
    {
        if (InstanceFinder.IsServer || !base.Owner.IsLocalClient)
        {
            winText.text = "";
            return;
        }

        GameResult result = await PokerServerManager.DidIWin(base.Owner, LoggedUser.Username);

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
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
             //   DisplayCardsClient();
                handleClientTurn();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && InstanceFinder.IsServer && !base.Owner.IsLocalClient)
        {
            PokerServerManager.RevealNewCardOnTable();
        }
    }

    private void handleClientTurn()
    {
        if (InstanceFinder.IsServer || !base.Owner.IsLocalClient) // If we want all client cards on table delete this line
        { 
            return;
        }

        DisplayCardsClient();

        if (PokerServerManager.IsMyTurn(base.Owner))
        {
            pokerComponentsParent.SetActive(true);
        }
        else
        {
            pokerComponentsParent.SetActive(false);
        }
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
        if (msg.IsNewRoundMessage)
        {
            if (!InstanceFinder.IsServer)
            {
                winText.text = "";
                handleClientTurn();
                StartCoroutine(ClientTurnInDelay());
            }

            DespawnAllCards();
            handleClientTurn();
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
    }

    private IEnumerator ClientTurnInDelay()
    {
        yield return new WaitForSeconds(0.8f);
        handleClientTurn();
        yield return new WaitForSeconds(1.8f);
        handleClientTurn();
    }

    private void DespawnAllCards()
    {
        foreach (GameObject cardObject in spawnedCards)
        {
            //   ServerManager.Despawn(cardObject);
            Destroy(cardObject);
        }
        Debug.Log("Despawning all cards");
        spawnedCards.Clear();
    }

    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessage;
    }
}
