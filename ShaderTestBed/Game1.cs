using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ShaderTestBed
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D tex;
        Dictionary<string, Effect> effectDict = new Dictionary<string, Effect>();

        float waterDelta = 0.5f;
        float waterTheta = 1.2f;
        string[] effectList = new[] { "GreyScale","Inverse","Bloom","Water" };

        public Game1()
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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            tex = Content.Load<Texture2D>("Idle__006");

            foreach (var eff in effectList)
            {
                var effect = Content.Load<Effect>(@"effects/" + eff);
                effectDict.Add(eff, effect);
            }
            
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

            
            // TODO: Add your update logic here
            waterDelta += (float)gameTime.ElapsedGameTime.TotalSeconds * 8f % (float)(2 * System.Math.PI);
            waterTheta += (float)gameTime.ElapsedGameTime.TotalSeconds * 10f % (float)(2 * System.Math.PI);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            var eff = effectDict["Water"];
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate,BlendState.Additive);

            //float waterLevel = map.water - (.2f * screenSize.Y);
            //float wLev = (GraphicsDevice.PresentationParameters.BackBufferHeight / 2f + waterLevel) / GraphicsDevice.PresentationParameters.BackBufferHeight;
            float waterLevel = 240 - (.2f * GraphicsDevice.PresentationParameters.BackBufferHeight);
            float wLev = (GraphicsDevice.PresentationParameters.BackBufferHeight / 2f + waterLevel) / GraphicsDevice.PresentationParameters.BackBufferHeight;
            eff.Parameters["delta"].SetValue(waterDelta);
            eff.Parameters["theta"].SetValue(waterTheta);
            eff.Parameters["horizon"].SetValue(wLev);

            eff.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(tex, new Rectangle(25, 25,100,200), Color.White);

            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
