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
using UMA.CharacterSystem;

public class PokerDisplayer : NetworkBehaviour
{
    [SerializeField] private TMP_Text WinText;
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
        //setPlayerCamera();
        InitGame();
    }

    private void setPlayerCamera()
    {
        if (base.Owner.IsLocalClient && !InstanceFinder.IsServer)
        {
            int playerIndex = GetPlayerIndex(base.Owner);
            Debug.LogWarning($"client index is {playerIndex}");
            SetCameraAndAvatar(GetPlayerIndex(base.Owner), GetAvatarString(base.Owner));
           // PlayerDisplayer.SetCameraPoker(playerIndex, LoggedUser.AvatarCompressedString); // -1 cuz index 1 is the host    }
        }
    }

    public void InitGame()
    {        
        StartCoroutine(ClientTurnInDelay());
        /////////// AllWithCoins File
        /*
        PokerServerManager.JoinWithName(base.Owner, LoggedUser.Username);
        setPlayerCamera();
        */
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
        WinText.text = "";
        DespawnAllCards();
        newRoundButton.gameObject.SetActive(false);
        if (!InstanceFinder.IsServer)
        {
            StartCoroutine(JoinWithNameInDelay());
            if (base.Owner.IsLocalClient)
            {
                //////// GameLogic File
                /*
                int playerIndex = GetPlayerIndex(base.Owner);
                PlayerDisplayer.SetCameraPoker(playerIndex);
                */
                setPlayerCamera();
                //betCoinsText.text = "Gave " + givenAmount.ToString();
            }
        }
    }

    private IEnumerator JoinWithNameInDelay()
    {
        yield return new WaitForSeconds(1.7f);
        PokerServerManager.JoinWithName(LoggedUser.Username);
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
                WinText.text = "You win!";
            }
        }
        if (msg.NewRound)
        {
            DespawnAllCards();
            NewRoundInit();
        }
        if (msg.Leave)
        {
            LeaveGame();
        }
    }

    public void LeaveGame()
    {
        PokerComponentsParent.SetActive(false);
        WinText.text = "";
        CountdownTimer.RemoveTimer();
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
        WinText.text = $"You won {coinsAmount} coins!";
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
        yield return new WaitForSeconds(1.8f);
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

    private void SetCameraAndAvatar(int playerIndex, string avatarCompressedString)
    {
        if (!base.Owner.IsLocalClient && !InstanceFinder.IsServer)
        {
            return;
        }
        GameObject instantiatedPlayer = Instantiate(Resources.Load<GameObject>("Players/PlayerWithCamera"));
        DynamicCharacterAvatar avatar = instantiatedPlayer.GetComponentInChildren<DynamicCharacterAvatar>();

        if (InstanceFinder.IsServer)
        {
            Transform playerViewCameraTransform = instantiatedPlayer.transform.Find("PlayerViewCamera");
            playerViewCameraTransform.gameObject.SetActive(false);


            if (!base.Owner.IsLocalClient)
            {
                StartCoroutine(ModifyAvatarInDelay(avatar));
            }
            else
            {
                ModifyAvatarAsHost(avatar, avatarCompressedString);
            }
        }
        else
        {
            PokerServerManager.SetAvatarString(avatarCompressedString);
            //StartCoroutine(ModifyAvatarInDelay(avatar));
        }
        instantiatedPlayer.transform.localScale = new Vector3(1f, 1f, 1f);
        instantiatedPlayer.transform.rotation = Quaternion.identity;
        if (playerIndex == 0)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(-1.11f, 0f, -0.09f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Debug.LogWarning("Displaying 1st player's camera");
        }
        else if (playerIndex == 1)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(26.77f, 0f, 24.1f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -74.1f, 0f);
            Debug.LogWarning("Displaying 2nd player's camera");

        }
        else if (playerIndex == 2)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(28.49612f, 0f, 41.40894f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -106.946f, 0f);
        }
        else if (playerIndex == 3)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(26.65f, 0f, 50.72f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -127.374f, 0f);
        }
        else if (playerIndex == 5)
        {
            instantiatedPlayer.transform.localPosition = new Vector3(-14.44f, 0f, 55.26f);
            instantiatedPlayer.transform.rotation = Quaternion.Euler(0f, -211.864f, 0f);
        }
        if (instantiatedPlayer == null)
        {
            Debug.LogWarning("No player object found in Resources");
            return;
        }
    }

    private IEnumerator ModifyAvatarInDelay(DynamicCharacterAvatar avatar)
    {
        yield return new WaitForSeconds(2f);
        string clientAvatarString = GetAvatarString(base.Owner);

        if (!string.IsNullOrEmpty(clientAvatarString))
        {
            AvatarDefinition adf = AvatarDefinition.FromCompressedString(clientAvatarString, '|');
            avatar.LoadAvatarDefinition(adf);
            avatar.BuildCharacter(false); // don't restore old DNA...
        }
        else
        {
            Debug.LogError("Avatar string is null");
        }
    }

    private void ModifyAvatarAsHost(DynamicCharacterAvatar avatar, string avatarString)
    {
        if (!string.IsNullOrEmpty(avatarString))
        {
            AvatarDefinition adf = AvatarDefinition.FromCompressedString(avatarString, '|');
            avatar.LoadAvatarDefinition(adf);
            avatar.BuildCharacter(false); // don't restore old DNA...
        }
        else
        {
            Debug.LogError("Avatar string is null");
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
