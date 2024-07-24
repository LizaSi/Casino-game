using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum CardsByOrder
{
    Card_HeartAce = 0, Card_Heart2, Card_Heart3, Card_Heart4, Card_Heart5, Card_Heart6, Card_Heart7, Card_Heart8, Card_Heart9, Card_Heart10, Card_HeartJack, Card_HeartQueen, Card_HeartKing,
    Card_ClubAce, Card_Club2, Card_Club3, Card_Club4, Card_Club5, Card_Club6, Card_Club7, Card_Club8, Card_Club9, Card_Club10, Card_ClubJack, Card_ClubQueen, Card_ClubKing,
    Card_DiamondAce, Card_Diamond2, Card_Diamond3, Card_Diamond4, Card_Diamond5, Card_Diamond6, Card_Diamond7, Card_Diamond8, Card_Diamond9, Card_Diamond10, Card_DiamondJack, Card_DiamondQueen, Card_DiamondKing,
    Card_SpadeAce, Card_Spade2, Card_Spade3, Card_Spade4, Card_Spade5, Card_Spade6, Card_Spade7, Card_Spade8, Card_Spade9, Card_Spade10, Card_SpadeJack, Card_SpadeQueen, Card_SpadeKing
}

public class Card
{
    public CardsByOrder CardType { get; private set; }

    public Card(CardsByOrder cardType)
    {
        CardType = cardType;
    }

    public static int StringToCard(string cardAsText)
    {
        return translateTextToEnumCard(cardAsText);
    }

    public string CardToString()
    {
        return translateEnumCardToText(CardType);
    }

