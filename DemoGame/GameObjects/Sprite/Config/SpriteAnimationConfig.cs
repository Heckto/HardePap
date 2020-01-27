using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Sprite
{
    public class SpriteAnimationConfig
    {
        public string AnimationName { get; set; }
        public bool Loop { get; set; }
        public float OffsetX { get; set; }
        public float OffsetY { get; set; }
        public List<SpriteAnimationFrameConfig> Frames { get; set; } = new List<SpriteAnimationFrameConfig>();
        
    }
}
