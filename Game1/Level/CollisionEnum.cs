using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1
{
    public enum CollisionTag
    {
        StaticBlock = 1,
        PolyLine = 2,
        Player = 4,
        Trigger = 8
    }
}
