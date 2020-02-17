using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.HUD
{
    public class LevelIntroText : HUDComponent
    {
        private SpriteFont levelIntrofont;

        private readonly float TimeToLive;
        private readonly float FadeTime;
        private readonly string TextToDisplay;

        private float fadeAmount = 0.0f;
        private float currentInFadeTime = 0.0f;
        private float currentAliveTime = 0.0f;
        private float currentOutFadeTime = 0.0f;

        private int state = 0;
        private Vector2 textPos;

        public LevelIntroText(string text, float TTL, float fadeTime)
        {
            TextToDisplay = text;
            TimeToLive = TTL;
            FadeTime = fadeTime;
        }

        public void LoadContent(ContentManager contentManager)
        {
            levelIntrofont = contentManager.Load<SpriteFont>("Font/LevelText");

            var size = levelIntrofont.MeasureString(TextToDisplay);
            textPos = new Vector2(1980 / 2 - size.X / 2, 1080 / 2 - size.Y / 2);
        }


        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(levelIntrofont, TextToDisplay, textPos, new Color(Color.Black, fadeAmount));
            spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            var elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (state == 0)
            {
                currentInFadeTime += elapsedSeconds;
                fadeAmount = MathHelper.Clamp(currentInFadeTime / FadeTime, 0.0f, 1.0f);
                if (currentInFadeTime > FadeTime)
                {
                    state++;
                }
            }
            else if (state == 1)
            {
                currentAliveTime += elapsedSeconds;
                if (currentAliveTime > TimeToLive)
                {
                    state++;
                }
            }
            else if (state == 2)
            {
                fadeAmount = 1 - MathHelper.Clamp(currentOutFadeTime / FadeTime, 0.0f, 1.0f);
                currentOutFadeTime += elapsedSeconds;
                if (currentOutFadeTime > FadeTime)
                {
                    IsAlive = false;
                }
            }


        }
    }
}
