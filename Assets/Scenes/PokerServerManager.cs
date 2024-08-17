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
    private int checkCounter = 0;
    private bool flopRevealed = false;
    public static event Action OnInitialized;

    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new(); //starting from 0
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersJoiningIndexes = new(); //starting from 0
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
        
        OnInitialized?.Invoke();
    }

    public static bool IsInitialized()
    {
        return Instance != null;
    }

    public static void JoinWithName(NetworkConnection conn, string username)
    {
        Instance._playerNames.Add(conn, username);
        Instance._playersJoiningIndexes.Add(conn, Instance._playersJoiningIndexes.Count + 1);
        
    }

    public static int GetPlayerIndex(NetworkConnection conn)
    {
        /*
        int index = 1;
        foreach (NetworkConnection netConn in Instance._playerNames.Keys)
        {
            if (conn.ClientId == netConn.ClientId)
            {
                return index;
            }
            index++;
        }
        */
        return Instance._playersJoiningIndexes.TryGetValue(conn, out int index) ? index : -1;

    }

    public static string GetMyHand(NetworkConnection conn)
    {
        return Instance._playerHands.TryGetValue(conn, out string hand) ? hand : string.Empty;
    }

    public static void NewRoundInit()
    {     
        if (Instance._deck == null || Instance._deck.Count < 5)
        {
            Instance._deck = new Deck(1);
        }
        Instance.checkCounter = 0;
        Instance.flopRevealed = false;
        Instance._deck.Shuffle();
        Instance.AssignPlayersIndexAndTurns();
        Instance.DealInitialCards();
        Instance._currentBet = Instance._bigBlind;
      //   Instance._blindIndex = Instance.getNextIndexTurn(Instance._playersIndexes.Count, Instance._blindIndex);
    }

    public static int HowManyCoinsToCall(NetworkConnection conn)
    {
        Debug.LogWarning("Current bet is " + Instance._currentBet);
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
            givenAmount = Instance._bigBlind;
        }
        else if (Instance._playersIndexes[conn] == 0)
        {
            await Instance.UpdateCoins(conn, Instance._smallBlind);
            Instance.Pot += Instance._smallBlind;
            Instance._playerBets[conn] = Instance._smallBlind;
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
                Debug.LogWarning("Set 2 cards for a client and is my turn is " + _playerIsMyTurn[conn]);
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
    public static void ClientFold()
    {
        Instance.FoldServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FoldServer(NetworkConnection sender = null)
    {
        _playerHands.Remove(sender);
        _playerIsMyTurn.Remove(sender);
        _playersIndexes.Remove(sender);
    }

    [Client]
    public static void ClientBet(NetworkConnection conn, int coinAmount)
    {
        Instance.RaiseBetServer(conn, coinAmount);
        Instance.CheckAndPassServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RaiseBetServer(NetworkConnection conn, int coinAmount)
    {
        checkCounter = 1; // So round will begin again, without the raiser
        int palyersBet = _playerBets[conn];
        int callAmount = _currentBet - palyersBet;
        Pot += coinAmount;
        _playerBets[conn] = palyersBet + coinAmount + callAmount;
        _currentBet = _playerBets[conn];
        Debug.LogWarning("Bet is raised to " + _currentBet);
    }

    [Client]
    public static void ClientCheck()
    {
        Instance.CheckAndPassServer();
    }

    private async void ClientCall(NetworkConnection sender = null)
    {
        int callAmount = _currentBet - _playerBets[sender];
        if (callAmount > 0)
        {
            await UpdateCoins(sender, callAmount);
            Pot += callAmount;
            _playerBets[sender] = _currentBet;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckAndPassServer(NetworkConnection sender = null)
    {
        checkCounter++;
        ClientCall(sender);
        if(checkCounter >= _playersIndexes.Count)
        {
            /*if (!EveryOneCalledOrSetTurn(sender))
            {
                return;
            }*/
            if (!flopRevealed)
            {
                flopRevealed = true;
                RevealNewCardOnTable();
                RevealNewCardOnTable();
            }
            RevealNewCardOnTable();
            checkCounter = 0;
        }
       
        PassTurnToNextClient(sender);
    }

    private bool EveryOneCalledOrSetTurn(NetworkConnection sender) 
    {
        foreach (var player in _playerBets)
        {
            if (player.Value < _currentBet)
            {
                _playerIsMyTurn[sender] = false;
                {
                    _playerIsMyTurn[player.Key] = true;
                    TurnPassBroadcast msg = new()
                    {
                        HostTurn = InstanceFinder.IsServer
                    };
                    InstanceFinder.ServerManager.Broadcast(msg);
                }
                return false;
            }
        }
        return true;
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
            if (i != 0) // assuming the first is the host
            {
                _playersIndexes[conn] = GenerateNewPlayerIndex();
                if (i == 1)
                {
                    _playerIsMyTurn[conn] = true;
                   // Instance._bigBlindConn = conn;
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

    /*
    public static int GetPlayerIndex(NetworkConnection conn)
    {
        return Instance._playersIndexes.TryGetValue(conn, out int index) ? index : 0;
    }
    */

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
        if (InstanceFinder.IsServer)
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
