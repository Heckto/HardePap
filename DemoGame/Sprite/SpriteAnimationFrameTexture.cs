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
    class SpriteAnimationFrameTexture : ISpriteAnimationFrame
    {
        public Texture2D Texture { get; set; }

        public SpriteAnimationFrameTexture(string location, ContentManager contentManager)
        {
            Texture = contentManager.Load<Texture2D>(location);
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float scale, Color color, Vector2 Offset)
        {
            var tex = Texture;
            var Origin = new Vector2(tex.Width / 2, tex.Height / 2);

            var actualX = flipEffects == SpriteEffects.FlipHorizontally ? position.X - Offset.X : position.X + Offset.X;
            var actualY = flipEffects == SpriteEffects.FlipVertically ? position.Y - Offset.Y : position.Y + Offset.Y; // TODO: Test this, no vertical flip yet

            var actualPosition = new Vector2(actualX, actualY);
            spriteBatch.Draw(tex, actualPosition, null, color, 0.0f, Origin, scale, flipEffects, 1.0f);
        }
    }
}
