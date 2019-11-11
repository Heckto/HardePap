using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib;
using AuxLib.CollisionDetection.Responses;

namespace AuxLib.CollisionDetection
{
    public interface IPolyLine : IShape
    {
        Vector2f[] Points  { get; }

        #region Movements

        RectangleF getPolylineBB();        

        #endregion
    }
}
