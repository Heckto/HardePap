using AuxLib;
using Game1.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using Game1.DataContext;
using AuxLib.CollisionDetection;
using AuxLib.CollisionDetection.Responses;

namespace Game1.Obstacles
{
    public class Kunai : SpriteObject
    {
        const float movementSpeed = 1.5f; //pixels per ms
        const float gravity = 0.0001f;

        private Vector2f kunaiPoint;

        public Kunai(Vector2 location, FaceDirection direction, GameContext context) : base(context)
        {
            Position = location;
            Direction = direction;

            var texSize = CurrentAnimation.Frames.First().Size;

            CollisionBox = (MoveableBody)context.lvl.CollisionWorld.CreateMoveableBody(location.X-(texSize.Y /2), location.Y - (texSize.X / 2), texSize.Y, texSize.X);
            CollisionBox.onCollisionResponse += OnResolveCollision;
            CollisionBox.Data = this;
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
            IsAlive = true;

            
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\KunaiSprite.xml");
            CurrentAnimation = Animations["Kunai"];

            
        }

        public override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            Trajectory = new Vector2(Trajectory.X, Trajectory.Y + (gravity * delta));

            var oldPos = Position;

            Position = new Vector2(Position.X + (delta * Trajectory.X), Position.Y + (delta * Trajectory.Y));

            //var hits = context.lvl.CollisionWorld.Hit(oldPos, Position, new List<IShape>() { this.CollisionBox });



            var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y, delta);
            var hits = move.Hits.ToList();
            //            if (hits.Any((c) => c.Box.HasTag(ItemTypes.Collider) && (c.Normal.Y < 0)))
            //{
            if (hits.Any())
            {
                if (hits[0].Box.HasTag(ItemTypes.Enemy))
                {
                    ((LivingSpriteObject)hits[0].Box.Data).DealDamage(this, 20);
                    Trajectory = Vector2.Zero;
                    IsAlive = false;
                    context.lvl.CollisionWorld.Remove(CollisionBox);
                }
                if (hits[0].Box.HasTag(ItemTypes.Collider))
                {
                    Trajectory = Vector2.Zero;
                    IsAlive = false;
                    context.lvl.CollisionWorld.Remove(CollisionBox);
                }
            }
            //}
            base.Update(gameTime);
        }

        private CollisionResponses OnResolveCollision(ICollision collision)
        {
            return CollisionResponses.None;
        }
    }
}
