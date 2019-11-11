using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AuxLib.ParticleEngine
{
    public class ParticleSystemSettings
    {
        public Texture2D Texture;

        public float RotateAmount;

        public bool RunOnce = false;
        public int Capacity;
        public int EmitPerSecond;

        public Vector2 ExternalForce;

        public Vector2 EmitPosition;
        public float EmitRadius;
        public Vector2 EmitRange;

        public Vector2 MinimumVelocity;
        public Vector2 MaximumVelocity;

        public Vector2 MinimumAcceleration;
        public Vector2 MaximumAcceleration;

        public float MinimumLifetime;
        public float MaximumLifetime;

        public float MinimumSize;
        public float MaximumSize;

        public Color[] Colors;
        public bool DisplayColorsInOrder;
    }

}
