using Microsoft.Xna.Framework;
using Game1.GameObjects;

namespace Game1.GameObjects.Levels
{
    public class MovingLayer : Layer, IUpdateableItem
    {
        public Vector2 ScrollVector { get; set; }

        public MovingLayer() : base()
        {
        }

        public void Update(GameTime gameTime, Level lvl)
        {

            var mapSize = new Vector2(lvl.CamBounds.Width, lvl.CamBounds.Height);
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
                if (!lvl.CamBounds.Intersects(itemBox))
                {
                    var posVector = Vector2.Min(scrollAss * mapSize, mapSize);

                    Item.Position -= (posVector);

                    var pos = ScrollSpeed * Item.Position + (cam + (Vector2.One - ScrollSpeed));

                    Item.Position = pos;
                }                
            }
        }

        #region Editor
        public MovingLayer(string name) : base(name)
        {
        }

        #endregion
    }
}
