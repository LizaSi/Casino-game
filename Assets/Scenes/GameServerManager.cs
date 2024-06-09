using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameServerManager : NetworkBehaviour
{
    private Deck _deck;
    private Player player;

    private int testIndex = 0;
    private int playerIndex = 0;
    private int playerTurnIndex = 0;

 //   public static event Action <NetworkConnection, List<string>> OnHandChanged;
    
    [SyncObject] private readonly SyncDictionary<NetworkConnection , string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection , bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection , int> _playersIndexes = new();


    private static GameServerManager _instance;

    private void Awake()
    {
        _instance = this;
       // _deck = new Deck(5);  // Initialize with five decks
        if (_deck == null || _deck.Count < 90)
        { // Check if a new deck is needed
            _deck = new Deck(5);
        }
        _deck.Shuffle();
        //     _playerHands.OnChange += playerHands_OnChange;
    }

    private void playerHands_OnChange(SyncDictionaryOperation op, NetworkConnection key, string value, bool asServer)
    {
        if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
        {
      //      OnHandChanged?.Invoke(key, value);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
      //  base.NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
   //     base.NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;

    }

    // Dictionary to map between client and its data

    private void Start()
    {
        DealInitialCards();
        DealPlayersIndex();
    }

    public static int GetPlayerIndex(NetworkConnection conn)
    {
        if (_instance._playersIndexes.ContainsKey(conn))
        {
            return _instance._playersIndexes[conn];
        }
        return 0;
    }

    public static bool IsMyTurn(NetworkConnection conn)
    {
        if(_instance._playerIsMyTurn.ContainsKey(conn))
        {
            return _instance._playerIsMyTurn[conn];
        }
        return false;
    }
    public static string GetPlayerHand(NetworkConnection conn)
    {
        if (_instance._playerHands.ContainsKey(conn))
        {
            return _instance._playerHands[conn];
        }
        return "";
    }

    public static List<string> GetAllPlayerHands(NetworkConnection conn)
    {
        List<string> allPlayersCards = new List<string>();
        foreach (string playerCards in _instance._playerHands.Values)
        {
            string cardsAsString = string.Join(", ", playerCards);
            allPlayersCards.Add(cardsAsString);
        }
        return allPlayersCards;
    }


    /*    public void HitCard_OnClick()
        {
            HitCard();
        }*/

    [Client]
    public static void HitCard()
    {
        //    string cardToAdd = _instance.PullCard();
        
        _instance.HitCardServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitCardServer(NetworkConnection sender = null)
    {
        string cardToAdd = _instance.PullCard();
        _playerHands[sender] += ", " + cardToAdd;
        string newPlayerHand = _playerHands[sender];

        if (GetHandValue(newPlayerHand) > 21)
        {
            _playerIsMyTurn[sender] = false;
            UnityEngine.Debug.LogWarning("Passed 21");
            NetworkConnection nextClient = GetNextPlayersTurn(sender);
            if(nextClient != null)
            {
                _playerIsMyTurn[nextClient] = true;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Cant find next client");
            }
        }

        UnityEngine.Debug.LogWarning("New hand: " + _playerHands[sender]);
    } 

    private NetworkConnection GetNextPlayersTurn(NetworkConnection sender)
    {
        _playersIndexes.TryGetValue(sender, out int currentUserIndex);
        return _playersIndexes.FirstOrDefault(x => x.Value == (currentUserIndex + 1) % _playersIndexes.Count).Key;
    }

    [Server]
    private void DealInitialCards()
    {
        bool isFirstTurnSet = false;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            if (!isFirstTurnSet)
            {
                isFirstTurnSet = true;
                _playerIsMyTurn[conn] = true;
            }
            else
                _playerIsMyTurn[conn] = false;

            _playerHands[conn] = PullCard() + ", " + PullCard();
        }

        if (base.NetworkManager.ServerManager.Clients.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No clients found to deal cards");
        }
    }

    [Server]
    private void DealPlayersIndex()
    {
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            _playersIndexes[conn] = GenerateNewPlayerIndex();
        }

        if (base.NetworkManager.ServerManager.Clients.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No clients found to deal index");
        }
    }

    [Server]
    private int GenerateNewPlayerIndex()
    {
        playerIndex++;
        return playerIndex;
    }

    [Server]
    private string PullCard()
    {
        UnityEngine.Debug.LogWarning("There are " + _deck.GetCards().Count + " cards in deck");
        return _deck.DrawCard();
    }

    public static bool IsInitialized()
    {
        return _instance != null;
    }

    public int GetHandValue(string hand)
    {
        List<Card> handCards = ParseHandString(hand);
        return CalculateHandValue(handCards);
    }
    private List<Card> ParseHandString(string hand)
    {
        List<Card> handCards = new List<Card>();
        string[] cardStrings = hand.Split(", ");

        foreach (string cardString in cardStrings)
        {
            int cardEnumValue = Card.StringToCard(cardString.Trim());
            CardsByOrder cardEnum = (CardsByOrder)cardEnumValue;
            handCards.Add(new Card(cardEnum));
        }

        return handCards;
    }

    private int CalculateHandValue(List<Card> hand)
    {
        int value = 0;
        int aceCount = 0;

        foreach (var card in hand)
        {
            int cardValue = card.GetValue();
            if (card.GetRank() == "Ace")
            {
                aceCount++;
                cardValue = 11; // Initially treat Aces as 11
            }
            value += cardValue;
        }
        // Adjust for Aces if the total value exceeds 21
        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
    {
        if (arg2.ConnectionState != RemoteConnectionState.Started)
        {
            _playerHands.Remove(arg1);
        }
    }
}
