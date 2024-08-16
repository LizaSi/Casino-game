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
using Unity.VisualScripting;
using System.Reflection;

public class PokerDisplayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text winText;
    [SerializeField] private Button newRoundButton;
    [SerializeField] private GameObject pokerComponentsParent;
    [SerializeField] private Transform CardTransform;
    [SerializeField] private Transform TableCardTransform;
    [SerializeField] private TMP_Text checkButtonText;
    [SerializeField] private TMP_Text betCoinsText;
    [SerializeField] private TMP_InputField betInput;

    private float cardSpacing = 2.8f;
    private float tableCardSpacing = 1.5f;
    private int tableSpaceIndex = 0;
    private List<GameObject> spawnedCards = new();
    private List<string> spawnedCardNames = new();
    private List<Card> cardsOnTable = new();

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
        setPlayerCamera();
        InitGame();
    }

    private void setPlayerCamera()
    {
        if (base.Owner.IsLocalClient && !InstanceFinder.IsServer)
        {
            int playerIndex = GetPlayerIndex(base.Owner);
            Debug.LogWarning($"client index is {playerIndex}");
            PlayerDisplayer.SetCameraPoker(playerIndex - 1); // -1 cuz index 1 is the host    }
        }
    }

    public void InitGame()
    {        
        StartCoroutine(ClientTurnInDelay());
        PokerServerManager.JoinWithName(base.Owner, LoggedUser.Username);        
        InstanceFinder.ClientManager.RegisterBroadcast<TurnPassBroadcast>(OnTurnPassBroadcast);
        InstanceFinder.ClientManager.RegisterBroadcast<UpdateBroadcast>(OnUpdateFromServer);
        InstanceFinder.ClientManager.RegisterBroadcast<ClientMsgBroadcast>(OnClientMsgBroadcast);
        NewRoundInit();
        if (base.Owner.IsLocalClient && !InstanceFinder.IsServer)
        {
            PlayerDisplayer.SetCamera(PokerServerManager.GetPlayerIndex(base.Owner)); // -1? cuz index 1 is the host    }
        }
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

        if (spawnedCardNames.Contains(cardNames[0]) || spawnedCardNames.Contains(cardNames[1]))
            return;
        for (int j = 0; j < cardNames.Length; j++)
        {
            string cardName = cardNames[j].Trim();
            Debug.LogWarning("Displaying card " + cardName);

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
            //Vector3 newPosition = CardTransform.position + new Vector3(spaceIndex * cardSpacing, 0, 0);

            //instantiatedCard.transform.SetPositionAndRotation(newPosition, CardTransform.rotation);
            //instantiatedCard.transform.localScale = CardTransform.localScale;

            ///////
            int playerIndex = PokerServerManager.GetPlayerIndex(base.Owner);
            instantiatedCard.transform.localScale = new Vector3(3f, 3f, 3f);
            instantiatedCard.transform.rotation = Quaternion.identity;
            if (playerIndex == 2)///V
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
            else if (playerIndex == 3)///V
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
            else if (playerIndex == 4)///V
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
            else if (playerIndex == 5)///V
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
            else if (playerIndex == 6)///V
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

    private void OnUpdateFromServer(UpdateBroadcast msg)
    {
        if (msg.UpdateCards && InstanceFinder.IsServer && !base.Owner.IsLocalClient)
        {
            DisplayCardDealer(msg.CardToAdd);
        }
        if (msg.UpdateCards && !InstanceFinder.IsServer && base.Owner.IsLocalClient)
        {
           // handleClientTurn();
        //    StartCoroutine(ClientTurnInDelay());
        }
    }

    private void DisplayCardDealer(string cardToAdd)
    {
        if (cardsOnTable.Contains(new Card(cardToAdd)))
            return;
        GameObject CardViewer = GameObject.Find("CardViewer");

     //   Debug.LogWarning("Displaying card " + cardToAdd);

        string cardDir = "Cards/" + cardToAdd;
        GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
        Vector3 newPosition = TableCardTransform.position + new Vector3(tableSpaceIndex * tableCardSpacing, 0, 0);

        instantiatedCard.transform.SetPositionAndRotation(newPosition, TableCardTransform.rotation);
        instantiatedCard.transform.localScale = TableCardTransform.localScale;

        instantiatedCard.transform.SetParent(CardViewer.transform, false);

        spawnedCards.Add(instantiatedCard);
        cardsOnTable.Add(new Card(cardToAdd));
        tableSpaceIndex++;
    }

    private async void NewRoundInit()
    {
        PokerServerManager.NewRoundInit();
        DespawnAllCards();
        newRoundButton.gameObject.SetActive(false);
        int givenAmount = await GiveBlindCoins(base.Owner);
        if(base.Owner.IsLocalClient)
            betCoinsText.text = "Gave " + givenAmount.ToString();
    }

    public void NewRound_OnClick()
    {
        NewRoundInit();
    }

    public void Fold_OnClick()
    {
        PokerServerManager.ClientCheck();
        PokerServerManager.ClientFold();
        pokerComponentsParent.SetActive(false);
    }

    public void Check_OnClick()
    {
        PokerServerManager.ClientCheck();
    }

    public void Bet_OnClick()
    {
        if(int.TryParse(betInput.text, out int amountFromUi))
            PokerServerManager.ClientBet(base.Owner, amountFromUi);
        else
        {
            Debug.LogError("Cant find input field coins");
        }
    }

    private void ShowWinMessage()
    {
        
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
            pokerComponentsParent.SetActive(true);
            int coinsToCall = HowManyCoinsToCall(base.Owner);
            if (coinsToCall > 0)
            {
                SetCheckButton(false, coinsToCall);
            }
            else
            {
                SetCheckButton(true);
            }
        }
        else
        {
            pokerComponentsParent.SetActive(false);
        }
    }

    private void SetCheckButton(bool isCheck, int callAmount = 0)
    {
        if (isCheck)
            checkButtonText.text = "Check";
        else
            checkButtonText.text = "Call " + callAmount;
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
           // handleClientTurn();
            StartCoroutine(ClientTurnInDelay());
        }
    }

    private IEnumerator ClientTurnInDelay()
    {
      //  handleClientTurn();
        yield return new WaitForSeconds(0.8f);
        handleClientTurn();
       // yield return new WaitForSeconds(1.8f);
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
