using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_countDownText;  // Reference to the TMP_Text component

    private int countdownTime = 30;    // Countdown start time in seconds
    private static CountdownTimer instance;  // Singleton instance to access from other scripts
    private static bool m_Started = false;

    private void Awake()
    {
        instance = this;  // Set the instance to this script
    }

    public static void StartCountdown(CardsDisplayer cardsDisplayer)
    {
        if (!m_Started)
        {
            m_Started = true;
            instance.countdownTime = 30;
            instance.StartCoroutine(instance.CountdownRoutine(cardsDisplayer));
        }
    }

    private IEnumerator CountdownRoutine(CardsDisplayer cardsDisplayer)
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
        cardsDisplayer.Check_OnClick();
        m_countDownText.text = "0";
    }
}
