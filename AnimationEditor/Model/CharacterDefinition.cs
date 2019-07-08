using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationEditor
{
    public partial class CharacterDefinition
    {
        public List<Animation> animations { get; set; } = new List<Animation>();
        List<Frame> frames { get; set; } = new List<Frame>();

        public string Name { get; set; }

        public CharacterDefinition()
        {
            Name = "New_Definintion";
        }

        public CharacterDefinition(string name)
        {
            Name = name;
        }

        public void Write()
        {
           
        }

        public static CharacterDefinition FromFile(string filename, ContentManager cm)
        {
            return new CharacterDefinition();
        }
  
    }
}
