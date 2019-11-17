using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using AuxLib.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Debug;
using AuxLib.Camera;
using AuxLib.ScreenManagement;
using Game1.Settings;

namespace Game1.Screens
{
    public sealed class OptionsMenuState : BaseGameState, IOptionsState
    {
        private BoundedCamera camera;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Rectangle targetRect;
        private MenuComponent Menu;
        private GameSettings settings;

        public OptionsMenuState(DemoGame game) : base(game)
        {
            BlockUpdating = true;
            BlockDrawing = false;

            camera = game.Services.GetService<BoundedCamera>();
            targetRect = new Rectangle((int)(0.1 * camera.viewport.Width), (int)(0.1 * camera.viewport.Height), (int)(0.8 * camera.viewport.Width), (int)(0.8 * camera.viewport.Height));

            settings = game.Services.GetService<GameSettings>();
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.WasPressed(0, Buttons.B, Keys.Escape))
            {
                settings.SaveToFile();
                GameManager.PopState();
            }

            Menu.Update(Input);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {            
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawRectangle(targetRect, Color.Black, 0.5f);

            Menu.Draw();

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Initialize()
        {
            LoadContent();
            
        }

        protected override void LoadContent()
        {
            
            spriteBatch = OurGame.Services.GetService<SpriteBatch>();
            font = DemoGame.ContentManager.Load<SpriteFont>("Font/DiagnosticFont");



            var items = new MenuItem[] { new MenuItem("Debug mode",settings.debugMode,(item,value) => {
                                                                                                        settings.debugMode = !settings.debugMode;
                                                                                                        item.Value = settings.debugMode;
                                                                                                        }),
                                         new MenuItem("Fullscreen", settings.isFullScreen,(item,value) => { settings.isFullScreen = !settings.isFullScreen;
                                                                                                            DemoGame.graphics.IsFullScreen = settings.isFullScreen;
                                                                                                            DemoGame.graphics.ApplyChanges();
                                                                                                             item.Value = settings.isFullScreen;
                                             })
            };
            var innerRect = new Rectangle(2 * targetRect.X, 2 * targetRect.Y, (int)(0.9 * targetRect.Width), (int)(0.9 * targetRect.Height));
            Menu = new MenuComponent(innerRect, Vector2.Zero, spriteBatch, font, items.ToList());
            Menu.Initialize();

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
                        base.UnloadContent();
        }
    }
}
