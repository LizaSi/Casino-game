using FishNet.Broadcast;
using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using PlayerData;
using UnityEngine;
using FishNet.Object;
using UnityEngine.UI;
using static PokerServerManager;

public class PokerDisplayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button newRoundButton;
    [SerializeField] private Transform CardTransform;
    [SerializeField] private Transform TableCardTransform;
    [SerializeField] private TMP_Text checkButtonText;
    [SerializeField] private TMP_Text betCoinsText;
    [SerializeField] private TMP_InputField betInput;
    [SerializeField] public GameObject PokerComponentsParent;

   // private float cardSpacing = 2.8f;
    private const float tableCardSpacing = 1.5f;
    private int tableSpaceIndex = 0;
    private readonly List<GameObject> spawnedCards = new();
    private readonly List<string> spawnedCardNames = new();
    private readonly List<Card> cardsOnTable = new();

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
        PokerServerManager.OnTurnChange += OnTurnChange;
        InstanceFinder.ClientManager.RegisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.RegisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        NewRoundInit();
    }

    private void NewRoundInit()
    {
        if (InstanceFinder.IsServer)
        {
            //PokerServerManager.NewRoundInit();
        }
        tableSpaceIndex = 0;
        DespawnAllCards();
        newRoundButton.gameObject.SetActive(false);
        if (!InstanceFinder.IsServer)
        {
            PokerServerManager.JoinWithName(LoggedUser.Username);
            if (base.Owner.IsLocalClient)
            {
                int playerIndex = GetPlayerIndex(base.Owner);
                PlayerDisplayer.SetCameraPoker(playerIndex);
                //betCoinsText.text = "Gave " + givenAmount.ToString();
            }
        }
    }

    private void OnTurnChange()
    {
        throw new System.NotImplementedException();
    }

    private void OnDisable()
    {
        PokerComponentsParent.SetActive(false);
        InstanceFinder.ClientManager.UnregisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.UnregisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
    }
    
    private void OnUpdateFromServer(UpdateBroadcast msg)
    {
        if (msg.UpdateCards && InstanceFinder.IsServer && !base.Owner.IsLocalClient)
        {
            DisplayCardDealer(msg.CardToAdd);
        }
        if (msg.IsWinMessage) 
        {
            PokerComponentsParent.SetActive(false);
            CountdownTimer.StopCountDown();
            if(msg.WinnerName == LoggedUser.Username)
            {
                winText.text = "You won!";
            }
        }
        if (msg.NewRound)
        {
            DespawnAllCards();
            NewRoundInit();
        }
    }

    private void DisplayCardDealer(string cardToAdd)
    {
        if (cardsOnTable.Contains(new Card(cardToAdd)))
            return;
        GameObject CardViewer = GameObject.Find("CardViewer");

        string cardDir = "Cards/" + cardToAdd;
        GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
        Debug.LogWarning("Index card on board is " + tableSpaceIndex);
        Vector3 newPosition = TableCardTransform.position + new Vector3(tableSpaceIndex * tableCardSpacing, 0, 0);

        instantiatedCard.transform.SetPositionAndRotation(newPosition, TableCardTransform.rotation);
        instantiatedCard.transform.localScale = TableCardTransform.localScale;

        instantiatedCard.transform.SetParent(CardViewer.transform, false);

        spawnedCards.Add(instantiatedCard);
        cardsOnTable.Add(new Card(cardToAdd));
        tableSpaceIndex++;
    }    

    public void NewRound_OnClick()
    {
        NewRoundInit();
    }

    public void Fold_OnClick()
    {
        PokerServerManager.ClientCheck(true);
        PokerServerManager.ClientFold();
    }

    public void Check_OnClick()
    {
        PokerServerManager.ClientCheck(false);
    }

    public void Bet_OnClick()
    {
        if(int.TryParse(betInput.text, out int amountFromUi))
            PokerServerManager.ClientBet(amountFromUi);
        else
        {
            Debug.LogError("Cant find input field coins");
        }
    }

    private void ShowWinMessage(int coinsAmount)
    {
        winText.text = $"You won {coinsAmount} coins!";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Just in case theres a non update bug
        {
            if (!InstanceFinder.IsServer)
            {
                handleClientTurn();
            }
        }
    }

    private void handleClientTurn()
    {
        Debug.LogWarning("Handling clients turn");
        if (!base.Owner.IsLocalClient)
        { 
            return;
        }
        DisplayCardsClient();

        if (PokerServerManager.IsMyTurn(base.Owner))
        {
            int coinsToCall = HowManyCoinsToCall(base.Owner);
            if (coinsToCall > 0)
            {
                SetCheckButton(false, coinsToCall);
            }
            else
            {
                SetCheckButton(true);
            }
            PokerComponentsParent.SetActive(true);
        }
        else
        {
            PokerComponentsParent.SetActive(false);
        }
    }

    private void SetCheckButton(bool isCheck, int callAmount = 0)
    {
        if (isCheck)
            checkButtonText.text = "Check";
        else
            checkButtonText.text = "Call " + callAmount;
    }

    /*private void OnClientMsgBroadcast(ClientMsgBroadcast msg)
    {
        if (msg.IsWinMessage)
        {
            if (!InstanceFinder.IsServer && base.Owner.IsLocalClient)
            {
                ShowWinMessage(10); // To change 
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
    }*/

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
           // StartCoroutine(ClientTurnInDelay());
            CountdownTimer.StartPokerCountdown(this, base.Owner);
        }
    }

    private IEnumerator ClientTurnInDelay()
    {
      //  handleClientTurn();
        yield return new WaitForSeconds(0.8f);
        handleClientTurn();
       // yield return new WaitForSeconds(1.8f);
    }

    private void DisplayCardsClient()
    {
        GameObject CardViewer = GameObject.Find("CardViewer");
        int spaceIndex = 0;
        string hand = GetMyHand(base.Owner);
        if (string.IsNullOrEmpty(hand))
            return;
        string[] cardNames = hand.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (spawnedCardNames.Contains(cardNames[0]) || spawnedCardNames.Contains(cardNames[1]))
            return;
        for (int j = 0; j < cardNames.Length; j++)
        {
            string cardName = cardNames[j].Trim();
            Debug.LogWarning("Displaying card " + cardName);

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));

            int playerIndex = PokerServerManager.GetPlayerIndex(base.Owner);
            instantiatedCard.transform.localScale = new Vector3(3f, 3f, 3f);
            instantiatedCard.transform.rotation = Quaternion.identity;
            if (playerIndex == 0)
            {
                if (j == 0)
                {
                    instantiatedCard.transform.localPosition = new Vector3(-4.98f, 2.36f, 31.41f);
                }
                else if (j == 1)
                {
                    instantiatedCard.transform.localPosition = new Vector3(-5.098367f, 2.36f, 30.92167f);
                }
                instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -76.942f);
            }
            else if (playerIndex == 1)///V
            {
                if (j == 0)
                {
                    instantiatedCard.transform.localPosition = new Vector3(-4.57f, 2.36f, 28.96f);
                }
                else if (j == 1)
                {
                    instantiatedCard.transform.localPosition = new Vector3(-4.125763f, 2.36f, 28.69727f);
                }
                instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -149.03f);
            }
            else if (playerIndex == 2)///V
            {
                if (j == 0)
                {
                    instantiatedCard.transform.localPosition = new Vector3(-0.42f, 2.36f, 28.5f);
                }
                else if (j == 1)
                {
                    instantiatedCard.transform.localPosition = new Vector3(0.046f, 2.36f, 28.51398f);
                }
                instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -181.876f);
            }
            else if (playerIndex == 3)///V
            {
                if (j == 0)
                {
                    instantiatedCard.transform.localPosition = new Vector3(4.413039f, 2.36f, 28.68334f);
                }
                else if (j == 1)
                {
                    instantiatedCard.transform.localPosition = new Vector3(3.955261f, 2.36f, 28.48266f);
                }
                instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -204.059f);
            }
            else if (playerIndex == 5)///V
            {
                if (j == 0)
                {
                    instantiatedCard.transform.localPosition = new Vector3(5.359648f, 2.36f, 31.00982f);
                }
                else if (j == 1)
                {
                    instantiatedCard.transform.localPosition = new Vector3(5.515435f, 2.36f, 30.53488f);
                }
                instantiatedCard.transform.rotation = Quaternion.Euler(270f, 0f, -288.549f);
            }
            ///////

            // Set the parent of the instantiated card to CardViewer
            //instantiatedCard.transform.SetParent(CardViewer.transform, false);

            spawnedCardNames.Add(cardName);
            spawnedCards.Add(instantiatedCard);
            spaceIndex++;
        }
    }

    private void DespawnAllCards()
    {
        GameObject[] cards = GameObject.FindGameObjectsWithTag("Card");

        foreach (GameObject card in cards)
        {
            Destroy(card);
        }

        spawnedCards.Clear();
        cardsOnTable.Clear();
    }

    public struct ClientMsgBroadcast : IBroadcast
    {
        public bool IsWinMessage;
        public bool IsNewRoundMessage;
    }
}
