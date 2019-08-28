using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using AuxLib.Debug;
using System.IO;
using Game1.Settings;
using AuxLib.Camera;
using Game1.Levels;
using Game1.DataContext;
using Game1.Scripting;

namespace Game1.Screens
{
    public sealed class PlayState : BaseGameState, IPlayGameState
    {
        private readonly GameContext context;
        private BoundedCamera camera;
        private GraphicsDeviceManager graphics;
        private readonly SpriteBatch spriteBatch;
        private ScriptingHost scriptingEngine;
        string lvlFile;
        GameSettings settings;
        FpsMonitor monitor;
        
        SpriteFont font;
        

        public static DebugMonitor DebugMonitor = new DebugMonitor();


        public PlayState(DemoGame game,string LevelFile) : base(game)
        {            
            graphics = game.Services.GetService<GraphicsDeviceManager>();
            spriteBatch = game.Services.GetService<SpriteBatch>();
            camera = game.Services.GetService<BoundedCamera>();
            settings = game.Services.GetService<GameSettings>();
            scriptingEngine = game.Services.GetService<ScriptingHost>();
            context = game.Services.GetService<GameContext>();
            monitor = new FpsMonitor();
            DebugMonitor.AddDebugValue(monitor, "Value", "FrameRate");
            lvlFile = LevelFile;

            var p = @"C:\Users\Heckto\Desktop\testGame\DemoGame\Scripts";
            var files = Directory.GetFiles(p);
            scriptingEngine.LoadScript(files, context);
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            scriptingEngine.Update(gameTime);

            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                // push our start menu onto the stack
                GameManager.PushState(new OptionsMenuState(OurGame));
            }
            if (Input.WasPressed(0,Buttons.DPadRight,Keys.F1))
                context.lvl.SpawnPlayer();
            if (Input.WasPressed(0, Buttons.LeftShoulder, Keys.OemMinus))
                camera.Zoom -= 0.2f;

            if (Input.WasPressed(0, Buttons.RightShoulder, Keys.OemPlus))
                camera.Zoom += 0.2f;

            if (Input.WasPressed(0, Buttons.LeftStick, Keys.OemTilde))
            {
                GameManager.PushState(new ConsoleScreen(OurGame));
            }
            context.lvl.Update(gameTime);


            if (camera.focussed)
                camera.LookAt(context.lvl.player.Position);

            monitor.Update();
            DebugMonitor.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            context.lvl.Draw(spriteBatch, camera);

            if (settings.debugMode)
            {
                context.lvl.DrawDebug(spriteBatch, font, camera);
                DebugMonitor.Draw(spriteBatch);
            }

            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("DiagnosticsFont");
            DebugMonitor.Initialize(font);

            if (!String.IsNullOrEmpty(lvlFile) && File.Exists(lvlFile))
            {
                context.lvl = Level.FromFile(lvlFile);

                // ????
                context.lvl.context = context;

                context.lvl.LoadContent(Content);

                var bounds = (Rectangle)context.lvl.CustomProperties["bounds"].value;

                context.lvl.GenerateCollision();

                context.lvl.SpawnPlayer();
                if (camera.focussed)
                    camera.LookAt(context.lvl.player.Position);
                camera.Limits = bounds;
            }
        }

        protected override void UnloadContent()                    
        {            
            base.UnloadContent();
        }
    }
}
