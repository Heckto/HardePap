using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using AuxLib.CollisionDetection;
using AuxLib.CollisionDetection.Responses;
using AuxLib;
using Game1.Screens;
using AuxLib.Input;
using Game1.Sprite;
using Game1.Levels;
using Game1.Sprite.Enums;
using Game1.DataContext;
using AuxLib.RandomGeneration;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1
{
    public class Zombie1 : LivingSpriteObject
    {
        private const float gravity = 64f;

        private BehaviourState state;
        private Vector2 hitBoxSize = new Vector2(220, 400);

        public override Vector2 Position => ConvertUnits.ToDisplayUnits(CollisionBox.Position);

        public override int MaxHealth => 100;

        private Vector2 movingTarget;
        private float IdleTimeout = 0;
        

        public Zombie1(Vector2 loc, GameContext context) : base(context)
        {
            colBodySize = scale * hitBoxSize;
            CollisionBox = context.lvl.CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(colBodySize.X), ConvertUnits.ToSimUnits(colBodySize.Y), 1, ConvertUnits.ToSimUnits(loc), 0, BodyType.Kinematic);
            CollisionBox.Tag = this;
            CollisionBox.SetCollisionCategories(Category.Cat20);

            controller = new Controller2D(CollisionBox, Category.Cat2 | Category.Cat4 | Category.Cat5);
            state = BehaviourState.Idle;
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\Characters\Zombie1\Zombie1_Definition.xml");
            CurrentAnimation = Animations["Idle"];
        }

        public override void Update(GameTime gameTime)
        {            
            base.Update(gameTime);
        }

        public override void ManagedUpdate(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (HandleInput)
                HandleKeyInput(delta, InputHandler.Instance);
        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            var targetDist = Math.Abs(context.lvl.player.Position.X - Position.X);
            switch (state)
            {
                case BehaviourState.Idle:
                    IdleTimeout += delta;
                    //if (targetDist < 500)
                    //{
                    //    state = BehaviourState.Chasing;
                    //    SetAnimation("Walk");
                    //}
                    //else
                    if (IdleTimeout > 2000)
                    {
                        var movementLength = Rand.GetRandomInt(300, 500);
                        var movementDir = Rand.GetRandomInt(0, 2);
                        movingTarget = movementDir == 0 ? movingTarget = new Vector2(Position.X + movementLength, 0) : movingTarget = new Vector2(Position.X - movementLength, 0);
                        state = BehaviourState.Walking;
                        SetAnimation("Walk");
                        IdleTimeout = 0;
                    }
                    break;
                case BehaviourState.Walking:
                    //if (targetDist < 500)
                    //{
                    //    SetAnimation("Walk");
                    //    state = BehaviourState.Chasing;
                    //}
                    var tv = movingTarget - Position;

                    tv.Normalize();
                    if (tv.X < 0)
                        velocity.X = -Math.Min(Math.Abs(-0.05f * delta), Math.Abs((movingTarget - Position).X / delta));
                    else
                        velocity.X = Math.Min(0.05f * delta, (movingTarget - Position).X / delta);

                    if (Math.Abs(movingTarget.X - Position.X) <= 1)
                    {
                        state = BehaviourState.Idle;
                        SetAnimation("Idle");
                        Console.WriteLine(Position + " - " + movingTarget);
                    }
                    break;
                case BehaviourState.Chasing:
                    if (targetDist > 500)
                        state = BehaviourState.Idle;
                    var v = context.lvl.player.Position - Position;
                    v.Normalize();
                    if (v.X < 0)
                        velocity.X = -Math.Min(Math.Abs(-0.03f * delta), Math.Abs((context.lvl.player.Position - Position).X / delta));
                    else
                        velocity.X = Math.Min(0.03f * delta, (context.lvl.player.Position - Position).X / delta);
                    break;
                case BehaviourState.Attack:
                    //if (targetDist > context.lvl.player.CollisionBox.Width / 2 && CurrentAnimation.AnimationState == AnimationState.Finished)
                    //    state = BehaviourState.Idle;
                    //if (CurrentAnimation.AnimationState != AnimationState.Running)
                    //    SetAnimation("Attack");
                    break;
            }

            Direction = velocity.X < 0 ? FaceDirection.Left : FaceDirection.Right;

            velocity.Y += gravity * delta;
            controller.Move(velocity);

            if (controller.collisions.edgeCase && controller.collisions.below)
            {
                movingTarget.X -= ((movingTarget - Position).X * controller.collisions.faceDirection);
            }
            else if (controller.collisions.below)
                velocity.Y = 0;



        }        

        private void HandleAttackCollisions()
        {
            //var width = (int)CollisionBox.Width;
            //var swordLength = width * 0.9f;

            //var xPosition = Direction == FaceDirection.Right ?
            //    CollisionBox.Bounds.Right + swordLength :
            //    CollisionBox.X - swordLength;

            //var yPositions = new List<float> {
            //    CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.1f),
            //    CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.5f),
            //    CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.9f)
            //};

            //var collisions = yPositions
            //                .Select(yPosition => context.lvl.CollisionWorld.Hit(new Vector2f(xPosition, yPosition)))
            //                .Where(collision => collision != null && collision.Box.HasTag(ItemTypes.Player))
            //                .Select(collision => collision.Box)
            //                .Distinct();

            //foreach (var collision in collisions)
            //{
            //    ((LivingSpriteObject)collision.Data).DealDamage(this, 50);
            //}
        }

        protected override void ManagedDraw(SpriteBatch spriteBatch)
        {
            base.ManagedDraw(spriteBatch);
        }

        protected override void OnDeath()
        {
            if (state != BehaviourState.Dying)
            {
                state = BehaviourState.Dying;
                SetAnimation("Dead");
            }
            else if (CurrentAnimation.AnimationName == "Dead" && CurrentAnimation.AnimationState == AnimationState.Finished)
            {
                context.lvl.CollisionWorld.Remove(CollisionBox);
                IsAlive = false;
            }
        }

        public enum BehaviourState
        {
            Idle = 0,
            Walking =1,
            Chasing = 2,
            Attack = 4,
            Dying = 8
        }
    }


}
