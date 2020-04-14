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
    public class FogPostProcessor : PostProcessor
    {
        private RenderTarget2D fogTarget;

        private int i=0;


        public FogPostProcessor(GameContext context) : base(context)
        {
            LoadContent(context.content);
        }

        public void LoadContent(ContentManager content)
        {

            Effect = content.Load<Effect>("Effects/Fog");
            fogTarget = new RenderTarget2D(game_context.graphics, game_context.graphics.Viewport.Width, game_context.graphics.Viewport.Height);
        }

        public override void Update(GameTime gameTime)
        {
            Effect.Parameters["_time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            Effect.Parameters["offset"].SetValue(new Vector2(game_context.camera.Position.X / game_context.camera.Bounds.Width, game_context.camera.Position.Y / game_context.camera.Bounds.Height));
            Effect.Parameters["res"].SetValue(new Vector2(game_context.camera.Bounds.Width, game_context.camera.Bounds.Height));
        }

        public override void Process(SpriteBatch sb, RenderTarget2D renderTarget)
        {           
            game_context.graphics.SetRenderTarget(fogTarget);
            game_context.graphics.Clear(Color.Transparent);

            base.Process(sb, renderTarget, fogTarget);

           

            //if (i++ == 0)
            //{
            //    using (var st = new FileStream(@"C:\Users\Heckto\Desktop\ComeOn\B.png", FileMode.Create))
            //    {
            //        fogTarget.SaveAsPng(st, renderTarget.Width, renderTarget.Height);
            //    }
            //}

            game_context.graphics.SetRenderTarget(renderTarget);
            
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sb.Draw(fogTarget, Vector2.Zero, Color.White);
            sb.End();

            //if (i++ == 1)
            //{
            //    using (var st = new FileStream(@"C:\Users\Heckto\Desktop\ComeOn\A.png", FileMode.Create))
            //    {
            //        renderTarget.SaveAsPng(st, renderTarget.Width, renderTarget.Height);
            //    }
            //}
        }

        public override void Unload()
        {
            base.Unload();
        }
    }
}
