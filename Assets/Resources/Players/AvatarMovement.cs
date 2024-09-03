using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class AvatarMovement : NetworkBehaviour
{
    private CharacterController characterController;
    private AvatarAnimating avatarAnimating;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        avatarAnimating = GetComponent<AvatarAnimating>();
    }

    // Update is called once per frame
    private void Update()
    {
        /*
        if (!base.IsOwner)
        {
            return;
        }
        */

        if (Input.GetKeyDown(KeyCode.Space))//need to get the maessage if the player won
        {
            avatarAnimating.WinAnimation();
        }
    }
}
