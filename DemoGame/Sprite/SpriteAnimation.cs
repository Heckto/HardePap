using AuxLib.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sprite
{
    public class SpriteAnimation
    {
        public string AnimationName { get; set; }
        public Texture2D[] Frames { get; set; } = new Texture2D[10];
        public int Frame_idx { get; set; } = 0;

        private float frameTime = 0.05f;
        private float timeout = 0f;

        public SpriteAnimation(string name, ContentManager cm)
        {
            AnimationName = name;
            for (var idx = 0; idx < 10; idx++)
            {
                var asset_name = "Player/" + name + $"__00{idx}";
                Frames[idx] = cm.Load<Texture2D>(asset_name);
            }
        }

        public SpriteAnimation(ContentManager cm, SpriteAnimationConfig config)
        {
            AnimationName = config.AnimationName;
            timeout = config.Timeout;
            frameTime = config.Frames.First().FrameTime;

            var frames = new List<Texture2D>();
            foreach (var frame in config.Frames)
                frames.Add(cm.Load<Texture2D>(frame.AssetName));
            Frames = frames.ToArray();
        }

        public void Update(GameTime gameTime)
        {
            float et = (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeout += et;

            if (timeout > (float)frameTime)
            {
                Frame_idx++;
                if (Frame_idx >= Frames.Length)
                    Frame_idx = 0;
                timeout = 0.0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, BoundedCamera camera, SpriteEffects flipEffects, Vector2 position, float scale)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());

            var tex = Frames[Frame_idx];
            var Origin = new Vector2(tex.Width / 2, tex.Height / 2);

            spriteBatch.Draw(tex, position, null, Color.White, 0.0f, Origin, scale, flipEffects, 1.0f);

            spriteBatch.End();
        }
    }
}
