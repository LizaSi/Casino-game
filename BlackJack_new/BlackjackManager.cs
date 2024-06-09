using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlackjackManager : MonoBehaviour
{
    private Deck deck;
    private List<Player> players;
    private Player dealer;
    private int currentPlayerIndex;
    public int numberOfPlayers = 1;  // Default to 1 player, can be set from Unity Inspector

    void Start()
    {
        InitializeGame();
        StartNewRound();
    }

    void InitializeGame()
    {
        deck = new Deck(5);  // Initialize with five decks
        players = new List<Player>();

        // Ensure number of players is between 1 and 5
        numberOfPlayers = Mathf.Clamp(numberOfPlayers, 1, 5);

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

        foreach (Player player in players)
        {
            player.Hand.Clear();
            player.HasStood = false;
            player.IsBusted = false;
            player.AddCard(deck.DrawCard());
            player.AddCard(deck.DrawCard());
        }

        dealer.Hand.Clear();
        dealer.AddCard(deck.DrawCard());
        dealer.AddCard(deck.DrawCard());

        currentPlayerIndex = 0;
        Debug.Log("New round started.");
    }

    public void PlayerHit()
    {
        if (!players[currentPlayerIndex].HasStood && !players[currentPlayerIndex].IsBusted)
        {
            players[currentPlayerIndex].AddCard(deck.DrawCard());
            CheckPlayerState();
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
        if (currentPlayerIndex >= players.Count)
        {
            DealerTurn();
        }
    }

    private void DealerTurn()
    {
        while (dealer.GetHandValue() < 17 && !dealer.IsBusted)
        {
            dealer.AddCard(deck.DrawCard());
        }
        Debug.Log("Dealer stands with a total of " + dealer.GetHandValue());
        EndRound();
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
}

