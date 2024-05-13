using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseAnAvatar : MonoBehaviour
{
    public Image avatarToShow, avatarToHide1, avatarToHide2, avatarToHide3, avatarToHide4, avatarToHide5, avatarToHide6, avatarToHide7, avatarToHide8, avatarToHide9;

    private void Start()
    {
        avatarToHide1.gameObject.SetActive(false);
        avatarToHide2.gameObject.SetActive(false);
        avatarToHide3.gameObject.SetActive(false);
        avatarToHide4.gameObject.SetActive(false);
        avatarToHide5.gameObject.SetActive(false);
        avatarToHide6.gameObject.SetActive(false);
        avatarToHide7.gameObject.SetActive(false);
        avatarToHide8.gameObject.SetActive(false);
        avatarToHide9.gameObject.SetActive(false);
        avatarToShow.gameObject.SetActive(false);
    }

    public void onChoosingAvatar()
    {
        avatarToHide1.gameObject.SetActive(false);
        avatarToHide2.gameObject.SetActive(false);
        avatarToHide3.gameObject.SetActive(false);
        avatarToHide4.gameObject.SetActive(false);
        avatarToHide5.gameObject.SetActive(false);
        avatarToHide6.gameObject.SetActive(false);
        avatarToHide7.gameObject.SetActive(false);
        avatarToHide8.gameObject.SetActive(false);
        avatarToHide9.gameObject.SetActive(false);
        avatarToShow.gameObject.SetActive(true);
    }
}
