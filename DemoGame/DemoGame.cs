using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.IO;
using System;
using Microsoft.Xna.Framework.Content;
using AuxLib.Debug;
using AuxLib.Camera;
using AuxLib.Input;
using AuxLib.Sound;
using AuxLib.ScreenManagement;
using Game1.Screens;
using Game1.Settings;

namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class DemoGame : Game
    {
        BoundedCamera camera;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private GameStateManager gameManager;
        private InputHandler inputHandler;
        
        public static ContentManager ContentManager;

        public string commandParam = string.Empty;


        public DemoGame(string param = "")
        {

            graphics = new GraphicsDeviceManager(this);
            commandParam = param;
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

            Content.RootDirectory = "Content";
            ContentManager = Content;
            Services.AddService(Content);

            
            Services.AddService(graphics);

            gameManager = new GameStateManager(this);
            Services.AddService(gameManager);

            
            var soundDir = new DirectoryInfo(Path.Combine(Content.RootDirectory,"SFX"));
            var musicDir = new DirectoryInfo(Path.Combine(Content.RootDirectory, "Music"));
            AudioManager.Initialize(this, soundDir,musicDir);           


            inputHandler = InputHandler.InitializeSingleton(this);
            Components.Add(inputHandler);
            Components.Add(gameManager);


            var settings = GameSettings.LoadFromFile();
            Services.AddService(settings);

            graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height;
            graphics.ApplyChanges();
            Window.IsBorderless = true;

            camera = new BoundedCamera(GraphicsDevice.Viewport);

            Services.AddService(camera);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            Services.AddService(spriteBatch);

            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            if (!String.IsNullOrEmpty(commandParam))
                gameManager.ChangeState(new PlayState(this,commandParam));
            else
                gameManager.ChangeState(new TitleIntroState(this));
            // Create a new SpriteBatch, which can be used to draw textures.            
            base.LoadContent();
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
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }
    }
}
