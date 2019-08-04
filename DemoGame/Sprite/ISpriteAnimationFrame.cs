using AuxLib.Camera;
using Game1.Sprite.AnimationEffects;
using Game1.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sprite
{
    public interface ISpriteAnimationFrame
    {
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float scale, Color color, Vector2 Offset, IAnimationEffect animationEffect);
    }
}
