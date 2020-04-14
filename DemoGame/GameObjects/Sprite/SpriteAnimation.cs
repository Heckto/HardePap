using AuxLib.Camera;
using Game1.GameObjects.Sprite.AnimationEffects;
using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Sprite
{
    public class SpriteAnimation
    {
        public string AnimationName { get; set; }
        public List<ISpriteAnimationFrame> Frames { get; set; } = new List<ISpriteAnimationFrame>();

        public bool Loop { get; private set; }
        public AnimationState AnimationState { get; private set; }

        public int currentFrame = 0;
        private float frameTime = 0.05f; // Total time a frame should be visible
        private float frameRunTime = 0f; // Active time spent executing the current frame

        private Vector2 Offset;

        private AnimationEffect currentEffect;
        private Dictionary<AnimationEffect, IAnimationEffect> animationEffects;

        //public SpriteAnimation(ContentManager contentManager, SpriteAnimationConfig config)
        //{
        //    LoadBasicConfigInfo(config);

        //    foreach (var frame in config.Frames)
        //        Frames.Add(new SpriteAnimationFrameTexture(frame.AssetName, contentManager));

        //    AnimationState = AnimationState.Loaded;
        //}

        public SpriteAnimation(SpriteAnimationConfig config, Dictionary<string, SpriteAnimationFrameSpriteSheet> framesDictionary)
        {
            LoadBasicConfigInfo(config);

            foreach (var frame in config.Frames)
                Frames.Add(framesDictionary[frame.AssetName]);

            AnimationState = AnimationState.Loaded;
        }


        private void LoadBasicConfigInfo(SpriteAnimationConfig config)
        {
            AnimationName = config.AnimationName;
            frameTime = config.Frames.First().FrameTime;
            Loop = config.Loop;
            Offset = new Vector2(config.OffsetX, config.OffsetY);

            InitializeEffects();
        }

        private void InitializeEffects()
        {
            animationEffects = new Dictionary<AnimationEffect, IAnimationEffect>();
            animationEffects.Add(AnimationEffect.None, new NoAnimationEffect());
            animationEffects.Add(AnimationEffect.FlashWhite, new FlashWhiteAnimationEffect());
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

            AnimationState = AnimationState.Running;

            if (frameRunTime > frameTime)
            {
                if (currentFrame < Frames.Count() - 1)
                    currentFrame++;
                else if (Loop)
                    currentFrame = 0;
                else
                    AnimationState = AnimationState.Finished;
                frameRunTime = 0.0f;
            }

            animationEffects[currentEffect].Update(gameTime);
        }

        //private void SetAnimationEffect(AnimationEffect animationEffect)
        //{
        //    if (currentEffect == animationEffect || !animationEffects.ContainsKey(animationEffect))
        //        return;

        //    animationEffects[animationEffect].Reset();
        //    currentEffect = animationEffect;
        //}

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float rotation, float scale, Color color, AnimationEffect animationEffect)
        {
            //SetAnimationEffect(animationEffect);

            Frames[currentFrame].Draw(spriteBatch, flipEffects, position, rotation, scale, color, animationEffects[currentEffect]);
        }

        public override string ToString() => $"{AnimationName}-{AnimationState}";
    }
    
    

    
}
