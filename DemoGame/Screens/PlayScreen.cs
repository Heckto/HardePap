using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Input;
using AuxLib.Debug;
using System.IO;
using Game1.Settings;
using Microsoft.Xna.Framework.Content;
using AuxLib.Camera;

namespace Game1.Screens
{
    public sealed class PlayState : BaseGameState, IIntroState
    {
        private BoundedCamera camera;
        private GraphicsDeviceManager graphics;
        private readonly SpriteBatch spriteBatch;
        Level lvl;
        string lvlFile;
        GameSettings settings;
        FpsMonitor monitor;
        Player player;
        SpriteFont font;

        public static DebugMonitor DebugMonitor = new DebugMonitor();

        public Vector2 spawnLocation = new Vector2(3500, 3300);
        

        public PlayState(DemoGame game,string LevelFile) : base(game)
        {            
            graphics = game.Services.GetService<GraphicsDeviceManager>();
            spriteBatch = game.Services.GetService<SpriteBatch>();

            camera = game.Services.GetService<BoundedCamera>();

            settings = game.Services.GetService<GameSettings>();

            monitor = new FpsMonitor();
            DebugMonitor.AddDebugValue(monitor, "Value", "FrameRate");

            lvlFile = LevelFile;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                // push our start menu onto the stack
                GameManager.PushState(new OptionsMenuState(OurGame));
            }
            if (Input.WasPressed(0,Buttons.DPadRight,Keys.S))
                SpawnPlayer();
            

            player.Update(gameTime,Input);

            if (!lvl.Bounds.Contains(player.Position))
                SpawnPlayer();

            camera.LookAt(player.Position);

            monitor.Update();
            DebugMonitor.Update(gameTime);

            
        }


        public override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            lvl.draw(spriteBatch, font, camera, settings.debugMode);

            if (settings.debugMode)
                DebugMonitor.Draw(spriteBatch);

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("DiagnosticsFont");
            DebugMonitor.Initialize(font);

            if (!String.IsNullOrEmpty(lvlFile) && File.Exists(lvlFile))
            {
                lvl = Level.FromFile(lvlFile);

                lvl.LoadContent(Content);

                var bounds = (Rectangle)lvl.CustomProperties["bounds"].value;

                lvl.GenerateCollision();

                player = new Player(spawnLocation, lvl.CollisionWorld);
                player.LoadContent(Content);

                SpawnPlayer();

                camera.LookAt(player.Position);
                camera.Limits = bounds;
            }
        }

        protected override void UnloadContent()                    
        {            
            base.UnloadContent();
        }

        public void SpawnPlayer()
        {
            if (player != null)
            {
                lvl.HandlePlayerDraw -= player.Draw;
                lvl.CollisionWorld.Remove(player.playerCol);

            }

            player = new Player(spawnLocation, lvl.CollisionWorld);
            player.LoadContent(Content);
            lvl.HandlePlayerDraw += player.Draw;
        }

    }
}
