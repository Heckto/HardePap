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

        /// <summary>
        /// Tries to move the box to specified coordinates with collisition detection.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Move(float x, float y, Func<ICollision, ICollisionResponse> filter);

        /// <summary>
        /// Tries to move the box to specified coordinates with collisition detection.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Move(float x, float y, Func<ICollision, CollisionResponses> filter);

        /// <summary>
        /// Simulate the move of the box to specified coordinates with collisition detection. The boxe's position isn't
        /// altered.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Simulate(float x, float y, Func<ICollision, ICollisionResponse> filter);

        /// <summary>
        /// Simulate the move of the box to specified coordinates with collisition detection. The boxe's position isn't
        /// altered.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Simulate(float x, float y, Func<ICollision, CollisionResponses> filter);

        #endregion
    }
}
