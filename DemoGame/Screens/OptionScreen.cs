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
using Microsoft.Xna.Framework.Content;
using AuxLib;

namespace Game1.Screens
{
    public sealed class OptionsMenuState : BaseGameState, IOptionsState
    {
        private FocusCamera camera;
        private SpriteBatch spriteBatch;
        private SpriteFont font;
        private Rectangle targetRect;
        private MenuComponent Menu;
        private GameSettings settings;

        public OptionsMenuState(DemoGame game) : base(game)
        {
            BlockUpdating = true;
            BlockDrawing = false;
            
            targetRect = new Rectangle((int)(0.1 * GraphicsDevice.Viewport.Width), (int)(0.1 * GraphicsDevice.Viewport.Height), (int)(0.8 * GraphicsDevice.Viewport.Width), (int)(0.8 * GraphicsDevice.Viewport.Height));

            settings = game.Services.GetService<GameSettings>();

            LoadContent(game.Content);
            Initialize();
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
            Primitives.Instance.drawBoxFilled(spriteBatch, targetRect, new Color(Color.Black, 0.5f));
            Menu.Draw();

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public override void Initialize()
        {
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

            base.Initialize();
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            
            spriteBatch = OurGame.Services.GetService<SpriteBatch>();
            font = contentManager.Load<SpriteFont>("Font/DiagnosticFont");

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
                        base.UnloadContent();
        }
    }
}
