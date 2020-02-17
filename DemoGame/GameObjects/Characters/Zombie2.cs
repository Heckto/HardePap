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

namespace Game1.GameObjects.Characters
{
    [Editable("Char")]
    public class Zombie2 : LivingSpriteObject, IUpdateableItem, IDrawableItem
    {
        private BehaviourState state;

        private const float acc = -45f;
        private const float gravity = 64f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;
        
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
            //colBodySize = hitBoxSize;
            
            //CollisionBox = (MoveableBody)context.lvl.CollisionWorld.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            //CollisionBox.onCollisionResponse += OnResolveCollision;

            //(CollisionBox as IBox).AddTags(ItemTypes.Player);
            //CollisionBox.Data = this;
        }

        public Zombie2() { }

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
            LoadFromSheet(@"Content\Characters\Zombie2\Zombie2_Definition.xml", contentManager);            
        }

        public override void Update(GameTime gameTime, Level lvl)
        {

            base.Update(gameTime, lvl);

            var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (HandleInput)
                HandleKeyInput(delta, InputHandler.Instance);

        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            

            Direction = velocity.X < 0 ? FaceDirection.Left : FaceDirection.Right;

            velocity.Y += gravity * delta;
            velocity.X = 0;
            controller.Move(velocity);

            Transform.Position = ConvertUnits.ToDisplayUnits(CollisionBox.Position);

            if (controller.collisions.below)
                velocity.Y = 0;
        }

        //private CollisionResponses OnResolveCollision(ICollision collision)
        //{

        //    if (thrownObjects.Contains(collision.Other.Data))
        //    {
        //        return CollisionResponses.None;
        //    }

        //    if (collision.Other.HasTag(ItemTypes.Transition) && !context.transitionManager.isTransitioning)
        //    {
        //        var item = collision.Other.Data as RectangleItem;
        //        var lvl = item.CustomProperties["map"].value.ToString();
        //        var f = Path.ChangeExtension(lvl, ".xml");
        //        context.transitionManager.TransitionToMap(f);
        //        return CollisionResponses.Cross;
        //    }
        //    if (collision.Other.HasTag(ItemTypes.ScriptTrigger))
        //    {
        //        var item = collision.Other.Data as RectangleItem;
        //        var script = item.CustomProperties["Script"].value.ToString();
        //        if (!String.IsNullOrEmpty(script))
        //        {
        //            //context.lvl.CollisionWorld.Remove((IBox)collision.Other);
        //            context.scripter.ExecuteScript(script);
        //        }
        //        return CollisionResponses.Cross;
        //    }
        //    if (collision.Hit.Normal.Y < 0)
        //    {
        //        return CollisionResponses.Slide;
        //    }
        //    if (collision.Hit.Normal.Y > 0)
        //    {
        //        Trajectory = new Vector2(Trajectory.X, -Trajectory.Y);
        //        return CollisionResponses.Touch;
        //    }


        //    return CollisionResponses.Slide;
        //}

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
            var effect = InvulnerabilityTimer > 0 ? AnimationEffect.FlashWhite : AnimationEffect.None;
            if (CurrentAnimation != null)
                CurrentAnimation.Draw(sb, (Direction == FaceDirection.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Transform.Position, 0, 0.5f, Color, effect);
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
