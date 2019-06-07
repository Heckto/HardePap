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
        public String fullpath;
        public Texture2D texture;

        public Brush(String fullpath)
        {
            this.fullpath = fullpath;
            this.texture = TextureLoader.Instance.FromFile(MainForm.Instance.picturebox.GraphicsDevice, fullpath);
        }
    }



}
