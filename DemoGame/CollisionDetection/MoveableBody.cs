using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game1.CollisionDetection.Responses;
using AuxLib;

namespace Game1.CollisionDetection
{
    public class MoveableBody : Box, IMoveableBody
    {
        public MoveableBody(World world, float x, float y, float width, float height,bool grounded) : base(world, x, y, width, height)
        {
            Grounded = grounded;
        }

        public bool Grounded { get; set; }

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
    }
}
