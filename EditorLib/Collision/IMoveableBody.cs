using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.CollisionDetection.Responses;

namespace AuxLib.CollisionDetection
{
    public interface IMoveableBody : IBox
    {
        #region Movements
        /// <summary>
        /// Tries to move the box to specified coordinates with collisition detection.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Move(float x, float y, float delta, List<IShape> ignoreList);

       // IMovement Move(float x, float y, IBox ignoreBodies,float delta);


        #endregion
    }
}
