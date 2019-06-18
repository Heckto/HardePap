using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.IO;
using System;
using Microsoft.Xna.Framework.Content;
using Game1.Helpers;

namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class PartyGame : Game
    {
        Camera camera;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Level lvl;
        bool debug = true;
        FpsMonitor monitor;
        Player player;
        KeyboardState pState;

        public static DebugMonitor DebugMonitor = new DebugMonitor();
        public static ContentManager ContentManager;

        public Vector2 spawnLocation = new Vector2(4800, 0);
        

        public PartyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            ContentManager = this.Content;
            monitor = new FpsMonitor();
            DebugMonitor.AddDebugValue(monitor, "Value", "FrameRate");

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            
            base.Initialize();

            
            graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height;            
            graphics.ApplyChanges();
            Window.IsBorderless = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var a = System.Reflection.Assembly.GetEntryAssembly().Location;
            var path = Path.Combine(Path.GetDirectoryName(a), "Content");
            lvl = Level.FromFile($"{path}\\ass.xml", Content);

            player = new Player(spawnLocation, lvl.CollisionWorld);
            player.LoadContent(Content);

            SpawnPlayer();

            camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
            {
                Position = new Vector2(100, 1200)
            };           
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

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var currentState = Keyboard.GetState();

            if (currentState.IsKeyUp(Keys.D) && pState.IsKeyDown(Keys.D))
                debug = !debug;
            if (currentState.IsKeyUp(Keys.S) && pState.IsKeyDown(Keys.S))
                SpawnPlayer();

            player.Update(gameTime,camera);


            monitor.Update();
            DebugMonitor.Update(gameTime);

            base.Update(gameTime);



            pState = currentState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
            lvl.draw(spriteBatch,camera,debug);
            //player.Draw(spriteBatch,camera);

            if(debug)
                DebugMonitor.Draw(spriteBatch);

            base.Draw(gameTime);
        }
    }
}
