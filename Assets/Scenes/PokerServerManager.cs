using Firebase.Database;
using FishNet;
using FishNet.Broadcast;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PokerServerManager : NetworkBehaviour
{
    private static PokerServerManager Instance;

    [SerializeField] private PokerDisplayer PokerDisplayerScript;
    private Deck _deck;
    private int playerIndex = 0;
    private List<string> _tableCards = new();

    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        NewRoundInit();
        PokerDisplayerScript.StartGame(this);
        //    PokerDisplayerScript.StartGame(this);
        //   AssignPlayersIndex();
    }

    public static List<string> GetTableCards()
    {
        return Instance._tableCards;
    }
    
    public static string GetMyHand(NetworkConnection conn)
    {
        return Instance._playerHands.TryGetValue(conn, out string hand) ? hand : string.Empty;
    }

    [Server]
    private void AssignPlayersIndex()
    {
        int i = 0;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            if (i != 0)
            {
                _playersIndexes[conn] = GenerateNewPlayerIndex(); // host is not a player
            }
            i++;
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

    public void NewRoundInit()
    {
        if (_deck == null || _deck.Count == 0)
        {
            _deck = new Deck(1);
        }
        _deck.Shuffle();
        Instance.DealInitialCards();
     //   PokerDisplayerScript.Init();
    }

    [Client]
    public static void RevealCardOnTable()
    {
        Instance.HitCardServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void HitCardServer(NetworkConnection sender = null)
    {
        string cardToAdd = Instance.PullCard();
        _tableCards.Add(cardToAdd);

        UpdateBroadcast msg = new()
        {
            NewRound = false,
            UpdateCards = true
        };
        InstanceFinder.ServerManager.Broadcast(msg);

        Debug.LogWarning("New table hand: " + _tableCards.ToArray());
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
            Debug.LogWarning("Set 2 cards for a client");
        }
    }

    [Server]
    private string PullCard()
    {
        if (_deck == null || _deck.Count == 0)
        {
            _deck = new Deck(1);
        }
        _deck.Shuffle();
        UnityEngine.Debug.Log("There are " + _deck.GetCards().Count + " cards in deck");
        return _deck.DrawCard();
    }

    [Client]
    public void ClientBet(int coinAmount)
    {
    }

    [Client]
    public void ClientCheck()
    {
        PassTurnServer();
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

    private NetworkConnection GetNextPlayersTurn(NetworkConnection sender)
    {
        _playersIndexes.TryGetValue(sender, out int currentUserIndex);

        int nextUserIndex = (currentUserIndex % _playersIndexes.Count) + 1;

        UnityEngine.Debug.LogWarning("Turn over for " + currentUserIndex);
        UnityEngine.Debug.LogWarning("Turn started for " + nextUserIndex);

        return _playersIndexes.FirstOrDefault(x => x.Value == nextUserIndex).Key;
    }

    public List<string> GetAllPlayerHands(NetworkConnection conn)
    {
        List<string> allPlayersCards = new();
        foreach (string playerCards in _playerHands.Values)
        {
            string cardsAsString = string.Join(", ", playerCards);
            allPlayersCards.Add(cardsAsString);
        }
        return allPlayersCards;
    }

    public async Task<GameResult> DidIWin(NetworkConnection conn, string username)
    {
        GameResult result;
        int clientValue = Deck.GetHandValue(_playerHands[conn]);
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

    private async Task UpdateCoinsBasedOnResult(string username, GameResult result)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);

        // Get the current coins
        DataSnapshot dataSnapshot = await userRef.GetValueAsync();
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
                updatedCoins += 100; 
                break;
            case GameResult.Tie:
                break;
            case GameResult.Lose:
                updatedCoins -= 100; 
                break;
        }

        // Set the updated coin value back to Firebase
        await userRef.Child("coins").SetValueAsync(updatedCoins);
    }

    public bool IsMyTurn(NetworkConnection conn = null)
    {
        if (conn == null)
        {
            UnityEngine.Debug.LogError("NetworkConnection is null.");
            return false;
        }

        if (_playerIsMyTurn == null)
        {
            UnityEngine.Debug.LogError("Player turn status dictionary is not initialized.");
            return false;
        }
        return _playerIsMyTurn.TryGetValue(conn, out bool isTurn) && isTurn;
    }

    public string GetInitCards()
    {
        return PullCard() + ", " + PullCard();
    }

    public struct UpdateBroadcast : IBroadcast
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
