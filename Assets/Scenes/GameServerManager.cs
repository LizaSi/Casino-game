using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Broadcast;
using FishNet;
using Firebase.Database;
using System.Threading.Tasks;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class GameServerManager : NetworkBehaviour
{
    private Deck _deck;
    private int playerIndex = 0;

    public static event Action OnInitialized;
    public static event Action OnTurnPass;

    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new();

    private static GameServerManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            OnInitialized?.Invoke();
        }
    }
    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
            OnInitialized?.Invoke();
        }
        NewRoundInit();
        _instance.AssignPlayersIndex();
    }

    public static void NewRoundInit()
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("Class not initiazlied");
        }
        if (_instance._deck == null || _instance._deck.Count < 90)
        {
            _instance._deck = new Deck(5);
        }
        _instance._deck.Shuffle();
        _instance.DealInitialCards();
    }

    public static bool IsInitialized()
    {
        return _instance != null;
    }

    [Server]
    private void DealInitialCards()
    {
        bool isFirstTurnSet = false;
        foreach (NetworkConnection conn in NetworkManager.ServerManager.Clients.Values)
        {
            _playerIsMyTurn[conn] = !isFirstTurnSet;
            isFirstTurnSet = true;

            _playerHands[conn] = PullCard() + ", " + PullCard();
        }

        UpdateBroadcast msg = new()
        {
            NewRound = true,
            UpdateCards = false
        };
        InstanceFinder.ServerManager.Broadcast(msg);

        /*if (base.NetworkManager.ServerManager.Clients.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No clients found to deal cards");
            return;
        }*/
    }

    [Server]
    private void AssignPlayersIndex()
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

    public static int GetPlayerIndex(NetworkConnection conn)
    {
        return _instance._playersIndexes.TryGetValue(conn, out int index) ? index : 0; ;
    }

    public static bool IsMyTurn(NetworkConnection conn = null)
    {
        if(_instance == null)
        {
            UnityEngine.Debug.LogError("GameServerManager instance is not initialized.");
            return false;
        }

        if (conn == null)
        {
            UnityEngine.Debug.LogError("NetworkConnection is null.");
            return false;
        }

        if (_instance._playerIsMyTurn == null)
        {
            UnityEngine.Debug.LogError("Player turn status dictionary is not initialized.");
            return false;
        }
        return _instance._playerIsMyTurn.TryGetValue(conn, out bool isTurn) && isTurn;
    }
    public static string GetPlayerHand(NetworkConnection conn)
    {
        return _instance._playerHands.TryGetValue(conn, out string hand) ? hand : string.Empty;
    }

    public static List<string> GetAllPlayerHands(NetworkConnection conn)
    {
        List<string> allPlayersCards = new();
        foreach (string playerCards in _instance._playerHands.Values)
        {
            string cardsAsString = string.Join(", ", playerCards);
            allPlayersCards.Add(cardsAsString);
        }
        return allPlayersCards;
    }

    public static async Task<GameResult> DidIWin(NetworkConnection conn, string username)
    {
        GameResult result;
        int clientValue = Deck.GetHandValue(_instance._playerHands[conn]);
        int dealerValue = Deck.GetHandValue(GetAllPlayerHands(conn)[0]);

        if ((clientValue > dealerValue && clientValue <= 21) || (dealerValue > 21 && clientValue <= 21))
        {
            result = GameResult.Win;
        }
        else if (dealerValue == clientValue)
        {
            result = GameResult.Tie;
        }
        else
        {
            result = GameResult.Lose;
        }

        await UpdateCoinsBasedOnResult(username, result);
        return result;
    }

    private static async Task UpdateCoinsBasedOnResult(string username, GameResult result)
    {
        // Get the reference to the user's data
        var userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);

        // Get the current coins
        var dataSnapshot = await userRef.GetValueAsync();
        int currentCoins = 0;

        if (dataSnapshot.Exists && dataSnapshot.Child("coins").Value != null)
        {
            int.TryParse(dataSnapshot.Child("coins").Value.ToString(), out currentCoins);
        }

        // Update coins based on the game result
        int updatedCoins = currentCoins;
        switch (result)
        {
            case GameResult.Win:
                updatedCoins += 100; // example coin reward for win
                break;
            case GameResult.Tie:
                // No coin change for tie
                break;
            case GameResult.Lose:
                updatedCoins -= 100; // example coin penalty for loss
                break;
        }

        // Set the updated coin value back to Firebase
        await userRef.Child("coins").SetValueAsync(updatedCoins);
    }

    [Client]
    public static void ClientCheck()
    {
        _instance.PassTurnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PassTurnServer(NetworkConnection sender = null)
    {
        PassTurnToNextClient(sender);
    }

    private void PassTurnToNextClient(NetworkConnection sender = null)
    {
        _playerIsMyTurn[sender] = false;
        NetworkConnection nextClient = GetNextPlayersTurn(sender);
        if (nextClient != null && !sender.Equals(nextClient))
        {
            _playerIsMyTurn[nextClient] = true;
            TurnPassBroadcast msg = new()
            {
                HostTurn = InstanceFinder.IsServer
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Cant find next client");
        }

    }

    [Client]
    public static void HitCard()
    {
        _instance.HitCardServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitCardServer(NetworkConnection sender = null)
    {
        string cardToAdd = _instance.PullCard();
        _playerHands[sender] += ", " + cardToAdd;
        string newPlayerHand = _playerHands[sender];

            // If its the hosts hit, he shouldnt pass the turn
        if (sender.IsHost)
        {
            UpdateBroadcast msg = new()
            {
                NewRound = false,
                UpdateCards = true
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }
        else if (Deck.GetHandValue(newPlayerHand) >= 21)
        {
            UnityEngine.Debug.LogWarning("Passed 21");

            PassTurnToNextClient(sender);
        }
        else // Client with 21 or less
        {
            UpdateBroadcast msg = new()
            {
                NewRound = false,
                UpdateCards = true
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }

        UnityEngine.Debug.Log("New hand: " + _playerHands[sender]);
    }

    private NetworkConnection GetNextPlayersTurn(NetworkConnection sender)
    {
        _playersIndexes.TryGetValue(sender, out int currentUserIndex);

        int nextUserIndex = (currentUserIndex % _playersIndexes.Count) + 1;

        UnityEngine.Debug.LogWarning("Turn over for " + currentUserIndex);
        UnityEngine.Debug.LogWarning("Turn started for " + nextUserIndex);

        return _playersIndexes.FirstOrDefault(x => x.Value == nextUserIndex).Key;
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
        if (_deck == null || _deck.Count < 90)
        {
            _deck = new Deck(5);
        }
        _deck.Shuffle();
        UnityEngine.Debug.Log("There are " + _deck.GetCards().Count + " cards in deck");
        return _deck.DrawCard();
    }

    public struct UpdateBroadcast: IBroadcast
    {
        public bool NewRound;
        public bool UpdateCards;
    }

    public struct TurnPassBroadcast : IBroadcast
    {
        public bool HostTurn;
    }

    public enum GameResult
    {
        Win,
        Lose,
        Tie
    }
}