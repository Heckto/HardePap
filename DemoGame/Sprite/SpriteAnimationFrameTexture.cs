using AuxLib.Camera;
using Game1.Sprite.AnimationEffects;
using Game1.Sprite.Enums;
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
    class SpriteAnimationFrameTexture : ISpriteAnimationFrame
    {
        public Texture2D Texture { get; set; }

        public Vector2 Size => new Vector2(Texture.Width, Texture.Height);
        public SpriteAnimationFrameTexture(string location, ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(location);
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float rotation, float scale, Color color, Vector2 Offset, IAnimationEffect animationEffect)
        {
            var texture = Texture;
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            var actualX = flipEffects == SpriteEffects.FlipHorizontally ? position.X - Offset.X : position.X + Offset.X;
            var actualY = flipEffects == SpriteEffects.FlipVertically ? position.Y - Offset.Y : position.Y + Offset.Y; // TODO: Test this, no vertical flip yet

            var actualPosition = new Vector2(actualX, actualY);
            animationEffect.Draw(spriteBatch, texture, actualPosition, new Rectangle(0, 0, texture.Width, texture.Height), color, rotation, origin, scale, flipEffects, 1.0f);
        }
    }
}
