using Assets.Scripts.nwcallback;
using Assets.Scripts.nwdata;
using HybridWebSocket;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AU_NetworkManagerWS : INetworkManager
{
    // TODO: change this to the deployed server
#if UNITY_EDITOR
    private const string m_ws_path = "wss://ws.gdenes.com";
    //private const string m_ws_path = "ws://localhost:8765";
#else
    private const string m_ws_path = "wss://ws.gdenes.com";
#endif

    private static List<string> Servers = new List<string>();

    private static WebSocket ws = null;
    private static bool questionsReceived = false;
    private static bool fullyJoined = false;

    private static ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

    private VoteState lastVoteState;
    private VoteUpdateListener voteUpdateListener;


    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void JoinGame(string name, int gameid)
    {
        if (ws != null)
        {
            return;
        }
        
        ws = WebSocketFactory.CreateInstance(m_ws_path); 
        ws.OnOpen += () =>
        {
            GameController.id = gameid;
            ws.Send(Encoding.UTF8.GetBytes("{\"intent\": \"join-game\", \"id\": \"" + s_id + "\", \"code\": \"" + gameid + "\", \"name\": \"" + name + " \"}"));
            ws.Send(Encoding.UTF8.GetBytes("{\"cmd\": \"get-qs\"}"));
        };
        ws.OnMessage += (byte[] msg) =>
        {
            string response = Encoding.UTF8.GetString(msg);
            //Debug.Log("on message" + response);
            messages.Enqueue(response);
        };

        // Add OnError event listener
        ws.OnError += (string errMsg) =>
        {
            Debug.Log("WS error: " + errMsg);
        };


        ws.OnClose += (WebSocketCloseCode code) =>
        {
            Debug.Log("WS closed with code: " + code.ToString());
            ws = null;
            fullyJoined = false;
            questionsReceived = false;
        };
        ws.Connect();
    }

    private void Update()
    {
        string response;
        while (messages.TryDequeue(out response))
        {
            onMessage(response);
        }
    }

    private void onMessage(string response)
    {
        //Debug.Log("on message in main thread " + response);
        if (questionsReceived && response.StartsWith("{\"typ\": \"u\""))
        {

            GameState gameState = JsonUtility.FromJson<GameState>(response);
            if (!fullyJoined)
            {
                fullyJoined = true;
                GameController.gameSceneName = gameState.data.map;
                SceneManager.LoadScene("Lobby");
                return;
            }
            if (sActiveInstace != null)
            {
                sActiveInstace.HandleStateUpdate(gameState);
            }
        }
        else if (response.StartsWith("{\"typ\": \"qs\""))
        {
            QuizQuestionString = response;
            questionsReceived = true;
        }
        else if (response.StartsWith("{\"typ\": \"votes\""))
        {
            VoteState voteState = JsonUtility.FromJson<VoteState>(response);
            if (voteUpdateListener != null)
            {
                lastVoteState = null;
                voteUpdateListener.voteUpdated(voteState);
                voteUpdateListener = null;
            }
            else
            {
                lastVoteState = voteState;
            }
        }
        else
        {
            Debug.Log("WS received: " + response);
        }
    }

    private int phase = 0;
    private int cycle = 5;

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
        updateNetwork();
    }

    protected void updateNetwork()
    {
        AU_PlayerController player = GameController.Instance.LocalPlayer;
        if (player == null)
        {
            return;
        }

        Player aPlayer = new Player
        {
            xs = string.Join(",", player.xs.ConvertAll(t => t.ToString("0.00"))),
            ys = string.Join(",", player.ys.ConvertAll(t => t.ToString("0.00"))),
            seq = player.seq / cycle
        };
        player.xs.Clear();
        player.ys.Clear();
        var rigidBody = player.GetComponent<Rigidbody>();
        aPlayer.vx = rigidBody.velocity.x;
        aPlayer.vy = rigidBody.velocity.y;
        ws.Send(Encoding.UTF8.GetBytes(JsonUtility.ToJson(aPlayer)));        
    }

    public override bool isConnected()
    {
        return fullyJoined;
    }

    public override void KillOtherPlayer(int otherPlayerId)
    {
        if (fullyJoined)
        {
            ws.Send(Encoding.UTF8.GetBytes("{\"cmd\": \"k\", \"whom\": " + otherPlayerId + "}"));
        }
    }

    public override void ReportBody(int bodyPlayerId)
    {
        if (fullyJoined)
        {
            ws.Send(Encoding.UTF8.GetBytes("{\"cmd\": \"r\", \"whom\": " + bodyPlayerId + "}"));
        }
    }

    public override void SolvedPuzzle()
    {
        int amount = GameController.Instance.LocalPlayer.m_isImpostor ? 1 : -1;
        if (fullyJoined)
        {
            ws.Send(Encoding.UTF8.GetBytes("{\"cmd\": \"p\", \"amount\": " + amount + "}"));
        }
    }

    public override void CastVote(int otherPlayerId)
    {
        if (fullyJoined)
        {
            ws.Send(Encoding.UTF8.GetBytes("{\"cmd\": \"v\", \"whom\": " + otherPlayerId + "}"));
        }
    }

    public override void UpdateVoteState(VoteUpdateListener listener)
    {
        if (lastVoteState != null)
        {
            // automatic broadcast expected from server
            // respond with last message
            listener.voteUpdated(lastVoteState);
            lastVoteState = null;
            Debug.Log("vote state passed on");
        } else
        {
            voteUpdateListener = listener;
        }
    }
}