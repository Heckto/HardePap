using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using AuxLib;
using Game1.Screens;
using AuxLib.Input;
using Game1.GameObjects.Sprite;
using Game1.GameObjects.Levels;
using Game1.GameObjects.Sprite.Enums;
using Game1.DataContext;
using tainicom.Aether.Physics2D.Dynamics;
using Game1.Rendering;
using AuxLib.RandomGeneration;
using Game1.GameObjects.Graphics.Effects;

namespace Game1.GameObjects.Characters
{
    [Editable("Char")]
    public class Zombie2 : LivingSpriteObject, IUpdateableItem, IDrawableItem
    {
        private const float gravity = 64f;
        private BehaviourState state;

        private const float acc = -45f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;

        private int movementSpeed = 0;
        private int movementDir = 0;

        private float IdleTimeout = 0;
        private float IdleTime = 0;
        private float WalkingTimeout = 0;
        private float WalkingTime = 0;

        private RenderMaterial<SpriteBlinkEffect> blinkMaterial;

        private Color blinkColor;

        private Vector2 hitBoxSize = new Vector2(110, 200);
        public override Vector2 Size
        {
            get { return hitBoxSize; }
        }

        public override int MaxHealth => 100;

        public Zombie2(Vector2 loc, GameContext context) : base(context)
        {
            Visible = true;
            Transform.Position = loc;

            Initialize();
            blinkColor = Rand.GetRandomColor();
        }

        public Zombie2() {
            blinkColor = Rand.GetNormalizedRandomColor();
        }

        public override void Initialize()
        {
            colBodySize = hitBoxSize;
            CollisionBox = context.lvl.CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(colBodySize.X), ConvertUnits.ToSimUnits(colBodySize.Y), 1, ConvertUnits.ToSimUnits(Transform.Position), 0, BodyType.Kinematic);
            CollisionBox.Tag = this;
            CollisionBox.SetCollisionCategories(Category.Cat20);

            controller = new Controller2D(CollisionBox, Category.Cat2 | Category.Cat4 | Category.Cat5);
            state = BehaviourState.Idle;

            //CurrentAnimation = Animations["Idle"];
            Material = RenderMaterial.DefaultMaterial;
            base.Initialize();
        }

        public override void LoadContent(ContentManager contentManager)
        {
            LoadFromSheet(@"Content\Characters\Zombie2\Zombie2_Definition.xml", contentManager);
            CurrentAnimation = Animations["Idle"];

            var testEffect = new SpriteBlinkEffect(contentManager.Load<Effect>("Effects/Test"));

            blinkMaterial = new RenderMaterial<SpriteBlinkEffect>(testEffect,BlendState.AlphaBlend);
            blinkMaterial.Effect.BlinkColor = new Color((float)(blinkColor.R / 255.0), (float)(blinkColor.G / 255.0), (float)(blinkColor.B / 255.0), 1);
        }

        public override void Update(GameTime gameTime, Level lvl)
        {

            base.Update(gameTime, lvl);
            blinkMaterial.Effect.Parameters["_time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (HandleInput)
                HandleKeyInput(delta, InputHandler.Instance);

        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            var targetDist = Math.Abs(context.lvl.player.Transform.Position.X - Transform.Position.X);
            switch (state)
            {
                case BehaviourState.Idle:
                    IdleTime += delta;
                    velocity.X = 0;
                    if (IdleTime > IdleTimeout)
                    {
                        movementDir = Rand.GetRandomInt(1, 3) == 1 ? -1 : 1;
                        movementSpeed = Rand.GetRandomInt(3, 5);
                        state = BehaviourState.Walking;
                        SetAnimation("Walk");
                        WalkingTimeout = Rand.GetRandomInt(2, 5);
                        IdleTime = 0;
                    }
                    break;
                case BehaviourState.Walking:
                    WalkingTime += delta;
                    velocity.X = movementDir * movementSpeed;

                    if (WalkingTime > WalkingTimeout)
                    {
                        state = BehaviourState.Idle;
                        SetAnimation("Idle");
                        IdleTimeout = Rand.GetRandomInt(1, 5);
                        WalkingTime = 0;
                    }
                    break;
                case BehaviourState.Chasing:
                    if (targetDist > 500)
                        state = BehaviourState.Idle;
                    var v = context.lvl.player.Transform.Position - Transform.Position;
                    v.Normalize();
                    if (v.X < 0)
                        velocity.X = -Math.Min(Math.Abs(-0.03f * delta), Math.Abs((context.lvl.player.Transform.Position - Transform.Position).X / delta));
                    else
                        velocity.X = Math.Min(0.03f * delta, (context.lvl.player.Transform.Position - Transform.Position).X / delta);
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

            if (controller.collisions.left || controller.collisions.right)
            {
                movementDir *= -1;
            }

            if (controller.collisions.above || controller.collisions.below)
            {
                if (controller.collisions.slidingDown)
                    velocity.Y -= controller.collisions.slopeNormal.Y * -gravity * delta;
                else
                    velocity.Y = 0;
            }

            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);
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
            //switch (CurrentState)
            //{
            //    case CharState.GroundAttack:
            //        SetAnimation("Attack");
            //        break;
            //    case CharState.JumpAttack:
            //        SetAnimation("JumpAttack");
            //        break;
            //    case CharState.GroundThrow:
            //        SetAnimation("Throw");
            //        break;
            //    case CharState.JumpThrow:
            //        SetAnimation("JumpThrow");
            //        break;
            //    case CharState.Grounded:
            //        if (Trajectory.X == 0)
            //            SetAnimation("Idle");
            //        else
            //            SetAnimation("Walk");
            //        break;
            //    case CharState.Air:
            //        SetAnimation("Jump");
            //        break;
            //    case CharState.Glide:
            //        SetAnimation("Glide");
            //        break;
            //}
        }

        //protected override void ManagedDraw(SpriteBatch spriteBatch)
        //{
        //    base.ManagedDraw(spriteBatch);
        //    foreach (var thrown in thrownObjects)
        //        thrown.Draw(spriteBatch);
        //}

        protected override void OnDeath()
        {
            if (state != BehaviourState.Dying)
            {
                state = BehaviourState.Dying;
                SetAnimation("Dead");
            }
            else if (CurrentAnimation.AnimationName == "Dead" && CurrentAnimation.AnimationState == AnimationState.Finished)
            {
                IsAlive = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (InvulnerabilityTimer > 0)
            {

                var def = (CurrentAnimation.Frames[CurrentAnimation.currentFrame] as SpriteAnimationFrameSpriteSheet);
                sb.Draw(def.spriteSheet, Transform.Position, def.definition.SrcRectangle, Color.White, 0f, def.definition.Origin, 1, effects: Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth: 1.0f);
            }
            else
            {
                var effect = InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None;
                if (CurrentAnimation != null)
                    CurrentAnimation.Draw(sb, (Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 1f, Color.White, effect);
            }
        }

        public override void DealDamage(SpriteObject sender, int damage)
        {
            if (InvulnerabilityTimer > 0)
                return;

            CurrentHealth -= damage;

            if (CurrentHealth > 0)
            {
                InvulnerabilityTimer = InvulnerabilityTime;
                Material = blinkMaterial;
                //                renderer = typeof(EffectRenderer<SpriteBlinkEffect>);

            }
        }

        public enum BehaviourState
        {
            Idle = 0,
            Walking = 1,
            Chasing = 2,
            Attack = 4,
            Dying = 8
        }
    }


}
