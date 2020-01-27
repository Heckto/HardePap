using AuxLib;
using Game1.GameObjects.Sprite;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1
{
    public class RaycastController
    {
        protected Vector2 UpVector = new Vector2(0, -1);
        protected Vector2 RightVector = new Vector2(1, 0);
        public Category collisionMask;

        public RectangleF skinRect;

        protected readonly float skinWidth = ConvertUnits.ToSimUnits(10f);

        const float distanceBetweenRays = 15f;
        public int horizontalRayCount;
        public int verticalRayCount;

        protected float horizontalRaySpacing;
        protected float verticalRaySpacing;

        public Body collider;
        protected World world;
        public RaycastOrigins raycastOrigins;
        protected RayCaster caster;

        public List<Ray> castList;

        public RaycastController(Body body,Category _collisionMask)
        {
            castList = new List<Ray>();

            
            collider = body;            
            world = body.World;
            collisionMask = _collisionMask;

            caster = new RayCaster(world);

            CalculateRaySpacing();
        }

        protected void UpdateRaycastOrigins()
        {
            
            var bounds = (ConvertUnits.ToSimUnits(((SpriteObject)collider.Tag).colBodySize));
            raycastOrigins.bottomLeft =     new Vector2(collider.Position.X - (bounds.X / 2) + skinWidth, collider.Position.Y + (bounds.Y / 2) - skinWidth);
            raycastOrigins.bottomRight =    new Vector2(collider.Position.X + (bounds.X / 2) - skinWidth, collider.Position.Y + (bounds.Y / 2) - skinWidth);
            raycastOrigins.topLeft =        new Vector2(collider.Position.X - (bounds.X / 2) + skinWidth, collider.Position.Y - (bounds.Y / 2) + skinWidth);
            raycastOrigins.topRight =       new Vector2(collider.Position.X + (bounds.X / 2) - skinWidth, collider.Position.Y - (bounds.Y / 2) + skinWidth);
        }

        protected void CalculateRaySpacing()
        {
            var bounds = ((SpriteObject)collider.Tag).colBodySize;

            float width = bounds.X;
            float height = bounds.Y;

            horizontalRayCount = (int)(height / distanceBetweenRays);
            horizontalRayCount = MathHelper.Clamp(horizontalRayCount, 2, int.MaxValue);

            verticalRayCount = (int)(width / distanceBetweenRays);
            verticalRayCount = MathHelper.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = (ConvertUnits.ToSimUnits(bounds.Y) - 2 * skinWidth) / (horizontalRayCount - 1);
            verticalRaySpacing = (ConvertUnits.ToSimUnits(bounds.X) - 2 * skinWidth) / (verticalRayCount - 1);
        }

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
        }
    }
    public struct Ray
    {
        public Vector2 from;
        public Vector2 to;

        public int source;

        public Ray(Vector2 f, Vector2 t, int src)
        {
            from = f;
            to = t;
            source = src;
        }
    }

    

}
