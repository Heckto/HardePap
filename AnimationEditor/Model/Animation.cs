using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationEditor
{
    public partial class Animation
    {
        public string Name { get; set; }

        public List<KeyFrame> keyFrames = new List<KeyFrame>();

        public Animation()
        {
            Name = String.Empty;
        }

        public Animation(string name)
        {
            Name = name;
        }

    }    
}
