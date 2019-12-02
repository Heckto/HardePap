using AuxLib;
using Game1.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using Game1.DataContext;
using AuxLib.CollisionDetection;
using AuxLib.CollisionDetection.Responses;
using tainicom.Aether.Physics2D.Dynamics;
using Game1.Controllers;
using Microsoft.Xna.Framework.Graphics;
using System;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace Game1.Obstacles
{
    public class Kunai : SpriteObject
    {
        const float movementSpeed = 30f; //pixels per ms

        private BulletController controller;
        private Vector2 movement;      

        public override Vector2 Position => ConvertUnits.ToDisplayUnits(CollisionBox.Position);

        public Kunai(Vector2 loc, int direction, GameContext context) : base(context)
        {
            movement = new Vector2(Math.Sign(direction) * movementSpeed, 0);
            var texSize = CurrentAnimation.Frames.First().Size;
            colBodySize = texSize;
            Rotation = (direction == 1) ? MathHelper.Pi / 2.0f : MathHelper.Pi / -2.0f;          

            CollisionBox = context.lvl.CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(colBodySize.X), ConvertUnits.ToSimUnits(colBodySize.Y), 1, ConvertUnits.ToSimUnits(loc), Rotation, BodyType.Kinematic);
            CollisionBox.Tag = this;
            CollisionBox.IsBullet = true;

            controller = new BulletController(CollisionBox, Category.Cat20 | Category.Cat2 | Category.Cat4 | Category.Cat5);

            controller.onCollision += hitInfo =>
            {
                if (hitInfo.fixture != null)
                {
                    if (hitInfo.fixture.CollisionCategories == Category.Cat20)
                    {
                        ((LivingSpriteObject)hitInfo.fixture.Body.Tag).DealDamage(this, 20);
                    }
                    IsAlive = false;
                }
            };

            IsAlive = true;          
        }

        public override void LoadContent()
        {            
            LoadFromSheet(@"Content\KunaiSprite.xml");
            CurrentAnimation = Animations["Kunai"];
        }

        public override void Update(GameTime gameTime)
        {
            var r = new Rectangle((int)Position.X, (int)Position.Y, (int)colBodySize.X, (int)colBodySize.Y);
            if (!context.camera.Bounds.Intersects(r))
                IsAlive = false;           

            controller.Move(movement);
            base.Update(gameTime);
        }
    }
}
