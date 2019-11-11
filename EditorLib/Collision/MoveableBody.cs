using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.CollisionDetection.Responses;
using AuxLib;

namespace AuxLib.CollisionDetection
{
    public class MoveableBody : Box, IMoveableBody
    {
        public event onCollisionResponseDelegate onCollisionResponse;
        public delegate CollisionResponses onCollisionResponseDelegate(ICollision collision);


        public delegate ICollision onCollisionDelegate(ICollision collision);

        public IShape MountedBody { get; set; }

        public Vector2f Velocity = Vector2f.Zero;

        public MoveableBody(World world, float x, float y, float width, float height) : base(world, x, y, width, height)
        {
        }

        #region Movements
        public IMovement Move(float x, float y,float delta,List<IShape> ignoreList = null)
        {
            var rel_pos = new Vector2f(x - this.X, y - this.Y);

            if (MountedBody != null && MountedBody is IMoveableBody)
            {
                var ass = ((MoveableBody)MountedBody);
                x += ass.Velocity.X * delta;
                y += ass.Velocity.Y * delta;
            }

            var movement = world.Simulate(this, x, y, OnCollision, ignoreList);
            this.bounds.X = movement.Destination.X;
            this.bounds.Y = movement.Destination.Y;
            this.world.Update(this, movement.Origin);

            
            


            return movement;
        }

        public ICollisionResponse OnCollision(ICollision collision)
        {
            if (collision.Hit == null)
                return null;

            var response = onCollisionResponse?.Invoke(collision);
            if (response.HasValue)
                return CollisionResponse.Create(collision, response.Value);
            else
                return null;
        }

        #endregion
    }
}
