using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace Assets.Scripts.nwdata
{
    [Serializable]
    class Player
    {
        public string cmd = "u";
        public string xs;
        public string ys;
        public float vx;
        public float vy;
        public int seq;
    }
}
