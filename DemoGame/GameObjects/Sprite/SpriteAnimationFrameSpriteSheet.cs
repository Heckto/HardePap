using AuxLib.Camera;
using Game1.GameObjects.Sprite.AnimationEffects;
using Game1.GameObjects.Sprite.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Sprite
{
    public class SpriteAnimationFrameSpriteSheet : ISpriteAnimationFrame
    {
        public Texture2D spriteSheet;
        public SpriteSheetImageDefinition definition;

        public Vector2 Size => new Vector2(definition.SrcRectangle.Size.X, definition.SrcRectangle.Size.Y);

        public SpriteAnimationFrameSpriteSheet(Texture2D spriteSheet, SpriteSheetImageDefinition definition)
        {
            this.spriteSheet = spriteSheet;
            this.definition = definition;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float rotation, float scale, Color color, IAnimationEffect animationEffect)
        {
            animationEffect.Draw(spriteBatch, spriteSheet, position, definition.SrcRectangle, color, rotation, definition.Origin, scale, flipEffects, 1.0f);
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(string definitionLocation, ContentManager content)
        {
            var definition = SpriteSheetDefinition.LoadFromFile(definitionLocation);
            return FromDefinitionFile(definition, content);
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(SpriteSheetDefinition definition, ContentManager content)
        {

            var spriteSheet = content.Load<Texture2D>(definition.AssetName);
            var result = new Dictionary<string, SpriteAnimationFrameSpriteSheet>();
            foreach (var imageDefinition in definition.ImageDefinitions)
            {
                result.Add(imageDefinition.Key, new SpriteAnimationFrameSpriteSheet(spriteSheet, imageDefinition.Value));
            }

            return result;
        }
    }
}
