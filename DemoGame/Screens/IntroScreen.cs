using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Input;
using System.IO;

namespace Game1.Screens
{
    public sealed class TitleIntroState : BaseGameState, IIntroState
    {
        private Texture2D texture;
        private Texture2D logo_texture;
        private Rectangle r1, r2;
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;
        private SoundEffectInstance sound;

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



                sound.Play();
                var levelfile = Path.Combine(Content.RootDirectory, "Level1.xml");
                // push our start menu onto the stack
                GameManager.ChangeState(new PlayState(OurGame, levelfile));
                //GameManager.ChangeState(new FadingState(OurGame));

            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            var c = new Color(0, 0, 0, 150);
            spriteBatch.Begin();
            spriteBatch.Draw(texture, r1, Color.White);
            spriteBatch.Draw(logo_texture, r2, Color.White);            
            base.Draw(gameTime);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = OurGame.Services.GetService<SpriteBatch>();
            graphics = OurGame.Services.GetService<GraphicsDeviceManager>();
            texture = Content.Load<Texture2D>(@"Misc\logo");
            logo_texture = Content.Load<Texture2D>(@"Misc\unmunnielogo");
            var effect = Content.Load<SoundEffect>(@"sfx\no munney");
            sound = effect.CreateInstance();
            var x1 = (graphics.GraphicsDevice.DisplayMode.Width - texture.Width) / 2;
            var y1 = (graphics.GraphicsDevice.DisplayMode.Height - texture.Height) / 2;
            r1 = new Rectangle(x1, y1 - 150, texture.Width, texture.Height);

            var x2 = (graphics.GraphicsDevice.DisplayMode.Width - logo_texture.Width) / 2;
            var y2 = r1.Y + texture.Height + 50;
            r2 = new Rectangle(x2, y2, logo_texture.Width, logo_texture.Height);

        }
        protected override void UnloadContent()
        {
            texture = null;
            base.UnloadContent();
        }

    }
}
