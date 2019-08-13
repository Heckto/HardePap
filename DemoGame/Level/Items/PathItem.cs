using AuxLib;
using Microsoft.Xna.Framework;
using System.Linq;

namespace Game1.Levels
{
    public partial class PathItem : Item
    {
        public Vector2[] LocalPoints;
        public Vector2[] WorldPoints;
        public bool IsPolygon;
        public int LineWidth;
        public Color LineColor;
        public ItemTypes ItemType { get; set; }

        public PathItem() { }

        public override Rectangle getBoundingBox()
        {
            var minX = WorldPoints.Min(min => min.X);
            var maxX = WorldPoints.Max(max => max.X);
            var minY = WorldPoints.Min(min => min.Y);
            var maxY = WorldPoints.Max(max => max.Y);

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }
    }
}
