using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AuxLib.ParticleEngine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class ParticleSystem : DrawableGameComponent
    {
        private Particle[] particles;
        private int lastParticleIndex = 0;
        private int totalParticlesEmitted = 0;
        private int numberOfActiveParticles;
        private float rotateAngle = 0;

        //private SpriteFont font;
        private SpriteBatch spriteBatch;
        //private Rectangle titleSafeArea;

        protected ParticleSystemSettings settings;
        protected ContentManager content;

        //public Matrix View;
        //public Matrix Projection;
        public bool DebugInfo = false;

        public ParticleSystem(Game game) : base(game)
        {
            //content = new ContentManager(game.Services);
            //content = game.Content;
            settings = new ParticleSystemSettings();
        }

        protected abstract ParticleSystemSettings InitializeSettings();

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            settings = InitializeSettings();

            particles = new Particle[settings.Capacity];

            for (var i = 0; i < settings.Capacity; i++)
            {
                particles[i] = new Particle();
                particles[i].Initialize(settings, false);
            }

            numberOfActiveParticles = 0;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            //font = content.Load<SpriteFont>(@"Fonts\Arial");
            spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var particlesToEmitThisFrame =
                (int)(settings.EmitPerSecond * elapsedTime + .99);
            var particlesEmitted = 0;
            bool canCreateParticle;

            for (var i = 0; i < particles.Length; i++)
            {
                canCreateParticle = false;

                if (particles[i].IsAlive)
                {
                    particles[i].Update(elapsedTime);
                    if (!particles[i].IsAlive)
                    {
                        numberOfActiveParticles--;
                        particles[i].CurrentColorTime = 0;
                        particles[i].CurrentColorIndex = 0;

                        canCreateParticle = ShouldCreateParticle(
                                               particlesEmitted, particlesToEmitThisFrame);
                    }
                    else
                    {
                        if (settings.DisplayColorsInOrder)
                        {
                            if (particles[i].CurrentColorTime >
                                particles[i].ColorChangeRate)
                            {
                                //due to rounding errors with floats we need to make sure
                                //we actually have another color
                                if (particles[i].CurrentColorIndex <
                                    settings.Colors.Length - 1)
                                {
                                    particles[i].CurrentColorIndex++;
                                    particles[i].SetColor(
                                        settings.Colors[particles[i].CurrentColorIndex]);
                                    particles[i].CurrentColorTime = 0;
                                }
                            }
                        }
                    }
                }
                else
                {
                    canCreateParticle = ShouldCreateParticle(particlesEmitted, particlesToEmitThisFrame);
                }

                if (canCreateParticle)
                {
                    particles[i] = CreateParticle();
                    particlesEmitted++;
                    numberOfActiveParticles++;
                    totalParticlesEmitted++;
                }
            }

            if (settings.RotateAmount > 0)
            {
                rotateAngle += settings.RotateAmount;
                if (rotateAngle > MathHelper.TwoPi)
                    rotateAngle = 0;
            }

            base.Update(gameTime);

        }

        public void Draw(GameTime gameTime,Matrix view)
        {

            if (DebugInfo)
            {
                //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                ////GraphicsDevice
                ////spriteBatch.Begin(SpriteBlendMode.AlphaBlend, ,                    SaveStateMode.SaveState);
                //spriteBatch.DrawString(font, "Particles Capacity: " + settings.Capacity.ToString(),                    new Vector2(titleSafeArea.Left, titleSafeArea.Top), Color.Black);
                //spriteBatch.DrawString(font, "Active Particles: " +
                //    numberOfActiveParticles.ToString(),
                //    new Vector2(titleSafeArea.Left, titleSafeArea.Top + 20), Color.Black);
                //spriteBatch.DrawString(font, "Free Particles: " +
                //    (particles.Length - numberOfActiveParticles).ToString(),
                //    new Vector2(titleSafeArea.Left, titleSafeArea.Top + 40), Color.Black);
                //spriteBatch.End();
            }

            //GraphicsDevice
            //GraphicsDevice.RenderState.PointSpriteEnable = true;
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            //SetBlendModes();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,null,null,null,null, view);
            foreach (var particle in particles)
                particle.Draw(spriteBatch, settings.Texture);

            spriteBatch.End();


            //GraphicsDevice.VertexDeclaration = vertexDeclaration;

            //PopulatePointSprites();

            if (numberOfActiveParticles > 0)
            {

                //effect.Parameters["View"].SetValue(View);
                //effect.Parameters["Projection"].SetValue(Projection);
                //effect.Parameters["ColorMap"].SetValue(settings.Texture);
                //effect.Parameters["World"].SetValue(Matrix.Identity);

                //effect.Parameters["RotateAngle"].SetValue(rotateAngle);

                //effect.Begin();
                //effect.CurrentTechnique.Passes[0].Begin();

                //GraphicsDevice.DrawUserPrimitives(
                //    PrimitiveType.PointList, vertices, 0, numberOfActiveParticles);

                //effect.CurrentTechnique.Passes[0].End();
                //effect.End();
            }

            base.Draw(gameTime);

        }

        protected virtual void SetBlendModes()
        {
            //GraphicsDevice.RenderState.AlphaBlendEnable = true;
            //GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            //GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
        }

        //private void PopulatePointSprites()
        //{
        //    if (numberOfActiveParticles == 0)
        //        return;

        //    var currVertex = 0;
        //    for (var i = 0; i < particles.Length; i++)
        //    {
        //        if (particles[i].IsAlive)
        //        {
        //            vertices[currVertex] = particles[i].Vertex;
        //            currVertex++;
        //        }

        //        if (currVertex >= numberOfActiveParticles)
        //            break; //stop looping, we have found all active particles
        //    }
        //}

        private bool ShouldCreateParticle(
            int particlesEmitted, int particlesToEmitThisFrame)
        {
            if (!settings.RunOnce || totalParticlesEmitted < settings.Capacity)
                return (particlesEmitted < particlesToEmitThisFrame);
            else
                return (false);
        }

        private Particle CreateParticle()
        {
            var index = lastParticleIndex;
            for (var i = 0; i < particles.Length; i++)
            {
                if (!particles[index].IsAlive)
                    break;
                else
                {
                    index++;
                    if (index >= particles.Length)
                        index = 0;
                }
            }

            //at this point index is the one we want ...
            particles[index].Initialize(settings);
            lastParticleIndex = index;

            return (particles[index]);
        }

        protected void SetTexture(Texture2D texture)
        {
            settings.Texture = texture;
        }

        public void SetPosition(Vector2 position)
        {
            settings.EmitPosition = position;
        }

        public void ResetSystem()
        {
            if (settings.RunOnce)
            {
                totalParticlesEmitted = 0;
            }
        }

    }
}