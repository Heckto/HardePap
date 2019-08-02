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
    class Enemy1 : SpriteObject
    {

        private const float acc = -25f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;

        public override Vector2 Position { get { return new Vector2(CollisionBox.X, CollisionBox.Y) + 0.5f * scale * hitBoxSize; } }

        private Player player1;

        public Enemy1(Vector2 loc, World world, Player player1, ContentManager cm) : base(cm)
        {
            colBodySize = scale * hitBoxSize;
            CollisionBox = (MoveableBody)world.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);

            (CollisionBox as IBox).AddTags(ItemTypes.Enemy);

            this.player1 = player1;
        }

        public override void LoadContent(ContentManager cm)
        {
            LoadFromFile(cm, @"Content\EnemySprite.xml");

            CurrentAnimation = Animations["Jump"];
        }

        public void SetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
            {
                var animation = Animations[name];
                if (CurrentAnimation != animation)
                {
                    animation.Reset();
                    CurrentAnimation = animation;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleKeyInput(delta);

            HandleCollision(delta);

            UpdateAnimation(gameTime);
        }

        private void HandleKeyInput(float delta)
        {
            if (mCurrentState == CharState.GroundAttack && CurrentAnimation.AnimationName == "Attack")
            {
                if (CurrentAnimation.AnimationState != AnimationState.Running)
                    mCurrentState = CharState.Grounded;
                return;
            }

            if (mCurrentState == CharState.Grounded && Math.Abs(player1.Position.X - Position.X) < 300)
            {
                trajectory.X = 0;
                mCurrentState = CharState.GroundAttack;
            }

            if (player1.Position.X < Position.X)
            {
                if (trajectory.X > 0)
                    trajectory.X = 0;
                else
                    trajectory.X = acc * friction * delta;

                Direction = FaceDirection.Left;
            }
            else if (player1.Position.X > Position.X)
            {
                if (trajectory.X < 0)
                    trajectory.X = 0;
                else
                    trajectory.X = -acc * friction * delta;

                Direction = FaceDirection.Right;
            }
            else if (trajectory.X != 0)
            {
                trajectory.X = 0;
            }

            if (Math.Abs(player1.Position.Y - Position.Y) > 300)
            {
                if (mCurrentState == CharState.Grounded)
                {
                    trajectory.Y -= jumpForce;
                    mCurrentState = CharState.Air;
                    CollisionBox.Grounded = false;
                }
            }
        }

        private void HandleCollision(float delta)
        {
            var move = (CollisionBox).Move(CollisionBox.X + delta * trajectory.X, CollisionBox.Y + delta * trajectory.Y, (collision) =>
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
                    trajectory.Y = -trajectory.Y;
                    return CollisionResponses.Touch;
                }
                return CollisionResponses.Slide;
            });

            if (move.Hits.Any((c) => c.Box.HasTag(ItemTypes.PolyLine, ItemTypes.StaticBlock) && (c.Normal.Y < 0)))
            {
                if (mCurrentState != CharState.Grounded && mCurrentState != CharState.GroundAttack)
                    mCurrentState = CharState.Grounded;
                CollisionBox.Grounded = true;
                trajectory.Y = delta * 0.001f;
            }
            else
            {
                trajectory.Y += delta * 0.001f;
                CollisionBox.Grounded = false;
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            switch (mCurrentState)
            {
                case CharState.GroundAttack:
                    SetAnimation("Attack");
                    break;
                case CharState.Grounded:
                    if (trajectory.X == 0)
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

            CurrentAnimation.Update(gameTime);
        }
    }
}
