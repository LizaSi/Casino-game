using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarAnimating : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
            Debug.Log("Animator found and DontDestroyOnLoad applied.");
        }
        else
        {
            Debug.LogWarning("Animator component is missing in Awake.");
        }
    }

    public void WinAnimation()
    {
        // Start the coroutine to delay the animation trigger
        StartCoroutine(TriggerWinAnimationWithDelay());
    }

    private IEnumerator TriggerWinAnimationWithDelay()
    {
        yield return new WaitForEndOfFrame(); // Wait for the end of the current frame

        if (animator != null)
        {
            Debug.Log("Triggering Win animation.");
            animator.SetTrigger("Win");
        }
        else
        {
            Debug.LogWarning("Animator is missing or has been destroyed in TriggerWinAnimationWithDelay method.");
        }
    }
}