    private static int translateTextToEnumCard(string cardAsText)
    {
        int card = -1;
        switch (cardAsText)
        {
            case "Card_HeartAce":
                card = (int)CardsByOrder.Card_HeartAce;
                break;
            case "Card_Heart2":
                card = (int)CardsByOrder.Card_Heart2;
                break;
            case "Card_Heart3":
                card = (int)CardsByOrder.Card_Heart3;
                break;
            case "Card_Heart4":
                card = (int)CardsByOrder.Card_Heart4;
                break;
            case "Card_Heart5":
                card = (int)CardsByOrder.Card_Heart5;
                break;
            case "Card_Heart6":
                card = (int)CardsByOrder.Card_Heart6;
                break;
            case "Card_Heart7":
                card = (int)CardsByOrder.Card_Heart7;
                break;
            case "Card_Heart8":
                card = (int)CardsByOrder.Card_Heart8;
                break;
            case "Card_Heart9":
                card = (int)CardsByOrder.Card_Heart9;
                break;
            case "Card_Heart10":
                card = (int)CardsByOrder.Card_Heart10;
                break;
            case "Card_HeartJack":
                card = (int)CardsByOrder.Card_HeartJack;
                break;
            case "Card_HeartQueen":
                card = (int)CardsByOrder.Card_HeartQueen;
                break;
            case "Card_HeartKing":
                card = (int)CardsByOrder.Card_HeartKing;
                break;
            case "Card_ClubAce":
                card = (int)CardsByOrder.Card_ClubAce;
                break;
            case "Card_Club2":
                card = (int)CardsByOrder.Card_Club2;
                break;
            case "Card_Club3":
                card = (int)CardsByOrder.Card_Club3;
                break;
            case "Card_Club4":
                card = (int)CardsByOrder.Card_Club4;
                break;
            case "Card_Club5":
                card = (int)CardsByOrder.Card_Club5;
                break;
            case "Card_Club6":
                card = (int)CardsByOrder.Card_Club6;
                break;
            case "Card_Club7":
                card = (int)CardsByOrder.Card_Club7;
                break;
            case "Card_Club8":
                card = (int)CardsByOrder.Card_Club8;
                break;
            case "Card_Club9":
                card = (int)CardsByOrder.Card_Club9;
                break;
            case "Card_Club10":
                card = (int)CardsByOrder.Card_Club10;
                break;
            case "Card_ClubJack":
                card = (int)CardsByOrder.Card_ClubJack;
                break;
            case "Card_ClubQueen":
                card = (int)CardsByOrder.Card_ClubQueen;
                break;
            case "Card_ClubKing":
                card = (int)CardsByOrder.Card_ClubKing;
                break;
            case "Card_DiamondAce":
                card = (int)CardsByOrder.Card_DiamondAce;
                break;
            case "Card_Diamond2":
                card = (int)CardsByOrder.Card_Diamond2;
                break;
            case "Card_Diamond3":
                card = (int)CardsByOrder.Card_Diamond3;
                break;
            case "Card_Diamond4":
                card = (int)CardsByOrder.Card_Diamond4;
                break;
            case "Card_Diamond5":
                card = (int)CardsByOrder.Card_Diamond5;
                break;
            case "Card_Diamond6":
                card = (int)CardsByOrder.Card_Diamond6;
                break;
            case "Card_Diamond7":
                card = (int)CardsByOrder.Card_Diamond7;
                break;
            case "Card_Diamond8":
                card = (int)CardsByOrder.Card_Diamond8;
                break;
            case "Card_Diamond9":
                card = (int)CardsByOrder.Card_Diamond9;
                break;
            case "Card_Diamond10":
                card = (int)CardsByOrder.Card_Diamond10;
                break;
            case "Card_DiamondJack":
                card = (int)CardsByOrder.Card_DiamondJack;
                break;
            case "Card_DiamondQueen":
                card = (int)CardsByOrder.Card_DiamondQueen;
                break;
            case "Card_DiamondKing":
                card = (int)CardsByOrder.Card_DiamondKing;
                break;
            case "Card_SpadeAce":
                card = (int)CardsByOrder.Card_SpadeAce;
                break;
            case "Card_Spade2":
                card = (int)CardsByOrder.Card_Spade2;
                break;
            case "Card_Spade3":
                card = (int)CardsByOrder.Card_Spade3;
                break;
            case "Card_Spade4":
                card = (int)CardsByOrder.Card_Spade4;
                break;
            case "Card_Spade5":
                card = (int)CardsByOrder.Card_Spade5;
                break;
            case "Card_Spade6":
                card = (int)CardsByOrder.Card_Spade6;
                break;
            case "Card_Spade7":
                card = (int)CardsByOrder.Card_Spade7;
                break;
            case "Card_Spade8":
                card = (int)CardsByOrder.Card_Spade8;
                break;
            case "Card_Spade9":
                card = (int)CardsByOrder.Card_Spade9;
                break;
            case "Card_Spade10":
                card = (int)CardsByOrder.Card_Spade10;
                break;
            case "Card_SpadeJack":
                card = (int)CardsByOrder.Card_SpadeJack;
                break;
            case "Card_SpadeQueen":
                card = (int)CardsByOrder.Card_SpadeQueen;
                break;
            case "Card_SpadeKing":
                card = (int)CardsByOrder.Card_SpadeKing;
                break;
        }

        return card;
    }
    private string translateEnumCardToText(CardsByOrder cardType)
    {
        string cardText = string.Empty;

        switch (cardType)
        {
            case CardsByOrder.Card_HeartAce:
                cardText = "Card_HeartAce";
                break;
            case CardsByOrder.Card_Heart2:
                cardText = "Card_Heart2";
                break;
            case CardsByOrder.Card_Heart3:
                cardText = "Card_Heart3";
                break;
            case CardsByOrder.Card_Heart4:
                cardText = "Card_Heart4";
                break;
            case CardsByOrder.Card_Heart5:
                cardText = "Card_Heart5";
                break;
            case CardsByOrder.Card_Heart6:
                cardText = "Card_Heart6";
                break;
            case CardsByOrder.Card_Heart7:
                cardText = "Card_Heart7";
                break;
            case CardsByOrder.Card_Heart8:
                cardText = "Card_Heart8";
                break;
            case CardsByOrder.Card_Heart9:
                cardText = "Card_Heart9";
                break;
            case CardsByOrder.Card_Heart10:
                cardText = "Card_Heart10";
                break;
            case CardsByOrder.Card_HeartJack:
                cardText = "Card_HeartJack";
                break;
            case CardsByOrder.Card_HeartQueen:
                cardText = "Card_HeartQueen";
                break;
            case CardsByOrder.Card_HeartKing:
                cardText = "Card_HeartKing";
                break;
            case CardsByOrder.Card_ClubAce:
                cardText = "Card_ClubAce";
                break;
            case CardsByOrder.Card_Club2:
                cardText = "Card_Club2";
                break;
            case CardsByOrder.Card_Club3:
                cardText = "Card_Club3";
                break;
            case CardsByOrder.Card_Club4:
                cardText = "Card_Club4";
                break;
            case CardsByOrder.Card_Club5:
                cardText = "Card_Club5";
                break;
            case CardsByOrder.Card_Club6:
                cardText = "Card_Club6";
                break;
            case CardsByOrder.Card_Club7:
                cardText = "Card_Club7";
                break;
            case CardsByOrder.Card_Club8:
                cardText = "Card_Club8";
                break;
            case CardsByOrder.Card_Club9:
                cardText = "Card_Club9";
                break;
            case CardsByOrder.Card_Club10:
                cardText = "Card_Club10";
                break;
            case CardsByOrder.Card_ClubJack:
                cardText = "Card_ClubJack";
                break;
            case CardsByOrder.Card_ClubQueen:
                cardText = "Card_ClubQueen";
                break;
            case CardsByOrder.Card_ClubKing:
                cardText = "Card_ClubKing";
                break;
            case CardsByOrder.Card_DiamondAce:
                cardText = "Card_DiamondAce";
                break;
            case CardsByOrder.Card_Diamond2:
                cardText = "Card_Diamond2";
                break;
            case CardsByOrder.Card_Diamond3:
                cardText = "Card_Diamond3";
                break;
            case CardsByOrder.Card_Diamond4:
                cardText = "Card_Diamond4";
                break;
            case CardsByOrder.Card_Diamond5:
                cardText = "Card_Diamond5";
                break;
            case CardsByOrder.Card_Diamond6:
                cardText = "Card_Diamond6";
                break;
            case CardsByOrder.Card_Diamond7:
                cardText = "Card_Diamond7";
                break;
            case CardsByOrder.Card_Diamond8:
                cardText = "Card_Diamond8";
                break;
            case CardsByOrder.Card_Diamond9:
                cardText = "Card_Diamond9";
                break;
            case CardsByOrder.Card_Diamond10:
                cardText = "Card_Diamond10";
                break;
            case CardsByOrder.Card_DiamondJack:
                cardText = "Card_DiamondJack";
                break;
            case CardsByOrder.Card_DiamondQueen:
                cardText = "Card_DiamondQueen";
                break;
            case CardsByOrder.Card_DiamondKing:
                cardText = "Card_DiamondKing";
                break;
            case CardsByOrder.Card_SpadeAce:
                cardText = "Card_SpadeAce";
                break;
            case CardsByOrder.Card_Spade2:
                cardText = "Card_Spade2";
                break;
            case CardsByOrder.Card_Spade3:
                cardText = "Card_Spade3";
                break;
            case CardsByOrder.Card_Spade4:
                cardText = "Card_Spade4";
                break;
            case CardsByOrder.Card_Spade5:
                cardText = "Card_Spade5";
                break;
            case CardsByOrder.Card_Spade6:
                cardText = "Card_Spade6";
                break;
            case CardsByOrder.Card_Spade7:
                cardText = "Card_Spade7";
                break;
            case CardsByOrder.Card_Spade8:
                cardText = "Card_Spade8";
                break;
            case CardsByOrder.Card_Spade9:
                cardText = "Card_Spade9";
                break;
            case CardsByOrder.Card_Spade10:
                cardText = "Card_Spade10";
                break;
            case CardsByOrder.Card_SpadeJack:
                cardText = "Card_SpadeJack";
                break;
            case CardsByOrder.Card_SpadeQueen:
                cardText = "Card_SpadeQueen";
                break;
            case CardsByOrder.Card_SpadeKing:
                cardText = "Card_SpadeKing";
                break;
        }

        return cardText;
    }

