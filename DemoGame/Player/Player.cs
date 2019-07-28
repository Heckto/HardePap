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

        private KeyboardState p_state;
        public MoveableBody playerCol;

        public string[] animation_name = { "Idle", "Run", "Attack", "Jump" };

        public delegate void onTransitionDelegate(Player sender, string level);
        public event onTransitionDelegate onTransition;

        public override Vector2 Position { get { return new Vector2(playerCol.X, playerCol.Y) + 0.5f * scale * hitBoxSize; } }

        public Player(Vector2 loc, World world)
        {
            colBodySize = scale * hitBoxSize;
            playerCol = (MoveableBody)world.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            
            (playerCol as IBox).AddTags(ItemTypes.Player);

            //PlayState.DebugMonitor.AddDebugValue(this,"current_animation");
            PlayState.DebugMonitor.AddDebugValue(this,"trajectory");
        }

        public override void LoadContent(ContentManager cm)
        {
            LoadFromFile(cm, @"Content\PlayerSprite.xml");

            CurrentAnimation = Animations["Jump"];
        }

        public void setAnimation(string name)
        {
            if (Animations.ContainsKey(name))
                CurrentAnimation = Animations[name];
        }

        public override void Update(GameTime gameTime, IInputHandler Input)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            KeyboardState kstate;
            kstate = Keyboard.GetState();

            #region Update Location By Trajectory
            
            var keyLeft = Input.IsPressed(0,Buttons.LeftThumbstickLeft,Keys.Left);
            var keyRight = Input.IsPressed(0, Buttons.LeftThumbstickRight, Keys.Right);
            var keyJump = Input.IsPressed(0,Buttons.A,Keys.Space);
            #endregion

            trajectory.X = 0;
            // kijk of valt ???
            //trajectory.Y += delta * 0.001f;
            
            
            #region Key input            
            if (keyLeft )
            {
                if (mCurrentState == CharState.Grounded)
                    setAnimation("Run");

                if (trajectory.X > 0)
                    trajectory.X = 0;
                else
                    
                    trajectory.X = acc * friction * delta;
                Direction = FaceDirection.Left;
            }
            else if (keyRight)
            {
                if (mCurrentState == CharState.Grounded)
                    setAnimation("Run");
                if (trajectory.X < 0)
                    trajectory.X = 0;
                else
                {
                    trajectory.X = -acc * friction * delta;

                }
                Direction = FaceDirection.Right;
            }
            else if (!keyLeft && !keyRight && mCurrentState == CharState.Grounded)
            {
                setAnimation("Idle");
            }
            if (keyJump && p_state.IsKeyUp(Keys.Space) && mCurrentState == CharState.Grounded)
            {
                setAnimation("Jump");
                trajectory.Y -= jumpForce;
                mCurrentState = CharState.Air;
                playerCol.Grounded = false;
                JumpCnt++;
            }
            else if (keyJump && p_state.IsKeyUp(Keys.Space) && mCurrentState == CharState.Air && trajectory.Y >= 0 && JumpCnt == 1 && JumpCnt < MaxJumpCount)
            {
                setAnimation("Jump");
                trajectory.Y -= jumpForce;
                JumpCnt++;
            }

            if (mCurrentState == CharState.Air)
            {
                trajectory.Y += delta * gravity;
            }
            #endregion


            #region Collision
            var move = (playerCol).Move(playerCol.X + delta * trajectory.X, playerCol.Y + delta * trajectory.Y, (collision) =>
            {
                if (collision.Other.HasTag(ItemTypes.Transition))
                {
                    var item = collision.Other.Data as RectangleItem;
                    var lvl = item.CustomProperties["map"].value.ToString();
                    var f = Path.ChangeExtension(lvl, ".xml");
                    onTransition?.Invoke(this, f);
                    return CollisionResponses.Slide;
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
                mCurrentState = CharState.Grounded;
                playerCol.Grounded = true;
                trajectory.Y = delta * 0.001f;
                JumpCnt = 0;
            }
            else
            {
                trajectory.Y += delta * 0.001f;
                playerCol.Grounded = false;
                setAnimation("Jump");
            }

            #endregion
            CurrentAnimation.Update(gameTime);
            p_state = kstate;
        }      
    }


    public enum CharState
    {
        Grounded = 0,
        Air = 1
    };
}
