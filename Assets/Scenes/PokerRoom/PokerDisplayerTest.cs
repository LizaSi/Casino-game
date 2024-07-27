using FishNet;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokerDisplayerTest : NetworkBehaviour
{
    [SerializeField] private PokerServerManager pokerServerManager;
    [SerializeField] private Transform CardTransform;

    private List<GameObject> spawnedCards = new();
    private float cardSpacing = 2.8f;

    private void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            DisplayCardsOnBoard(PokerServerManager.GetMyHand(base.Owner));
        }
    }

    public void Init()
    {
        DisplayCardsOnBoard(PokerServerManager.GetMyHand(base.Owner));
     //   string cards = GameServerManager.GetPlayerHand(base.Owner);
    }

    private void DisplayCardsOnBoard(string cards)
    {

        string[] cardNames = cards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < cardNames.Length; i++)
        {
            string cardName = cardNames[i].Trim();

            string cardDir = "Cards/" + cardName;
            GameObject instantiatedCard = Instantiate(Resources.Load<GameObject>(cardDir));
            Vector3 newPosition = CardTransform.position + new Vector3(i * cardSpacing, 0, 0);

            instantiatedCard.transform.SetPositionAndRotation(newPosition, CardTransform.rotation);
            instantiatedCard.transform.localScale = CardTransform.localScale;

            spawnedCards.Add(instantiatedCard);
        }
    }
}
