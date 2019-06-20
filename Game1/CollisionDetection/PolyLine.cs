﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.CollisionDetection.Base;
using Game1.CollisionDetection.Responses;

namespace Game1.CollisionDetection
{
    public class PolyLine : Shape, IPolyLine
    {
        #region Constructors 

        public PolyLine(World world, Vector2[] points)
        {
            this.world = world;
            this.points = points;
        }

        #endregion

        #region Fields        

        private Vector2[] points;
        #endregion

        public Vector2[] Points => points;

        #region Resolves

        public override IHit Resolve(Vector2 origin, Vector2 destination)
        {
            var result = Resolve(origin, destination, this.Points);
            if (result != null) result.Box = this;
            return result;
        }

        public override IHit Resolve(RectangleF origin, RectangleF destination)
        {
            var result = Resolve(origin, destination, this.Points);
            if (result != null) result.Box = this;
            return result;
        }

        public RectangleF getPolylineBB()
        {
            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;

            foreach (var p in Points)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
                if (p.Y > maxY)
                    maxY = p.Y;
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public Hit Resolve(RectangleF origin, RectangleF destination, Vector2[] points)
        {
            var broadphaseArea = RectangleF.Union(origin, destination);
            if (broadphaseArea.Intersects(getPolylineBB()))
            {
                return ResolveNarrow(origin, destination, points);
            }
            return null;
        }

        public Hit Resolve(Vector2 origin, Vector2 destination, Vector2[] points)
        {
            var min = Vector2.Min(origin, destination);
            var size = Vector2.Max(origin, destination) - min;
            var broadphaseArea = new RectangleF(min, size);
            if (broadphaseArea.Intersects(getPolylineBB()))
            {
                return ResolveNarrow(origin, destination, points);
            }

            return null;
        }

        public override IHit Resolve(Vector2 point)
        {
            return null;
        }
        public bool IsOnPolyLine(Vector2 point, Vector2[] Points, out float height, out Vector2 segmentVector)
        {
            if (!(point.X < Points[0].X || point.X > Points[Points.Length - 1].X))
            {
                for (var i = 0; i < Points.Length - 1; i++)
                {
                    if (point.X > Points[i].X && point.X < Points[i + 1].X)
                    {
                        var slope = (Points[i + 1].Y - Points[i].Y) / (Points[i + 1].X - Points[i].X);
                        height = slope * (point.X - Points[i].X) + Points[i].Y;
                        segmentVector = Points[i + 1] - Points[i];
                        return true;
                    }
                }
            }
            height = point.Y;
            segmentVector = Vector2.Zero;
            return false;
        }

        private Hit ResolveNarrow(RectangleF origin, RectangleF destination, Vector2[] points)
        {
            var velocity = (destination.Location - origin.Location);
            if (velocity == Vector2.Zero)
                return null;

            var offset = new Vector2(0.5f * origin.Width, origin.Height);
            var bottom_origin_Offset = origin.Location + offset;
            var bottom_destination_Offset = destination.Location + offset;
            var SlopeVector = Vector2.Zero;
            var origin_on_line = IsOnPolyLine(bottom_origin_Offset, points, out var h, out SlopeVector);
            var dest_on_line = IsOnPolyLine(bottom_destination_Offset, points, out var new_h, out SlopeVector);

            Hit result = null;

            if (dest_on_line && velocity.Y > 0 && (bottom_origin_Offset.Y <= h || world.isGrounded) && /*destination.Y <= new_h &&*/ bottom_destination_Offset.Y >= new_h)
            {
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2(bottom_origin_Offset.X + velocity.X, new_h) - offset,
                    Normal = new Vector2(SlopeVector.Y, -SlopeVector.X),

                };
            }
            //System.Diagnostics.Debug.WriteLine($"grounded {world.isGrounded} dest on line {dest_on_line} vel : {velocity} boo {bottom_origin_Offset.Y} booh {h} bdo {bottom_destination_Offset.Y} bdoh {new_h}");
            return result;

        }

        private Hit ResolveNarrow(Vector2 origin, Vector2 destination, Vector2[] points)
        {
            var velocity = (destination - origin);
            if (velocity == Vector2.Zero)
                return null;

            var SlopeVector = Vector2.Zero;
            var origin_on_line = IsOnPolyLine(origin, points, out var h, out SlopeVector);
            var dest_on_line = IsOnPolyLine(destination, points, out var new_h, out SlopeVector);

            Hit result = null;

            if (dest_on_line && velocity.Y > 0 && origin.Y >= h && destination.Y <= new_h)
            {
                velocity.Y = 0f;
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2(origin.X + velocity.X, new_h),
                    Normal = new Vector2(SlopeVector.Y, -SlopeVector.X),

                };
            }
            return result;
        }

        #endregion
    }
}
