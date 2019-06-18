
using System;
using System.Linq;
using Game1.CollisionDetection.Base;
using Game1.CollisionDetection.Responses;

namespace Game1.CollisionDetection
{

    public class Box : Shape,IBox
    {
        #region Constructors 

        public Box(World world, float x, float y, float width, float height)
        {
            this.world = world;
            this.bounds = new RectangleF(x, y, width, height);
        }

        #endregion

        #region Fields

        private World world;

        private RectangleF bounds;

        #endregion

        #region Properties

        public RectangleF Bounds
        {
            get { return bounds; }
        }

        public float Height => Bounds.Height;

        public float Width => Bounds.Width;

        public float X => Bounds.X;

        public float Y => Bounds.Y;

        #endregion

        #region Movements

        public IMovement Simulate(float x, float y, Func<ICollision, ICollisionResponse> filter)
        {
            return world.Simulate(this, x, y, filter);
        }

        public IMovement Simulate(float x, float y, Func<ICollision, CollisionResponses> filter)
        {
            return Move(x, y, (col) =>
            {
                if (col.Hit == null)
                    return null;

                return CollisionResponse.Create(col, filter(col));
            });
        }

        public IMovement Move(float x, float y, Func<ICollision, ICollisionResponse> filter)
        {
            var movement = this.Simulate(x, y, filter);
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);
            return movement;
        }

