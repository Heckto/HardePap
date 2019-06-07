﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace LevelEditor
{
    class TextureLoader
    {

        private static TextureLoader instance;
        public static TextureLoader Instance
        {
            get
            {
                if (instance == null) instance = new TextureLoader();
                return instance;
            }
        }

        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();



        public Texture2D FromFile(GraphicsDevice gd, string filename)
        {
            if (!textures.ContainsKey(filename))
            {
                //TextureCreationParameters tcp = TextureCreationParameters.Default;
                //tcp.Format = SurfaceFormat.Color;
                //tcp.ColorKey = Constants.Instance.ColorTextureTransparent;
                var fileStream = new FileStream(filename, FileMode.Open);               
                textures[filename] = Texture2D.FromStream(gd, fileStream);
                fileStream.Dispose();
                //Texture2D.FromStream(gd,)
                //= Texture2D.FromFile(gd, filename, tcp);
            }
            return textures[filename];
        }

        public void Clear()
        {
            textures.Clear();
        }

    }
}
