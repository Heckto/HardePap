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
using AuxLib.ScreenManagement.Transitions;
using Game1.Sprite;

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
        bool transitioning = false;

        public static DebugMonitor DebugMonitor = new DebugMonitor();


        public PlayState(DemoGame game,string LevelFile) : base(game)
        {            
            graphics = game.Services.GetService<GraphicsDeviceManager>();
            spriteBatch = game.Services.GetService<SpriteBatch>();
            camera = game.Services.GetService<BoundedCamera>();
            settings = game.Services.GetService<GameSettings>();
            monitor = new FpsMonitor();
            DebugMonitor.AddDebugValue(monitor, "Value", "FrameRate");

            lvlFile = LevelFile;
            transitioning = false;


        }

        private void Player_onTransition(Player sender,string level)
        {
            sender.onTransition -= Player_onTransition;
            var levelfile = Path.Combine(Content.RootDirectory, level);
            // push our start menu onto the stack
            GameManager.PushState(new PlayState(OurGame, levelfile), new FadeTransition(graphics.GraphicsDevice, Color.Black,2.0f));
            transitioning = true;
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
            if (Input.WasPressed(0, Buttons.LeftShoulder, Keys.OemMinus))
                camera.Zoom -= 0.2f;

            if (Input.WasPressed(0, Buttons.RightShoulder, Keys.OemPlus))
                camera.Zoom += 0.2f;

            if (Input.WasPressed(0, Buttons.DPadLeft, Keys.I))
                GameManager.PushState(new DialogState(OurGame));

            lvl.Update(gameTime,camera);
            player.Update(gameTime);


            if (!lvl.Bounds.Contains(player.Position) && !transitioning)
                SpawnPlayer();

            camera.LookAt(player.Position);

            monitor.Update();
            DebugMonitor.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            lvl.Draw(spriteBatch, camera);

            if (settings.debugMode)
            {
                lvl.DrawDebug(spriteBatch, font, camera);
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
                lvl = Level.FromFile(lvlFile);

                lvl.LoadContent(Content);

                var bounds = (Rectangle)lvl.CustomProperties["bounds"].value;

                lvl.GenerateCollision();
                var spawnLocation = (Vector2)lvl.CustomProperties["spawnVector"].value;
                player = new Player(spawnLocation, lvl, Content);

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
            //Shouldn't Level handle the Spawn?

            if (player != null)
            {
                lvl.CollisionWorld.Remove(player.CollisionBox);
            }

            var spawnLocation = (Vector2)lvl.CustomProperties["spawnVector"].value;
            lvl.RemoveSprite("Player");
            player = new Player(spawnLocation, lvl, Content);
            player.onTransition += Player_onTransition;
            lvl.AddSprite("Player", player);

            for (int i = 1; i < 5; i++)
            {
                lvl.RemoveSprite("Enemy" + i);
                var enemy = new Enemies.Enemy1(new Vector2(spawnLocation.X - 500 + (500 * i), spawnLocation.Y), lvl, player, Content);
                lvl.AddSprite("Enemy" + i, enemy);
            }
        }

    }
}
