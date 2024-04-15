using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SearchNearby : MonoBehaviour
{
    public GameObject searchingText;
    public Text resultText;

    public void StartSearch()
    {
        searchingText.gameObject.SetActive(true);
	SceneManager.LoadScene("JoinARoom");
     //   resultText.gameObject.SetActive(true);
        SearchForUsers();
    }

    private void SearchForUsers()
    {
        Invoke("UserFound", 2f);
    }

    private void UserFound()
    {
   //     resultText.text = "User Found!";
        searchingText.gameObject.SetActive(false);
    }
}