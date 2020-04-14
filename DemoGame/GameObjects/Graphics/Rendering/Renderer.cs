using Game1.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Game1.Rendering
{

    public class Renderer
    {
        public RenderMaterial Material;

        protected List<IDrawableItem> renderItems = new List<IDrawableItem>();

        //public abstract void Render(SpriteBatch sb, Matrix proj);

        public void Add(IDrawableItem item)
        {
            renderItems.Add(item);
        }

        public void Remove(IDrawableItem item)
        {
            renderItems.Remove(item);
        }

        public void Clear()
        {
            renderItems.Clear();
        }

        public int getRenderCount()
        {
            return renderItems.Count;
        }

        public void Render(SpriteBatch sb, Matrix proj)
        {
            sb.Begin(SpriteSortMode.Deferred, Material.blendState, null, null, null, Material.Effect, proj);
            foreach (var item in renderItems)
            {
                item.Draw(sb);
            }
            sb.End();
        }

        public Renderer(RenderMaterial mat)
        {
            Material = mat;
        }

    }

    public class RenderMaterial
    {
        /// <summary>
		/// default Material instance
		/// </summary>
		public static RenderMaterial DefaultMaterial = new RenderMaterial(BlendState.AlphaBlend, null);

        /// <summary>
        /// default opaque Material used for PostProcessors
        /// </summary>
        public static RenderMaterial DefaultParticleMaterial = new RenderMaterial(new BlendState
        {
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.One,
            ColorSourceBlend = Blend.SourceAlpha,
        }, null);

        public Effect Effect;

        public BlendState blendState;

        public RenderMaterial() { }

        public RenderMaterial(BlendState state,Effect effect)
        {
            blendState = state;
            Effect = effect;
        }

        public override string ToString()
        {
            if (Effect != null)
                return Effect.Name + blendState.ToString();
            return blendState.ToString();
        }

    }

    public class RenderMaterial<T> : RenderMaterial where T : Effect
    {
        public new T Effect
        {
            get => (T)base.Effect;
            set => base.Effect = value;
        }

        public RenderMaterial()
        {
        }

        public RenderMaterial(T effect,BlendState blendState) : base(blendState,effect)
        {
        }
    }
}
