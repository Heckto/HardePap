using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Levels
{
    public class MovingLayer : Layer, IUpdateableItem
    {
        public Vector2 ScrollVector { get; set; }

        public MovingLayer() : base()
         {
        }

        public void Update(GameTime gameTime, Level lvl)
        {
            //           screen position x = (world position x *scroll factor) +
            //   (camera width * (1 - scroll factor))

            //screen position y = (world position y *scroll factor) +
            //   (camera height * (1 - scroll factor))



            var mapSize = new Vector2(lvl.Bounds.Width, lvl.Bounds.Height);
            foreach (var Item in Items)
            {
                var cam = new Vector2(1920, 1080);
                var screenPost = Item.Position * ScrollSpeed + (cam * (Vector2.One - ScrollSpeed));

                var screenPost2 = -((cam * (Vector2.One - ScrollSpeed)) - Item.Position) / ScrollSpeed;


                var scrollAss = ScrollVector;
                scrollAss.Normalize();
                Item.Position += ScrollVector;
                var itemBox = Item.getBoundingBox();
                itemBox.X = (int)screenPost2.X;
                itemBox.Y = (int)screenPost2.Y;
                if (!lvl.Bounds.Intersects(itemBox))
                {
                    var posVector = Vector2.Min(scrollAss * mapSize, mapSize);

                    Item.Position -= (posVector);

                    var pos = ScrollSpeed * Item.Position + (cam + (Vector2.One - ScrollSpeed));

                    Item.Position = pos;
                }
            }
        }
    }
}
