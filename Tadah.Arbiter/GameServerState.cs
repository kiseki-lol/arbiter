using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public enum GameServerState
    {
        Online = 0,
        Offline = 1,
        Crashed = 2,
        Paused = 3
    };
}
