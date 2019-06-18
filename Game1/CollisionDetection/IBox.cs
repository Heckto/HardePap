﻿using System;
using Game1.CollisionDetection.Base;
using Game1.CollisionDetection.Responses;

namespace Game1.CollisionDetection
{
        /// <summary>
        /// Represents a physical body in the world.
        /// </summary>
        public interface IBox : IShape
    {
        #region Properties
        RectangleF Bounds { get; }
        /// <summary>
        /// Gets the top left corner X coordinate of the box.
        /// </summary>
        /// <value>The x.</value>
        float X { get; }

        /// <summary>
        /// Gets the top left corner Y coordinate of the box.
        /// </summary>
        /// <value>The y.</value>
        float Y { get; }

        /// <summary>
        /// Gets the width of the box.
        /// </summary>
        /// <value>The width.</value>
        float Width { get; }

        /// <summary>
        /// Gets the height of the box.
        /// </summary>
        /// <value>The height.</value>
        float Height { get; }

        /// <summary>
        /// Gets the bounds of the box.
        /// </summary>
        /// <value>The bounds.</value>



        #endregion


        #region Movements

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

