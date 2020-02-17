using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using AuxLib;
using Game1.Screens;
using AuxLib.Input;
using Game1.GameObjects.Sprite;
using Game1.GameObjects.Sprite.Enums;
using Game1.DataContext;
using Game1.GameObjects.Levels;
using tainicom.Aether.Physics2D.Dynamics;
using Game1.GameObjects.Obstacles;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Game1.GameObjects.Characters
{
    [Editable("Char")]
    public class Ninja1 : LivingSpriteObject, IUpdateableItem, IDrawableItem
    {
        private CharState CurrentState;

        public float maxJumpHeight = 5.2f;
        public float minJumpHeight = 1.0f;

        public float timeToJumpApex = .4f;
        float accelerationTimeAirborne = .2f;
        float accelerationTimeGrounded = .1f;
        float moveSpeed = 10.5f;

        public Vector2 wallJumpClimb = new Vector2(7.5f, -16);
        public Vector2 wallJumpOff = new Vector2(8.5f, -7);
        public Vector2 wallLeap = new Vector2(18, -17);

        public float wallSlideSpeed = 2;
        public float wallStickTime = 0.25f;
        public float timeToWallUnstick;

        float gravity;
        
        float maxJumpVelocity;
        float minJumpVelocity;
        
        float velocityXSmoothing;
        bool wallSliding;
        int wallDirX;

        public Vector2 hitBoxSize = new Vector2(110, 200);

        private ColliderTriggerHelper trigger;

        public override Vector2 Size
        {
            get { return hitBoxSize; }
        }

        public override int MaxHealth => 100;

        private readonly List<SpriteObject> thrownObjects = new List<SpriteObject>();

        public Ninja1() { }

        public Ninja1(Vector2 loc, GameContext context) : base(context)
        {
            Transform.Position = loc;
            Visible = true;
            
        }

        public override void LoadContent(ContentManager contentManager)
        {
            LoadFromSheet(@"Content\Characters\Ninja1\Ninja1_Definition.xml", contentManager);
            
        }

        public override void Initialize()
        {
            colBodySize = hitBoxSize;
            CollisionBox = context.lvl.CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(colBodySize.X), ConvertUnits.ToSimUnits(colBodySize.Y), 1, ConvertUnits.ToSimUnits(Transform.Position), 0, BodyType.Kinematic);
            CollisionBox.Tag = this;
            CurrentState = CharState.Jumping;

            gravity = (float)((2 * maxJumpHeight) / Math.Pow(timeToJumpApex, 2));
            maxJumpVelocity = Math.Abs(gravity) * timeToJumpApex;
            minJumpVelocity = (float)Math.Sqrt(2 * Math.Abs(gravity) * minJumpHeight);

            controller = new Controller2D(CollisionBox, Category.Cat1 | Category.Cat2 | Category.Cat4 | Category.Cat5);

            context.camera.SetViewTarget(this);

            trigger = new ColliderTriggerHelper(CollisionBox, Category.Cat9);
            trigger.onEnterZone += Trigger_onEnterZone;

            CurrentAnimation = Animations["Jump"];

            base.Initialize();
        }

        public override void Update(GameTime gameTime,Level lvl)
        {
            CalculateVelocity(gameTime);
            HandleWallSliding(gameTime);

            HandleJumpInput();

            controller.Move(velocity);

            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);

            if (controller.collisions.above || controller.collisions.below)
            {
                if (controller.collisions.slidingDown)
                    velocity.Y -= controller.collisions.slopeNormal.Y * -gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    velocity.Y = 0;
            }

            trigger.Update();

            UpdateAnimation(gameTime);

            base.Update(gameTime,lvl);

            if (!context.lvl.LevelBounds.Contains(context.lvl.player.Transform.Position) && !context.transitionManager.isTransitioning)
                context.lvl.SpawnPlayer(null);

            PlayState.DebugMonitor.Clear();
            PlayState.DebugMonitor.AddDebugValue("Current Animation", CurrentAnimation);
            PlayState.DebugMonitor.AddDebugValue("Current State", CurrentState);
            PlayState.DebugMonitor.AddDebugValue("Position", Transform.Position);
            PlayState.DebugMonitor.AddDebugValue("Velocity", velocity);
            PlayState.DebugMonitor.AddDebugValue("Below", controller.collisions.below);
            PlayState.DebugMonitor.AddDebugValue("Left", controller.collisions.left);
            PlayState.DebugMonitor.AddDebugValue("Right", controller.collisions.right);
            PlayState.DebugMonitor.AddDebugValue("Above", controller.collisions.above);
            PlayState.DebugMonitor.AddDebugValue("Face Dir", controller.collisions.faceDirection);
            PlayState.DebugMonitor.AddDebugValue("Slope Angle", controller.collisions.slopeAngle);
            PlayState.DebugMonitor.AddDebugValue("Slope Normal", controller.collisions.slopeNormal);

            PlayState.DebugMonitor.AddDebugValue("Camera Focussed", context.camera.focussedOnPlayer);
            PlayState.DebugMonitor.AddDebugValue("Camera Position", context.camera.Position);

            PlayState.DebugMonitor.AddDebugValue("Kunai count", thrownObjects.Count());
        }

        private void CalculateVelocity(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyLeft = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
            var keyRight = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

            float targetVelocityX = 0.0f;
            if (keyLeft)
                targetVelocityX = -moveSpeed;
            else if (keyRight)
                targetVelocityX = moveSpeed;

            velocity.X = targetVelocityX;
            //velocity.X = Maths.SmoothDamp(velocity.X, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne, float.MaxValue, delta);
            velocity.Y += gravity * delta;
        }

        private void HandleWallSliding(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyLeft = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
            var keyRight = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

            wallDirX = controller.collisions.left ? -1 : 1;
            wallSliding = false;
            if (((controller.collisions.left && keyLeft) || (controller.collisions.right && keyRight)) && !controller.collisions.below && velocity.Y > 0)
            {
                wallSliding = true;
                if (velocity.Y > wallSlideSpeed)
                    velocity.Y = wallSlideSpeed;

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;
                    if ((wallDirX == 1 && keyLeft || wallDirX == -1 && keyRight) && (!keyLeft && !keyRight))
                        timeToWallUnstick -= delta;
                    else
                        timeToWallUnstick = wallStickTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
        }

        private void HandleJumpInput()
        {
            var directionalInputX = 0;

            var keyLeft = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
            var keyRight = InputHandler.Instance.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

            if (keyLeft)
                directionalInputX = -1;
            if (keyRight)
                directionalInputX = 1;          

            var isKeyJumpPressed = InputHandler.Instance.IsPressed(0, Buttons.A, Keys.Space);
            var isKeyJumpUp = !InputHandler.Instance.IsPressed(0, Buttons.A, Keys.Space);
            var isBKey = InputHandler.Instance.WasPressed(0, Buttons.B, Keys.LeftControl);
            var isXKey = InputHandler.Instance.WasPressed(0, Buttons.X, Keys.LeftShift);
            if (isKeyJumpPressed)
            {
                if (CurrentState == CharState.Jumping)
                {
                    CurrentState = CharState.Glide;
                    
                }

                CurrentState = CharState.Jumping;
                if (wallSliding)
                {
                    if (wallDirX == -1 && keyLeft || wallDirX == 1 && keyRight)
                    {
                        velocity.X = -wallDirX * wallJumpClimb.X;
                        velocity.Y = wallJumpClimb.Y;
                    }
                    else if (!keyLeft && !keyRight)
                    {
                        velocity.X = -wallDirX * wallJumpOff.X;
                        velocity.Y = wallJumpOff.Y;
                    }
                    else
                    {
                        velocity.X = -wallDirX * wallLeap.X;
                        velocity.Y = wallLeap.Y;
                    }
                }
                if (controller.collisions.below)
                {
                    if (controller.collisions.slidingDown)
                    {
                        if (directionalInputX != -Math.Sign(controller.collisions.slopeNormal.X))
                        {
                            velocity.Y = maxJumpVelocity * controller.collisions.slopeNormal.Y;
                            velocity.X = maxJumpVelocity * controller.collisions.slopeNormal.X;
                        }
                    }
                    else
                        velocity.Y = -maxJumpVelocity;
                }
            }
            if (isKeyJumpUp)
            {
                if (velocity.Y < -minJumpVelocity)
                    velocity.Y = -minJumpVelocity;
                
            }

            if (isBKey)
            {
                CurrentState = CharState.GroundThrow;
                var location = new Vector2(Transform.Position.X, Transform.Position.Y); /*+ (trajectoryY * 50)); // Adding something since the kunai spawns before the animation*/
                var kunai = new Kunai(location, controller.collisions.faceDirection, context);
                kunai.LoadContent(context.content);
                kunai.Initialize();
                context.lvl.AddSprite(Guid.NewGuid().ToString(), kunai);
            }

            if (isXKey)
                CurrentState = CharState.GroundAttack;

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

        

        private void HandleAttackCollisions()
        {
            //var width = CollisionBox
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
            //                .Where(collision => collision != null && collision.Box.HasTag(ItemTypes.Enemy))
            //                .Select(collision => collision.Box)
            //                .Distinct();

            //foreach (var collision in collisions)
            //{
            //    ((LivingSpriteObject)collision.Data).DealDamage(this, 50);
            //}
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            if (controller.collisions.below & velocity.X == 0 && CurrentState != CharState.GroundThrow && CurrentState != CharState.GroundAttack)
                CurrentState = CharState.Idle;
            else if (controller.collisions.below & Math.Abs(velocity.X) > 0)
                CurrentState = CharState.Running;

            if (CurrentState == CharState.GroundThrow && CurrentAnimation.AnimationState == AnimationState.Finished)
                CurrentState = CharState.Idle;

            if (CurrentState == CharState.GroundAttack && CurrentAnimation.AnimationState == AnimationState.Finished)
                CurrentState = CharState.Idle;

            if (velocity.X < 0)
                Direction = FaceDirection.Left;
            if (velocity.X > 0)
                Direction = FaceDirection.Right;

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
                case CharState.Idle:                
                    SetAnimation("Idle");
                    break;
                case CharState.Running:
                    SetAnimation("Run");
                    break;
                case CharState.Jumping:
                    SetAnimation("Jump");
                    break;
                case CharState.Glide:
                    SetAnimation("Glide");
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            var effect = InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None;
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(sb, (Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 0.5f, Color, effect);
        }

        private void Trigger_onEnterZone(Fixture body)
        {
            var rect = ((RectangleItem)body.Body.Tag);
            if (rect.CustomProperties.ContainsKey("map"))
            {
                var destMap = rect.CustomProperties["map"].value.ToString();
                var f = Path.ChangeExtension(destMap, ".xml");
                context.transitionManager.TransitionToMap(f);
            }

            if (rect.CustomProperties.ContainsKey("Script"))
            {
                var script = rect.CustomProperties["Script"].value.ToString();
                if (context.scripter.getRunningScripts() == String.Empty)
                {
                    if (context.scripter.hasScriptLoaded(script))
                        context.scripter.ExecuteScript(script);
                }
            }
        }

        public enum CharState
        {
            Idle = 1,
            Running = 2,
            Jumping = 4,
            Glide = 8,
            GroundAttack = 16,
            JumpAttack = 32,
            GroundThrow = 64,
            JumpThrow = 128,
            Dead = 256
        };


    }

    

   
}
