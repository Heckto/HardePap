using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Game1.CollisionDetection;
using Game1.CollisionDetection.Responses;
using AuxLib;
using Game1.Screens;
using AuxLib.Input;
using AuxLib.Camera;
using Game1.Sprite;
using System.Xml.Serialization;
using System.Xml;

namespace Game1
{
    public class Player : SpriteObject
    {
        private int JumpCnt = 0;
        private int MaxJumpCount = 2;

        private const float acc = -45f;
        private const float gravity = 0.0012f;
        private const float friction = 0.001f;
        public const float jumpForce = 1.0f;

        public delegate void onTransitionDelegate(Player sender, string level);
        public event onTransitionDelegate onTransition;

        public override Vector2 Position { get { return new Vector2(CollisionBox.X, CollisionBox.Y) + 0.5f * scale * hitBoxSize; } }

        public Player(Vector2 loc, World world, ContentManager cm) : base(cm)
        {
            colBodySize = scale * hitBoxSize;
            CollisionBox = (MoveableBody)world.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);

            (CollisionBox as IBox).AddTags(ItemTypes.Player);

            PlayState.DebugMonitor.AddDebugValue(this, nameof(CurrentAnimation));
            PlayState.DebugMonitor.AddDebugValue(this, nameof(mCurrentState));
            //PlayState.DebugMonitor.AddDebugValue(this, nameof(Position));
            PlayState.DebugMonitor.AddDebugValue(this, nameof(trajectory));
        }

        public override void LoadContent(ContentManager cm)
        {
            LoadFromFile(cm, @"Content\PlayerSprite.xml");

            CurrentAnimation = Animations["Jump"];
            SaveToXml();
        }

        public void SetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
            {
                var animation = Animations[name];
                if(CurrentAnimation != animation)
                {
                    animation.Reset();
                    CurrentAnimation = animation;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // kijk of valt ???
            //trajectory.Y += delta * 0.001f;


            HandleKeyInput(delta, InputHandler.Instance);

            HandleCollision(delta);

            UpdateAnimation(gameTime);
        }

        private void HandleKeyInput(float delta, IInputHandler Input)
        {
            var keyLeft = Input.IsPressed(0, Buttons.LeftThumbstickLeft, Keys.Left);
            var keyRight = Input.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);

            var isKeyJump = Input.IsPressed(0, Buttons.A, Keys.Space);
            var wasKeyJump = Input.WasPressed(0, Buttons.A, Keys.Space);

            var isKeyAttack = Input.IsPressed(0, Buttons.X, Keys.LeftControl);
            var wasKeyAttack = Input.WasPressed(0, Buttons.X, Keys.LeftControl); //Charge Attacks?

            if (mCurrentState == CharState.GroundAttack && CurrentAnimation.AnimationName == "Attack")
            {
                if (CurrentAnimation.AnimationState == AnimationState.Running)
                    return;
                else
                    mCurrentState = CharState.Grounded;
            }
            
            if (keyLeft)
            {
                if (trajectory.X > 0)
                    trajectory.X = 0;
                else
                    trajectory.X = acc * friction * delta;

                Direction = FaceDirection.Left;
            }
            else if (keyRight)
            {
                if (trajectory.X < 0)
                    trajectory.X = 0;
                else
                    trajectory.X = -acc * friction * delta;

                Direction = FaceDirection.Right;
            }
            else if (trajectory.X != 0)
            {
                trajectory.X = 0;
            }

            if(wasKeyJump)
            {
                if (mCurrentState == CharState.Grounded)
                {
                    trajectory.Y -= jumpForce;
                    mCurrentState = CharState.Air;
                    CollisionBox.Grounded = false;
                    JumpCnt++;
                }
                else if (mCurrentState == CharState.Air && JumpCnt > 0 && JumpCnt < MaxJumpCount)
                {
                    if (trajectory.Y < 0)
                        trajectory.Y = -1 * jumpForce;
                    else
                        trajectory.Y -= jumpForce;
                    JumpCnt++;
                }
            }
            else if(isKeyJump)
            {
                if(mCurrentState == CharState.Air && trajectory.Y > 0)
                {
                    mCurrentState = CharState.Glide;
                }
            }

            if (mCurrentState == CharState.Air || mCurrentState == CharState.Glide)
            {
                if (mCurrentState == CharState.Glide && !isKeyJump)
                    mCurrentState = CharState.Air;

                var multiplier = mCurrentState == CharState.Air ? 1f : 0.00001f;

                trajectory.Y += delta * gravity * multiplier;
            }
            else if (mCurrentState == CharState.Grounded && isKeyAttack)
            {
                trajectory.X = 0;
                mCurrentState = CharState.GroundAttack;
            }

        }

        private void HandleCollision(float delta)
        {
            var move = (CollisionBox).Move(CollisionBox.X + delta * trajectory.X, CollisionBox.Y + delta * trajectory.Y, (collision) =>
            {
                if (collision.Other.HasTag(ItemTypes.Transition))
                {
                    var item = collision.Other.Data as RectangleItem;
                    var lvl = item.CustomProperties["map"].value.ToString();
                    var f = Path.ChangeExtension(lvl, ".xml");
                    onTransition?.Invoke(this, f);
                    return CollisionResponses.Cross;
                }
                if (collision.Hit.Normal.Y < 0)
                {
                    return CollisionResponses.Slide;
                }
                if (collision.Hit.Normal.Y > 0)
                {
                    trajectory.Y = -trajectory.Y;
                    return CollisionResponses.Touch;
                }
                return CollisionResponses.Slide;
            });

            if (move.Hits.Any((c) => c.Box.HasTag(ItemTypes.PolyLine, ItemTypes.StaticBlock) && (c.Normal.Y < 0)))
            {
                if(mCurrentState != CharState.Grounded && mCurrentState != CharState.GroundAttack)
                    mCurrentState = CharState.Grounded;
                CollisionBox.Grounded = true;
                trajectory.Y = delta * 0.001f;
                JumpCnt = 0;
            }
            else
            {
                trajectory.Y += delta * 0.001f;
                CollisionBox.Grounded = false;
            }
        }

        private void UpdateAnimation(GameTime gameTime)
        {
            switch (mCurrentState)
            {
                case CharState.GroundAttack:
                    SetAnimation("Attack");
                    break;
                case CharState.Grounded:
                    if (trajectory.X == 0)
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

            CurrentAnimation.Update(gameTime);
        }

        private void SaveToXml()
        {
            var xmlObject = new SpriteConfig();
            xmlObject.SpriteName = "Player";
            foreach (var animation in Animations)
            {
                var xmlAnimation = new SpriteAnimationConfig();

                xmlAnimation.AnimationName = animation.Key;
                xmlAnimation.Loop = true;
                xmlAnimation.OffsetX = 0.0f;
                xmlAnimation.OffsetY = 0.0f;

                for (var idx = 0; idx < 10; idx++)
                {
                    var xmlFrame = new SpriteAnimationFrameConfig();

                    xmlFrame.AssetName = "Player/" + xmlAnimation.AnimationName + $"__00{idx}";
                    xmlFrame.FrameTime = 0.05f;

                    xmlAnimation.Frames.Add(xmlFrame);
                }

                xmlObject.Animations.Add(xmlAnimation);
            }

            xmlObject.Serialize(Path.Combine(Directory.GetCurrentDirectory(), "PlayerSprite.xml"));
        }
    }

    public enum CharState
    {
        Grounded        = 0x01,
        Air             = 0x02,
        Glide           = 0x04,
        GroundAttack    = 0x08,
    };
}
