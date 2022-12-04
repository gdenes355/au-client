using Assets.Scripts.nwcallback;
using Assets.Scripts.nwdata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public abstract class INetworkManager : MonoBehaviour
{
    public static int s_id = -1;
    public static string QuizQuestionString = null;
    public static INetworkManager sActiveInstace = null;


    public abstract bool isConnected();

    public abstract void JoinGame(string name, int gameid);

    public abstract void KillOtherPlayer(int otherPlayerId);

    public abstract void ReportBody(int bodyPlayerId);

    public abstract void SolvedPuzzle();

    public abstract void CastVote(int otherPlayerId);

    public abstract void UpdateVoteState(VoteUpdateListener listener);


    [SerializeField] public GameObject m_playerPrefab;

    // Start is called before the first frame update
    protected void Start()
    {
        if (s_id < 0)
        {
            s_id = UnityEngine.Random.Range(1, 100000);
        }
        sActiveInstace = this;
    }

    protected void OnDestroy()
    {
        if (sActiveInstace == this)
        {
            sActiveInstace = null;
        }
    }

    public void HandleStateUpdate(GameState gameState)
    {
        // keep track of players to remove
        var inactivePlayerIds = new List<int>(GameController.Instance.OtherPlayers.Keys);

        foreach (GameState.Player aaPlayer in gameState.data.players)
        {
            AU_PlayerController playerToUpdate = null;
            if (aaPlayer.id == s_id)
            {
                playerToUpdate = GameController.Instance.LocalPlayer;
                playerToUpdate.id = s_id;

            }
            else
            {
                if (!GameController.Instance.OtherPlayers.ContainsKey(aaPlayer.id))
                {
                    playerToUpdate = Instantiate(m_playerPrefab).GetComponent<AU_PlayerController>();
                    playerToUpdate.id = aaPlayer.id;
                    GameController.Instance.OtherPlayers.Add(aaPlayer.id, playerToUpdate);
                }
                else
                {
                    inactivePlayerIds.Remove(aaPlayer.id);
                    playerToUpdate = GameController.Instance.OtherPlayers[aaPlayer.id];
                }

                if (playerToUpdate.seq < aaPlayer.seq || playerToUpdate.seq - aaPlayer.seq > 2)
                {
                    try
                    {
                        // new path, save it
                        playerToUpdate.xs = new List<string>(aaPlayer.xs.Split(',')).ConvertAll(s => float.Parse(s));
                        playerToUpdate.ys = new List<string>(aaPlayer.ys.Split(',')).ConvertAll(s => float.Parse(s));
                    }
                    catch (FormatException e)
                    {
                        playerToUpdate.xs = new List<float>();
                        playerToUpdate.ys = new List<float>();
                    }
                    playerToUpdate.finalvx = aaPlayer.vx;
                    playerToUpdate.finalvy = aaPlayer.vy;
                    playerToUpdate.seq = aaPlayer.seq;

                }
                playerToUpdate.m_movementInput = new Vector2(aaPlayer.vx, aaPlayer.vy);
            }

            // if (gameState.data.state == "Lobby")
            {
                playerToUpdate.SetColor(aaPlayer.getCol());
                playerToUpdate.setName(aaPlayer.name);
            }

            if ((aaPlayer.isDead() || aaPlayer.isVotedOut()) && !playerToUpdate.m_isDead)
            {
                playerToUpdate.Die();
            }
            playerToUpdate.m_isImpostor = aaPlayer.isImpostor();
            playerToUpdate.m_hasCalledVote = aaPlayer.hasCalledVote();
        }

        if (inactivePlayerIds.Count > 0)
        {
            // remove players who are no longer tracked by the server
            foreach (var id in inactivePlayerIds)
            {
                AU_PlayerController player = GameController.Instance.OtherPlayers[id];
                GameController.Instance.OtherPlayers.Remove(id);
                Destroy(player.gameObject);
            }
        }

        GameController.Instance.transitionToState(gameState.data.state);
    }

}