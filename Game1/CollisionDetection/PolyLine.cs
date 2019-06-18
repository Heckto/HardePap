using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.CollisionDetection.Base;
using Game1.CollisionDetection.Responses;

namespace Game1.CollisionDetection
{
    public class PolyLine : Shape,IPolyLine
    {
        #region Constructors 

        public PolyLine(World world, Vector2[] points)
        {
            this.world = world;
            this.points = points;            
        }

        #endregion

        #region Fields

        private World world;

        private Vector2[] points;
        #endregion

        public Vector2[] Points => points;
      

        public IMovement Move(float x, float y, Func<ICollision, ICollisionResponse> filter)
        {
            throw new NotImplementedException();
        }

        public IMovement Move(float x, float y, Func<ICollision, CollisionResponses> filter)
        {
            throw new NotImplementedException();
        }        

        public IMovement Simulate(float x, float y, Func<ICollision, ICollisionResponse> filter)
        {
            throw new NotImplementedException();
        }

        public IMovement Simulate(float x, float y, Func<ICollision, CollisionResponses> filter)
        {
            throw new NotImplementedException();
        }


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

        private RectangleF getLineSegmentBB(Vector2 from,Vector2 to)
        {
            var minX = Math.Min(from.X,to.X);
            var minY = Math.Min(from.Y, to.Y);
            var maxX = Math.Max(from.X, to.X);
            var maxY = Math.Max(from.Y, to.Y);
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
            //if (this.Bounds.Contains(point))
            //{
            //    var outside = PushOutside(point, this.Bounds);
            //    return new Hit()
            //    {
            //        Amount = 0,
            //        Box = this,
            //        Position = outside.Item1,
            //        Normal = outside.Item2,
            //    };
            //}

            return null;
        }

        public static bool IsOnPolyLine(Vector2 point, Vector2[] Points, out float height, out Vector2 segmentVector)
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

        private static Hit ResolveNarrow(RectangleF origin, RectangleF destination, Vector2[] points)
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

            if (dest_on_line && velocity.Y > 0 && bottom_origin_Offset.Y >= h && destination.Y <= new_h)
            {
                velocity.Y = 0f;
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2(bottom_origin_Offset.X + velocity.X, new_h) - offset,
                    Normal = new Vector2(SlopeVector.Y, -SlopeVector.X),

                };
            }
            return result;
        }

        private static Hit ResolveNarrow(Vector2 origin, Vector2 destination, Vector2[] points)
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

        private static Vector2 GetNormal(Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        private static Tuple<Vector2, Vector2> PushOutside(Vector2 origin, RectangleF other)
        {
            var position = origin;
            var normal = Vector2.Zero;

            var top = origin.Y - other.Top;
            var bottom = other.Bottom - origin.Y;
            var left = origin.X - other.Left;
            var right = other.Right - origin.X;

            var min = Math.Min(top, Math.Min(bottom, Math.Min(right, left)));

            if (Math.Abs(min - top) < Constants.Threshold)
            {
                normal = -Vector2.UnitY;
                position = new Vector2(position.X, other.Top);
            }
            else if (Math.Abs(min - bottom) < Constants.Threshold)
            {
                normal = Vector2.UnitY;
                position = new Vector2(position.X, other.Bottom);
            }
            else if (Math.Abs(min - left) < Constants.Threshold)
            {
                normal = -Vector2.UnitX;
                position = new Vector2(other.Left, position.Y);
            }
            else if (Math.Abs(min - right) < Constants.Threshold)
            {
                normal = Vector2.UnitX;
                position = new Vector2(other.Right, position.Y);
            }

            return new Tuple<Vector2, Vector2>(position, normal);
        }

        private static Tuple<RectangleF, Vector2> PushOutside(RectangleF origin, RectangleF other)
        {
            var position = origin;
            var normal = Vector2.Zero;

            var top = origin.Center.Y - other.Top;
            var bottom = other.Bottom - origin.Center.Y;
            var left = origin.Center.X - other.Left;
            var right = other.Right - origin.Center.X;

            var min = Math.Min(top, Math.Min(bottom, Math.Min(right, left)));

            if (Math.Abs(min - top) < Constants.Threshold)
            {
                normal = -Vector2.UnitY;
                position.Location = new Vector2(position.Location.X, other.Top - position.Height);
            }
            else if (Math.Abs(min - bottom) < Constants.Threshold)
            {
                normal = Vector2.UnitY;
                position.Location = new Vector2(position.Location.X, other.Bottom);
            }
            else if (Math.Abs(min - left) < Constants.Threshold)
            {
                normal = -Vector2.UnitX;
                position.Location = new Vector2(other.Left - position.Width, position.Location.Y);
            }
            else if (Math.Abs(min - right) < Constants.Threshold)
            {
                normal = Vector2.UnitX;
                position.Location = new Vector2(other.Right, position.Location.Y);
            }

            return new Tuple<RectangleF, Vector2>(position, normal);
        }

        #endregion
    }
}
