using AuxLib;
using Game1.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Game1.Levels;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Obstacles
{
    public class Kunai : SpriteObject
    {
        const float movementSpeed = 1.5f; //pixels per ms
        const float gravity = 0.0001f;

        private Vector2f kunaiPoint;

        public Kunai(Vector2 location, FaceDirection direction, Level level, ContentManager contentManager) : base(contentManager)
        {
            Position = location;
            Direction = direction;
            Level = level;
            Trajectory = new Vector2(movementSpeed, 0f);

            if (direction == FaceDirection.Right)
            {
                Rotation = MathHelper.Pi / 2.0f;
                Trajectory = new Vector2(movementSpeed, 0f);
            }
            else
            {
                Rotation = MathHelper.Pi / -2.0f;
                Trajectory = new Vector2(-movementSpeed, 0f);
            }
        }

        public override void LoadContent(ContentManager contentManager)
        {
            LoadFromSheet(contentManager, @"Content\KunaiSprite.xml");
            CurrentAnimation = Animations["Kunai"];

            var texSize = CurrentAnimation.Frames.First().Size;

            //Can calculate this here since it shouldn't change (unless we add rotation later)
            kunaiPoint = Direction == FaceDirection.Right ?
                new Vector2f(texSize.X, texSize.Y * 0.5f) :
                new Vector2f(0, texSize.Y * 0.5f);
        }

        public override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Trajectory = new Vector2(Trajectory.X, Trajectory.Y + (gravity * delta));
            Position = new Vector2(Position.X + (delta * Trajectory.X), Position.Y + (delta * Trajectory.Y));

            HandleCollision();

            base.Update(gameTime);
        }

        private void HandleCollision()
        {
            var xPosition = Position.X + kunaiPoint.X;

            var yPositions = new List<float> {
                Position.Y + kunaiPoint.Y
            };

            var collisions = yPositions
                            .Select(yPosition => Level.CollisionWorld.Hit(new Vector2f(xPosition, yPosition)))
                            .Where(collision => collision != null)
                            .Select(collision => collision.Box)
                            .Distinct();

            foreach (var collision in collisions)
            {
                if (collision.HasTag(ItemTypes.Enemy))
                {
                    ((LivingSpriteObject)collision.Data).DealDamage(this, 20);
                    Trajectory = new Vector2(0f, 0f);
                }
                else if (collision.HasTag(ItemTypes.PolyLine) || collision.HasTag(ItemTypes.StaticBlock))
                    Trajectory = new Vector2(0f, 0f);
            }
        }
    }
}
