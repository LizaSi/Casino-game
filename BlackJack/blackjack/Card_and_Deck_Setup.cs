using System.Collections.Generic;
using System;

public enum Suit { Clubs, Diamonds, Hearts, Spades }
public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

public class Card
{
    public Suit Suit;
    public Rank Rank;

    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }
}

public class Deck
{
    private List<Card> cards;

    public Deck()
    {
        cards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(suit, rank));
            }
        }
        Shuffle();
    }

    public void Shuffle()
    {
        System.Random random = new System.Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card DrawCard()
    {
        if (cards.Count > 0)
        {
            Card card = cards[0];
            cards.RemoveAt(0);
            return card;
        }
        throw new InvalidOperationException("No cards left in the deck");
    }
}
