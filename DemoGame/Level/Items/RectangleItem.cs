using AuxLib;
using Microsoft.Xna.Framework;

namespace Game1.Levels
{
    public partial class RectangleItem : Item
    {
        public float Width;
        public float Height;
        public Color FillColor;
        public ItemTypes ItemType { get; set; }

        public RectangleItem() { }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }
    }
}
