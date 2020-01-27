using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Sprite.AnimationEffects
{
    public interface IAnimationEffect
    {
        void Reset();

        void Update(GameTime gameTime);

        void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 actualPosition, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects flipEffects, float layerDepth);
    }
}
