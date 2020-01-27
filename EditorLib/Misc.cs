using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib
{
    public enum ItemTypes
    {
        None = 0,
        Collider = 1,
        Player = 2,        
        Enemy = 4,
        Transition = 8,
        ScriptTrigger = 16
    }
}
