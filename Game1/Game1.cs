using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Collision;
using System.Linq;
using System.IO;
using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace Game1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        Camera camera;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Level lvl;

        Player player;
        Body rec;
        Body circle;
        Rectangle lvlBoundary;
        World world;
        DebugView debug;

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
            world = new World(Vector2.UnitY * 10);
            
            debug = new DebugView(world);
            debug.RemoveFlags(DebugViewFlags.Controllers);
            debug.RemoveFlags(DebugViewFlags.Joint);            
            debug.LoadContent(GraphicsDevice, Content);

            var l = lvl.Layers.FirstOrDefault(elem => elem.Name == "collision");
            foreach(var elem in l.Items)
            {
                if (elem is RectangleItem)
                {
                    var r = elem as RectangleItem;
                    world.CreateRectangle(r.Width, r.Height, 1f, new Vector2(r.Position.X + 0.5f * r.Width,r.Position.Y + 0.5f * r.Height), 0f, BodyType.Static);
                }
                else if (elem is PathItem)
                {
                    var p = elem as PathItem;
                    for (var idx = 0; idx < p.WorldPoints.Length - 1; idx++)
                    {
                        var body = world.CreateEdge(p.WorldPoints[idx], p.WorldPoints[idx + 1]);
                        body.BodyType = BodyType.Static;
                    }
                }
            }


            player = new Player(new Vector2(-1500, -600));
            player.LoadContent(Content);
            //rec = world.CreateRectangle(100, 150,1, new Vector2(-1500, -600), 0, BodyType.Dynamic);
            circle = world.CreateCircle(10.0f, 0.5f, new Vector2(-1500, -600), BodyType.Dynamic);
            circle.SetFriction(10f);
            //world.JointList.Add(new RevoluteJoint(rec, circle, new Vector2(-1500, -600)));


            circle.ApplyForce(new Vector2(0f, -10f));
            //rec.Mass = 100000f;
            //rec.SetRestitution(0.3f);
            //rec.SetFriction(0.5f);
            //rec.Inertia = 10f;
            camera = new Camera(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height)
            {
                Position = new Vector2(-1700, -500)
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

            if (kstate.IsKeyDown(Keys.Left))
                circle.ApplyForce(new Vector2(-20, 0f));

            if (kstate.IsKeyDown(Keys.Right))
                //camera.Position = new Vector2(camera.Position.X + 10, camera.Position.Y);
                
                circle.ApplyForce(new Vector2(20f, 0f));

            if (kstate.IsKeyDown(Keys.Space))
                // TODO: Add your update logic here
                circle.ApplyForce(new Vector2(0f, -2000000f));
            world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            player.Update(gameTime);

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

            debug.RenderDebugData(projection,camera.matrix);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
