using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AuxLib.RandomGeneration;

namespace AuxLib.ParticleEngine
{
    public class Particle
    {
        private Vector2 velocity;
        private Vector2 acceleration;
        private float lifetime;
        private Vector2 externalForce;

        internal float Age;
        internal bool IsAlive;
        internal Vector2 location;
        internal Color color;
        internal float ColorChangeRate;
        internal float CurrentColorTime;
        internal int CurrentColorIndex;

        public Particle()
        {
            Age = 0.0f;
            location = new Vector2();
            CurrentColorIndex = 0;
            CurrentColorTime = 0;
        }

        internal void Update(float elapsedTime)
        {
            Age += elapsedTime;
            CurrentColorTime += elapsedTime;

            if (Age >= lifetime)
                IsAlive = false;
            else
            {
                velocity += acceleration;
                velocity -= externalForce;
                location += velocity * elapsedTime;
            }
        }

        public virtual void Draw(SpriteBatch sprite, Texture2D spritesTex)
        {
            sprite.Draw(spritesTex, location, color);
        }

        internal void Initialize(ParticleSystemSettings settings)
        {
            Initialize(settings, true);
        }

        internal void Initialize(ParticleSystemSettings settings, bool makeAlive)
        {
            Age = 0;
            IsAlive = makeAlive;

            var minPosition = (settings.EmitPosition - (settings.EmitRange * .5f));
            var maxPosition = (settings.EmitPosition + (settings.EmitRange * .5f));
           
            location = new Vector2(Rand.GetRandomFloat(minPosition.X, maxPosition.X), Rand.GetRandomFloat(minPosition.Y, maxPosition.Y));

            if (settings.EmitRadius != 0.0f)
            {                
                var angle = Rand.GetRandomFloat(0,MathHelper.TwoPi);
                location = new Vector2(
                    location.X + (float)Math.Sin(angle) * settings.EmitRadius,
                    location.Y + (float)Math.Cos(angle) * settings.EmitRadius);
            }

            velocity = Rand.GetRandomVector2(settings.MinimumVelocity.X, settings.MaximumVelocity.X, settings.MinimumVelocity.Y, settings.MaximumVelocity.Y);
            acceleration = Rand.GetRandomVector2(settings.MinimumAcceleration.X, settings.MaximumAcceleration.X, settings.MinimumAcceleration.Y, settings.MaximumAcceleration.Y);
            lifetime = Rand.GetRandomFloat(settings.MinimumLifetime, settings.MaximumLifetime);

            if (settings.DisplayColorsInOrder)
            {
                color = settings.Colors[0];
                ColorChangeRate = lifetime / settings.Colors.Length;
            }
            else
            {
                color = settings.Colors[Rand.GetRandomInt(0, settings.Colors.Length)];
            }
            externalForce = settings.ExternalForce;
        }

        internal void SetColor(Color color)
        {
            this.color = color;
        }
    }
}
