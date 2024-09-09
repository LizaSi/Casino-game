using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Broadcast;
using FishNet;
//using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;
using UnityEngine.UI;

public class GameServerManager : NetworkBehaviour
{
    private Deck _deck;
    private int playerIndex = 0;
    [SerializeField] Button ExitButton;

    public static event Action OnInitialized;
    public static event Action OnTurnPass;

    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> AvatarsSyncDict = new();

    private static GameServerManager _instance;
    private static int cameraIndex = 0;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
          //  OnInitialized?.Invoke();
        }
    }

    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        _instance._playerHands.OnChange += playerHands_OnChange;
        if (InstanceFinder.IsServer)
        {
            ExitButton.gameObject.SetActive(true);
        }
        NewRoundInit();
        OnInitialized?.Invoke();
    }

    private void OnDisable()
    {
        _instance._playerHands.OnChange -= playerHands_OnChange;
    }

    [Client]
    public static void SetAvatarString(string avatarString)
    {
        _instance.SetAvatarStringServer(avatarString);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAvatarStringServer(string avatarString, NetworkConnection sender = null)
    {
        AvatarsSyncDict[sender] = avatarString;
    }

    public static string GetAvatarString(NetworkConnection sender)
    {
        return _instance.AvatarsSyncDict[sender];
    }

    public static void LeaveGame()
    {
        _instance.playerIndex = 0;
        _instance._playerHands.Clear();
        _instance._playerIsMyTurn.Clear();
        _instance._playersIndexes.Clear();
        UpdateBroadcast msg = new()
        {
            Leave = true
        };
        InstanceFinder.ServerManager.Broadcast(msg);
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
        _instance.AssignPlayersIndex();


        UpdateBroadcast msg = new() // So clients can despawn all cards
        {
            NewRound = true,
            UpdateCards = false
        };
        InstanceFinder.ServerManager.Broadcast(msg);

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
            _instance._playerIsMyTurn[conn] = !isFirstTurnSet;
            isFirstTurnSet = true;

            _instance._playerHands[conn] = PullCard() + ", " + PullCard();
        }
    }

    private void playerHands_OnChange(SyncDictionaryOperation op, NetworkConnection key, string value, bool asServer)
    {
        UpdateBroadcast msg = new()
        {
            NewRound = false,
            UpdateCards = true
        };
        InstanceFinder.ServerManager.Broadcast(msg);
    }

    /*private void PlayerIsMyTurn_OnChange(SyncDictionaryOperation op, NetworkConnection key, bool isTurn, bool asServer)
    {
        TurnPassBroadcast msg = new()
        {            
            HostTurn = _playersIndexes[key] == 0 && isTurn
        };
        InstanceFinder.ServerManager.Broadcast(msg);
    }*/

    [Server]
    private void AssignPlayersIndex()
    {
        playerIndex = 0;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            _playersIndexes[conn] = GenerateNewPlayerIndex();
        }
    }



    public static int GetPlayerIndex(NetworkConnection conn)
    {
        return _instance._playersIndexes.TryGetValue(conn, out int index) ? index : 0;
    }

    public static bool IsMyTurn(NetworkConnection conn = null)
    {
        if (_instance == null)
        {
            UnityEngine.Debug.LogError("GameServerManager instance is not initialized.");
            return false;
        }
        _instance._playerIsMyTurn.TryGetValue(conn, out bool isTurn);
        return isTurn;
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
        if (!_instance._playerHands.TryGetValue(conn, out string hand))
        {
            return GameResult.Tie;
        }
        int clientValue = Deck.GetHandValue(hand);
        int dealerValue = Deck.GetHandValue(GetAllPlayerHands(conn)[0]);

        if ((clientValue > dealerValue && clientValue <= 21) || (dealerValue > 21 && clientValue <= 21))
        {
            result = GameResult.Win;
        }
        else if (dealerValue == clientValue && clientValue <= 21)
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
            _playerIsMyTurn[nextClient] = true;
            TurnPassBroadcast msg = new()
            {
                HostTurn = nextClient.IsLocalClient
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
        string cardToAdd = PullCard();
        _instance._playerHands[sender] += ", " + cardToAdd;
        string newPlayerHand = _playerHands[sender];

        // If the host hits, he shouldnt pass the turn
        if (_playersIndexes[sender] == 1)
        {
            UpdateBroadcast msg = new()
            {
                NewRound = false,
                UpdateCards = false,
                DealerTurn = true
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }
        else if (Deck.GetHandValue(newPlayerHand) >= 21)
        {
            Debug.LogWarning("Passed 21");

            PassTurnToNextClient(sender);
        }
        else // Client with less then 21
        {
            UpdateBroadcast msg = new()
            {
                NewRound = false,
                UpdateCards = true,
                DealerTurn = false,
                NewCard = cardToAdd
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }

        Debug.Log("New hand: " + _playerHands[sender]);
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

    private static async Task UpdateCoinsBasedOnResult(string username, GameResult result)
    {
        DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference("users").Child(username);

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

        await userRef.Child("coins").SetValueAsync(updatedCoins);
    }

    public struct UpdateBroadcast : IBroadcast
    {
        public bool NewRound;
        public bool UpdateCards;
        public bool DealerTurn;
        public string NewCard;
        public bool Leave;
    }

    public struct TurnPassBroadcast : IBroadcast
    {
        public bool HostTurn;
        //  public NetworkConnection TurnOwner;
    }

    public enum GameResult
    {
        Win,
        Lose,
        Tie
    }
}