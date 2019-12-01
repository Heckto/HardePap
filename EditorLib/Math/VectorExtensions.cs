using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib
{
    public static class Vector2Ext
    {
        public static Vector2 Up(this Vector2 v1)
        {
            return new Vector2(0, 1);
        }
    }

}
