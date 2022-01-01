using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AU_Game_Bootstrapper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.state == "Game")
        {
            GameController.state = "Lobby";
            GameController.Instance.transitionToState("Game");
        }
    }
}
