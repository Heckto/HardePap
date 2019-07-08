using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimationEditor
{
    public class Frame
    {
        public Vector2 Location;
        public float Rotation;
        public Vector2 Scaling;
        public Vector2 Origin;

        public int Index, Flip;        
        public string AssetName { get; set; }
        public string AssetIndex { get; set; }
        public string Name { get; set; }


        public Frame()
        {
            AssetName = String.Empty;
            AssetIndex = String.Empty;
            Name = String.Empty;
            Scaling = new Vector2(1f, 1f);
        }

        public Frame(string assetName,string frameIdx)
        {
            AssetName = assetName;
            AssetIndex = frameIdx;
            Name = $"KF_{assetName}_{frameIdx}";
            Scaling = new Vector2(1f, 1f);
        }
    }
}
