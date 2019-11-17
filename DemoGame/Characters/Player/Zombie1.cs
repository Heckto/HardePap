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

namespace Game1
{
    public class Zombie1 : LivingSpriteObject
    {
        private CharState CurrentState;

        private const float acc = 3f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;

        private BehaviourState state;
        private Vector2 hitBoxSize = new Vector2(220, 400);

        public override Vector2 Position => new Vector2(CollisionBox.X, CollisionBox.Y) + 0.5f * scale * hitBoxSize;

        public override int MaxHealth => 100;

        private Vector2 movingTarget;
        private float IdleTimeout = 0;
        

        public Zombie1(Vector2 loc, GameContext context, ItemTypes objectType) : base(context)
        {
            colBodySize = scale * hitBoxSize;
            
            CollisionBox = (MoveableBody)context.lvl.CollisionWorld.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            CollisionBox.onCollisionResponse += OnResolveCollision;
            (CollisionBox as IBox).AddTags(objectType);
            CollisionBox.Data = this;
            state = BehaviourState.Idle;
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\Characters\Zombie1\Zombie1_Definition.xml");
            CurrentAnimation = Animations["Idle"];
            CurrentState = CharState.Grounded;
        }

        public override void Update(GameTime gameTime)
        {            
            base.Update(gameTime);
        }

        public override void ManagedUpdate(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (HandleInput)
                HandleKeyInput(delta, InputHandler.Instance);
            HandleCollision(delta);
            //UpdateAnimation(gameTime);
        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            var trajectoryX = Trajectory.X;
            var trajectoryY = Trajectory.Y;

            var targetDist = Math.Abs(context.lvl.player.Position.X - Position.X);
            switch (state)
            {
                case BehaviourState.Idle:
                    trajectoryX = 0;
                    IdleTimeout += delta;
                    if (targetDist < 500)
                    {
                        state = BehaviourState.Chasing;
                        SetAnimation("Walk");
                    }
                        
                    else if (IdleTimeout > 2000)
                    {
                        var movementLength = Rand.GetRandomInt(300, 500);
                        var movementDir = Rand.GetRandomInt(0, 2);
                        movingTarget = movementDir == 0 ? movingTarget = new Vector2(Position.X + movementLength, 0) : movingTarget = new Vector2(Position.X - movementLength, 0);
                        CurrentState = CharState.Grounded;                      
                        state = BehaviourState.Walking;
                        SetAnimation("Walk");
                        IdleTimeout = 0;
                    }                   
                    break;
                case BehaviourState.Walking:
                    if (targetDist < 500)
                    {
                        SetAnimation("Walk");
                        state = BehaviourState.Chasing;
                    }
                    var tv = movingTarget - Position;                    
                    
                    tv.Normalize();
                    if (tv.X < 0)
                        trajectoryX = -Math.Min(Math.Abs(-0.025f * delta),Math.Abs((movingTarget - Position).X / delta));
                    else
                        trajectoryX = Math.Min(0.025f * delta, (movingTarget - Position).X / delta);

                    Console.WriteLine(trajectoryX);
                    if (Math.Abs(movingTarget.X - Position.X) <= 1)
                    {
                        CurrentState = CharState.Grounded;
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
                        trajectoryX = -Math.Min(Math.Abs(-0.03f * delta), Math.Abs((context.lvl.player.Position - Position).X / delta));
                    else
                        trajectoryX = Math.Min(0.03f * delta, (context.lvl.player.Position - Position).X / delta);
                    break;
                case BehaviourState.Attack:
                    if (targetDist > context.lvl.player.CollisionBox.Width / 2 && CurrentAnimation.AnimationState == AnimationState.Finished)
                        state = BehaviourState.Idle;
                    if (CurrentAnimation.AnimationState != AnimationState.Running)
                        SetAnimation("Attack");
                    break;
            }
            
            if (trajectoryX < 0)
            {                
                Direction = FaceDirection.Left;
            }
            else if (trajectoryX > 0)
            {
                Direction = FaceDirection.Right;
                //trajectoryX = acc * friction * delta;
            }

            trajectoryY += delta * gravity;
            Trajectory = new Vector2(trajectoryX, trajectoryY);
        }

        private void HandleCollision(float delta)
        {
            var hit = context.lvl.CollisionWorld.Hit(new Vector2f(Position.X,Position.Y), new Vector2f(Position.X + (delta * Trajectory.X), Position.Y + (delta * Trajectory.Y)));
            if (hit == null)
            {
                // van het scherm af...
                Console.WriteLine("ass");
            }

            var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y,delta);

            var hits = move.Hits.ToList();

            if (hits.Any((c) => c.Box.HasTag(ItemTypes.Collider) && (c.Normal.Y < 0)))
            {
                var mounted = move.Hits.Where(elem => elem.Normal.Y < 0);
                if (mounted.Any())
                {
                    CollisionBox.MountedBody = mounted.First().Box;
                }
                Trajectory = new Vector2(Trajectory.X, delta * 0.001f);                
            }
            else if((hits.Any((c) => (c.Normal.Y < 0)) && Trajectory.Y > 0) || 
                    (hits.Any((c) => (c.Normal.Y > 0)) && Trajectory.Y < 0))
            {
                Trajectory = new Vector2(Trajectory.X, delta * 0.001f);
            }

            if (state == BehaviourState.Attack)
            {
                HandleAttackCollisions();
            }
        }

        private CollisionResponses OnResolveCollision(ICollision collision)
        {            
            if (collision.Hit.Normal.Y < 0)
            {
                return CollisionResponses.Slide;
            }
            if (collision.Hit.Normal.Y > 0)
            {
                Trajectory = new Vector2(Trajectory.X, -Trajectory.Y);
                return CollisionResponses.Touch;
            }
            if (collision.Hit.Normal.X > 0 || collision.Hit.Normal.X < 0)
            {
                if (collision.Hit.Box.HasTag(ItemTypes.Player))
                {
                    Trajectory = Vector2.Zero;
                    SetAnimation("Attack");
                    state = BehaviourState.Attack;
                }
            }
            return CollisionResponses.Slide;
        }

        private void HandleAttackCollisions()
        {
            var width = (int)CollisionBox.Width;
            var swordLength = width * 0.9f;

            var xPosition = Direction == FaceDirection.Right ?
                CollisionBox.Bounds.Right + swordLength :
                CollisionBox.X - swordLength;

            var yPositions = new List<float> {
                CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.1f),
                CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.5f),
                CollisionBox.Bounds.Top + (CollisionBox.Bounds.Height * 0.9f)
            };

            var collisions = yPositions
                            .Select(yPosition => context.lvl.CollisionWorld.Hit(new Vector2f(xPosition, yPosition)))
                            .Where(collision => collision != null && collision.Box.HasTag(ItemTypes.Player))
                            .Select(collision => collision.Box)
                            .Distinct();

            foreach (var collision in collisions)
            {
                ((LivingSpriteObject)collision.Data).DealDamage(this, 50);
            }
        }

        protected override void ManagedDraw(SpriteBatch spriteBatch)
        {
            base.ManagedDraw(spriteBatch);
        }

        public enum BehaviourState
        {
            Idle = 0,
            Walking =1,
            Chasing = 2,
            Attack = 4
        }

        public enum CharState
        {
            Grounded = 0x01,
            GroundAttack = 0x02,
        };
    }


}
