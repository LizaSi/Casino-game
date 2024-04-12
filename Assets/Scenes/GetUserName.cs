using System;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    public class GetUserName : MonoBehaviour
    {
        [SerializeField]
        TMP_Text userNameText;
        // Start is called before the first frame update
        void Start()
        {
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {                
                userNameText.text = AuthenticationService.Instance.PlayerName;
            }
            else
            {
                userNameText.text = "Guest";
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
