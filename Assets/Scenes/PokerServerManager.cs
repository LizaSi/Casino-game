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
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PokerServerManager : NetworkBehaviour
{
    private static PokerServerManager Instance;

    [SerializeField] private PokerDisplayer PokerDisplayerScript;
    [SerializeField] private TMP_Text PotValue;

    private Deck _deck;
    private int playerIndex = 0;
    private List<string> _tableCards = new();
    private int _currentBet = 0;
    private int _pot = 0;
    private int _smallBlind = 10;
    private int _bigBlind = 20;
    private int _currentTurnIndex = 0;
    private int _blindIndex = 0;
    private NetworkConnection _bigBlindConn;

    public static event Action OnInitialized;

    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new(); //starting from 0
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playerBets = new();
    private readonly Dictionary<NetworkConnection, string> _playerNames = new();
    public int Pot
    {
        get { return _pot; }
        set
        {
            _pot = value;
            UpdatePotValueText();
        }
    }
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
        
     //   NewRoundInit();

        OnInitialized?.Invoke();
    }

    public static void JoinWithName(NetworkConnection conn, string username)
    {
        Instance._playerNames.Add(conn, username);
    }

    public static bool IsInitialized()
    {
        return Instance != null;
    }

    public static List<string> GetTableCards()
    {
        return Instance._tableCards;
    }
    
    public static string GetMyHand(NetworkConnection conn)
    {
        return Instance._playerHands.TryGetValue(conn, out string hand) ? hand : string.Empty;
    }       

    public static void NewRoundInit()
    {     
        if (Instance._deck == null || Instance._deck.Count == 0)
        {
            Instance._deck = new Deck(1);
        }
        Instance._deck.Shuffle();
        Instance.AssignPlayersIndexAndTurns();
        Instance.DealInitialCards();
        Instance._currentBet = Instance._bigBlind;
     //   Instance._blindIndex = Instance.getNextIndexTurn(Instance._playersIndexes.Count, Instance._blindIndex);
    }

    public static int HowManyCoinsToCall(NetworkConnection conn)
    {
        return Instance._currentBet - Instance._playerBets[conn];
    }

    public async static Task<int> GiveBlindCoins(NetworkConnection conn)
    {
        int givenAmount = 0;

        if (Instance._playersIndexes[conn] == 1)
        {
            await Instance.UpdateCoins(conn, Instance._bigBlind);
            Instance.Pot += Instance._bigBlind;
            Instance._playerBets[conn] = Instance._bigBlind;
            Debug.LogWarning("Gave big");
            givenAmount = Instance._bigBlind;
        }
        else if (Instance._playersIndexes[conn] == 0)
        {
            await Instance.UpdateCoins(conn, Instance._smallBlind);
            Instance.Pot += Instance._smallBlind;
            Instance._playerBets[conn] = Instance._smallBlind;
            Debug.LogWarning("Gave small");
            givenAmount = Instance._smallBlind;
        }
        else
        {
            Instance._playerBets[conn] = 0;
        }

        return givenAmount;
    }

    private int getNextIndexTurn(int playersLength, int blindIndex)
    {
        return (blindIndex + 1) % (playersLength + 1) + 1;
    }

    [Client]
    public static void RevealNewCardOnTable()
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
            UpdateCards = true,
            CardToAdd = cardToAdd
        };
        InstanceFinder.ServerManager.Broadcast(msg);

        Debug.LogWarning("New table hand: " + string.Join("",_tableCards));
    }

    [Server]
    private void DealInitialCards()
    {
        int i = 0;
        foreach (NetworkConnection conn in NetworkManager.ServerManager.Clients.Values)
        {
            if (i != 0) // Dealer doesnt need cards
            {
                _playerHands[conn] = PullCard() + ", " + PullCard();
                Debug.Log("Set 2 cards for a client and is my turn is " + _playerIsMyTurn[conn]);
            }
          
            i++;
        }
    }

    [Server]
    private string PullCard()
    {
        if (_deck == null || _deck.Count == 0)
        {
            _deck = new Deck(1);
        }
        //_deck.Shuffle();
        UnityEngine.Debug.Log("There are " + _deck.GetCards().Count + " cards in deck");
        return _deck.DrawCard();
    }

    [Client]
    public static void ClientFold(NetworkConnection conn)
    {
        Instance._playerHands.Remove(conn);
        Instance._playerIsMyTurn.Remove(conn);
        Instance._playersIndexes.Remove(conn);
    }

    [Client]
    public static void ClientBet(int coinAmount)
    {
    }

    [Client]
    public static void ClientCheck()
    {
        Instance.ClientCall();
        Instance.PassTurnServer();
    }

    private async void ClientCall(NetworkConnection sender = null)
    {
        if (_currentBet > _playerBets[sender])
        {
            await UpdateCoins(sender, _currentBet - _playerBets[sender]);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PassTurnServer(NetworkConnection sender = null)
    {
        PassTurnToNextClient(sender);
    }

    private void PassTurnToNextClient(NetworkConnection sender = null)
    {
        if (sender == null)
            Debug.LogError("Sender conn is null");
        _playerIsMyTurn[sender] = false;
        NetworkConnection nextClient = GetNextPlayersTurn(sender);
        if (nextClient != null && !sender.Equals(nextClient)) // To Add a not dealer check
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

        int nextUserIndex = (currentUserIndex + 1) % _playersIndexes.Count;

        UnityEngine.Debug.LogWarning("Turn over for " + currentUserIndex);
        UnityEngine.Debug.LogWarning("Turn started for " + nextUserIndex);

        return _playersIndexes.FirstOrDefault(x => x.Value == nextUserIndex).Key;
    }

    private async Task UpdateCoins(NetworkConnection conn, int coinToDecrease)
    {
        string name = _playerNames[conn];
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(name);

        DataSnapshot dataSnapshot = await userRef.GetValueAsync();
        int currentCoins = 0;

        if (dataSnapshot.Exists && dataSnapshot.Child("coins").Value != null)
        {
            int.TryParse(dataSnapshot.Child("coins").Value.ToString(), out currentCoins);
        }

        int updatedCoins = currentCoins - coinToDecrease;

        await userRef.Child("coins").SetValueAsync(updatedCoins);
    }

    public static async Task<GameResult> DidIWin(NetworkConnection conn, string username)
    {
        GameResult result;
        result = GameResult.Win;
        await UpdateCoinsBasedOnResult(username, result);
        return result;
    }

    private static async Task UpdateCoinsBasedOnResult(string username, GameResult result)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);

        DataSnapshot dataSnapshot = await userRef.GetValueAsync();
        int currentCoins = 0;

        if (dataSnapshot.Exists && dataSnapshot.Child("coins").Value != null)
        {
            int.TryParse(dataSnapshot.Child("coins").Value.ToString(), out currentCoins);
        }

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

        await userRef.Child("coins").SetValueAsync(updatedCoins);
    }

    public static bool IsMyTurn(NetworkConnection conn)
    {
        if(!Instance._playerIsMyTurn.TryGetValue(conn, out bool isTurn))
        {            
            Debug.LogError("No network connection in PlayerIsMyTurn");
            return false;
        }
        return isTurn;
    }

    public string GetInitCards()
    {
        return PullCard() + ", " + PullCard();
    }

    [Server]
    private void AssignPlayersIndexAndTurns()
    {
        int i = 0;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            if (i != 0)
            {
                _playersIndexes[conn] = GenerateNewPlayerIndex(); // host is not a player
                if (i == 1) // index 0 will be big blind and first
                {
                    _playerIsMyTurn[conn] = true;
                    Instance._bigBlindConn = conn;
                    Debug.LogWarning("player index 1 is big blind");
                }
                else
                {
                    _playerIsMyTurn[conn] = false;
                    Debug.LogWarning($"player index {i} joined");
                }

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
        int numOfTotalPlayers = base.NetworkManager.ServerManager.Clients.Values.Count - 1;
        playerIndex = (playerIndex + 1) % numOfTotalPlayers;
        Debug.LogWarning("Player index " + playerIndex + " joined");
        return playerIndex;
    }

    private void UpdatePotValueText()
    {
        if (PotValue != null)
        {
            PotValue.text = _pot.ToString();
        }
    }

    public struct UpdateBroadcast : IBroadcast
    {
        public bool NewRound;
        public bool UpdateCards;
        public string CardToAdd;
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
