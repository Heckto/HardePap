using AuxLib.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game666
{
    public class MovingPlatform : ControlledEntity
    {
        public PlatformController controller;

        public MovingPlatform(Vector2 size, Vector2 loc, World world, Vector2[] wayPoints,Category cat = Category.None)
        {
            colliderSize = size;


            collider = world.CreateRectangle((float)ConvertUnits.ToSimUnits(size.X), (float)ConvertUnits.ToSimUnits(size.Y), 1, ConvertUnits.ToSimUnits(loc));
            collider.SetCollisionCategories(cat);
            collider.Tag = this;
            controller = new PlatformController(collider, world, Category.Cat2)
            {
                speed = 0.1f,
                easeAmount = 1.7f,
                waitTime = 0.5f,
                globalWaypoints = wayPoints
            };
        }

        public void Update(GameTime gameTime)
        {
            controller.Update(gameTime);
        }
    }
}
