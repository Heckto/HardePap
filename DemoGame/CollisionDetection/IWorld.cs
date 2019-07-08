using System;
using System.Collections.Generic;
using AuxLib;
using Game1.CollisionDetection.Responses;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.CollisionDetection
{
    /// <summary>
    /// Represents a physical world that contains AABB box colliding bodies.
    /// </summary>
    public interface IWorld
    {
        #region Boxes

        /// <summary>
        /// Create a new box in the world.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        IBox CreateRectangle(float x, float y, float width, float height);

        IPolyLine CreatePolyLine(Vector2f[] points);

        /// <summary>
        /// Remove the specified box from the world.
        /// </summary>
        /// <param name="box">Box.</param>
        bool Remove(IBox box);

        bool Remove(IPolyLine box);

        /// <summary>
        /// Update the specified box in the world (needed to be called tu update spacial hash).
        /// </summary>
        /// <param name="box">Box.</param>
        /// <param name="from">From.</param>
        void Update(IShape box, RectangleF from);

        #endregion

        #region Queries

        /// <summary>
        /// Find the boxes contained in the given area of the world.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="w">The width.</param>
        /// <param name="h">The height.</param>
        IEnumerable<IShape> Find(float x, float y, float w, float h);

        /// <summary>
        /// Find the boxes contained in the given area of the world.
        /// </summary>
        /// <param name="area">Area.</param>
        IEnumerable<IShape> Find(RectangleF area);

        #endregion

        #region Hits

        /// <summary>
        /// Queries the world to find the nearest colliding point from a given position.
        /// </summary>
        /// <param name="point">Point.</param>
        /// <param name="ignoring">A collection of boxes that will be ignored during hit test (optionnal).</param>
        IHit Hit(Vector2f point, IEnumerable<IShape> ignoring = null);

        /// <summary>
        /// Queries the world to find the nearest colliding position from an oriented segment.
        /// </summary>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="ignoring">A collection of boxes that will be ignored during hit test (optionnal).</param>
        IHit Hit(Vector2f origin, Vector2f destination, IEnumerable<IShape> ignoring = null);

        /// <summary>
        /// Queries the world to find the nearest colliding position from a moving rectangle.
        /// </summary>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        /// <param name="ignoring">Ignoring.</param>
        IHit Hit(RectangleF origin, RectangleF destination, IEnumerable<IShape> ignoring = null);

        #endregion

        #region Movements

        /// <summary>
        /// Simulate the specified box movement without moving it.
        /// </summary>
        /// <param name="box">Box.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="filter">Filter.</param>
        IMovement Simulate(Box box, float x, float y, Func<ICollision, ICollisionResponse> filter);

        #endregion

        #region Diagnostics

        /// <summary>
        /// Draws the debug layer.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="w">The width.</param>
        /// <param name="h">The height.</param>
        /// <param name="drawCell">Draw cell.</param>
        /// <param name="drawBox">Draw box.</param>
        /// <param name="drawString">Draw string.</param>
       // void DrawDebug(SpriteBatch spriteBatch,int x, int y, int w, int h);

        #endregion
    }
}

