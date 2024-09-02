using System;
using System.Collections.Generic;
using System.Linq;

public static class PokerHandEvaluator
{
    public static HandValue EvaluateBestHand(List<Card> allCards)
    {
        // Sort the cards by rank to make it easier to evaluate hands
        allCards = allCards.OrderByDescending(card => card.GetValue()).ToList();

        // Check for the highest ranking hands in order of poker hand rankings
        if (IsStraightFlush(allCards, out HandValue handValue)) return handValue;
        if (IsFourOfAKind(allCards, out handValue)) return handValue;
        if (IsFullHouse(allCards, out handValue)) return handValue;
        if (IsFlush(allCards, out handValue)) return handValue;
        if (IsStraight(allCards, out handValue)) return handValue;
        if (IsThreeOfAKind(allCards, out handValue)) return handValue;
        if (IsTwoPair(allCards, out handValue)) return handValue;
        if (IsOnePair(allCards, out handValue)) return handValue;

        // If no other hand is found, return the high card
        return GetHighCard(allCards);
    }

    private static bool IsStraightFlush(List<Card> cards, out HandValue handValue)
    {
        if (IsFlush(cards, out handValue) && IsStraight(cards, out _))
        {
            handValue = new HandValue(HandRank.StraightFlush, cards.Take(5).ToList());
            return true;
        }
        handValue = null;
        return false;
    }

    private static bool IsFourOfAKind(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetValue());
        var fourOfAKindGroup = groups.FirstOrDefault(g => g.Count() == 4);

        if (fourOfAKindGroup != null)
        {
            var remainingCard = groups.First(g => g.Count() != 4).First();
            handValue = new HandValue(HandRank.FourOfAKind, fourOfAKindGroup.Concat(new[] { remainingCard }).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsFullHouse(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetValue()).OrderByDescending(g => g.Count());

        if (groups.ElementAt(0).Count() == 3 && groups.ElementAt(1).Count() == 2)
        {
            handValue = new HandValue(HandRank.FullHouse, groups.ElementAt(0).Concat(groups.ElementAt(1)).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsFlush(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetSuit());
        var flushGroup = groups.FirstOrDefault(g => g.Count() >= 5);

        if (flushGroup != null)
        {
            handValue = new HandValue(HandRank.Flush, flushGroup.Take(5).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsStraight(List<Card> cards, out HandValue handValue)
    {
        cards = cards.Distinct(new CardValueComparer()).OrderByDescending(card => card.GetValue()).ToList();
        int consecutive = 1;
        Card highCard = cards[0];

        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].GetValue() == cards[i - 1].GetValue() - 1)
            {
                consecutive++;
                if (consecutive == 5)
                {
                    handValue = new HandValue(HandRank.Straight, cards.Skip(i - 4).Take(5).ToList());
                    return true;
                }
            }
            else
            {
                consecutive = 1;
            }
        }

        // Special case for Ace-low straight (A, 2, 3, 4, 5)
        if (consecutive == 4 && cards.Last().GetRank() == "Ace" && cards.First().GetValue() == 5)
        {
            handValue = new HandValue(HandRank.Straight, new List<Card> { cards.Last() }.Concat(cards.Take(4)).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsThreeOfAKind(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetValue());
        var threeOfAKindGroup = groups.FirstOrDefault(g => g.Count() == 3);

        if (threeOfAKindGroup != null)
        {
            var kickers = groups.Where(g => g.Count() < 3).SelectMany(g => g).OrderByDescending(card => card.GetValue()).Take(2).ToList();
            handValue = new HandValue(HandRank.ThreeOfAKind, threeOfAKindGroup.Concat(kickers).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsTwoPair(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetValue()).Where(g => g.Count() == 2).OrderByDescending(g => g.Key).ToList();

        if (groups.Count >= 2)
        {
            var remainingCard = cards.Except(groups.SelectMany(g => g)).OrderByDescending(card => card.GetValue()).First();
            handValue = new HandValue(HandRank.TwoPair, groups.Take(2).SelectMany(g => g).Concat(new[] { remainingCard }).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static bool IsOnePair(List<Card> cards, out HandValue handValue)
    {
        var groups = cards.GroupBy(card => card.GetValue());
        var pairGroup = groups.FirstOrDefault(g => g.Count() == 2);

        if (pairGroup != null)
        {
            var kickers = groups.Where(g => g.Count() < 2).SelectMany(g => g).OrderByDescending(card => card.GetValue()).Take(3).ToList();
            handValue = new HandValue(HandRank.OnePair, pairGroup.Concat(kickers).ToList());
            return true;
        }

        handValue = null;
        return false;
    }

    private static HandValue GetHighCard(List<Card> cards)
    {
        return new HandValue(HandRank.HighCard, cards.Take(5).ToList());
    }
}

public class HandValue : IComparable<HandValue>
{
    public HandRank Rank { get; }
    public List<Card> Cards { get; }

    public HandValue(HandRank rank, List<Card> cards)
    {
        Rank = rank;
        Cards = cards;
    }

    public int CompareTo(HandValue other)
    {
        // Compare by rank first
        int rankComparison = Rank.CompareTo(other.Rank);
        if (rankComparison != 0)
        {
            return rankComparison;
        }

        // If ranks are the same, compare the individual card values (kickers)
        for (int i = 0; i < Cards.Count; i++)
        {
            int comparison = Cards[i].CompareRank(other.Cards[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0; // Hands are exactly equal
    }
}

public enum HandRank
{
    HighCard,
    OnePair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}

public class CardValueComparer : IEqualityComparer<Card>
{
    public bool Equals(Card x, Card y)
    {
        return x.GetValue() == y.GetValue();
    }

    public int GetHashCode(Card obj)
    {
        return obj.GetValue().GetHashCode();
    }
}