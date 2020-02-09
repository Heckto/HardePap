using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Game1.GameObjects.Sprite
{
    public class SpriteSheetDefinition
    {
        public string AssetName { get; set; }
        public string PresumedAssetLocation { get; set; }
        public Vector2 Dimensions { get; set; }
        public float Scale { get; set; }

        public Dictionary<string, SpriteSheetImageDefinition> ImageDefinitions { get; } = new Dictionary<string, SpriteSheetImageDefinition>();

        public static SpriteSheetDefinition LoadFromFile(string location)
        {

            var xDoc = new XmlDocument();
            xDoc.Load(Path.Combine("Content", location));

            var rootNode = xDoc.GetElementsByTagName("TextureAtlas")[0];

            var result = new SpriteSheetDefinition
            {
                AssetName = rootNode.Attributes["imagePath"].Value
            };

            result.PresumedAssetLocation = Path.Combine(Path.GetDirectoryName(location), result.AssetName);

            result.Dimensions = new Vector2(
                Convert.ToInt32(rootNode.Attributes["width"].Value),
                Convert.ToInt32(rootNode.Attributes["height"].Value));

            result.Scale = Convert.ToSingle(rootNode.Attributes["scale"].Value);

            var spriteNodes = xDoc.GetElementsByTagName("sprite");
            foreach (XmlNode n in spriteNodes)
            {
                var imageDefinition = new SpriteSheetImageDefinition
                {
                    Name = n.Attributes["n"].Value,

                    Position = new Point(
                    Convert.ToInt32(n.Attributes["x"].Value),
                    Convert.ToInt32(n.Attributes["y"].Value)),

                    Dimensions = new Point(
                    Convert.ToInt32(n.Attributes["w"].Value),
                    Convert.ToInt32(n.Attributes["h"].Value)),

                    PivotPoint = new Vector2(
                    Convert.ToSingle(n.Attributes["pX"].Value),
                    Convert.ToSingle(n.Attributes["pY"].Value)),


                    Offset = new Vector2(
                    n.Attributes["oX"] != null ? Convert.ToInt32(n.Attributes["oX"].Value) : 0,
                    n.Attributes["oY"] != null ? Convert.ToInt32(n.Attributes["oY"].Value) : 0)
                };

                imageDefinition.OriginalDimensions = new Vector2(
                    n.Attributes["oW"] != null ? Convert.ToInt32(n.Attributes["oW"].Value) : imageDefinition.Dimensions.X,
                    n.Attributes["oH"] != null ? Convert.ToInt32(n.Attributes["oH"].Value) : imageDefinition.Dimensions.Y);

                imageDefinition.Rotated = n.Attributes["r"] != null ? n.Attributes["r"].Value.Equals("y", StringComparison.InvariantCultureIgnoreCase) : false;

                result.ImageDefinitions.Add(imageDefinition.Name, imageDefinition);
            }

            return result;
        }
    }

    public class SpriteSheetImageDefinition
    {
        public string Name { get; set; }
        public Point Position { get; set; }
        public Point Dimensions { get; set; }
        public Vector2 PivotPoint { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 OriginalDimensions { get; set; }
        public bool Rotated { get; set; }
    }
}
