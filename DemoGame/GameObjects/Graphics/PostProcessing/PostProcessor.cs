using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Game1.DataContext;
using System.IO;

namespace Game1.GameObjects.Graphics.PostProcessing
{
	/// <summary>
	/// Post Processing step for rendering actions after everthing done.
	/// </summary>
	public class PostProcessor
	{

        private int i = 0;

        protected GameContext game_context;
		/// <summary>
		/// Step is Enabled or not.
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// The effect used to render with
		/// </summary>
		public Effect Effect;

		public PostProcessor(GameContext context,Effect effect = null)
		{
            game_context = context;
            Enabled = true;
			Effect = effect;
		}

		

		/// <summary>
		/// this is the meat method here. The source passed in contains the full scene with any previous PostProcessors
		/// rendering. Render it into the destination RenderTarget. The drawFullScreenQuad methods are there to make
		/// the process even easier. The default implementation renders source into destination with effect.
		/// 
		/// Note that destination might have a previous render! If your PostProcessor Effect is discarding you should clear
		/// the destination before writing to it!
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public virtual void Process(SpriteBatch sb,RenderTarget2D source, RenderTarget2D destination)
		{
			DrawFullscreenQuad(sb,source, destination, Effect);
		}

        public virtual void Update(GameTime gameTime)
        {

        }

        public virtual void Process(SpriteBatch sb, Texture2D source, RenderTarget2D destination = null)
        {
            DrawFullscreenQuad(sb, source, destination, Effect);
        }

        public virtual void Process(SpriteBatch sb, RenderTarget2D renderTarget) {
            DrawFullscreenQuad(sb, renderTarget, null,Effect);
        }

        /// <summary>
        /// called when a scene is ended or this PostProcessor is removed. use this for cleanup.
        /// </summary>
        public virtual void Unload()
		{
			// Nez-specific Effects will have a null name. We don't want to try to remove them.
			if (Effect != null && Effect.Name != null)
			{
				Effect = null;
			}
		}

		/// <summary>
		/// helper for drawing a texture into a rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void DrawFullscreenQuad(SpriteBatch sb,Texture2D texture, RenderTarget2D renderTarget, Effect effect = null)
		{
            //game_context.graphics.SetRenderTarget(renderTarget);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, effect: effect);
            if (renderTarget != null)
                sb.Draw(texture, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            else
                sb.Draw(texture, Vector2.Zero, Color.White);
            sb.End();

            //GraphicsDeviceExt.SetRenderTarget(Core.GraphicsDevice, renderTarget);
            //DrawFullscreenQuad(sb);
        }

		/// <summary>
		/// helper for drawing a texture into the current rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void DrawFullscreenQuad(SpriteBatch sb)
		{
			sb.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend, effect: Effect);

			//sb.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			sb.End();
		}
	}

    /// <summary>
    /// helper subclass for strongly typed Effects loaded from EffectResource. The effect will automatically be unloaded when the scene
    /// completes.
    /// </summary>
    public class PostProcessor<T> : PostProcessor where T : Effect
    {
        /// <summary>
        /// The effect used to render the scene with
        /// </summary>
        public new T Effect;


        public PostProcessor(GameContext context,T effect = null) : base(context, effect)
        {
            Effect = effect;
        }

        /// <summary>
        /// we have to override the default implementation here because we use a custom Effect subclass that differes from the effect
        /// field of the base class
        /// will be null.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="destination">Destination.</param>
        public override void Process(SpriteBatch sb, RenderTarget2D source, RenderTarget2D destination)
        {
            DrawFullscreenQuad(sb,source, destination, Effect);
        }



        public override void Unload()
        {
            base.Unload();
        }
    }
}