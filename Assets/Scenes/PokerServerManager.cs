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
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Unity.VisualScripting;

public class PokerServerManager : NetworkBehaviour
{
    private static PokerServerManager Instance;

    [SerializeField] private PokerDisplayer PokerDisplayerScript;
    [SerializeField] private TMP_Text PotValue;
    [SerializeField] private TMP_Text WinText;
    [SerializeField] private Button NewRoundButton;

    private Deck _deck;
    private int playerIndex = 0;
    private List<string> _tableCards = new();
    private int _pot = 0;
    private const int _smallBlind = 10;
    private const int _bigBlind = 20;
    private int checkCounter = 0;
    private bool flopRevealed = false;
    private int cardsOnBoardCounter = 0;
    public static event Action OnInitialized;
    public static event Action OnTurnChange;

    [SyncVar] private int _currentBet = 0;
    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerHands = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, bool> _playerIsMyTurn = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersIndexes = new(); //starting from 0
    /// <summary>
    /// snir file
    /// </summary>
    /*
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playersJoiningIndexes = new(); //starting from 0
    */
    [SyncObject] private readonly SyncDictionary<NetworkConnection, int> _playerBets = new();
    [SyncObject] private readonly SyncDictionary<NetworkConnection, string> _playerNames = new();
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

        Instance._playerIsMyTurn.OnChange += playerTurn_OnChange;
        newRoundInit();
        OnInitialized?.Invoke();
    }

    private void playerTurn_OnChange(SyncDictionaryOperation op, NetworkConnection key, bool value, bool asServer)
    {
        TurnPassBroadcast msg = new()
        {
            HostTurn = false
        };
        InstanceFinder.ServerManager.Broadcast(msg);
    }

    public static bool IsInitialized()
    {
        return Instance != null;
    }

    private void newRoundInit()
    {
        //snir file
        /*
        Instance._playerNames.Add(conn, username);
        Instance._playersJoiningIndexes.Add(conn, Instance._playersJoiningIndexes.Count + 1);
        */
        NewRoundButton.gameObject.SetActive(false);
        if (_deck == null || _deck.Count < 5)
        {
            _deck = new Deck(1);
        }
        _deck.Shuffle();
        _tableCards.Clear();
        checkCounter = 0;
        flopRevealed = false;
        _currentBet = _bigBlind;
        _pot = 0;
        WinText.text = "";
        PlayersNewRoundInit();
       // DealInitialCards();
        UpdateBroadcast msg = new()
        {
            NewRound = true,
            UpdateCards = false
        };
        InstanceFinder.ServerManager.Broadcast(msg);

        //   Instance._blindIndex = Instance.getNextIndexTurn(Instance._playersIndexes.Count, Instance._blindIndex);
    }

    [Client]
    public static void JoinWithName(string username)
    {
        Instance.AddPlayerName(username);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerName(string username, NetworkConnection sender = null)
    {
        _playerNames[sender] = username;
    }

    public static int GetPlayerIndex(NetworkConnection conn)
    {
        return Instance._playersIndexes.TryGetValue(conn, out int index) ? index : -1;
    }

    public static string GetMyHand(NetworkConnection conn)
    {
        return Instance._playerHands.TryGetValue(conn, out string hand) ? hand : string.Empty;
    }
    
    public static int HowManyCoinsToCall(NetworkConnection conn)
    {
        Debug.LogWarning($"Current bet is {Instance._currentBet}, client bet is {Instance._playerBets[conn]}");
        return Instance._currentBet - Instance._playerBets[conn];
    }

    private int GetCoinsDifference(NetworkConnection conn)
    {
        Debug.LogWarning($"Current bet is {_currentBet}, client bet is {_playerBets[conn]}");
        return _currentBet - _playerBets[conn];
    }

    private async Task GiveBlindCoins(NetworkConnection conn, int giveAmount)
    {
        _playerBets[conn] = giveAmount;
        Pot += giveAmount;
    //    await UpdateCoins(conn, giveAmount);
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

       // Debug.LogWarning("New table hand: " + string.Join(", ", _tableCards));
    }

    [Server]
    private string PullCard()
    {
        if (_deck == null || _deck.Count == 0)
        {
            _deck = new Deck(1);
        }
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
        if (_playersIndexes.Count <= 1)
        {
            cardsOnBoardCounter = 0;
            NewRoundButton.gameObject.SetActive(true);
            BroadcastWinner(_playersIndexes.Keys.First());
        }
    }

    [Client]
    public static void ClientBet(int coinAmount)
    {
        Instance.RaiseBetServer(coinAmount);
        Instance.CheckAndPassServer(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RaiseBetServer(int raiseAmount, NetworkConnection sender = null)
    {
        checkCounter = 0; // So round will begin again
        int playersBet = _playerBets[sender];
        int callAmount = _currentBet - playersBet;
        Pot += raiseAmount + callAmount;
        _playerBets[sender] = playersBet + raiseAmount + callAmount;
        _currentBet = _playerBets[sender];
    }

    [Client]
    public static void ClientCheck(bool folding)
    {
        Instance.CheckAndPassServer(folding);
    }

    private async void ClientCall(NetworkConnection sender)
    {
        int callAmount = _currentBet - _playerBets[sender];
        if (callAmount > 0)
        {
            Pot += callAmount;
            _playerBets[sender] = _currentBet;
            await UpdateCoins(sender, callAmount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckAndPassServer(bool isFolding, NetworkConnection sender = null)
    {
        int numOfPlayers = _playersIndexes.Count;

        checkCounter++;

        if(!isFolding)
            ClientCall(sender);

        if(checkCounter >= numOfPlayers)
        {
            if (cardsOnBoardCounter == 5)
            {
                RoundFinished();
                return;
            }
            if (!flopRevealed)
            {
                flopRevealed = true;
                RevealNewCardOnTable();
                RevealNewCardOnTable();
                cardsOnBoardCounter = 2;
            }
            RevealNewCardOnTable();
            checkCounter = 0;
            cardsOnBoardCounter++;
        }
       
        PassTurnToNextClient(sender);
    } 

    private void RoundFinished()
    {
        cardsOnBoardCounter = 0;
        DetermineWinner();
        NewRoundButton.gameObject.SetActive(true);
    }

    /*private bool EveryOneCalledOrSetTurn(NetworkConnection sender) 
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
    }*/

    private void PassTurnToNextClient(NetworkConnection sender = null)
    {
        if (sender == null)
            Debug.LogError("Sender conn is null");
        _playerIsMyTurn[sender] = false;
        NetworkConnection nextClient = GetNextPlayersTurn(sender);

        if (nextClient != null && !sender.Equals(nextClient)) // To Add a not dealer check
        {
            _playerIsMyTurn[nextClient] = true;
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
        if (!_playerNames.TryGetValue(conn, out string name))
            Debug.LogError("Player's name is not asigned");

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
            Debug.LogWarning("The player asking for his turn does not exist");
            return false;
        }
        return isTurn;
    }

    [Server]
    private async void PlayersNewRoundInit()
    {
        int i = 0;
        foreach (NetworkConnection conn in base.NetworkManager.ServerManager.Clients.Values)
        {
            if (i == 0)
            {
                i++;
                continue; // Skip the host
            }
            _playersIndexes[conn] = GenerateNewPlayerIndex();
            if (i == 1)
            {
                _playerIsMyTurn[conn] = true;
                await GiveBlindCoins(conn, _bigBlind);
            }
            else if (i == 2)
            {
                _playerIsMyTurn[conn] = false;
                await GiveBlindCoins(conn, _smallBlind);
            }
            else
            {
                _playerIsMyTurn[conn] = false;
                _playerBets[conn] = 0;
            }
            _playerHands[conn] = PullCard() + ", " + PullCard();
            Debug.LogWarning($"Gave 2 cards to player index {_playersIndexes[conn]}");
            i++;
        }
    }

    public static SyncDictionary<NetworkConnection, string> GetPlayersNames()
    {
        return Instance._playerNames;
    }

    [Server]
    private int GenerateNewPlayerIndex()
    {
        int numOfTotalPlayers = base.NetworkManager.ServerManager.Clients.Values.Count - 1;
        playerIndex = (playerIndex + 1) % numOfTotalPlayers;
        return playerIndex;
    }

    public void DetermineWinner()
    {
        Dictionary<NetworkConnection, HandValue> playerHandValues = new();

        foreach (var player in _playerHands)
        {
            List<Card> handCards = Deck.ParseHandString(player.Value);
            List<Card> communityCards = Deck.ParseHandString(string.Join(", ", _tableCards));

            List<Card> allCards = handCards.Concat(communityCards).ToList();

            HandValue bestHand = PokerHandEvaluator.EvaluateBestHand(allCards);

            playerHandValues[player.Key] = bestHand;
        }

        NetworkConnection winningPlayer = playerHandValues.Aggregate((l, r) => l.Value.CompareTo(r.Value) > 0 ? l : r).Key;

        BroadcastWinner(winningPlayer);
    }

    [Server]
    private async void BroadcastWinner(NetworkConnection winner)
    {
        string winnerName = _playerNames[winner];
        WinText.text = winnerName + " Won " + PotValue.text + " coins!";
        await UpdateCoins(winner, -1 * int.Parse(PotValue.text));
        NewRoundButton.gameObject.SetActive(true);

        UpdateBroadcast msg = new()
        {
            IsWinMessage = true,
            WinnerName = winnerName
        };
        InstanceFinder.ServerManager.Broadcast(msg);
    }

    public void NewRoundButton_OnClick()
    {
        newRoundInit();
    }


    private void UpdatePotValueText()
    {
        if (InstanceFinder.IsServer)
        {
            Debug.LogWarning("Updating pot value to " + _pot);
            PotValue.text = _pot.ToString();
        }
    }

    public struct UpdateBroadcast : IBroadcast
    {
        public bool NewRound;
        public bool UpdateCards;
        public string CardToAdd;
        public bool IsWinMessage;
        public string WinnerName;
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
