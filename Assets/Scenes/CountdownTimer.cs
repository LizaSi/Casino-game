using System.Collections;
using UnityEngine;
using TMPro;
using FishNet.Connection;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_countDownText;

    private int countdownTime = 30;   
    private static bool m_Started = false;
    private NetworkConnection m_turnOwner;

    private static CountdownTimer instance; 

    private void Awake()
    {
        instance = this; 
    }

    public static void RemoveTimer()
    {
        instance.m_countDownText.text = "";
        instance.countdownTime = 30;
        StopCountDown();
    }

    public static void StartBlackjackCountdown(CardsDisplayer cardsDisplayer, NetworkConnection turnOwner)
    {
        if (!m_Started)
        {
            m_Started = true;
            instance.countdownTime = 30;
            instance.m_turnOwner = turnOwner;
            instance.StartCoroutine(instance.CountdownBlackjack(cardsDisplayer));
            Debug.LogWarning("Starting countdown");
        }
        else
        {
            instance.countdownTime = 30;
            instance.m_turnOwner = turnOwner;
        }
    }

    public static void StartPokerCountdown(PokerDisplayer pokerDisplayer, NetworkConnection turnOwner)
    {
        if (!m_Started)
        {
            m_Started = true;
            instance.countdownTime = 30;
            instance.m_turnOwner = turnOwner;
            instance.StartCoroutine(instance.CountdownPoker(pokerDisplayer));
        }
        else
        {
            instance.countdownTime = 30;
            instance.m_turnOwner = turnOwner;
        }
    }

    public static void StopCountDown()
    {
        m_Started = false;
        instance.StopAllCoroutines();
    }

    private IEnumerator CountdownPoker(PokerDisplayer pokerDisplayer)
    {

        while (countdownTime > 0)
        {
            m_countDownText.text = countdownTime.ToString();
                         
            if(countdownTime > 5)
            {
                m_countDownText.color = Color.white;
            }
            else if (countdownTime == 5)
            {
                m_countDownText.color = Color.red;
            }
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }
        m_Started = false;
        if (PokerServerManager.IsMyTurn(m_turnOwner))
        {
            if (PokerServerManager.HowManyCoinsToCall(m_turnOwner) > 0)
                pokerDisplayer.Fold_OnClick();
            else
                pokerDisplayer.Check_OnClick();
        }
        m_countDownText.text = "0";
    }

    private IEnumerator CountdownBlackjack(CardsDisplayer cardsDisplayer)
    {
        m_countDownText.color = Color.white;

        while (countdownTime > 0)
        {
            m_countDownText.text = countdownTime.ToString(); 
            if(countdownTime == 10)
            {
                m_countDownText.color = Color.red;
            }
            yield return new WaitForSeconds(1f);
            countdownTime--; 
        }
        m_Started = false;
        if (GameServerManager.IsMyTurn(m_turnOwner))
        {
            cardsDisplayer.Check_OnClick();
        }
        m_countDownText.text = "0";
    }
}
