using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Card
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

    public Suit CardSuit { get; private set; }
    public Rank CardRank { get; private set; }

    public Card(Suit suit, Rank rank)
    {
        CardSuit = suit;
        CardRank = rank;
    }

    public int GetValue()
    {
        if (CardRank <= Rank.Ten)
        {
            return (int)CardRank;
        }
        else if (CardRank <= Rank.King)
        {
            return 10;
        }
        else // Ace
        {
            return 11;
        }
    }
}