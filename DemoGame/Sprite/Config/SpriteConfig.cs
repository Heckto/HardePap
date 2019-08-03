using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Game1.Sprite
{
    public class SpriteConfig
    {
        public string SpriteName { get; set; }

        public int ColorR { get; set; }
        public int ColorG { get; set; }
        public int ColorB { get; set; }
        public int ColorA { get; set; }
        public float SpriteSheetScale { get; set; }
        public string SpritesheetDefinitionFile { get; set; }

        public List<SpriteAnimationConfig> Animations { get; set; } = new List<SpriteAnimationConfig>();

        public void Serialize(string fileLocation)
        {
            using (var writer = new System.IO.StreamWriter(fileLocation))
            {
                var serializer = new XmlSerializer(GetType());
                serializer.Serialize(writer, this);
                writer.Flush();
            }
        }

        public static SpriteConfig Deserialize(string fileLocation)
        {
            using (var stream = System.IO.File.OpenRead(fileLocation))
            {
                var serializer = new XmlSerializer(typeof(SpriteConfig));
                return serializer.Deserialize(stream) as SpriteConfig;
            }
        }
    }
}
