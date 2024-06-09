using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

public class Player
{
    public List<Card> Hand { get; private set; }
    public bool HasStood { get; set; }
    public bool IsBusted { get; set; }

    public Player()
    {
        Hand = new List<Card>();
        HasStood = false;
        IsBusted = false;
    }

    public int GetHandValue()
    {
        int value = 0;
        int aceCount = 0;
        foreach (var card in Hand)
        {
            int cardValue = card.GetValue();
            if (card.GetRank() == "Ace")
            {
                aceCount++;
                cardValue = 11; // Initially treat Aces as 11
            }
            value += cardValue;
        }

        // Adjust for Aces if the total value exceeds 21
        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    public void AddCard(Card card)
    {
        Hand.Add(card);
        if (GetHandValue() > 21) IsBusted = true;
    }
}


//public class Player
//{
//    public List<Card> Hand { get; private set; }
//    public bool HasStood { get; set; }
//    public bool IsBusted { get; set; }

//    public Player()
//    {
//        Hand = new List<Card>();
//        HasStood = false;
//        IsBusted = false;
//    }

//    public int GetHandValue()
//    {
//        int value = 0;
//        int aceCount = 0;
//        foreach (var card in Hand)
//        {
//            int cardValue = card.GetValue();
//            if (card.CardRank == Card.Rank.Ace)
//            {
//                aceCount++;
//            }
//            value += cardValue;
//        }

//        while (value > 21 && aceCount > 0)
//        {
//            value -= 10;
//            aceCount--;
//        }

//        return value;
//    }

//    public void AddCard(Card card)
//    {
//        Hand.Add(card);
//        if (GetHandValue() > 21) IsBusted = true;
//    }
//}