using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SyncVar]
    public bool IsStood = false;
    [SyncVar]
    public bool IsBust = false;

    [SyncObject]
    public readonly SyncList<Card> Hand = new SyncList<Card>();

    // Add a card to the player's hand
    public void AddCardToHand(Card card)
    {
        Hand.Add(card);
    }

    // Calculate the value of the hand, accounting for Aces as 1 or 11
    public int CalculateHandValue()
    {
        int value = 0;
        int aces = 0;

        foreach (Card card in Hand)
        {
            int cardValue = card.GetCardValue(); // Assume GetCardConflict() is implemented in Card
            value += cardValue;

            if (card.Rank == Rank.Ace)
            {
                aces++;
            }
        }

        // Adjust for Aces
        while (value > 21 && aces > 0)
        {
            value -= 10;
            aces--;
        }

        return value;
    }

    public void CheckForBust()
    {
        if (CalculateHandValue() > 21)
        {
            IsBust = true;
        }
    }
}
