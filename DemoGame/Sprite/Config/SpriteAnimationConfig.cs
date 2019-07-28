using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.Sprite
{
    public class SpriteAnimationConfig
    {
        public string AnimationName { get; set; }

        public List<SpriteAnimationFrameConfig> Frames { get; set; } = new List<SpriteAnimationFrameConfig>();
        
        public float Timeout { get; set; }
    }
}
