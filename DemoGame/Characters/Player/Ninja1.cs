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

namespace Game1
{
    public class Ninja1 : LivingSpriteObject
    {
        private CharState CurrentState;

        private int JumpCnt = 0;
        private int MaxJumpCount = 2;

        private const float acc = -45f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;        

        private Vector2 hitBoxSize = new Vector2(220, 400);

        public override Vector2 Position => new Vector2(CollisionBox.X, CollisionBox.Y) + 0.5f * scale * hitBoxSize;

        public override int MaxHealth => 100;

        private readonly List<SpriteObject> thrownObjects = new List<SpriteObject>();

        public Ninja1(Vector2 loc, GameContext context, ItemTypes objectType) : base(context)
        {
            colBodySize = scale * hitBoxSize;
            
            CollisionBox = (MoveableBody)context.lvl.CollisionWorld.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            CollisionBox.onCollisionResponse += OnResolveCollision;

            (CollisionBox as IBox).AddTags(objectType);
            CollisionBox.Data = this;

            PlayState.DebugMonitor.AddDebugValue(this, nameof(CurrentAnimation));
            PlayState.DebugMonitor.AddDebugValue(this, nameof(CurrentState));
            PlayState.DebugMonitor.AddDebugValue(this, nameof(Position));
            PlayState.DebugMonitor.AddDebugValue(this, nameof(Trajectory));
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\Characters\Ninja1\Ninja1_Definition.xml");
            CurrentAnimation = Animations["Jump"];
        }

        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);

