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

namespace Game1
{
    public class Player
    {
        public const float acc = -30f;
        public const float gravity = 0.001f;
        public const float friction = 0.001f;

        public CharState mCurrentState = CharState.Air;
        Vector2 trajectory = Vector2.Zero;

        public string[] animation_name = { "Idle", "Run", "Attack", "Jump" };
        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        private string current_animation;
        private KeyboardState p_state;
        public IBox playerCol;

        private FaceDirection dir = FaceDirection.Right;

        public Player(Vector2 loc, World world)
        {
            current_animation = "Jump";
            playerCol = world.CreateRectangle(loc.X, loc.Y, 110, 200);
            (playerCol as IBox).AddTags(CollisionTag.Player);

            Game1.PartyGame.DebugMonitor.AddDebugValue(this,"current_animation");
            Game1.PartyGame.DebugMonitor.AddDebugValue(this,"trajectory");
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

        

        public void Update(GameTime gameTime,Camera camera)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            KeyboardState kstate;
            kstate = Keyboard.GetState();

            #region Update Location By Trajectory
            var keyLeft = kstate.IsKeyDown(Keys.Left);
            var keyRight = kstate.IsKeyDown(Keys.Right);
            var keyJump = kstate.IsKeyDown(Keys.Space);
            #endregion

            trajectory.X = 0;
            // kijk of valt ???
            //trajectory.Y += delta * 0.001f;
            
            
            #region Key input            
            if (keyLeft)
            {
                setAnimation("Run");

                if (trajectory.X > 0)
                    trajectory.X = 0;
                else
                    
                    trajectory.X = acc * friction * delta;
                dir = FaceDirection.Left;
            }
            else if (keyRight)
            {
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
                trajectory.Y -= 1.0f;
                mCurrentState = CharState.Air;
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
                if (mCurrentState != CharState.Grounded)
                  setAnimation("Idle");
                mCurrentState = CharState.Grounded;                
                trajectory.Y = 0;
            }
            else
                trajectory.Y += delta * 0.001f;




            #endregion
            animations[current_animation].Update(gameTime);

            camera.Position = new Vector2(playerCol.X-600, playerCol.Y-100);

            p_state = kstate;
        }      

        public void Draw(SpriteBatch spriteBatch,Camera camera)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.matrix); ;
            bool flip = (dir == FaceDirection.Right);

            var tex = animations[current_animation].frames[animations[current_animation].frame_idx];
            var Origin = Vector2.Zero;
            var dRect = new Rectangle((int)playerCol.Bounds.X, (int)playerCol.Bounds.Y, (int)playerCol.Width, (int)playerCol.Height);
            spriteBatch.Draw(tex, dRect, null,Color.White, 0.0f, Origin, (flip ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 1.0f);
          
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