    public int GetValue()
    {
        int value = (int)CardType % 13 + 1; // Cards are indexed 0-12 for each suit
        if (value > 10)
        {
            value = 10; // Jack, Queen, King are all valued at 10
        }
        return value;
    }

    public string GetSuit()
    {
        if (CardType <= CardsByOrder.Card_HeartKing)
        {
            return "Hearts";
        }
        else if (CardType <= CardsByOrder.Card_ClubKing)
        {
            return "Clubs";
        }
        else if (CardType <= CardsByOrder.Card_DiamondKing)
        {
            return "Diamonds";
        }
        else
        {
            return "Spades";
        }
    }

    public string GetRank()
    {
        int value = (int)CardType % 13 + 1;
        switch (value)
        {
            case 1:
                return "Ace";
            case 11:
                return "Jack";
            case 12:
                return "Queen";
            case 13:
                return "King";
            default:
                return value.ToString();
        }
    }
}

//public class Card
//{

//    public enum Suit { Hearts, Diamonds, Clubs, Spades }
//    public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }

//    public Suit CardSuit { get; private set; }
//    public Rank CardRank { get; private set; }

//    public Card(Suit suit, Rank rank)
//    {
//        CardSuit = suit;
//        CardRank = rank;
//    }

//    public int GetValue()
//    {
//        if (CardRank <= Rank.Ten)
//        {
//            return (int)CardRank;
//        }
//        else if (CardRank <= Rank.King)
//        {
//            return 10;
//        }
//        else // Ace
//        {
//            return 11;
//        }
//    }
//}