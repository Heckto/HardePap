using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.HUD
{
    public abstract class HUDComponent
    {
        public bool IsAlive = true;
        public bool Enabled = true;
        public bool Visible = true;

        public abstract void Draw(SpriteBatch spriteBatch,GameTime gameTime);
        public abstract void Update(GameTime gameTime);
    }

    public class HeadsUpDisplay
    {
        public bool Enabled { get; set; }
        public bool Visible { get; set; }

        public Dictionary<string,HUDComponent> componentList = new Dictionary<string,HUDComponent>();

        public HeadsUpDisplay()
        {
            Visible = true;
            Enabled = true;
        }

        public void Draw(SpriteBatch spriteBatch,GameTime gameTime)
        {
            if (Visible)
            {
                foreach (var component in componentList.Values)
                    component.Draw(spriteBatch,gameTime);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                foreach (var component in componentList.Values)
                    component.Update(gameTime);
            }

            var toRemove = componentList.Where(pair => !pair.Value.IsAlive)
                         .Select(pair => pair.Key)
                         .ToList();

            foreach (var key in toRemove)
            {
                componentList.Remove(key);
            }
        }

        public void AddHUDComponent(string componentName,HUDComponent component)
        {
            componentList.Add(componentName,component);
        }

        public void RemoveHUDComponent(string componentName, HUDComponent component)
        {
            if (componentList.ContainsKey(componentName))
                componentList.Remove(componentName);
        }
    }

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

        public LevelIntroText(string text,float TTL,float fadeTime)
        {
            TextToDisplay = text;
            TimeToLive = TTL;
            FadeTime = fadeTime;            
            LoadContent();
        }

        public void LoadContent()
        {            
            levelIntrofont = DemoGame.ContentManager.Load<SpriteFont>("Font/LevelText");
            
            var size = levelIntrofont.MeasureString(TextToDisplay);
            textPos = new Vector2(1980 / 2 - size.X / 2, 1080 / 2 - size.Y / 2);
        }


        public override void Draw(SpriteBatch spriteBatch,GameTime gameTime)
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
