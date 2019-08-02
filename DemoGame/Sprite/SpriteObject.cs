using AuxLib.Camera;
using AuxLib.Input;
using Game1.CollisionDetection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Game1.Sprite
{
    public abstract class SpriteObject
    {
        protected float scale = 0.5f;
        
        public CharState mCurrentState = CharState.Air;
        protected Vector2 trajectory = Vector2.Zero;
        protected Vector2 hitBoxSize = new Vector2(220, 400);

        public MoveableBody CollisionBox { get; set; }

        protected Color Color { get; set; }

        protected Dictionary<string, SpriteAnimation> Animations = new Dictionary<string, SpriteAnimation>();
        protected SpriteAnimation CurrentAnimation;

        private FaceDirection dir = FaceDirection.Right;
        public FaceDirection Direction
        {
            get { return dir; }
            protected set
            {
                if (dir != value)
                    dir = value;
            }
        }

        public virtual Vector2 Position { get; protected set; }
        protected Vector2 colBodySize;

        public SpriteObject(ContentManager contentManager)
        {
            LoadContent(contentManager);
        }

        public virtual void LoadFromFile(ContentManager cm, string fileLocation)
        {
            var config = SpriteConfig.Deserialize(fileLocation);

            Color = new Color(config.ColorR, config.ColorG, config.ColorB, config.ColorA);

            foreach(var animation in config.Animations)
            {
                Animations.Add(animation.AnimationName, new SpriteAnimation(cm, animation));
            }
        }

        public virtual void ToFile(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public abstract void LoadContent(ContentManager cm);

        public abstract void Update(GameTime gameTime);

        public virtual void Draw(SpriteBatch spriteBatch, BoundedCamera camera)
        {
            var flip = (Direction == FaceDirection.Left);

            if(CurrentAnimation != null)
                CurrentAnimation.Draw(spriteBatch, camera, (flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None), Position, scale, Color);
        }

        public enum FaceDirection
        {
            Left,
            Right
        }
    }
}
