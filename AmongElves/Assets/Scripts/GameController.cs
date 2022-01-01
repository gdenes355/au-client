using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject pauseScene;
    public GameObject impostorFlag;

    public IDictionary<int, AU_PlayerController> OtherPlayers = new Dictionary<int, AU_PlayerController>();
    public AU_PlayerController LocalPlayer = null;

    public static GameController Instance = null;

    public static int id;

    public static string state = "Welcome";

    public static Color myColor;

    public static string gameSceneName = "Game";

    private void Awake()
    {
        AU_PlayerController[] currentPlayers = FindObjectsOfType<AU_PlayerController>();
        Instance = this;
    }

    private void Update()
    {
        if (LocalPlayer != null && impostorFlag != null)
        {
            impostorFlag.SetActive(LocalPlayer.m_isImpostor && LocalPlayer.killCooldown == 0.0f);
        }
    }

    public void transitionToState(string newState)
    {
        if (state == newState)
        {
            return;
        }

        string oldState = state;
        state = newState;

        if (oldState == "Lobby" && state == "Game")
        {
            if (LocalPlayer != null) { 
                myColor = LocalPlayer.m_color;
            }
            SceneManager.LoadScene(gameSceneName);
        }
        else if (state == "Voting")
        {
            pauseScene.SetActive(false);
            pauseScene.SetActive(true);
        }
        else if (oldState == "Voting" && state == "Game")
        {
            // even if we think we are finished
            AU_Body[] bodies = FindObjectsOfType<AU_Body>();
            foreach (AU_Body body in bodies)
            {
                Destroy(body.gameObject);
            }
            pauseScene.GetComponent<AU_PauseScene>().votingEnded();
        }
        else if (state == "Finished")
        {
            pauseScene.SetActive(false);
            pauseScene.SetActive(true);
        }
    }
}

