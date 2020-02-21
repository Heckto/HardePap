using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Game1.GameObjects.Sprite
{
    public class SpriteSheetDefinition
    {
        public string AssetName { get; set; }
        public Dictionary<string, SpriteSheetImageDefinition> ImageDefinitions { get; } = new Dictionary<string, SpriteSheetImageDefinition>();

        public static SpriteSheetDefinition LoadFromFile(string location)
        {

            var xDoc = new XmlDocument();
            xDoc.Load(Path.Combine("Content", location));

            var rootNode = xDoc.GetElementsByTagName("TextureAtlas")[0];

            var asset = rootNode.Attributes["imagePath"].Value;
            var result = new SpriteSheetDefinition();
            result.AssetName = Path.Combine(Path.GetDirectoryName(location), asset);      

            var spriteNodes = xDoc.GetElementsByTagName("sprite");
            foreach (XmlNode n in spriteNodes)
            {
                var imageDefinition = new SpriteSheetImageDefinition
                {
                    Name = n.Attributes["n"].Value,

                    SrcRectangle = new Rectangle(
                        Convert.ToInt32(n.Attributes["x"].Value),
                        Convert.ToInt32(n.Attributes["y"].Value),
                        Convert.ToInt32(n.Attributes["w"].Value),
                        Convert.ToInt32(n.Attributes["h"].Value)),

                    Origin = new Vector2(
                    Convert.ToSingle(n.Attributes["pX"].Value, new CultureInfo("en-US")) * Convert.ToInt32(n.Attributes["w"].Value),
                    Convert.ToSingle(n.Attributes["pY"].Value, new CultureInfo("en-US")) * Convert.ToInt32(n.Attributes["h"].Value))
                };
                result.ImageDefinitions.Add(imageDefinition.Name, imageDefinition);
            }
            return result;
        }
    }

    public class SpriteSheetImageDefinition
    {
        public string Name { get; set; }
        public Rectangle SrcRectangle { get; set; }
        public Vector2 Origin { get; set; }
    }
}
