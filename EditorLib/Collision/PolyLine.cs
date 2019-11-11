using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib;
using AuxLib.CollisionDetection.Responses;

namespace AuxLib.CollisionDetection
{
    public class PolyLine : Shape, IPolyLine
    {
        #region Constructors 

        public PolyLine(World world, Vector2f[] points)
        {
            this.world = world;
            this.points = points;
        }

        #endregion

        #region Fields        

        private Vector2f[] points;
        #endregion

        public Vector2f[] Points => points;

        #region Resolves

        public override IHit Resolve(Vector2f origin, Vector2f destination)
        {
            var min = Vector2f.Min(origin, destination);
            var size = Vector2f.Max(origin, destination) - min;
            var broadphaseArea = new RectangleF(min, size);
            if (broadphaseArea.Intersects(getPolylineBB()))
            {
                var result = ResolveNarrow(origin, destination, points);
                if (result != null)
                    result.Box = this;
                return result;
            }
            return null;
        }

        public override IHit Resolve(RectangleF origin, RectangleF destination, MoveableBody box)
        {
            var broadphaseArea = RectangleF.Union(origin, destination);
            if (broadphaseArea.Intersects(getPolylineBB()))
            {
                var result = ResolveNarrow(origin, destination, points,box);
                if (result != null)
                    result.Box = this;
                return result;
            }
            return null;

            
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

        public override IHit Resolve(Vector2f point)
        {
            return null;
        }

        public bool IsOnPolyLine(Vector2f point, Vector2f[] Points, out float height)
        {
            return IsOnPolyLine(point, Points, out height, out var slope);
        }
        public bool IsOnPolyLine(Vector2f point, Vector2f[] Points, out float height, out Vector2f segmentVector)
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
            segmentVector = Vector2f.Zero;
            return false;
        }

        private Hit ResolveNarrow(RectangleF origin, RectangleF destination, Vector2f[] points, MoveableBody box)
        {
            var velocity = (destination.Location - origin.Location);
            if (velocity == Vector2f.Zero)
                return null;

            var offset = new Vector2f(0.5f * origin.Width, origin.Height);
            var topOffset = new Vector2f(0.5f * origin.Width, 0);
            var bottom_origin_Offset = origin.Location + offset;
            var bottom_destination_Offset = destination.Location + offset;
            var top_origin_Offset = origin.Location + topOffset;
            var top_destination_Offset = destination.Location + topOffset;

            var bottom_origin_on_line = IsOnPolyLine(bottom_origin_Offset, points, out var old_bottom_h);
            var bottom_dest_on_line = IsOnPolyLine(bottom_destination_Offset, points, out var new_bottom_h, out var bottom_slope_vector);
            var top_origin_on_line = IsOnPolyLine(top_origin_Offset, points, out var old_top_h, out var top_slope_);
            var top_dest_on_line = IsOnPolyLine(top_destination_Offset, points, out var new_top_h, out var top_slope_vector);

            Hit result = null;


            if (box.MountedBody != null && IsOnPolyLine(bottom_destination_Offset, points, out var grounded_bottom_h, out var ground_slope_vector))
            {
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2f(bottom_destination_Offset.X, grounded_bottom_h) - offset,
                    Normal = new Vector2f(ground_slope_vector.Y, -ground_slope_vector.X),

                };
            }
            else if (velocity.Y > 0 && bottom_origin_on_line && bottom_dest_on_line && ((bottom_origin_Offset.Y <= old_bottom_h) && bottom_destination_Offset.Y >= new_bottom_h))
            {
                var normalVector = new Vector2f(bottom_slope_vector.Y, -bottom_slope_vector.X);
                normalVector.Normalize();
                var reflectVector = Vector2f.Reflect(velocity, normalVector);
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2f(bottom_origin_Offset.X + velocity.X, new_bottom_h) - offset,
                    Normal = new Vector2f(bottom_slope_vector.Y, -bottom_slope_vector.X),

                };
            }
            else if (velocity.Y < 0 && top_dest_on_line && ((top_origin_Offset.Y > old_top_h) && top_destination_Offset.Y <= new_top_h))
            {
                var normalVector = new Vector2f(top_slope_vector.Y, -top_slope_vector.X);
                normalVector.Normalize();
                var reflectVector = Vector2f.Reflect(velocity, normalVector);
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2f(top_origin_Offset.X + velocity.X, top_origin_Offset.Y) - topOffset,
                    Normal = new Vector2f(-bottom_slope_vector.Y, bottom_slope_vector.X),


                };
            }
            //else
            //    Console.WriteLine("ASS");
            return result;
        }

        private bool Intersects(Vector2f p1, Vector2f p2, Vector2f[] points, out Vector2f intersection)
        {
            for (var i = 0; i < Points.Length - 1; i++)
            {
                var result = FindIntersection(p1, p2, points[i], points[i + 1], out intersection);
                if (result)
                    return true;
            }

            intersection = Vector2f.Zero;
            return false;
        }

        private bool FindIntersection(Vector2f p1, Vector2f p2, Vector2f p3, Vector2f p4, out Vector2f intersection)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);
            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                //segments_intersect = false;
                intersection = new Vector2f(float.NaN, float.NaN);
                //close_p1 = new Vector2f(float.NaN, float.NaN);
                //close_p2 = new Vector2f(float.NaN, float.NaN);
                return false;
            }

            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new Vector2f(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            var result = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            //if (t1 < 0)
            //{
            //    t1 = 0;
            //}
            //else if (t1 > 1)
            //{
            //    t1 = 1;
            //}

            //if (t2 < 0)
            //{
            //    t2 = 0;
            //}
            //else if (t2 > 1)
            //{
            //    t2 = 1;
            //}

            //close_p1 = new Vector2f(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            //close_p2 = new Vector2f(p3.X + dx34 * t2, p3.Y + dy34 * t2);

            return result;
        }


        private Hit ResolveNarrow(Vector2f origin, Vector2f destination, Vector2f[] points)
        {
            var velocity = (destination - origin);
            if (velocity == Vector2f.Zero)
                return null;

            var SlopeVector = Vector2f.Zero;
            var origin_on_line = IsOnPolyLine(origin, points, out var h, out SlopeVector);
            var dest_on_line = IsOnPolyLine(destination, points, out var new_h, out SlopeVector);

            Hit result = null;

            if (dest_on_line && velocity.Y > 0 && origin.Y >= h && destination.Y <= new_h)
            {
                velocity.Y = 0f;
                result = new Hit()
                {
                    Amount = 1f,
                    Position = new Vector2f(origin.X + velocity.X, new_h),
                    Normal = new Vector2f(SlopeVector.Y, -SlopeVector.X),

                };
            }
            return result;
        }

        #endregion
    }
}
