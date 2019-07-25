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

namespace Game1
{
    public class Player
    {
        private float scale = 0.5f;
        public const float acc = -35f;
        public const float gravity = 0.0012f;
        public const float friction = 0.001f;
        public const float jumpForce = 1.0f;
        private int JumpCnt = 0;
        private int MaxJumpCount = 2;

        public CharState mCurrentState = CharState.Air;
        Vector2 trajectory = Vector2.Zero;
        Vector2 hitBoxSize = new Vector2(220, 400);

        public string[] animation_name = { "Idle", "Run", "Attack", "Jump" };
        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        private string current_animation;
        private KeyboardState p_state;
        public MoveableBody playerCol;

        private FaceDirection dir = FaceDirection.Right;

        public Vector2 Position { get { return new Vector2(playerCol.X, playerCol.Y) + 0.5f * scale * hitBoxSize; } }
        private Vector2 colBodySize;

        public Player(Vector2 loc, World world)
        {
            colBodySize = scale * hitBoxSize;
            current_animation = "Jump";
            playerCol = (MoveableBody)world.CreateMoveableBody(loc.X, loc.Y, colBodySize.X, colBodySize.Y);
            
            (playerCol as IBox).AddTags(CollisionTag.Player);

            PlayState.DebugMonitor.AddDebugValue(this,"current_animation");
            PlayState.DebugMonitor.AddDebugValue(this,"trajectory");
        }

        public FaceDirection Direction
        {
            get { return dir; }
            set
            {
                if (dir != value)
                    dir = value;
            }
        }

        public void LoadContent(ContentManager cm)
        {
            for (var idx=0;idx < animation_name.Length;idx++)
            {
                animations.Add(animation_name[idx], new Animation(animation_name[idx],cm));

            }
        }

        public void setAnimation(string name)
        {
            if (animations.ContainsKey(name))
                current_animation = name;
        }

        

        public void Update(GameTime gameTime, IInputHandler Input)
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
                dir = FaceDirection.Left;
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
                dir = FaceDirection.Right;
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
                if (collision.Other.HasTag(CollisionTag.Trigger))
                {
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

            if (move.Hits.Any((c) => c.Box.HasTag(CollisionTag.PolyLine, CollisionTag.StaticBlock) && (c.Normal.Y < 0)))
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
            animations[current_animation].Update(gameTime);
            p_state = kstate;
        }      

        public void Draw(SpriteBatch spriteBatch,BoundedCamera camera)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());
            var flip = (dir == FaceDirection.Right);

            var tex = animations[current_animation].frames[animations[current_animation].frame_idx];
            var Origin = new Vector2(tex.Width / 2,tex.Height /2);
            var ass = scale * colBodySize;
            var dRect = new Rectangle((int)(playerCol.Bounds.X), (int)(playerCol.Bounds.Y), (int)(tex.Width), (int)(tex.Height));
            spriteBatch.Draw(tex, Position, null,Color.White, 0.0f, Origin, scale, (flip ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 1.0f);
          
            spriteBatch.End();
        }
    }

    public class Animation
    {
        public Texture2D[] frames = new Texture2D[10];
        public int frame_idx = 0;
        float frameTime = 0.05f;
        float timeout = 0f;

        public Animation(string name, ContentManager cm)
        {
            for (var idx = 0; idx < 10; idx++)
            {
                var asset_name = "Player/" + name + $"__00{idx}";                
                frames[idx] = cm.Load<Texture2D>(asset_name);
            }
        }

        public void Update(GameTime gameTime)
        {
            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeout += et;

            if (timeout > (float)frameTime)
            {
                frame_idx++;
                if (frame_idx >= frames.Length)
                    frame_idx = 0;
                timeout = 0.0f;
            }

        }
    }

    public enum FaceDirection
    {
        Left,
        Right
    };

    public enum CharState
    {
        Grounded = 0,
        Air = 1
    };
}
