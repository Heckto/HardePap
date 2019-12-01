using AuxLib;
using AuxLib.CollisionDetection;
using AuxLib.CollisionDetection.Responses;
using Game1.Sprite;
using Game1.Sprite.Enums;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Game1.DataContext;
using Game1.Levels;

namespace Game1.Enemies
{
    class Enemy1 : LivingSpriteObject
    {
        private CharState CurrentState;

        private const float acc = -25f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 0.8f;

        private Vector2 hitBoxSize = new Vector2(220, 400);

        public override Vector2 Position => ConvertUnits.ToDisplayUnits(CollisionBox.Position) + 0.5f * scale * hitBoxSize;

        public override int MaxHealth => 100;

        private LivingSpriteObject player1;

        public Enemy1(Vector2 loc, GameContext context, LivingSpriteObject player1) : base(context)
        {
            
            //colBodySize = scale * hitBoxSize;
            //CollisionBox = (MoveableBody)context.lvl.CollisionWorld.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            //CollisionBox.onCollisionResponse += OnResolveCollision;
            //(CollisionBox as IBox).AddTags(ItemTypes.Enemy);
            //CollisionBox.Data = this;

            //this.player1 = player1;
        }

        public override void LoadContent()
        {
            LoadFromSheet(@"Content\EnemySprite.xml");

            CurrentAnimation = Animations["Jump"];
        }

        public override void ManagedUpdate(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;          
            HandleCollision(delta);
            UpdateAnimation(gameTime);
        }

    

        private void HandleCollision(float delta)
        {
            //var move = CollisionBox.Move(CollisionBox.X + delta * Trajectory.X, CollisionBox.Y + delta * Trajectory.Y,delta);

            //var hits = move.Hits.ToList();
            //if (hits.Any((c) => c.Box.HasTag(ItemTypes.Collider) && (c.Normal.Y < 0)))
            //{
            //    if (CurrentState != CharState.Grounded && CurrentState != CharState.GroundAttack)
            //        CurrentState = CharState.Grounded;

            //    var mounted = move.Hits.Where(elem => elem.Normal.Y < 0);
            //    if (mounted.Any())
            //    {
            //        CollisionBox.MountedBody = mounted.First().Box;
            //    }
            //    Trajectory = new Vector2(Trajectory.X, delta * 0.001f);
            //}
            //else if ((hits.Any((c) => (c.Normal.Y < 0)) && Trajectory.Y > 0) || 
            //        (hits.Any((c) => (c.Normal.Y > 0)) && Trajectory.Y < 0))
            //{
            //    Trajectory = new Vector2(Trajectory.X, 0);
            //}
            //else
            //{
            //    Trajectory = new Vector2(Trajectory.X, Trajectory.Y + delta * 0.001f);
            //    CollisionBox.MountedBody = null;
            //}
        }

       

            private void UpdateAnimation(GameTime gameTime)
        {
            switch (CurrentState)
            {
                case CharState.GroundAttack:
                    SetAnimation("Attack");
                    break;
                case CharState.Grounded:
                    //if (Trajectory.X == 0)
                        //SetAnimation("Idle");
                    //else
                      //  SetAnimation("Run");
                    break;
                case CharState.Air:
                    SetAnimation("Jump");
                    break;
                case CharState.Glide:
                    SetAnimation("Glide");
                    break;
            }
        }

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
