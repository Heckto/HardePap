using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using AuxLib.ScreenManagement.Transitions;
using AuxLib.Sound;
using Microsoft.Xna.Framework.Content;

namespace Game1.Screens
{
    public sealed class TitleIntroState : BaseGameState, IIntroState
    {
        private Texture2D texture;
        private Texture2D logo_texture;
        private Rectangle r1, r2;
        private SpriteBatch spriteBatch;

        private string rootDir;
        

        public TitleIntroState(DemoGame game) : base(game)
        {
            
        }

        public override void Initialize()
        {
            LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                //AudioManager.PlaySoundEffect("bitch");
                var levelfile = Path.Combine(rootDir, "Level1.xml");
                // push our start menu onto the stack
                GameManager.PushState(new PlayState(OurGame, levelfile), new ExpandTransition(DemoGame.graphics.GraphicsDevice,Color.Black, 2.0f));
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, r1, Color.White);
            spriteBatch.Draw(logo_texture, r2, Color.White);            
            base.Draw(gameTime);
            spriteBatch.End();
        }

        protected override void LoadContent(ContentManager contentManager)
        {
            spriteBatch = OurGame.Services.GetService<SpriteBatch>();
            
            
            texture = contentManager.Load<Texture2D>(@"Misc\why");
            logo_texture = contentManager.Load<Texture2D>(@"Misc\unmunnielogo");
            var x1 = (DemoGame.graphics.GraphicsDevice.DisplayMode.Width - texture.Width) / 2;
            var y1 = (DemoGame.graphics.GraphicsDevice.DisplayMode.Height - texture.Height) / 2;
            var size = new Vector2(275, 183);
            r1 = new Rectangle(x1, y1 -100, texture.Width, texture.Height);

            var x2 = (DemoGame.graphics.GraphicsDevice.DisplayMode.Width - logo_texture.Width) / 2;
            var y2 = r1.Y + texture.Height + 50;
            r2 = new Rectangle(x2, y2, logo_texture.Width, logo_texture.Height);

            rootDir = contentManager.RootDirectory;

        }
        protected override void UnloadContent()
        {
            texture = null;
            base.UnloadContent();
        }

    }
}
