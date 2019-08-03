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
        public List<ISpriteAnimationFrame> Frames { get; set; } = new List<ISpriteAnimationFrame>();

        public bool Loop { get; private set; }
        public AnimationState AnimationState { get; private set; }

        private int currentFrame = 0;
        private float frameTime = 0.05f; // Total time a frame should be visible
        private float frameRunTime = 0f; // Active time spent executing the current frame

        private Vector2 Offset;

        public SpriteAnimation(ContentManager contentManager, SpriteAnimationConfig config)
        {
            AnimationName = config.AnimationName;
            frameTime = config.Frames.First().FrameTime;
            Loop = config.Loop;
            Offset = new Vector2(config.OffsetX, config.OffsetY);

            Frames = new List<ISpriteAnimationFrame>();
            foreach (var frame in config.Frames)
                Frames.Add(new SpriteAnimationFrameTexture(frame.AssetName, contentManager));

            AnimationState = AnimationState.Loaded;
        }

        public SpriteAnimation(SpriteAnimationConfig config, Dictionary<string, SpriteAnimationFrameSpriteSheet> framesDictionary)
        {
            AnimationName = config.AnimationName;
            frameTime = config.Frames.First().FrameTime;
            Loop = config.Loop;
            Offset = new Vector2(config.OffsetX, config.OffsetY);

            Frames = new List<ISpriteAnimationFrame>();
            foreach (var frame in config.Frames)
                Frames.Add(framesDictionary[frame.AssetName]);

            AnimationState = AnimationState.Loaded;
        }

        private Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        private Texture2D LoadTexture(ContentManager cm, string assetName)
        {
            if (loadedTextures.ContainsKey(assetName))
                return loadedTextures[assetName];
            else
                return cm.Load<Texture2D>(assetName);
        }

        public void Reset()
        {
            currentFrame = 0;
            AnimationState = AnimationState.Loaded;
        }

        public void Update(GameTime gameTime)
        {
            var et = (float)gameTime.ElapsedGameTime.TotalSeconds;
            frameRunTime += et;

            if (frameRunTime > frameTime)
            {
                AnimationState = AnimationState.Running;

                if (currentFrame < Frames.Count() - 1)
                    currentFrame++;
                else if (Loop)
                    currentFrame = 0;
                else
                    AnimationState = AnimationState.Finished;
                frameRunTime = 0.0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float scale, Color color)
        {
            Frames[currentFrame].Draw(spriteBatch, flipEffects, position, scale, color, Offset);
        }

        public override string ToString() => $"{AnimationName}-{AnimationState}";
    }
    
    public enum AnimationState
    { None, Loaded, Running, Finished}
}
