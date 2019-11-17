using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Serialization;
using System.IO;

namespace Game1.Settings
{



    public class GameSettings
    {
        private const string settingsfile = "Settings.xml";

        private readonly Vector2[] ScreenSizeOptions = { new Vector2(1280, 720), new Vector2(1980, 1080) };


        public bool debugMode { get; set; } = false;        
        public bool isFullScreen { get; set; } = false;

        public float SoundVolume { get; set; }  = 0.0f;
        public float MusicVolume { get; set; } = 0.0f;

        public void SaveToFile()
        {            
            XMLFileManager<GameSettings>.WriteToFile(settingsfile, this);
        }

        public static GameSettings LoadFromFile()
        {
            if (File.Exists(settingsfile))
                return XMLFileManager<GameSettings>.OpenFromFile(settingsfile);
            else
                return new GameSettings();
        }

    }
}
