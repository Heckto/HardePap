using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AuxLib.ParticleEngine;
using AuxLib.Camera;

namespace Game1.ParticleEffects
{
    public class Rain : ParticleSystem
    {
        public Rain(Game game, int capacity, Vector2 externalForce,FocusCamera camera)
            : base(game)
        {
            settings.Capacity = capacity;
            settings.ExternalForce = externalForce;
        }

        public Rain(Game game, int capacity, FocusCamera camera)
            : this(game, capacity, Vector2.Zero,camera) { }

        public Rain(Game game) : this(game, 5000,null) { }

        protected override ParticleSystemSettings InitializeSettings()
        {
            settings.EmitPerSecond = 1100;

            settings.EmitRadius = 2;

            settings.EmitPosition = new Vector2(9000,2000);
            settings.EmitRange = new Vector2(11450, 0);

            settings.MinimumVelocity = new Vector2(0, 10);
            settings.MaximumVelocity = new Vector2(0, 50);

            settings.MinimumAcceleration = new Vector2(0, 10);
            settings.MaximumAcceleration = new Vector2(0, 50);

            settings.MinimumLifetime = 5.0f;
            settings.MaximumLifetime = 5.0f;

            settings.MinimumSize = 5.0f;
            settings.MaximumSize = 15.0f;

            settings.Colors = new Color[] {
                Color.CornflowerBlue,
                Color.LightBlue
            };

            settings.DisplayColorsInOrder = false;

            return (settings);
        }

        protected override void  LoadContent()
        {
            SetTexture(
                base.Game.Content.Load<Texture2D>(@"Particles\raindrop"));

            base.LoadContent();
        }
    }
}
