using AuxLib;
using Microsoft.Xna.Framework;

namespace Game1.Levels
{
    public partial class CircleItem : Item
    {
        public float Radius;
        public Color FillColor;
        public ItemTypes ItemType { get; set; }

        public CircleItem() { }

        //public override void Update(GameTime gameTime)
        //{
        //}

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)(Position.X - 0.5f * Radius), (int)(Position.X - 0.5f * Radius), (int)(2 * Radius), (int)(2 * Radius));
        }
    }
}
