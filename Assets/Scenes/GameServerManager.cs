using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Broadcast;
using FishNet;

public class GameServerManager : NetworkBehaviour
{
    private Deck _deck;
    private int playerIndex = 0;

    public static int HostId { get; private set; }

    public static event Action OnInitialized;
    public static event Action OnTurnPass;

    //   public static event Action <NetworkConnection, List<string>> OnHandChanged;

    [SyncObject] private readonly SyncDictionary<NetworkConnection , string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection , bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection , int> _playersIndexes = new();
   // [SyncVar] private bool _dealerHasResults;


    private static GameServerManager _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        if (_deck == null || _deck.Count < 90)
        {
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

    public static GameResult DidIWin(NetworkConnection conn)
    {
        int clientValue = Deck.GetHandValue(_instance._playerHands[conn]);
        int dealerValue = Deck.GetHandValue(GetAllPlayerHands(conn)[0]);

        if ((clientValue > dealerValue && clientValue <= 21) || (dealerValue > 21 && clientValue <= 21))
        {
            return GameResult.Win;
        }
        if (dealerValue == clientValue)
        {
            return GameResult.Tie;
        }
        return GameResult.Lose;
    }


    /*    public void HitCard_OnClick()
        {
            HitCard();
        }*/
    [Client]
    public static void ClientCheck()
    {
        //    string cardToAdd = _instance.PullCard();

        _instance.PassTurnServer();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PassTurnServer(NetworkConnection sender = null)
    {
        PassTurnToNextClient(sender);
    }
    //  [ServerRpc(RequireOwnership = false)]
    private void PassTurnToNextClient(NetworkConnection sender = null)
    {
        _playerIsMyTurn[sender] = false;
        NetworkConnection nextClient = GetNextPlayersTurn(sender);
        if (nextClient != null && !sender.Equals(nextClient))
        {
            _playerIsMyTurn[nextClient] = true;
            TurnPassBroadcast msg = new()
            {
                PlayerId = nextClient.ClientId
            };
            InstanceFinder.ServerManager.Broadcast(msg);
          //  OnTurnPass?.Invoke();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Cant find next client");
        }

    }

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

            // If its the hosts hit, he shouldnt pass the turn
        if (IsHost && HostId == sender.ClientId /* double check */)
        {
            TurnPassBroadcast msg = new()
            {
                PlayerId = HostId       
            };
            InstanceFinder.ServerManager.Broadcast(msg);
        }
        else if (Deck.GetHandValue(newPlayerHand) > 21)
        {
            UnityEngine.Debug.Log("Passed 21");

            PassTurnToNextClient(sender);
        }
        else 
        {
            UpdateBroadcast msg = new()
            {
                NewRound = false,
                NewCards = true
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
    private void DealInitialCards()
    {
        bool isFirstTurnSet = false;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            if (!isFirstTurnSet)
            {
                isFirstTurnSet = true;
                _playerIsMyTurn[conn] = true;
                HostId = conn.ClientId;
            }
            else
                _playerIsMyTurn[conn] = false;

            _playerHands[conn] = PullCard() + ", " + PullCard();
        }

        UpdateBroadcast msg = new()
        {
            NewRound = true,
            NewCards = false
        };
        InstanceFinder.ServerManager.Broadcast(msg);
        //  OnInitialized?.Invoke(); // Invoke the SERVER when game begins

        if (base.NetworkManager.ServerManager.Clients.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No clients found to deal cards");
        }
    }

 
    [ServerRpc(RequireOwnership = false)]
    private void DidIWinServer(NetworkConnection sender = null)
    {
        
       // _dealerHasResults = doesDealerHasResults;
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
        UnityEngine.Debug.Log("There are " + _deck.GetCards().Count + " cards in deck");
        return _deck.DrawCard();
    }

    private void InvokeHandleMyTurn(NetworkConnection client)
    {
        var clientId = base.NetworkManager.ServerManager.Clients.FirstOrDefault(c => c.Key == client.ClientId).Key;
    }

    public static bool IsInitialized()
    {
        return _instance != null;
    }   

    private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
    {
        if (arg2.ConnectionState != RemoteConnectionState.Started)
        {
            _playerHands.Remove(arg1);
        }
    }

    public struct UpdateBroadcast: IBroadcast
    {
        public bool NewRound;
        public bool NewCards;
    }

    public struct TurnPassBroadcast : IBroadcast
    {
        public int PlayerId;
    }

    public enum GameResult
    {
        Win,
        Lose,
        Tie
    }
}
