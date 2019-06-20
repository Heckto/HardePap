using Game1.CollisionDetection.Base;
using Game1.CollisionDetection.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.CollisionDetection
{
    public interface IPolyLine : IShape
    {
        Vector2[] Points  { get; }

        #region Movements

        RectangleF getPolylineBB();        

        #endregion
    }
}
