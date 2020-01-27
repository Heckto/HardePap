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
        Texture2D spriteSheet;
        SpriteSheetImageDefinition definition;

        private float spriteSheetScale = 1.0f;

        public Vector2 Size => new Vector2(definition.Dimensions.X, definition.Dimensions.Y);

        public SpriteAnimationFrameSpriteSheet(Texture2D spriteSheet, SpriteSheetImageDefinition definition, float scale)
        {
            this.spriteSheet = spriteSheet;
            this.definition = definition;

            if(scale > 0)
                spriteSheetScale = scale;
        }


        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, SpriteEffects flipEffects, Vector2 position, float rotation, float scale, Color color, Vector2 Offset, IAnimationEffect animationEffect)
        {
            var tex = spriteSheet;

            var dimensions = new Point(
                (definition.Rotated ? definition.Dimensions.Y : definition.Dimensions.X),
                (definition.Rotated ? definition.Dimensions.X : definition.Dimensions.Y) 
                );
            var rectangle = new Rectangle(definition.Position, dimensions);

            //if rotated, rotate an additional 90 degrees
            var spriteRotation = CalcRotation();

            var origin = CalcOrigin();

            var actualPosition = CalcActualPosition();

            animationEffect.Draw(spriteBatch, tex, actualPosition, rectangle, color, spriteRotation + rotation, origin, scale / spriteSheetScale, flipEffects, 1.0f);

            float CalcRotation()
            {
                if (!definition.Rotated)
                    return 0f;
                if (flipEffects.HasFlag(SpriteEffects.FlipHorizontally))
                    return MathHelper.Pi / 2.0f;
                return MathHelper.Pi / -2.0f;
            }

            Vector2 CalcOrigin()
            {
                return new Vector2((dimensions.X) * 0.5f, (dimensions.Y) * 0.5f);
            }

            Vector2 CalcActualPosition()
            {
                var flipXValue = flipEffects.HasFlag(SpriteEffects.FlipHorizontally) ? -1 : 1;
                var flipYValue = flipEffects.HasFlag(SpriteEffects.FlipVertically) ? -1 : 1;

                var originalOriginX = definition.OriginalDimensions.X * 0.5f;
                var originalOriginY = definition.OriginalDimensions.Y * 0.5f;

                var fauxOriginX = (definition.Dimensions.X * 0.5f) + definition.Offset.X;
                var fauxOriginY = (definition.Dimensions.Y * 0.5f) + definition.Offset.Y;

                var originDifferenceX = fauxOriginX - originalOriginX;
                var originDifferenceY = fauxOriginY - originalOriginY;

                var actualX = position.X + flipXValue * (Offset.X + originDifferenceX);
                var actualY = position.Y + flipYValue * (Offset.Y + originDifferenceY);

                return new Vector2(actualX, actualY);
            }
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(string definitionLocation, float scale)
        {
            var definition = SpriteSheetDefinition.LoadFromFile(definitionLocation);
            return FromDefinitionFile(definition, scale);
        }

        public static Dictionary<string, SpriteAnimationFrameSpriteSheet> FromDefinitionFile(SpriteSheetDefinition definition, float scale)
        {
            var spriteSheet = LoadTexture(definition.PresumedAssetLocation);
            var result = new Dictionary<string, SpriteAnimationFrameSpriteSheet>();
            foreach (var imageDefinition in definition.ImageDefinitions)
            {
                result.Add(imageDefinition.Key, new SpriteAnimationFrameSpriteSheet(spriteSheet, imageDefinition.Value, scale));
            }

            return result;
        }


        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        private static Texture2D LoadTexture(string assetLocation)
        {
            if (loadedTextures.ContainsKey(assetLocation))
                return loadedTextures[assetLocation];

            
            var texture = DemoGame.ContentManager.Load<Texture2D>(assetLocation);
            loadedTextures.Add(assetLocation, texture);
            return texture;
        }
    }
}
