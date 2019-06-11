using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.IO;
using System;
using Game1.Helper;


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

        Player player;
        Rectangle lvlBoundary;

        

        public PartyGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            
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

            LineBatch.Init(graphics.GraphicsDevice);

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

            
            

            player = new Player(new Vector2(700, 1300),lvl.CollisionWorld);
            player.LoadContent(Content);          
            
            camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
            {
                Position = new Vector2(100, 1200)
            };

            lvlBoundary = lvl.getLevelBounds();
            
            // TODO: use this.Content to load your game content here
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
            KeyboardState kstate;
            kstate = Keyboard.GetState();
            if (kstate.IsKeyDown(Keys.W))
                
                camera.Position = new Vector2(camera.Position.X, camera.Position.Y - 10);
            if (kstate.IsKeyDown(Keys.S))
                camera.Position = new Vector2(camera.Position.X, camera.Position.Y + 10);
            if (kstate.IsKeyDown(Keys.A))
                camera.Position = new Vector2(camera.Position.X-10, camera.Position.Y);
            if (kstate.IsKeyDown(Keys.D))
                camera.Position = new Vector2(camera.Position.X+10, camera.Position.Y);

            player.Update(gameTime,camera);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
            lvl.draw(spriteBatch,camera);
            player.Draw(spriteBatch,camera);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.matrix); ;
            
            var projection = Matrix.CreateOrthographicOffCenter(0f, GraphicsDevice.Viewport.Width,GraphicsDevice.Viewport.Height, 0f, 0f,1f);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
