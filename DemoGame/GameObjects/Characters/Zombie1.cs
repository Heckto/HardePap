using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using AuxLib;
using AuxLib.Input;
using Game1.GameObjects.Sprite;
using Game1.GameObjects.Levels;
using Game1.GameObjects.Sprite.Enums;
using Game1.DataContext;
using AuxLib.RandomGeneration;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework.Content;
using Game1.GameObjects.Graphics.Effects;
using Game1.Rendering;

namespace Game1.GameObjects.Characters
{
    [Editable("Char")]
    public class Zombie1 : LivingSpriteObject, IUpdateableItem, IDrawableItem
    {
        private const float gravity = 64f;

        private BehaviourState state;
        private Vector2 hitBoxSize = new Vector2(110, 180);

        public override int MaxHealth => 100;

        private int movementSpeed = 0;
        private int movementDir = 0;

        private float IdleTimeout = 0;
        private float IdleTime = 0;
        private float WalkingTimeout = 0;
        private float WalkingTime = 0;
                
        private RenderMaterial<SpriteBlinkEffect> blinkMaterial;

        private Color blinkColor;

        public override Vector2 Size
        {
            get { return hitBoxSize; }
        }
        public Zombie1(Vector2 loc, GameContext context) : base(context)
        {
            Visible = true;
            Transform.Position = loc;

            blinkColor = Rand.GetRandomColor();
            //Initialize();
        }

        public Zombie1() {
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

            CurrentAnimation = Animations["Idle"];

            base.Initialize();

            Material = RenderMaterial.DefaultMaterial;
        }

        public override void LoadContent(ContentManager contentManager)
        {
            LoadFromSheet(@"Content\Characters\Zombie1\Zombie1_Definition.xml", contentManager);

            var testEffect = new SpriteBlinkEffect(contentManager.Load<Effect>("Effects/Test"));

            blinkMaterial = new RenderMaterial<SpriteBlinkEffect>(testEffect, BlendState.AlphaBlend);
            blinkMaterial.Effect.BlinkColor = new Color((float)(blinkColor.R / 255.0), (float)(blinkColor.G / 255.0), (float)(blinkColor.B / 255.0),1);
            
        }

        public override void Update(GameTime gameTime, Level lvl)
        {            
            base.Update(gameTime,lvl);

            blinkMaterial.Effect.Parameters["_time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (HandleInput)
                HandleKeyInput(delta);
        }

        private void HandleKeyInput(float delta)
        {
            var targetDist = Math.Abs(context.lvl.player.Transform.Position.X - Transform.Position.X);
            switch (state)
            {
                case BehaviourState.Idle:
                    IdleTime += delta;
                    velocity.X = 0;
                    if (IdleTime > IdleTimeout)
                    {                        
                        movementDir = Rand.GetRandomInt(1,3) == 1 ? -1 : 1;
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
            //                .Where(collision => collision != null && collision.Box.HasTag(ItemTypes.Player))
            //                .Select(collision => collision.Box)
            //                .Distinct();

            //foreach (var collision in collisions)
            //{
            //    ((LivingSpriteObject)collision.Data).DealDamage(this, 50);
            //}
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
                IsAlive = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (InvulnerabilityTimer > 0)
            {

                var def = (CurrentAnimation.Frames[CurrentAnimation.currentFrame] as SpriteAnimationFrameSpriteSheet);

                blinkMaterial.Effect.BlinkColor = new Color((float)(blinkColor.R / 255.0), (float)(blinkColor.G / 255.0), (float)(blinkColor.B / 255.0), 1);
                sb.Draw(def.spriteSheet, Transform.Position, def.definition.SrcRectangle, Color.White, 0f,def.definition.Origin, 1,effects: Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth:1.0f);
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
            Walking =1,
            Chasing = 2,
            Attack = 4,
            Dying = 8
        }
    }


}