            if (!context.lvl.LevelBounds.Contains(context.lvl.player.Position) && !context.transitionManager.isTransitioning)
                context.lvl.SpawnPlayer(null);

        }

        public override void ManagedUpdate(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (HandleInput)
                HandleKeyInput(delta, InputHandler.Instance);
            HandleCollision(delta);
            UpdateAnimation(gameTime);

            var deadObjects = thrownObjects.Where(x => !x.IsAlive).ToList();
            foreach(var deadObject in deadObjects)
            {
                thrownObjects.Remove(deadObject);
            }

            foreach (var thrown in thrownObjects)
            {
                thrown.Update(gameTime);
            }
        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            var keyLeft = Input.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
            var keyRight = Input.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

            var isKeyJump = Input.IsPressed(0, Buttons.A, Keys.Space);
            var wasKeyJump = Input.WasPressed(0, Buttons.A, Keys.Space);

            var isKeyAttack = Input.IsPressed(0, Buttons.X, Keys.LeftControl);
            var isKeyThrow = Input.WasPressed(0, Buttons.Y, Keys.LeftShift);

            if ((CurrentState == CharState.GroundAttack && CurrentAnimation.AnimationName == "Attack") ||
                (CurrentState == CharState.GroundThrow && CurrentAnimation.AnimationName == "Throw"))
            {
                if (CurrentAnimation.AnimationState == AnimationState.Running)
                    return;
                else
                    CurrentState = CharState.Grounded;
            }
            else if ((CurrentState == CharState.JumpAttack && CurrentAnimation.AnimationName == "JumpAttack") ||
                (CurrentState == CharState.JumpThrow && CurrentAnimation.AnimationName == "JumpThrow"))
            {
                if (CurrentAnimation.AnimationState == AnimationState.Finished)
                    CurrentState = CharState.Air;
            }

            var trajectoryX = Trajectory.X;
            var trajectoryY = Trajectory.Y;
            if (keyLeft)
            {
                if (Trajectory.X > 0)
                    trajectoryX = 0;
                else
                    trajectoryX = acc * friction * delta;

                if (CurrentState != CharState.JumpAttack && CurrentState != CharState.JumpThrow)
                    Direction = FaceDirection.Left;
            }
            else if (keyRight)
            {
                if (Trajectory.X < 0)
                    trajectoryX = 0;
                else
                    trajectoryX = -acc * friction * delta;

                if (CurrentState != CharState.JumpAttack && CurrentState != CharState.JumpThrow)
                    Direction = FaceDirection.Right;
            }
            else if (Trajectory.X != 0)
            {
                trajectoryX = 0;
            }

            if (wasKeyJump)
            {
                if (CurrentState == CharState.Grounded)
                {
                    trajectoryY = -jumpForce;
                    CurrentState = CharState.Air;
                    CollisionBox.MountedBody = null;
                    JumpCnt++;
                }
                else if (CurrentState == CharState.Air && JumpCnt > 0 && JumpCnt < MaxJumpCount)
                {
                    if (Trajectory.Y < 0)
                        trajectoryY = -1 * jumpForce;
                    else
                        trajectoryY -= jumpForce;
                    JumpCnt++;
                }
            }
            else if (isKeyJump)
            {
                if (CurrentState == CharState.Air && Trajectory.Y > 0)
                {
                    CurrentState = CharState.Glide;
                }
            }

            if (CurrentState == CharState.Air || CurrentState == CharState.Glide)
            {
                if (isKeyAttack)
                {
                    CurrentState = CharState.JumpAttack;
                }
                else if(isKeyThrow)
                {
                    var location = new Vector2(Position.X, Position.Y + (trajectoryY * 50)); // Adding something since the kunai spawns before the animation
                    thrownObjects.Add(new Obstacles.Kunai(location, Direction, context)); // null shouldnt be a problem here since the texture should be loaded already
                    CurrentState = CharState.JumpThrow;
                }
                else
                {
                    if (CurrentState == CharState.Glide && !isKeyJump)
                        CurrentState = CharState.Air;

                    var multiplier = CurrentState == CharState.Air ? 1f : 0.00001f;

                    trajectoryY += delta * gravity * multiplier;
                }
            }
            else if (CurrentState == CharState.Grounded)
            {
                if (isKeyAttack)
                {
                    trajectoryX = 0;
                    CurrentState = CharState.GroundAttack;
                }
                else if (isKeyThrow)
                {
                    trajectoryX = 0;                    
                    var location = new Vector2(Position.X, Position.Y);
                    thrownObjects.Add(new Obstacles.Kunai(location, Direction, context)); // null shouldnt be a problem here since the texture should be loaded already
                    CurrentState = CharState.GroundThrow;
                }
                else if (Math.Abs(Trajectory.Y) > 0.5)
                {
                    CurrentState = CharState.Air;
                }
            }

            Trajectory = new Vector2(trajectoryX, trajectoryY);
        }

        private void HandleCollision(float delta)
        {

            var ignore = thrownObjects.Select(elem => (IShape)elem.CollisionBox).ToList();
            var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y,delta, ignore);

            var hits = move.Hits.ToList();

            if (hits.Any((c) => c.Box.HasTag(ItemTypes.Collider) && (c.Normal.Y < 0)))
            {
                if (CurrentState != CharState.Grounded && CurrentState != CharState.GroundAttack && CurrentState != CharState.GroundThrow)
                    CurrentState = CharState.Grounded;
                var mounted = move.Hits.Where(elem => elem.Normal.Y < 0);
                if (mounted.Any())
                {
                    CollisionBox.MountedBody = mounted.First().Box;
                }
                Trajectory = new Vector2(Trajectory.X, delta * 0.001f);                
                JumpCnt = 0;
            }
            else if((hits.Any((c) => (c.Normal.Y < 0)) && Trajectory.Y > 0) || 
                    (hits.Any((c) => (c.Normal.Y > 0)) && Trajectory.Y < 0))
            {
                Trajectory = new Vector2(Trajectory.X, delta * 0.001f);
            }
            else
            {
                Trajectory = new Vector2(Trajectory.X, Trajectory.Y + delta * 0.001f);
                CollisionBox.MountedBody = null;
            }

            if (CurrentState == CharState.GroundAttack || CurrentState == CharState.JumpAttack)
            {
                HandleAttackCollisions();
            }
        }

        protected override void OnDeath()
        {
            if (CurrentState != CharState.Dead)
            {
                CurrentState = CharState.Dead;
                SetAnimation("Dead");
            }
            else if (CurrentAnimation.AnimationName == "Dead" && CurrentAnimation.AnimationState == AnimationState.Finished)
            {
                context.lvl.CollisionWorld.Remove(CollisionBox);
                IsAlive = false;
                context.lvl.SpawnPlayer(null);
            }
        }

        private CollisionResponses OnResolveCollision(ICollision collision)
        {

            if (thrownObjects.Contains(collision.Other.Data))
            {
                return CollisionResponses.None;
            }

            if (collision.Other.HasTag(ItemTypes.Transition) && !context.transitionManager.isTransitioning)
            {
                var item = collision.Other.Data as RectangleItem;
                var lvl = item.CustomProperties["map"].value.ToString();
                var f = Path.ChangeExtension(lvl, ".xml");
                context.transitionManager.TransitionToMap(f);
                return CollisionResponses.Cross;
            }
            if (collision.Other.HasTag(ItemTypes.ScriptTrigger))
            {
                var item = collision.Other.Data as RectangleItem;
                var script = item.CustomProperties["Script"].value.ToString();
                if (!String.IsNullOrEmpty(script))
                {
                    context.lvl.CollisionWorld.Remove((IBox)collision.Other);
                    context.scripter.ExecuteScript(script);
                }
                return CollisionResponses.Cross;
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
                            .Where(collision => collision != null && collision.Box.HasTag(ItemTypes.Enemy))
                            .Select(collision => collision.Box)
                            .Distinct();

            foreach (var collision in collisions)
            {
                ((LivingSpriteObject)collision.Data).DealDamage(this, 50);
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            switch (CurrentState)
            {
                case CharState.GroundAttack:
                    SetAnimation("Attack");
                    break;
                case CharState.JumpAttack:
                    SetAnimation("JumpAttack");
                    break;
                case CharState.GroundThrow:
                    SetAnimation("Throw");
                    break;
                case CharState.JumpThrow:
                    SetAnimation("JumpThrow");
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

        protected override void ManagedDraw(SpriteBatch spriteBatch)
        {
            base.ManagedDraw(spriteBatch);
            foreach (var thrown in thrownObjects)
                thrown.Draw(spriteBatch);
        }

        public enum CharState
        {
            Grounded = 1,
            Air = 2,
            Glide = 4,
            GroundAttack = 8,
            JumpAttack = 16,
            GroundThrow = 32,
            JumpThrow = 64,
            Dead = 128
        };
    }

   
}
