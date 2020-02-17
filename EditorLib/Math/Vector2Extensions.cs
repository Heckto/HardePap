using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib
{
    public static class Extensions
    {
        public static Vector2 Up(this Vector2 v1)
        {
            return new Vector2(0, 1);
        }

        public static Vector2 Round(this Vector2 v)
        {            
            return new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
        }

        public static Point ToPoint(this Vector2 v)
        {
            return new Point((int)Math.Round(v.X), (int)Math.Round(v.Y));
        }

        public static Vector2 ToVector2(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static float DistanceTo(this Vector2 v0, Vector2 v)
        {
            return (v - v0).Length();
        }

        public static Vector2 Sign(this Vector2 v0)
        {
            return new Vector2(Math.Sign(v0.X), Math.Sign(v0.Y));
        }

        public static float DistanceToLineSegment(this Vector2 v, Vector2 a, Vector2 b)
        {
            var x = b - a;
            x.Normalize();
            var t = Vector2.Dot(x, v - a);
            if (t < 0) return (a - v).Length();
            var d = (b - a).Length();
            if (t > d) return (b - v).Length();
            return (a + x * t - v).Length();

        }

        public static Rectangle Transform(this Rectangle r, Matrix m)
        {
            var poly = new Vector2[2];
            poly[0] = new Vector2(r.Left, r.Top);
            poly[1] = new Vector2(r.Right, r.Bottom);
            var newpoly = new Vector2[2];
            Vector2.Transform(poly, ref m, newpoly);

            var result = new Rectangle
            {
                Location = newpoly[0].ToPoint(),
                Width = (int)(newpoly[1].X - newpoly[0].X),
                Height = (int)(newpoly[1].Y - newpoly[0].Y)
            };
            return result;
        }

        /// <summary>
        /// Convert the Rectangle to an array of Vector2 containing its 4 corners.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="m"></param>
        /// <returns>An array of Vector2 representing the rectangle's corners starting from top/left and going clockwise.</returns>
        public static Vector2[] ToPolygon(this Rectangle r)
        {
            var poly = new Vector2[4];
            poly[0] = new Vector2(r.Left, r.Top);
            poly[1] = new Vector2(r.Right, r.Top);
            poly[2] = new Vector2(r.Right, r.Bottom);
            poly[3] = new Vector2(r.Left, r.Bottom);
            return poly;
        }

        public static Rectangle RectangleFromVectors(Vector2 v1, Vector2 v2)
        {
            var distance = v2 - v1;
            var result = new Rectangle
            {
                X = (int)Math.Min(v1.X, v2.X),
                Y = (int)Math.Min(v1.Y, v2.Y),
                Width = (int)Math.Abs(distance.X),
                Height = (int)Math.Abs(distance.Y)
            };
            return result;
        }

    }
}
