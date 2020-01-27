using AuxLib;
using AuxLib.Debug;
using Game1.DataContext;
using Game1.GameObjects.Levels;
using Game1.GameObjects.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1.GameObjects.Obstacles
{
    public class MovingPlatform : LivingSpriteObject
    {
        public PlatformController cntlr;

        public MovingPlatform(GameContext context, Vector2 size, Vector2 loc, World world, Vector2[] wayPoints, Category cat = Category.None) : base(context)
        {

            colBodySize = size;


            CollisionBox = world.CreateRectangle((float)ConvertUnits.ToSimUnits(size.X), (float)ConvertUnits.ToSimUnits(size.Y), 1, ConvertUnits.ToSimUnits(loc));
            CollisionBox.SetCollisionCategories(cat);
            CollisionBox.Tag = this;
            cntlr = new PlatformController(CollisionBox, Category.Cat2)
            {
                speed = 0.1f,
                easeAmount = 1.7f,
                waitTime = 0.5f,
                globalWaypoints = wayPoints
            };
        }

        public override int MaxHealth => throw new NotImplementedException();

        public override void LoadContent()
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime, Level lvl)
        {
            cntlr.Update(gameTime);
        }
    }
}
