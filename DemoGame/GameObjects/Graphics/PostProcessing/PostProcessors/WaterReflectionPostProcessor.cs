using Game1.DataContext;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.Graphics.PostProcessing.PostProcessors
{
    public class WaterReflectionPostProcessor : PostProcessor
    {
        private RenderTarget2D waterTarget;

        public float waterLevel = 2750;

        public WaterReflectionPostProcessor(GameContext context) : base(context)
        {            
            LoadContent(context.content);
        }

        public void LoadContent(ContentManager content)
        {
            Effect = content.Load<Effect>("Effects/water");            
            waterTarget = new RenderTarget2D(game_context.graphics, game_context.graphics.Viewport.Width, game_context.graphics.Viewport.Height);
        }

        public override void Update(GameTime gameTime)
        {
            Effect.Parameters["_time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
        }

        public override void Process(SpriteBatch sb, RenderTarget2D renderTarget)
        {
            if (waterLevel >= game_context.camera.Position.Y && waterLevel <= game_context.camera.Position.Y + game_context.graphics.Viewport.Height)
            {
                var wLev = (waterLevel - game_context.camera.Position.Y) / game_context.graphics.Viewport.Height;
                Effect.Parameters["horizon"].SetValue(wLev);

                game_context.graphics.SetRenderTarget(waterTarget);

                base.Process(sb, renderTarget, waterTarget);

                game_context.graphics.SetRenderTarget(renderTarget);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                sb.Draw(waterTarget, new Rectangle(0, (int)(wLev * renderTarget.Height), game_context.graphics.PresentationParameters.BackBufferWidth, game_context.graphics.PresentationParameters.BackBufferHeight), Color.White);
                sb.End();   
            }
        }

        public override void Unload()
        {
            base.Unload();
        }
    }

   
}
