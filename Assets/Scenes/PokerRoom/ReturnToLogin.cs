using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLogin : MonoBehaviour
{
    public void Button_OnClick()
    {
        SceneManager.LoadScene("RoomSelection");
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
