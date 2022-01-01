using Assets.Scripts.nwcallback;
using Assets.Scripts.nwdata;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class AU_PauseScene : MonoBehaviour, VoteUpdateListener
{

    public List<PausePlayer> pausePlayers;
    public Button skipVoteButton;
    public Text statusText;
    private bool hasVotingEnded;
    public float secondsLeft;
    private VoteState voteState;

    private const int VOTE_TIMEOUT = 20;



    private void OnEnable()
    {
        pausePlayers = new List<PausePlayer>(transform.GetComponentsInChildren<PausePlayer>(true));
        var localPlayer = GameController.Instance.LocalPlayer;
        bool showImpostors = (localPlayer != null && localPlayer.m_isImpostor) || (GameController.state == "Finished");
        bool canVote = (GameController.state == "Voting") && (!GameController.Instance.LocalPlayer.m_isDead);

        List<AU_PlayerController> allPlayers = new List<AU_PlayerController>();
        allPlayers.Add(GameController.Instance.LocalPlayer);
        allPlayers.AddRange(GameController.Instance.OtherPlayers.Values);
        for (int i = 0; i < allPlayers.Count; i++)
        {
            pausePlayers[i].playerName = allPlayers[i].m_name;
            pausePlayers[i].isImpostor = showImpostors && allPlayers[i].m_isImpostor;
            pausePlayers[i].isDead = allPlayers[i].m_isDead;
            pausePlayers[i].hasCalledMeeting = canVote && allPlayers[i].m_hasCalledVote;
            pausePlayers[i].color = allPlayers[i].m_color;
            pausePlayers[i].isEnabled = canVote && (!allPlayers[i].m_isDead);
            pausePlayers[i].gameObject.SetActive(true);
            pausePlayers[i].otherPlayerId = allPlayers[i].id;
            pausePlayers[i].voters = new List<Color>();
        }
        for (int i = allPlayers.Count; i < pausePlayers.Count; i++)
        {
            pausePlayers[i].gameObject.SetActive(false);
        }
        hasVotingEnded = false;
        skipVoteButton.enabled = canVote;

        if (GameController.state == "Finished")
        {
            skipVoteButton.gameObject.SetActive(false);
            statusText.text = "Finshed";
        }
        else
        {
            statusText.text = "Vote";
        }

        voteState = null;
        INetworkManager.sActiveInstace.UpdateVoteState(this);
    }

    public void castVote(int playerId)
    {
        INetworkManager.sActiveInstace.CastVote(playerId);
        for (int i = 0; i < pausePlayers.Count; i++)
        {
            pausePlayers[i].isEnabled = false;
        }
        skipVoteButton.enabled = false;
    }


    public void votingEnded()
    {
        if (hasVotingEnded) {
            return;
        }
        hasVotingEnded = true;
        secondsLeft = 10.0f;
        for (int i = 0; i < pausePlayers.Count; i++)
        {
            pausePlayers[i].isEnabled = false;
        }
        skipVoteButton.enabled = false;
        INetworkManager.sActiveInstace.UpdateVoteState(this);
    }

    public void FixedUpdate()
    {
        if (GameController.state == "Finished")
        {
            return;
        }

        if (hasVotingEnded)
        {
            secondsLeft -= Time.fixedDeltaTime;
            if (secondsLeft < 0.0f)
            {
                gameObject.SetActive(false);
            }
            statusText.text = "Voting ended. Game resuming in " + secondsLeft.ToString("0.") + "s";
        } else
        {
            if (voteState != null)
            {
                double now = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds * 1000;
                statusText.text = "Time left " + (VOTE_TIMEOUT - (now - voteState.started) / 1000).ToString("0.00") + "s";
            }
        }
    }

    public void voteUpdated(VoteState voteState)
    {
        this.voteState = voteState;
        if (this.voteState != null && hasVotingEnded)
        {
            var idToCol = new Dictionary<int, Color>();
            idToCol.Add(INetworkManager.s_id, GameController.Instance.LocalPlayer.m_color);
            foreach (var e in GameController.Instance.OtherPlayers)
            {
                idToCol[e.Key] = e.Value.m_color;
            }

            var idToPausePlayer = new Dictionary<int, PausePlayer>();
            foreach (var pp in pausePlayers)
            {
                pp.voters = new List<Color>();
                idToPausePlayer[pp.otherPlayerId] = pp;
            }

            foreach (var vote in voteState.votes)
            {
                if (idToPausePlayer.ContainsKey(vote.whom) && idToCol.ContainsKey(vote.who))
                {
                    idToPausePlayer[vote.whom].voters.Add(idToCol[vote.who]);
                }
            }
        }
    }
}
