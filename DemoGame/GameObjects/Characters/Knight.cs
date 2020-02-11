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
using Game1.GameObjects.Sprite;
using Game1.GameObjects.Levels;
using Game1.GameObjects.Sprite.Enums;
using Game1.DataContext;

namespace Game1.GameObjects.Characters
{
    public class Knight : LivingSpriteObject
    {
        private CharState CurrentState;

        private int JumpCnt = 0;
        private int MaxJumpCount = 2;

        private const float acc = -45f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;        

        private Vector2 hitBoxSize = new Vector2(220, 400);


        public override Vector2 Size
        {
            get { return hitBoxSize; }
        }
        //public override Vector2 Position => ConvertUnits.ToDisplayUnits(CollisionBox.Position) + 0.5f * scale * hitBoxSize;

        public override int MaxHealth => 100;

        private readonly List<SpriteObject> thrownObjects = new List<SpriteObject>();

        public Knight(Vector2 loc, GameContext context) : base(context)
        {
            colBodySize = hitBoxSize;
           
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\Characters\Knight\Knight_Definition.xml");
            CurrentAnimation = Animations["Jump"];
        }

        public override void Update(GameTime gameTime,Level lvl)
        {
            
            base.Update(gameTime,lvl);

            if (!context.lvl.LevelBounds.Contains(context.lvl.player.Transform.Position) && !context.transitionManager.isTransitioning)
                context.lvl.SpawnPlayer(null);

            HandleCollision(gameTime);

            UpdateAnimation(gameTime);
        }

        //public override void ManagedUpdate(GameTime gameTime)
        //{
        //    var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            
        //    HandleCollision(delta);
        //    UpdateAnimation(gameTime);

        //    var deadObjects = thrownObjects.Where(x => !x.IsAlive).ToList();
        //    foreach(var deadObject in deadObjects)
        //    {
        //        thrownObjects.Remove(deadObject);
        //    }

        //    foreach (var thrown in thrownObjects)
        //    {
        //        thrown.Update(gameTime);
        //    }
        //}

        

        private void HandleCollision(GameTime gameTime)
        {

            //var ignore = thrownObjects.Select(elem => (IShape)elem.CollisionBox).ToList();
            //var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y,delta, ignore);

            //var hits = move.Hits.ToList();

            //if (hits.Any((c) => c.Box.HasTag(ItemTypes.Collider) && (c.Normal.Y < 0)))
            //{
            //    if (CurrentState != CharState.Grounded && CurrentState != CharState.GroundAttack && CurrentState != CharState.GroundThrow)
            //        CurrentState = CharState.Grounded;
            //    var mounted = move.Hits.Where(elem => elem.Normal.Y < 0);
            //    if (mounted.Any())
            //    {
            //        CollisionBox.MountedBody = mounted.First().Box;
            //    }
            //    Trajectory = new Vector2(Trajectory.X, delta * 0.001f);                
            //    JumpCnt = 0;
            //}
            //else if((hits.Any((c) => (c.Normal.Y < 0)) && Trajectory.Y > 0) || 
            //        (hits.Any((c) => (c.Normal.Y > 0)) && Trajectory.Y < 0))
            //{
            //    Trajectory = new Vector2(Trajectory.X, delta * 0.001f);
            //}
            //else
            //{
            //    Trajectory = new Vector2(Trajectory.X, Trajectory.Y + delta * 0.001f);
            //    CollisionBox.MountedBody = null;
            //}

            //if (CurrentState == CharState.GroundAttack || CurrentState == CharState.JumpAttack)
            //{
            //    HandleAttackCollisions();
            //}
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
                    //if (Trajectory.X == 0)
                        //SetAnimation("Idle");
                    //else
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

        //protected override void ManagedDraw(SpriteBatch spriteBatch)
        //{
        //    base.ManagedDraw(spriteBatch);
        //    foreach (var thrown in thrownObjects)
        //        thrown.Draw(spriteBatch);
        //}

        public enum CharState
        {
            Grounded = 0x01,
            Air = 0x02,
            Glide = 0x04,
            GroundAttack = 0x08,
            JumpAttack = 0x10,
            GroundThrow = 0x20,
            JumpThrow = 0x40
        };
    }


}
