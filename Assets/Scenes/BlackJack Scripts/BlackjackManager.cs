using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class BlackjackManager : MonoBehaviour
{
    private Deck deck;
    private List<Player> players;
    private Player dealer;
    private int currentPlayerIndex;
    public int numberOfPlayers = 1;  // Default to 1 player, can be set from Unity Inspector

    public TextMeshProUGUI deckText; // Reference to the TextMesh Pro object
    public List<TextMeshProUGUI> playerHandsText; // List of TextMesh Pro objects for player hands
    public TextMeshProUGUI dealerHandText; // Reference to the TextMesh Pro object for dealer hand
    public TextMeshProUGUI currentPlayerText; // Reference to the TextMesh Pro object for displaying current player's turn

    public Button hitButton; // Reference to the Hit button
    public Button standButton; // Reference to the Stand button

    void Start()
    {
        InitializeGame(numberOfPlayers);
        StartNewRound();
        AttachButtonListeners();
    }
    void AttachButtonListeners()
    {
        if (hitButton != null)
        {
            hitButton.onClick.AddListener(PlayerHit);
        }
        if (standButton != null)
        {
            standButton.onClick.AddListener(PlayerStand);
        }
    }

    void InitializeGame(int numberOfPlayers)
    {
        deck = new Deck(5);  // Initialize with five decks
        players = new List<Player>();

        // Ensure number of players is between 1 and 5
      //  numberOfPlayers = 2; // Mathf.Clamp(numberOfPlayers, 1, 5); get number of playrs from server

        // Initialize players
        for (int i = 0; i < numberOfPlayers; i++)
        {
            players.Add(new Player());
        }

        dealer = new Player();
    }

    void StartNewRound()
    {
        if (deck == null || deck.Count < 90)
        { // Check if a new deck is needed
            deck = new Deck(5);
        }
        deck.Shuffle();

        // To add to game server manager 
        /*foreach (Player player in players) 
        {
            player.Hand.Clear();
            player.HasStood = false;
            player.IsBusted = false;
            player.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());
        }

        dealer.Hand.Clear();
        dealer.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());*/

        currentPlayerIndex = 0;
        Debug.Log("New round started.");


        UpdateDeckText(); // Update the TextMesh Pro object with the current deck contents
        UpdatePlayerHandsText(); // Update the TextMesh Pro objects with the player hands
        UpdateDealerHandText(); // Update the TextMesh Pro object with the dealer hand
        UpdateCurrentPlayerText(); // Update the TextMesh Pro object with the current player's turn
    }

    public void PlayerHit()
    {
        if (!players[currentPlayerIndex].HasStood && !players[currentPlayerIndex].IsBusted)
        {
        //    players[currentPlayerIndex].AddCard(deck.DrawCard());
            CheckPlayerState();

            UpdateDeckText(); // Update the TextMesh Pro object with the current deck contents
            UpdatePlayerHandsText(); // Update the TextMesh Pro objects with the player hands
            UpdateDealerHandText(); // Update the TextMesh Pro object with the dealer hand
        }
    }

    public void PlayerStand()
    {
        players[currentPlayerIndex].HasStood = true;
        CheckPlayerState();
    }

    private void CheckPlayerState()
    {
        if (players[currentPlayerIndex].IsBusted)
        {
            Debug.Log("Player " + currentPlayerIndex + " has busted.");
        }

        if (players[currentPlayerIndex].HasStood || players[currentPlayerIndex].IsBusted)
        {
            NextPlayer();
        }
    }

    private void NextPlayer()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex < players.Count)
        {
            UpdateCurrentPlayerText(); // Update the TextMesh Pro object with the current player's turn
        }
        else
        {
            DealerTurn();
        }
    }

    private void DealerTurn()
    {
        while (dealer.GetHandValue() < 17 && !dealer.IsBusted)
        {
      //      dealer.AddCard(deck.DrawCard());
        }
        Debug.Log("Dealer stands with a total of " + dealer.GetHandValue());
        EndRound();

        UpdateDeckText(); // Update the TextMesh Pro object with the current deck contents
        UpdatePlayerHandsText(); // Update the TextMesh Pro objects with the player hands
        UpdateDealerHandText(); // Update the TextMesh Pro object with the dealer hand
    }

    private void EndRound()
    {
        int dealerScore = dealer.GetHandValue();
        foreach (Player player in players)
        {
            int playerScore = player.GetHandValue();
            if (!player.IsBusted)
            {
                if (dealerScore > 21 || playerScore > dealerScore)
                {
                    Debug.Log("Player " + players.IndexOf(player) + " wins.");
                }
                else if (playerScore == dealerScore)
                {
                    Debug.Log("Player " + players.IndexOf(player) + " pushes.");
                }
                else
                {
                    Debug.Log("Player " + players.IndexOf(player) + " loses.");
                }
            }
            else
            {
                Debug.Log("Player " + players.IndexOf(player) + " busts and loses.");
            }
        }
        // Optionally, ask if players want to start a new round
        Invoke("StartNewRound", 5); // Delay new round start by 5 seconds
    }

    private void UpdateDeckText()
    {
        if (deckText != null)
        {
            deckText.text = GetDeckContents();
        }
    }

    private void UpdatePlayerHandsText()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i < playerHandsText.Count && playerHandsText[i] != null)
            {
                playerHandsText[i].text = GetPlayerHandContents(players[i]);
            }
        }
    }
    private void UpdateDealerHandText()
    {
        if (dealerHandText != null)
        {
            dealerHandText.text = GetDealerHandContents();
        }
    }
    private void UpdateCurrentPlayerText()
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = "Player " + (currentPlayerIndex + 1) + "'s turn";
        }
    }
    private string GetDeckContents()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var card in deck.GetCards())
        {
            sb.Append(card.GetRank()).Append(" of ").Append(card.GetSuit()).Append("\n");
        }
        return sb.ToString();
    }
    private string GetPlayerHandContents(Player player)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var card in player.Hand)
        {
            sb.Append(card.GetRank()).Append(" of ").Append(card.GetSuit()).Append("\n");
        }
        return sb.ToString();
    }
    private string GetDealerHandContents()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var card in dealer.Hand)
        {
            sb.Append(card.GetRank()).Append(" of ").Append(card.GetSuit()).Append("\n");
        }
        return sb.ToString();
    }
}
