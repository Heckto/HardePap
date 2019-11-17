using AuxLib.CollisionDetection;
using Game1.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Game1.DataContext;

namespace Game1.Sprite
{
    public abstract class SpriteObject
    {
        public virtual bool IsAlive { get; protected set; }

        protected float scale = 0.5f;
        
        //public CharState CurrentState { get; protected set; } = CharState.Air;
        public Vector2 Trajectory { get; set; } = Vector2.Zero;
        

        public MoveableBody CollisionBox { get; set; }

        protected GameContext context { get; set; }

        protected Color Color { get; set; }

        protected Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();
        protected SpriteAnimation CurrentAnimation;

        private FaceDirection dir = FaceDirection.Right;
        public FaceDirection Direction
        {
            get { return dir; }
            set
            {
                if (dir != value)
                    dir = value;
            }
        }

        public float Rotation { get; set; }

        public virtual Vector2 Position { get; protected set; }
        protected Vector2 colBodySize;

        public SpriteObject(GameContext context)
        {
            LoadContent();
            this.context = context;
        }

        public virtual void LoadFromFile(ContentManager contentManager, string fileLocation)
        {
            var config = SpriteConfig.Deserialize(fileLocation);

            Color = new Color(config.ColorR, config.ColorG, config.ColorB, config.ColorA);

            foreach(var animation in config.Animations)
            {
                Animations.Add(animation.AnimationName, new SpriteAnimation(contentManager, animation));
            }
        }

        public virtual void LoadFromSheet(string fileLocation)
        {
            var config = SpriteConfig.Deserialize(fileLocation);

            Color = new Color(config.ColorR, config.ColorG, config.ColorB, config.ColorA);

            var sheetDef = config.SpritesheetDefinitionFile;
            var frameDictionary = SpriteAnimationFrameSpriteSheet.FromDefinitionFile(sheetDef, config.SpriteSheetScale);

            foreach (var animation in config.Animations)
            {
                Animations.Add(animation.AnimationName, new SpriteAnimation(animation, frameDictionary));
            }
        }

        public virtual void ToFile(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public abstract void LoadContent();

        public virtual void Update(GameTime gameTime)
        {
            CurrentAnimation.Update(gameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
            => Draw(spriteBatch, AnimationEffect.None);

        public virtual void Draw(SpriteBatch spriteBatch, AnimationEffect animationEffect)
        {
            var flip = (Direction == FaceDirection.Left);

            if(CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Position, Rotation, scale, Color, animationEffect);
        }

        public void SetAnimation(string name)
        {
            if (Animations.ContainsKey(name))
            {
                var animation = Animations[name];
                if (CurrentAnimation != animation)
                {
                    animation.Reset();
                    CurrentAnimation = animation;
                }
            }
            //else
            //{
            //    throw new InvalidOperationException($"Animation {name} not found");
            //}
        }

        public enum FaceDirection
        {
            Left,
            Right
        }
    }
}
