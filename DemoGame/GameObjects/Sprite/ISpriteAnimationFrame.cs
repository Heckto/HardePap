using AuxLib.Camera;
using Game1.GameObjects.Sprite.AnimationEffects;
using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Sprite
{
    public interface ISpriteAnimationFrame
    {
        Vector2 Size { get; }
        void Update(GameTime gameTime);
        void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float rotation, float scale, Color color, Vector2 Offset, IAnimationEffect animationEffect);
    }
}
