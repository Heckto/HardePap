using AuxLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tainicom.Aether.Physics2D.Dynamics;

namespace Game1.Controllers
{
    public class BulletController : RaycastController
    {
        public delegate void onCollisionDelegate(RayCastHit HitInfo);
        public event onCollisionDelegate onCollision;





        public BulletController(Body body, Category collisionMask) : base(body, collisionMask)
        {
            body.SetCollisionCategories(Category.Cat1);
        }

        public void Move(Vector2 moveAmount)
        {
            var simVelocity = ConvertUnits.ToSimUnits(moveAmount);
            UpdateRaycastOrigins();
            if (moveAmount.X != 0)
            {
                HorizontalCollisions(ref simVelocity);
            }

            if (moveAmount.Y != 0)
            {
                VerticalCollisions(ref simVelocity);
            }

            collider.Position += simVelocity;
        }

        void HorizontalCollisions(ref Vector2 moveAmount)
        {
            var directionX = Math.Sign(moveAmount.X);
            var rayLength = Math.Abs(moveAmount.X) + skinWidth;
            var info = default(RayCastHit);
            var isHit=false;
            for (var i = 0; i < horizontalRayCount; i++)
            {
                var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += UpVector * (horizontalRaySpacing * i);

                var from = ConvertUnits.ToDisplayUnits(rayOrigin);
                var to = ConvertUnits.ToDisplayUnits(rayOrigin + directionX * new Vector2(rayLength, 0));
                castList.Add(new Ray(from, to, 1));

                var hit = caster.RayCast(rayOrigin, RightVector * directionX, rayLength, out var hitInfo, collisionMask);
                if (hit)
                {
                    if (hitInfo.distance < Math.Abs(moveAmount.X))
                    {
                        moveAmount.X = hitInfo.distance;
                        info = hitInfo;
                    }
                    isHit = true;
                }
            }

            if (isHit)
                onCollision?.Invoke(info);

            
        }

        void VerticalCollisions(ref Vector2 moveAmount)
        {
            float directionY = Math.Sign(moveAmount.Y);
            var rayLength = Math.Abs(moveAmount.Y) + skinWidth;
            var info = default(RayCastHit);
            var isHit = false;
            for (var i = 0; i < verticalRayCount; i++)
            {
                var rayOrigin = (directionY == -1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
                rayOrigin += RightVector * (verticalRaySpacing * i + moveAmount.X);

                var from = ConvertUnits.ToDisplayUnits(rayOrigin);
                var to = ConvertUnits.ToDisplayUnits(rayOrigin + directionY * new Vector2(0, rayLength));
                castList.Add(new Ray(from, to, 2));

                var hit = caster.RayCast(rayOrigin, -UpVector * directionY, rayLength, out var hitInfo, collisionMask);
                if (hit)
                {
                    if (hitInfo.distance < moveAmount.Y)
                    {
                        moveAmount.Y = hitInfo.distance;
                        info = hitInfo;
                    }
                    isHit = true;
                }
            }

            if (isHit)
                onCollision?.Invoke(info);

            
        }
    }
}
