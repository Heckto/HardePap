﻿using Microsoft.Xna.Framework;
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
using AuxLib.Debug;

namespace Game1.Screens
{
    public sealed class DialogState : BaseGameState, IIntroState
    {
        
        private Rectangle dialogRect;
        private SpriteFont dialogFont;
        private Rectangle pictureRect;
        private Texture2D pictureTex;

        private Rectangle textRect;
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;
        private SoundEffect sound;

        private string CappedMsg;
        private string Msg = "President MolenKampf has been kidnapped by the ninjas. Are you a bad enough dude to rescue him.\nWhat is a man? A miserable little pile of secrets. But enough talk... Have at you!";
        private string wrappedMsg = String.Empty;
        private int lineCnt = 0;
        private string[] lines;
        private Vector2[] lineIdx;
        private int dialogIdx = 0;

        string currentText = "";
        int currentTextIdx = 0;
        float timeSinceLastIncrement = 0;


        public DialogState(DemoGame game) : base(game)
        {
            BlockDrawing = false;
            BlockUpdating = true;
        }

        public override void Initialize()
        {
            LoadContent();
        }

        private string[] WrapText(string text,float lineSize,int dialogHeigth)
        {
            string[] words = text.Split(new char[] { ' '
            });
            StringBuilder sb = new StringBuilder();
            float linewidth = 0f;
            float spaceWidth = dialogFont.MeasureString(" ").X;
            float fontHeight = dialogFont.MeasureString(" ").Y;
            lineCnt = (int)((dialogHeigth - 20) / fontHeight);
            foreach (string word in words)
            {
                Vector2 size = dialogFont.MeasureString(word);
                if (linewidth + size.X < lineSize)
                {
                    sb.Append(word + " ");
                    linewidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    linewidth = size.X + spaceWidth;
                }
            }

            CappedMsg = sb.ToString();
            var strings = sb.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            lineIdx = new Vector2[strings.Length];
            for (var idx = 0; idx < strings.Length; idx++)
            {
                lineIdx[idx] = new Vector2(textRect.X + 10, textRect.Y + (fontHeight * idx + 10));
            }

            return strings;
        }

        public override void Update(GameTime gameTime)
        {
            timeSinceLastIncrement += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeSinceLastIncrement >= 0.1 && currentTextIdx < CappedMsg.Length)
            {
                
                currentText += CappedMsg[currentTextIdx];
                if (CappedMsg[currentTextIdx] != ' ')
                    sound.Play();

                currentTextIdx++;
                timeSinceLastIncrement = 0;
                
            }


            if (Input.WasPressed(0, Buttons.B, Keys.Escape))
            {
                GameManager.PopState();
            }

            if (Input.WasPressed(0, Buttons.Start, Keys.Enter))
            {
                if (dialogIdx < lines.Length - lineCnt)
                    dialogIdx++;
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            //graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.DrawRectangle(dialogRect, Color.Black, 0.5f);
            //spriteBatch.DrawRectangle(textRect, Color.Red, 0.5f);            
            spriteBatch.Draw(pictureTex, pictureRect, Color.White);
            //for(var idx= dialogIdx;idx< dialogIdx+lineCnt;idx++)
              //  spriteBatch.DrawString(dialogFont, lines[idx], lineIdx[idx-dialogIdx], Color.White);

            spriteBatch.DrawString(dialogFont, currentText, lineIdx[0], Color.White);
            base.Draw(gameTime);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = OurGame.Services.GetService<SpriteBatch>();
            graphics = OurGame.Services.GetService<GraphicsDeviceManager>();
            pictureTex = Content.Load<Texture2D>(@"Misc\Macho");

            dialogFont = Content.Load<SpriteFont>("DialogFont");
            sound = Content.Load<SoundEffect>(@"sfx\typewriter");
            

            var x1 = (int)(0.1f * graphics.GraphicsDevice.DisplayMode.Width);
            var y1 = (int)(0.7f * graphics.GraphicsDevice.DisplayMode.Height);
            var width = (int)(0.8f * graphics.GraphicsDevice.DisplayMode.Width);
            var height = (int)(0.25f * graphics.GraphicsDevice.DisplayMode.Height);
            dialogRect = new Rectangle(x1, y1, width, height);

            pictureRect = new Rectangle(x1 + (int)(0.025f * dialogRect.Width), y1 + (int)(0.1f * dialogRect.Height), (int)(0.2 * dialogRect.Width), (int)(0.8 * dialogRect.Height));

            textRect = new Rectangle(pictureRect.X + pictureRect.Width + (int)(0.025f * dialogRect.Width), y1 + (int)(0.1f * dialogRect.Height), (int)(0.725 * dialogRect.Width), (int)(0.8 * dialogRect.Height));

            lines = WrapText(Msg,0.9f * textRect.Width,textRect.Height);

           
        }
        protected override void UnloadContent()
        {
            pictureTex = null;
            base.UnloadContent();
        }

    }
}
