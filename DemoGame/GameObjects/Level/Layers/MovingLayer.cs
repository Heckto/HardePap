using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using AuxLib;
using Game1.GameObjects.Sprite;
using System.Collections.Generic;

namespace Game1.GameObjects.Levels
{
    public class MovingLayer : Layer, IUpdateableItem
    {
        public Vector2 ScrollVector { get; set; }

        public MovingLayer() : base() {}

        public override void Update(GameTime gameTime, Level lvl)
        {
            var mapSize = new Vector2(lvl.LevelBounds.Width, lvl.LevelBounds.Height);
            foreach (var Item in Items)
            {
                Item.Transform.Position += ScrollVector;
                var bb = Item.getBoundingBox();
                var movedBB= new Rectangle((int)(Item.Transform.Position.X), (int)(Item.Transform.Position.Y), bb.Width / 2, bb.Height / 2);
                if (!lvl.LevelBounds.Intersects(movedBB))
                {
                    var d = mapSize;
                    var screenPos = (Item.Transform.Position - (ScrollVector.Sign() * d));
                    Item.Transform.Position = screenPos;
                }
            }

            base.Update(gameTime, lvl);
        }

        #region Editor
        public MovingLayer(string name) : base(name) {}


        #endregion
    }
}
