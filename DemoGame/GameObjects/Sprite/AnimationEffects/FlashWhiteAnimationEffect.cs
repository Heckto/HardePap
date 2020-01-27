using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.GameObjects.Sprite.AnimationEffects
{
    class FlashWhiteAnimationEffect : IAnimationEffect
    {
        private const float effectTime = 100f;
        private float effectTimer = 0f;
        private bool white = false;

        public void Reset()
        {
            white = true;
            effectTimer = effectTime;
        }

        public void Update(GameTime gameTime)
        {
            if (effectTimer > 0)
            {
                var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                effectTimer -= delta;
            }
            else
            {
                white = !white;
                effectTimer = effectTime;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 actualPosition, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects flipEffects, float layerDepth)
        {
            if (white)
                color = new Color(color.R, color.G, color.B, 0f);

            spriteBatch.Draw(texture, actualPosition, rectangle, color, rotation, origin, scale, flipEffects, layerDepth);
        }
    }
}
