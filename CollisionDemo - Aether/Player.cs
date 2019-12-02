using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AuxLib.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;


namespace Game666
{
    public class ControlledEntity
    {
        public Body collider;
        public Vector2 colliderSize;
        protected World world;
    }

    public class Player : ControlledEntity
    {
        public float maxJumpHeight = 3.2f;
        public float minJumpHeight = 1.0f;

        public float timeToJumpApex = .4f;
        float accelerationTimeAirborne = .2f;
        float accelerationTimeGrounded = .1f;
        float moveSpeed = 6.5f;

        public Vector2 wallJumpClimb = new Vector2(7.5f, -16);
        public Vector2 wallJumpOff = new Vector2(8.5f, -7);
        public Vector2 wallLeap = new Vector2(18, -17);

        public float wallSlideSpeed = 2;
        public float wallStickTime = 0.25f;
        public float timeToWallUnstick;

        float gravity;
        float maxJumpVelocity;
        float minJumpVelocity;
        Vector2 velocity;
        float velocityXSmoothing;

        public Controller2D controller;

        bool wallSliding;
        int wallDirX;

        public Player(Vector2 loc, Vector2 size, World world)
        {
            colliderSize = size;
            collider = world.CreateRectangle(ConvertUnits.ToSimUnits(colliderSize.X), ConvertUnits.ToSimUnits(colliderSize.Y), 1, ConvertUnits.ToSimUnits(loc), 0, BodyType.Kinematic);
            collider.SetCollisionCategories(Category.Cat1);

            collider.Tag = this;
            controller = new Controller2D(collider, world, Category.Cat1 | Category.Cat2 | Category.Cat4 | Category.Cat5);
            this.world = world;
            gravity = (float)((2 * maxJumpHeight) / Math.Pow(timeToJumpApex, 2));

            maxJumpVelocity = Math.Abs(gravity) * timeToJumpApex;
            minJumpVelocity = (float)Math.Sqrt(2 * Math.Abs(gravity) * minJumpHeight);
        }

        public void Update(GameTime gameTime)
        {
            var keyState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(0);
            var keyDown = keyState.IsKeyDown(Keys.Down) || gamePadState.ThumbSticks.Left.Y < 0;

            

            CalculateVelocity(gameTime);
            HandleWallSliding(gameTime);

            HandleJumpInput();

            controller.Move(velocity, keyDown);

            if (controller.collisions.above || controller.collisions.below)
            {
                if (controller.collisions.slidingDown)
                    velocity.Y -= controller.collisions.slopeNormal.Y * -gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    velocity.Y = 0;
            }

        }

        public void Kill()
        {
            world.Remove(collider);
        }

        private void CalculateVelocity(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(0);

            var keyLeft = keyState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Left.X < 0;
            var keyRight = keyState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Left.X > 0;

            float targetVelocityX = 0.0f;
            if (keyLeft)
                targetVelocityX = -moveSpeed;
            else if (keyRight)
                targetVelocityX = moveSpeed;

            velocity.X = SmoothDamp(velocity.X, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne, float.MaxValue, delta);
            velocity.Y += gravity * delta;
        }

        private void HandleWallSliding(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var keyState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(0);

            var keyLeft = keyState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Left.X < 0;
            var keyRight = keyState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Left.X > 0;

            wallDirX = controller.collisions.left ? -1 : 1;
            wallSliding = false;
            if (((controller.collisions.left && keyLeft)|| (controller.collisions.right && keyRight)) && !controller.collisions.below && velocity.Y > 0)
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
            var keyState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(0);

            var directionalInputX = 0;

            var keyLeft = keyState.IsKeyDown(Keys.Left) || gamePadState.ThumbSticks.Left.X < 0;
            if (keyLeft)
                directionalInputX = -1;
            var keyRight = keyState.IsKeyDown(Keys.Right) || gamePadState.ThumbSticks.Left.X > 0;
            if (keyRight)
                directionalInputX = 1;

            var keyDown = keyState.IsKeyDown(Keys.Down) || gamePadState.ThumbSticks.Left.Y > 0;

            var isKeyJumpPressed = keyState.IsKeyDown(Keys.Up) /*|| gamePadState.IsButtonDown(Buttons.A)*/;
            var isKeyJumpUp = keyState.IsKeyUp(Keys.Up) /*|| gamePadState.IsButtonUp(Buttons.A)*/;

            if (isKeyJumpPressed)
            {
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
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            smoothTime = Math.Max(0.0001f, smoothTime);
            float num = 2f / smoothTime;
            float num2 = num * deltaTime;
            float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
            float num4 = current - target;
            float num5 = target;
            float num6 = maxSpeed * smoothTime;
            num4 = MathHelper.Clamp(num4, -num6, num6);
            target = current - num4;
            float num7 = (currentVelocity + num * num4) * deltaTime;
            currentVelocity = (currentVelocity - num * num7) * num3;
            float num8 = target + (num4 + num7) * num3;
            if (num5 - current > 0f == num8 > num5)
            {
                num8 = num5;
                currentVelocity = (num8 - num5) / deltaTime;
            }
            return num8;
        }
    }
}
