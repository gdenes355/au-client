using Assets.Scripts.nwcallback;
using Assets.Scripts.nwdata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AU_NetworkManagerXhttp : INetworkManager
{
    // TODO: change this to the deployed server
#if UNITY_EDITOR
    private const string m_basePath = "http://localhost:12345/g/au/";
#else
    private const string m_basePath = "/g/au/";
#endif

    public override void JoinGame(string name, int gameid)
    {
        StartCoroutine(joinGame(name, gameid));
    }

    public override void KillOtherPlayer(int otherPlayerId)
    {
        StartCoroutine(killOtherPlayer(otherPlayerId));
    }

    public override void ReportBody(int bodyPlayerId)
    {
        StartCoroutine(reportBody(bodyPlayerId));
    }

    public override void SolvedPuzzle()
    {
        StartCoroutine(solvedPuzzle());
    }

    public override void CastVote(int otherPlayerId)
    {
        StartCoroutine(castVote(otherPlayerId));
    }

    public override void UpdateVoteState(VoteUpdateListener listener)
    {
        StartCoroutine(updateVoteState(listener));
    }

   

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FetchServerCodes());
        if (s_id < 0)
        {
            s_id = UnityEngine.Random.Range(1, 100000);
        }
    }


    public static string QuizQuestionString = null;
    
    public IEnumerator FetchServerCodes()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "l/"))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                //Servers.Clear();
                //Connected = false;
            }
            else
            {
                string results = www.downloadHandler.text;
                ServerList serverList = JsonUtility.FromJson<ServerList>(results);
                //Servers = serverList.data;
            }
        }
    }

    private IEnumerator joinGame(string name, int gameid)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "qs/?" + "gameid=" + gameid))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                yield break;
            }
            else
            {
                QuizQuestionString = www.downloadHandler.text;
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "j/?" +
            "name=" + System.Web.HttpUtility.UrlEncode(name) +
            "&id=" + s_id + "&gameid=" + gameid))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string results = www.downloadHandler.text;
                GameState gameState = JsonUtility.FromJson<GameState>(results);
                GameController.id = gameid;
                GameController.gameSceneName = gameState.data.map;
                SceneManager.LoadScene("Lobby");
            }
        }
    }

    private IEnumerator killOtherPlayer(int otherPlayerId)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "k/?" +
            "whom=" + otherPlayerId +
            "&who=" + s_id + "&gameid=" + GameController.id))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Killed " + otherPlayerId);
            }
        }
    }

    private IEnumerator reportBody(int bodyPlayerId)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "r/?" +
            "whom=" + bodyPlayerId +
            "&who=" + s_id + "&gameid=" + GameController.id))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Reported body for " + bodyPlayerId);
            }
        }
    }

    private IEnumerator solvedPuzzle()
    {
        int amount = GameController.Instance.LocalPlayer.m_isImpostor ? 1 : -1;
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "p/?" +
            "amount=" + amount + "&gameid=" + GameController.id))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Puzzle point accounted");
            }
        }
    }

    private IEnumerator castVote(int otherPlayerId)
    {
        using (UnityWebRequest www = new UnityWebRequest(m_basePath + "v/?" +
            "whom=" + otherPlayerId + "&who=" + s_id + "&gameid=" + GameController.id, "POST"))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Vote casted");
            }
        }
    }

    private IEnumerator updateVoteState(VoteUpdateListener listener)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(m_basePath + "v/?gameid=" + GameController.id))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                listener.voteUpdated(null);
            }
            else
            {
                string results = www.downloadHandler.text;
                VoteState voteState = JsonUtility.FromJson<VoteState>(results);
                listener.voteUpdated(voteState);
            }
        }
    }

    private int phase = 0;
    private int cycle = 80;

    private void FixedUpdate()
    {
        if (GameController.id < 1)
        {
            return;
        }

        phase++;
        if (phase < cycle)
        {
            return;
        }
        phase = 0;
        StartCoroutine(updateNetwork());
    }

    private IEnumerator updateNetwork()
    {
        using (UnityWebRequest www = new UnityWebRequest(m_basePath + "u/?id=" + s_id +
            "&gameid=" + GameController.id, "POST"))
        {
            AU_PlayerController player = GameController.Instance.LocalPlayer;
            if (player == null)
            {
                yield break;
            }

            Player aPlayer = new Player();
            aPlayer.xs = string.Join(",", player.xs.ConvertAll(t => t.ToString("0.00")));
            aPlayer.ys = string.Join(",", player.ys.ConvertAll(t => t.ToString("0.00")));
            aPlayer.seq = player.seq;
            player.xs.Clear();
            player.ys.Clear();
            var rigidBody = player.GetComponent<Rigidbody>();
            aPlayer.vx = rigidBody.velocity.x;
            aPlayer.vy = rigidBody.velocity.y;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(aPlayer));
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                string results = www.downloadHandler.text;
                GameState gameState = JsonUtility.FromJson<GameState>(results);
                foreach (GameState.Player aaPlayer in gameState.data.players)
                {
                    AU_PlayerController playerToUpdate = null;
                    if (aaPlayer.id == s_id)
                    {
                        playerToUpdate = GameController.Instance.LocalPlayer;
                        playerToUpdate.id = s_id;
                        
                    } else
                    {
                        if (!GameController.Instance.OtherPlayers.ContainsKey(aaPlayer.id))
                        {
                            playerToUpdate = Instantiate(m_playerPrefab).GetComponent<AU_PlayerController>();
                            playerToUpdate.id = aaPlayer.id;
                            GameController.Instance.OtherPlayers.Add(aaPlayer.id, playerToUpdate);
                        }
                        else
                        {
                            playerToUpdate = GameController.Instance.OtherPlayers[aaPlayer.id];
                        }

                        if (playerToUpdate.seq < aaPlayer.seq)
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

                GameController.Instance.transitionToState(gameState.data.state);
            }
        }
    }

    public override bool isConnected()
    {
        throw new NotImplementedException();
    }
}