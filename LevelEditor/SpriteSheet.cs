using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LevelEditor
{
    public class SpriteSheet
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double Scale { get; set; }
        public Texture2D Texture {get; set;}

        public Dictionary<string, SpriteDefinition> SpriteDef { get; set; } = new Dictionary<string, SpriteDefinition>();

        public SpriteSheet() { }

        public SpriteSheet(Texture2D asset_name,string defFile)
        {
            this.Texture = asset_name;

            var xDoc = new XmlDocument();
            xDoc.Load(defFile);

            var rootNode = xDoc.GetElementsByTagName("TextureAtlas")[0];
            var sheet_name = rootNode.Attributes["imagePath"].Value;
            var width = rootNode.Attributes["width"].Value;
            var height = rootNode.Attributes["height"].Value;
            var scale = rootNode.Attributes["scale"].Value;

            this.Name = sheet_name;
            this.Width = Convert.ToInt32(width);
            this.Height = Convert.ToInt32(height);
            this.Scale = Convert.ToDouble(scale);


            var spriteNodes = xDoc.GetElementsByTagName("sprite");
            foreach(XmlNode n in spriteNodes)
            {
                var name = n.Attributes["n"].Value;
                var x = Convert.ToInt32(n.Attributes["x"].Value);
                var y = Convert.ToInt32(n.Attributes["y"].Value);
                var w = Convert.ToInt32(n.Attributes["w"].Value);
                var h = Convert.ToInt32(n.Attributes["h"].Value);

                var o_x = Convert.ToDouble(n.Attributes["pX"].Value);
                var o_y = Convert.ToDouble(n.Attributes["pY"].Value);

                var srcRectangle = new Rectangle(x, y, w, h);
                var origin = new Vector2((float)o_x, (float)o_y);

                SpriteDef.Add(name, new SpriteDefinition(srcRectangle, origin));
            }
        }

    }

    public class SpriteDefinition
    {
        public Rectangle SrcRectangle { get; set; }
        public Vector2 Origin { get; set; }

        public SpriteDefinition() { }

        public SpriteDefinition(Rectangle rec,Vector2 org)
        {
            SrcRectangle = rec;
            Origin = org;
        }
    }
}
