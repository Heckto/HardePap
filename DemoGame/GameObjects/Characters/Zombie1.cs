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

namespace Game1.GameObjects.Characters
{
    [Editable("Char")]
    public class Zombie1 : LivingSpriteObject, IUpdateableItem, IDrawableItem
    {
        private const float gravity = 64f;

        private BehaviourState state;
        private Vector2 hitBoxSize = new Vector2(110, 200);

        public override int MaxHealth => 150;

        private Vector2 movingTarget;
        private float IdleTimeout = 0;

        public override Vector2 Size
        {
            get { return hitBoxSize; }
        }
        public Zombie1(Vector2 loc, GameContext context) : base(context)
        {
            Visible = true;
            Transform.Position = loc;

            //Initialize();
        }

        public Zombie1() {}

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
        }

        public override void LoadContent(ContentManager contentManager)
        {
            LoadFromSheet(@"Content\Characters\Zombie1\Zombie1_Definition.xml", contentManager);
            
        }

        public override void Update(GameTime gameTime, Level lvl)
        {            
            base.Update(gameTime,lvl);

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
                    IdleTimeout += delta;
                    //if (targetDist < 500)
                    //{
                    //    state = BehaviourState.Chasing;
                    //    SetAnimation("Walk");
                    //}
                    //else
                    if (IdleTimeout > 5)
                    {
                        var movementLength = Rand.GetRandomInt(-500, 500);
                        //var movementDir = Rand.GetRandomInt(0, 2);
                        movingTarget = new Vector2(Transform.Position.X + movementLength, 0);
                        state = BehaviourState.Walking;
                        SetAnimation("Walk");
                        IdleTimeout = 0;
                    }
                    break;
                case BehaviourState.Walking:
                    var tv = movingTarget - Transform.Position;
                    tv.Normalize();
                    velocity.X = tv.X * delta * 1000;

                    if (Math.Abs(movingTarget.X - Transform.Position.X) <= 1)
                    {
                        state = BehaviourState.Idle;
                        SetAnimation("Idle");
                        //Console.WriteLine(Transform.Position + " - " + movingTarget);
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
            var effect = InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None;
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(sb, (Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 0.5f, Color, effect);
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
