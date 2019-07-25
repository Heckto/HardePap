using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.Screens
{
    public sealed class FadingState : BaseGameState, IFadingState
    {
        private readonly SpriteBatch spriteBatch;
        private Texture2D fadeTexture;
        private float fadeAmount;
        private double fadeStartTime;
        private Color color = Color.Black;
        public Color Color
        {
            get { return (color); }
            set { color = value; }
        }

        public FadingState(Game game) : base(game)
        {
            //game.Services.AddService(typeof(IFadingState), this);
            spriteBatch = game.Services.GetService<SpriteBatch>();
        }

        public override void Update(GameTime gameTime)
        {
            if (fadeStartTime == 0)
                fadeStartTime = gameTime.TotalGameTime.TotalMilliseconds;
            fadeAmount += (.05f * (float)gameTime.ElapsedGameTime.TotalSeconds);
            if (gameTime.TotalGameTime.TotalMilliseconds > fadeStartTime + 4000)
            {
                //Once we are done fading, change back to title screen.
                GameManager.ChangeState(new PlayState(OurGame,"Level1"));
            }
        }

        public override void Draw(GameTime gameTime)
        {

            //GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            //GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            spriteBatch.Begin();
            var fadeColor = color.ToVector4();
            fadeColor.W = fadeAmount; //set transparancy
            spriteBatch.Draw(fadeTexture, Vector2.Zero,
            new Color(fadeColor));
            base.Draw(gameTime);
            spriteBatch.End();
        }
        protected override void StateChanged(object sender, EventArgs e)
        {
            //Set up our initial fading values
            if (GameManager.State == this.Value)
            {
                fadeAmount = 0;
                fadeStartTime = 0;
            }
        }

        protected override void LoadContent()
        {
            fadeTexture = CreateFadeTexture(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            base.LoadContent();
        }
        private Texture2D CreateFadeTexture(int width, int height)
        {
            var texture = new Texture2D(GraphicsDevice, width, height, true, SurfaceFormat.Color);
            var pixelCount = width * height;
            var pixelData = new Color[pixelCount];
            var rnd = new Random();
            for (var i = 0; i < pixelCount; i++)
            {
                pixelData[i] = Color.White;
            }
            texture.SetData(pixelData);
            return (texture);
        }
    }
}
