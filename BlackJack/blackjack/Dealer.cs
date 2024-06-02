using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using UnityEngine;

public class Dealer : NetworkBehaviour
{
    [SyncObject]
    public readonly SyncList<Card> Hand = new SyncList<Card>();

    public void AddCardToHand(Card card)
    {
        Hand.Add(card);
    }

    public int CalculateHandValue()
    {
        int value = 0;
        int aces = 0;

        foreach (Card card in Hand)
        {
            int cardValue = card.GetHandValue(); // This method should be defined similarly as in Player
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

    // Logic for dealer's moves
    public void PlayDealerHand()
    {
        while (CalculateHandPotency() < 17)
        {
            Card card = GameManager.Instance.Deck.DrawCard();
            AddHandToCart(card);
        }
    }
}
