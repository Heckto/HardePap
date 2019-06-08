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

namespace Game1
{
    public class Player
    {
        private const int START_POSITION_X = -1200;
        private const int START_POSITION_Y = -425;
        private const int PLAYER_SPEED = 350;
        private const int MOVE_UP = -1;
        private const int MOVE_DOWN = 1;
        private const int MOVE_LEFT = -1;
        private const int MOVE_RIGHT = 1;

        private enum State
        {
            Walking
        }
        State mCurrentState = State.Walking;

        Vector2 mDirection = Vector2.Zero;
        Vector2 mSpeed = Vector2.Zero;

        KeyboardState mPreviousKeyboardState;




        private void UpdateMovement(KeyboardState aCurrentKeyboardState)
        {
            if (mCurrentState == State.Walking)
            {
                mSpeed = Vector2.Zero;
                mDirection = Vector2.Zero;

                if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true)
                {
                    mSpeed.X = PLAYER_SPEED;
                    mDirection.X = MOVE_LEFT;
                }
                else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true)
                {
                    mSpeed.X = PLAYER_SPEED;
                    mDirection.X = MOVE_RIGHT;
                }

                if (aCurrentKeyboardState.IsKeyDown(Keys.Up) == true)
                {
                    mSpeed.Y = PLAYER_SPEED;
                    mDirection.Y = MOVE_UP;
                }
                else if (aCurrentKeyboardState.IsKeyDown(Keys.Down) == true)
                {
                    mSpeed.Y = PLAYER_SPEED;
                    mDirection.Y = MOVE_DOWN;
                }
            }
        }

        public Vector2 location { get; set; }

        public string[] animation_name = { "Idle", "Run", "Attack" };
        Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
        private string current_animation;

        private FaceDirection dir = FaceDirection.Right;

        public Player(Vector2 loc)
        {
            location = loc;
            current_animation = "Idle";
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
            location = new Vector2(START_POSITION_X, START_POSITION_Y);

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

        public void Update(GameTime gameTime)
        {
            KeyboardState kstate;
            kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.R))
                setAnimation("Run");
            if (kstate.IsKeyDown(Keys.T))
                setAnimation("Idle");
            if (kstate.IsKeyDown(Keys.Y))
                setAnimation("Attack");
            if (kstate.IsKeyDown(Keys.Left))
                dir = FaceDirection.Left;
            if (kstate.IsKeyDown(Keys.Right))
                dir = FaceDirection.Right; ;

            KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            UpdateMovement(aCurrentKeyboardState);

            mPreviousKeyboardState = aCurrentKeyboardState;
            location += mDirection * mSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            animations[current_animation].Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch,Camera camera)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.matrix); ;
            bool flip = (dir == FaceDirection.Right);

            var tex = animations[current_animation].frames[animations[current_animation].frame_idx];            
            Rectangle dRect = new Rectangle((int)location.X, (int)location.Y, (int)(0.5* tex.Width), (int)(0.5 * tex.Height));
            //dRect.X += (int)(0.5 * megaTex[texIndex].Width);
            //dRect.Y += (int)(0.5 * megaTex[texIndex].Height);

            //var tex = animations[current_animation].frames[animations[current_animation].frame_idx];
            spriteBatch.Draw(tex, dRect, null, Color.White, 0.0f, new Vector2(tex.Width / 2f, tex.Height / 2f), (flip ? SpriteEffects.None : SpriteEffects.FlipHorizontally), 1.0f);
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
}
