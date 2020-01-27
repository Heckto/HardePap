using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using AuxLib.Debug;
using System.IO;
using Game1.Settings;
using AuxLib.Camera;
using AuxLib.ParticleEngine;
using Game1.GameObjects.Levels;
using Game1.DataContext;
using Game1.Scripting;
using Game1.HUD;
using System.Collections.Generic;

namespace Game1.Screens
{
    public sealed class PlayState : BaseGameState, IPlayGameState
    {
        private readonly GameContext context;
        private FocusCamera camera;
        private readonly SpriteBatch spriteBatch;
        private ScriptingEngine scriptingEngine;
        string lvlFile;
        GameSettings settings;
        FpsMonitor monitor;
        HeadsUpDisplay hud;
        SpriteFont font;

        
        

        public static DebugMonitor DebugMonitor = new DebugMonitor();


        public PlayState(DemoGame game,string LevelFile) : base(game)
        {            
            spriteBatch = game.Services.GetService<SpriteBatch>();
            camera = game.Services.GetService<FocusCamera>();
            settings = game.Services.GetService<GameSettings>();
            scriptingEngine = game.Services.GetService<ScriptingEngine>();
            context = game.Services.GetService<GameContext>();
            monitor = new FpsMonitor();
            
            lvlFile = LevelFile;
            var dir = Path.Combine(DemoGame.ContentManager.RootDirectory,"Scripts");
            var files = Directory.GetFiles(dir);
            //scriptingEngine.LoadScript(files);

            hud = new HeadsUpDisplay();        
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            scriptingEngine.Update(gameTime);

            if (Input.WasPressed(0, Buttons.Start, Keys.Escape))
            {
                // push our start menu onto the stack
                GameManager.PushState(new OptionsMenuState(OurGame));
            }
            if (Input.WasPressed(0,Buttons.DPadRight,Keys.F1))
                context.lvl.SpawnPlayer(null);
            if (Input.WasPressed(0, Buttons.LeftShoulder, Keys.OemMinus))
                camera.Zoom -= 0.2f;

            if (Input.WasPressed(0, Buttons.RightShoulder, Keys.OemPlus))
                camera.Zoom += 0.2f;

            if (Input.WasPressed(0, Buttons.LeftStick, Keys.OemTilde))
            {
                GameManager.PushState(new ConsoleScreen(OurGame));
            }
            context.lvl.Update(gameTime);
            foreach (var p in context.particleSystems)
                p.Update(gameTime);

            //if (camera.focussedOnPlayer)
              //  camera.Position = context.lvl.player.Position;

            hud.Update(gameTime);

            monitor.Update();

            DebugMonitor.AddDebugValue("FrameRate", monitor.Value);
        }


        public override void Draw(GameTime gameTime)
        {
            DemoGame.graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            context.lvl.Draw(spriteBatch, camera);

            foreach (var p in context.particleSystems)
                p.Draw(gameTime);

            hud.Draw(spriteBatch, gameTime);

            if (settings.debugMode)
            {
                context.lvl.DrawDebug(spriteBatch, font, camera);
                DebugMonitor.Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            font = DemoGame.ContentManager.Load<SpriteFont>("Font/DiagnosticFont");
            DebugMonitor.Initialize(font);

            if (!String.IsNullOrEmpty(lvlFile) && File.Exists(lvlFile))
            {
                context.lvl = Level.FromFile(lvlFile);

                // ????
                context.lvl.context = context;
                context.HUD = hud;

                context.lvl.LoadContent();

                var bounds = (Rectangle)context.lvl.CustomProperties["bounds"].value;

                context.lvl.GenerateCollision();

                context.lvl.SpawnPlayer(null);
                if (camera.focussedOnPlayer)
                    camera.Position = context.lvl.player.Position;

                camera.Bounds = bounds;

                //if (context.lvl.CustomProperties.ContainsKey("IntroScript"))
                //{
                //    var intro_script = (string)context.lvl.CustomProperties["IntroScript"].value;
                //    if (!String.IsNullOrEmpty(intro_script) && scriptingEngine.hasScriptLoaded(intro_script))
                //    {
                //        scriptingEngine.ExecuteScript(intro_script);
                //    }
                //}
            }
        }

        protected override void UnloadContent()                    
        {            
            base.UnloadContent();
        }
    }
}
