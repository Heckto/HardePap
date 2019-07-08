using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace LevelEditor
{

    class Brush
    {
        public string spriteSheet;
        public string spriteName;
        public Texture2D texture;

        public Brush(Texture2D tex,SpriteSheet ss, string name)
        {
            this.spriteSheet = ss.Name;
            this.spriteName = name;
            this.texture = tex;
        }
    }



}
