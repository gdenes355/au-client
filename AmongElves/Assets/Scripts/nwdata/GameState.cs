using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.nwdata
{
    [Serializable]
    public class GameState
    {
        public Data data;
        

        [Serializable]
        public class Data
        {
            public List<Player> players;
            public string state;
            public string map;
        }

        [Serializable]
        public class Player
        {
            public string name;
            public int id;
            public float vx;
            public float vy;
            public string col;
            public int mask;

            public string xs;
            public string ys;
            public int seq;

            public bool isDead() { return (mask & 1) == 1; }
            public bool isImpostor() { return (mask & 2) == 2; }
            public bool isVotedOut() { return (mask & 4) == 4; }
            public bool hasCalledVote() { return (mask & 8) == 8; }

            public Color getCol() {
                Color acol = Color.magenta;
                ColorUtility.TryParseHtmlString("#" + col, out acol);
                return acol;
            }
        }
    }
}
