using AuxLib;
using Game1.GameObjects.Sprite;
using Microsoft.Xna.Framework;
using System.Linq;
using Game1.DataContext;
using tainicom.Aether.Physics2D.Dynamics;
using Game1.Controllers;
using System;
using Game1.GameObjects.Levels;
using Game1.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework.Content;

namespace Game1.GameObjects.Obstacles
{
    public class Kunai : SpriteObject, IUpdateableItem, IDrawableItem
    {
        const float movementSpeed = 30f; //pixels per ms

        private BulletController controller;
        private Vector2 movement;
        private int dir;
        private float Rotation;

        public override Vector2 Size
        {
            get { return Vector2.One; }
        }

        public Kunai(Vector2 loc, int direction, GameContext context) : base(context)
        {
            Visible = true;
            Transform.Position = loc;
            dir = direction;
            IsAlive = true;          
        }

        public override void LoadContent(ContentManager contentManager)
        {            
            LoadFromSheet(@"Content\KunaiSprite.xml", contentManager);
            CurrentAnimation = Animations["Kunai"];
        }

        public override void Initialize()
        {
            movement = new Vector2(Math.Sign(dir) * movementSpeed, 0);
            var texSize = CurrentAnimation.Frames.First().Size;
            colBodySize = 0.5f * texSize;
            Rotation = (dir == 1) ? MathHelper.Pi / 2.0f : MathHelper.Pi / -2.0f;



            CollisionBox = context.lvl.CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(colBodySize.X), ConvertUnits.ToSimUnits(colBodySize.Y), 1, ConvertUnits.ToSimUnits(Transform.Position), Rotation, BodyType.Kinematic);
            CollisionBox.Tag = this;
            CollisionBox.IsBullet = true;

            //controller = new BulletController(CollisionBox, Category.Cat20 | Category.Cat2 | Category.Cat4 | Category.Cat5);
            controller = new BulletController(CollisionBox, Category.All);

            controller.onCollision += hitInfo =>
            {
                if (hitInfo.fixture != null)
                {
                    if (hitInfo.fixture.CollisionCategories == Category.Cat20)
                    {
                        ((LivingSpriteObject)hitInfo.fixture.Body.Tag).DealDamage(this, 50);
                    }
                    IsAlive = false;
                }
            };


            base.Initialize();
        }

        public override void Update(GameTime gameTime,Level lvl)
        {
            var r = new Rectangle((int)Transform.Position.X, (int)Transform.Position.Y, (int)colBodySize.Y, (int)colBodySize.X);
            if (!context.camera.Bounds.Intersects(r))
                IsAlive = false;           

            controller.Move(movement);

            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);
            base.Update(gameTime, lvl);
        }

        public void Draw(SpriteBatch sb)
        {
            var flip = (Direction == FaceDirection.Left);
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(sb, (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, Rotation, 0.5f, Color, AnimationEffect.None);
        }
    }
}
