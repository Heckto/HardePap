using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectMercury;
//using MonoGame.Extended.Particles;
using ProjectMercury.Emitters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game1.GameObjects.ParticleEffects
{
    public static class ParticleExtensions
    {
        public static void Draw(this SpriteBatch spriteBatch, ParticleEffect effect,ref Matrix mat)
        {

            for (var i = 0; i < effect.Count; i++)
                spriteBatch.RenderEmitter(effect[i], ref mat);
        }

        public static void RenderEmitter(this SpriteBatch spriteBatch,Emitter emitter, ref Matrix transform)
        {
            if (emitter.ParticleTexture != null && emitter.ActiveParticlesCount > 0)
            {
                // Bail if the emitter blend mode is "None"...
                if (emitter.BlendMode == EmitterBlendMode.None)
                    return;

                // Calculate the source rectangle and origin offset of the Particle texture...
                var source = new Rectangle(0, 0, emitter.ParticleTexture.Width, emitter.ParticleTexture.Height);
                var origin = new Vector2(source.Width / 2f, source.Height / 2f);

                var blendState = GetBlendState(emitter.BlendMode);
                
                spriteBatch.Begin(SpriteSortMode.Immediate, blendState, null, null, null, null, transform);

                for (var i = 0; i < emitter.ActiveParticlesCount; i++)
                {
                    var particle = emitter.Particles[i];

                    float scale = particle.Scale / emitter.ParticleTexture.Width;
                    spriteBatch.Draw(emitter.ParticleTexture, particle.Position, source, new Color(particle.Colour), particle.Rotation, origin, scale, SpriteEffects.None, 0f);
                }

                spriteBatch.End();
            }
        }

        private static BlendState GetBlendState(EmitterBlendMode emitterBlendMode)
        {
            switch (emitterBlendMode)
            {
                case EmitterBlendMode.Alpha:
                    {
                        return BlendState.NonPremultiplied;
                    }
                case EmitterBlendMode.Add:
                    {
                        return new BlendState
                        {
                            AlphaBlendFunction = BlendFunction.Add,
                            AlphaDestinationBlend = Blend.One,
                            AlphaSourceBlend = Blend.SourceAlpha,
                            ColorBlendFunction = BlendFunction.Add,
                            ColorDestinationBlend = Blend.One,
                            ColorSourceBlend = Blend.SourceAlpha,
                        }; ;
                    }
            }
            return BlendState.AlphaBlend;
        }
    }
}
