using AuxLib;
using AuxLib.Input;
using Game1.CollisionDetection;
using Game1.CollisionDetection.Responses;
using Game1.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Enemies
{
    class Enemy1 : LivingSpriteObject
    {
        private const float acc = -25f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;

        private Vector2 hitBoxSize = new Vector2(220, 400);

        public override Vector2 Position => new Vector2(CollisionBox.X, CollisionBox.Y) + 0.5f * scale * hitBoxSize;

        public override int MaxHealth => 100;

        private Player player1;

        public Enemy1(Vector2 loc, World world, Player player1, ContentManager cm) : base(cm)
        {
            colBodySize = scale * hitBoxSize;
            CollisionBox = (MoveableBody)world.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);

            (CollisionBox as IBox).AddTags(ItemTypes.Enemy);
            CollisionBox.Data = this;

            World = world;

            this.player1 = player1;
        }

        public override void LoadContent(ContentManager cm)
        {
            LoadFromSheet(cm, @"Content\EnemySprite.xml");

            CurrentAnimation = Animations["Jump"];
        }

        public override void ManagedUpdate(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleKeyInput(delta);
            HandleCollision(delta);
            UpdateAnimation(gameTime);
        }

        private void HandleKeyInput(float delta)
        {
            if (CurrentState == CharState.GroundAttack && CurrentAnimation.AnimationName == "Attack")
            {
                if (CurrentAnimation.AnimationState != AnimationState.Running)
                    CurrentState = CharState.Grounded;
                return;
            }


            var trajectoryX = Trajectory.X;
            var trajectoryY = Trajectory.Y;
            if (CurrentState == CharState.Grounded && Math.Abs(player1.Position.X - Position.X) < 150)
            {
                Trajectory = new Vector2(0f, Trajectory.Y);
                CurrentState = CharState.GroundAttack;
                return;
            }

            if (player1.Position.X < Position.X)
            {
                if (Trajectory.X > 0)
                    trajectoryX = 0;
                else
                    trajectoryX = acc * friction * delta;

                Direction = FaceDirection.Left;
            }
            else if (player1.Position.X > Position.X)
            {
                if (Trajectory.X < 0)
                    trajectoryX = 0;
                else
                    trajectoryX = -acc * friction * delta;

                Direction = FaceDirection.Right;
            }
            else if (Trajectory.X != 0)
            {
                trajectoryX = 0;
            }

            if (Math.Abs(player1.Position.Y - Position.Y) > 300)
            {
                if (CurrentState == CharState.Grounded)
                {
                    trajectoryY = -1 * jumpForce;
                    CurrentState = CharState.Air;
                    CollisionBox.Grounded = false;
                }
            }

            Trajectory = new Vector2(trajectoryX, trajectoryY);
        }

        private void HandleCollision(float delta)
        {
            var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y, (collision) =>
            {
                if (collision.Other.HasTag(ItemTypes.Transition))
                {
                    //throw new NotImplementedException();
                }
                
                if (collision.Hit.Normal.Y < 0)
                {
                    return CollisionResponses.Slide;
                }
                if (collision.Hit.Normal.Y > 0)
                {
                    Trajectory = new Vector2(Trajectory.X, -Trajectory.Y);
                    return CollisionResponses.Touch;
                }
                return CollisionResponses.Slide;
            });

            if (move.Hits.Any((c) => c.Box.HasTag(ItemTypes.PolyLine, ItemTypes.StaticBlock) && (c.Normal.Y < 0)))
            {
                if (CurrentState != CharState.Grounded && CurrentState != CharState.GroundAttack)
                    CurrentState = CharState.Grounded;
                CollisionBox.Grounded = true;
                Trajectory = new Vector2(Trajectory.X, delta * 0.001f);
            }
            else
            {
                Trajectory = new Vector2(Trajectory.X, Trajectory.Y + delta * 0.001f);
                CollisionBox.Grounded = false;
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            switch (CurrentState)
            {
                case CharState.GroundAttack:
                    SetAnimation("Attack");
                    break;
                case CharState.Grounded:
                    if (Trajectory.X == 0)
                        SetAnimation("Idle");
                    else
                        SetAnimation("Run");
                    break;
                case CharState.Air:
                    SetAnimation("Jump");
                    break;
                case CharState.Glide:
                    SetAnimation("Glide");
                    break;
            }
        }
    }
}