        public IMovement Move(float x, float y, Func<ICollision, CollisionResponses> filter)
        {
            var movement = this.Simulate(x, y, filter);
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);
            return movement;
        }

        #endregion

        #region Resolve
        public override IHit Resolve(Vector2 origin, Vector2 destination)
        {
            var result = Resolve(origin, destination, this.Bounds);
            if (result != null) result.Box = this;
            return result;
        }

        public override IHit Resolve(RectangleF origin, RectangleF destination)
        {
            var result = Resolve(origin, destination, this.Bounds);
            if (result != null) result.Box = this;
            return result;
        }

        public Hit Resolve(RectangleF origin, RectangleF destination, RectangleF other)
        {
            var broadphaseArea = RectangleF.Union(origin, destination);

            if (broadphaseArea.Intersects(other) || broadphaseArea.Contains(other))
            {
                return ResolveNarrow(origin, destination, other);
            }

            return null;
        }

        public Hit Resolve(Vector2 origin, Vector2 destination, RectangleF other)
        {
            var min = Vector2.Min(origin, destination);
            var size = Vector2.Max(origin, destination) - min;

            var broadphaseArea = new RectangleF(min, size);

            if (broadphaseArea.Intersects(other) || broadphaseArea.Contains(other))
            {
                return ResolveNarrow(origin, destination, other);
            }

            return null;
        }

        public override IHit Resolve(Vector2 point)
        {
            if (this.Bounds.Contains(point))
            {
                var outside = PushOutside(point, this.Bounds);
                return new Hit()
                {
                    Amount = 0,
                    Box = this,
                    Position = outside.Item1,
                    Normal = outside.Item2,
                };
            }

            return null;
        }

        private static Hit ResolveNarrow(RectangleF origin, RectangleF destination, RectangleF other)
        {
            // if starts inside, push it outside at the neareast place
            if (other.Contains(origin) || other.Intersects(origin))
            {
                var outside = PushOutside(origin, other);
                return new Hit()
                {
                    Amount = 0,
                    Position = outside.Item1.Location,
                    Normal = outside.Item2,
                };
            }

            var velocity = (destination.Location - origin.Location);

            Vector2 invEntry, invExit, entry, exit;

            if (velocity.X > 0)
            {
                invEntry.X = other.Left - origin.Right;
                invExit.X = other.Right - origin.Left;
            }
            else
            {
                invEntry.X = other.Right - origin.Left;
                invExit.X = other.Left - origin.Right;
            }

            if (velocity.Y > 0)
            {
                invEntry.Y = other.Top - origin.Bottom;
                invExit.Y = other.Bottom - origin.Top;
            }
            else
            {
                invEntry.Y = other.Bottom - origin.Top;
                invExit.Y = other.Top - origin.Bottom;
            }

            if (Math.Abs(velocity.X) < Constants.Threshold)
            {
                entry.X = float.MinValue;
                exit.X = float.MaxValue;
            }
            else
            {
                entry.X = invEntry.X / velocity.X;
                exit.X = invExit.X / velocity.X;
            }

            if (Math.Abs(velocity.Y) < Constants.Threshold)
            {
                entry.Y = float.MinValue;
                exit.Y = float.MaxValue;
            }
            else
            {
                entry.Y = invEntry.Y / velocity.Y;
                exit.Y = invExit.Y / velocity.Y;
            }

            if (entry.Y > 1.0f) entry.Y = float.MinValue;
            if (entry.X > 1.0f) entry.X = float.MinValue;

            var entryTime = Math.Max(entry.X, entry.Y);
            var exitTime = Math.Min(exit.X, exit.Y);

            if (
                (entryTime > exitTime || entry.X < 0.0f && entry.Y < 0.0f) ||
                (entry.X < 0.0f && (origin.Right < other.Left || origin.Left > other.Right)) ||
                entry.Y < 0.0f && (origin.Bottom < other.Top || origin.Top > other.Bottom))
                return null;


            var result = new Hit()
            {
                Amount = entryTime,
                Position = origin.Location + velocity * entryTime,
                Normal = GetNormal(invEntry, invExit, entry),
            };


            return result;
        }

        private static Hit ResolveNarrow(Vector2 origin, Vector2 destination, RectangleF other)
        {
            // if starts inside, push it outside at the neareast place
            if (other.Contains(origin))
            {
                var outside = PushOutside(origin, other);
                return new Hit()
                {
                    Amount = 0,
                    Position = outside.Item1,
                    Normal = outside.Item2,
                };
            }

            var velocity = (destination - origin);

            Vector2 invEntry, invExit, entry, exit;

            if (velocity.X > 0)
            {
                invEntry.X = other.Left - origin.X;
                invExit.X = other.Right - origin.X;
            }
            else
            {
                invEntry.X = other.Right - origin.X;
                invExit.X = other.Left - origin.X;
            }

            if (velocity.Y > 0)
            {
                invEntry.Y = other.Top - origin.Y;
                invExit.Y = other.Bottom - origin.Y;
            }
            else
            {
                invEntry.Y = other.Bottom - origin.Y;
                invExit.Y = other.Top - origin.Y;
            }

            if (Math.Abs(velocity.X) < Constants.Threshold)
            {
                entry.X = float.MinValue;
                exit.X = float.MaxValue;
            }
            else
            {
                entry.X = invEntry.X / velocity.X;
                exit.X = invExit.X / velocity.X;
            }

            if (Math.Abs(velocity.Y) < Constants.Threshold)
            {
                entry.Y = float.MinValue;
                exit.Y = float.MaxValue;
            }
            else
            {
                entry.Y = invEntry.Y / velocity.Y;
                exit.Y = invExit.Y / velocity.Y;
            }

            if (entry.Y > 1.0f) entry.Y = float.MinValue;
            if (entry.X > 1.0f) entry.X = float.MinValue;

            var entryTime = Math.Max(entry.X, entry.Y);
            var exitTime = Math.Min(exit.X, exit.Y);

            if (
                (entryTime > exitTime || entry.X < 0.0f && entry.Y < 0.0f) ||
                (entry.X < 0.0f && (origin.X < other.Left || origin.X > other.Right)) ||
                entry.Y < 0.0f && (origin.Y < other.Top || origin.Y > other.Bottom))
                return null;

            var result = new Hit()
            {
                Amount = entryTime,
                Position = origin + velocity * entryTime,
                Normal = GetNormal(invEntry, invExit, entry),
            };

            return result;
        }

        private static Vector2 GetNormal(Vector2 invEntry, Vector2 invExit, Vector2 entry)
        {
            if (entry.X > entry.Y)
            {
                return (invEntry.X < 0.0f) || (Math.Abs(invEntry.X) < Constants.Threshold && invExit.X < 0) ? Vector2.UnitX : -Vector2.UnitX;
            }

            return (invEntry.Y < 0.0f || (Math.Abs(invEntry.Y) < Constants.Threshold && invExit.Y < 0)) ? Vector2.UnitY : -Vector2.UnitY;
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

