using AuxLib.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sprite
{
    public class SpriteAnimationFrameSpriteSheet : ISpriteAnimationFrame
    {
        Texture2D spriteSheet;
        SpriteSheetImageDefinition definition;

        public SpriteAnimationFrameSpriteSheet(Texture2D spriteSheet, SpriteSheetImageDefinition definition)
        {
            this.spriteSheet = spriteSheet;
            this.definition = definition;
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float scale, Color color, Vector2 Offset)
        {
            var tex = spriteSheet;
            var actualPosition = CalcActualPosition();

            var dimensions = new Point(
                (definition.Rotated ? definition.Dimensions.Y : definition.Dimensions.X) - 1,
                (definition.Rotated ? definition.Dimensions.X : definition.Dimensions.Y) - 1
                );
            var rectangle = new Rectangle(definition.Position, dimensions);

            //if rotated, rotate an additional 90 degrees
            var rotation = CalcRotation();

            var origin = CalcOrigin();

            spriteBatch.Draw(tex, actualPosition, rectangle, color, rotation, origin, scale * 1.85f, flipEffects, 1.0f);

            Vector2 CalcActualPosition()
            {
                var actualX = flipEffects.HasFlag(SpriteEffects.FlipHorizontally) ? position.X - Offset.X : position.X + Offset.X;
                var actualY = flipEffects.HasFlag(SpriteEffects.FlipVertically) ? position.Y - Offset.Y : position.Y + Offset.Y; // TODO: Test this, no vertical flip yet

                return new Vector2(actualX, actualY);
            }

            float CalcRotation()
            {
                if (!definition.Rotated)
                    return 0;
                if (flipEffects.HasFlag(SpriteEffects.FlipHorizontally))
                    return MathHelper.Pi / 2.0f;
                return MathHelper.Pi / -2.0f;
            }

            Vector2 CalcOrigin()
            {
                //return new Vector2((dimensions.X) * 0.5f, (dimensions.Y) * 0.5f);

                var originalOriginX = definition.OriginalDimensions.X * 0.5f;
                var originalOriginY = definition.OriginalDimensions.Y * 0.5f;

                var newOriginX = originalOriginX - definition.Offset.X;
                var newOriginY = originalOriginY - definition.Offset.Y;

                return new Vector2(
                    definition.Rotated ? newOriginY : newOriginX,
                    definition.Rotated ? newOriginX : newOriginY
                    );
            }
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(string definitionLocation, ContentManager contentManager)
        {
            var definition = SpriteSheetDefinition.LoadFromFile(definitionLocation);
            return FromDefinitionFile(definition, contentManager);
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(SpriteSheetDefinition definition, ContentManager contentManager)
        {
            var spriteSheet = contentManager.Load<Texture2D>(definition.PresumedAssetLocation);
            var result = new Dictionary<string, SpriteAnimationFrameSpriteSheet>();
            foreach (var imageDefinition in definition.ImageDefinitions)
            {
                result.Add(imageDefinition.Key, new SpriteAnimationFrameSpriteSheet(spriteSheet, imageDefinition.Value));
            }

            return result;
        }
    }
}
