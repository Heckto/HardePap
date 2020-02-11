using Game1.GameObjects.Sprite;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using AuxLib;


namespace Game1
{
    public class PlatformController : RaycastController
    {
        public Category passengerMask = Category.Cat1;
        public Vector2[] globalWaypoints;

        public float speed;
        public bool cyclic;
        public float waitTime;        
        public float easeAmount;

        int fromWaypointIndex;
        float percentBetweenWaypoints;
        float nextMoveTime;

        List<PassengerMovement> passengerMovement;
        Dictionary<Fixture, Controller2D> passengerDictionary = new Dictionary<Fixture, Controller2D>();

        public PlatformController(Body body, Category collisionMask) : base(body, collisionMask)
        {
            castList = new List<Ray>();            
        }

        public void Update(GameTime gameTime)
        {
            castList.Clear();

            UpdateRaycastOrigins();
            var velocity = CalculatePlatformMovement(gameTime);
            var simVelocity = ConvertUnits.ToSimUnits(velocity);
            CalculatePassengerMovement(simVelocity);
            MovePassengers(true);
            collider.Position += simVelocity;
            MovePassengers(false);
        }

        float Ease(float x)
        {
            var a = easeAmount + 1;
            return (float)(Math.Pow(x, a) / (Math.Pow(x, a) + Math.Pow(1 - x, a)));
        }

        Vector2 CalculatePlatformMovement(GameTime gameTime)
        {
            var delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (gameTime.TotalGameTime.TotalMilliseconds < nextMoveTime)
            {
                return Vector2.Zero;
            }
            if (globalWaypoints.Length < 2)
                return Vector2.Zero;

            fromWaypointIndex %= globalWaypoints.Length;
            int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
            float distanceBetweenWaypoints = Vector2.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
            percentBetweenWaypoints += delta * speed / distanceBetweenWaypoints;
            percentBetweenWaypoints = MathHelper.Clamp(percentBetweenWaypoints,0,1);
            float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

            var newPos = Vector2.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

            if (percentBetweenWaypoints >= 1)
            {
                percentBetweenWaypoints = 0;
                fromWaypointIndex++;

                if (!cyclic)
                {
                    if (fromWaypointIndex >= globalWaypoints.Length - 1)
                    {
                        fromWaypointIndex = 0;
                        Array.Reverse(globalWaypoints);
                    }
                }
                nextMoveTime = (float)gameTime.TotalGameTime.TotalMilliseconds + waitTime;
            }

            return newPos - ConvertUnits.ToDisplayUnits(collider.Position);
        }

        void MovePassengers(bool beforeMovePlatform)
        {
            foreach (var passenger in passengerMovement)
            {
                if (passenger.fixture.Body.Tag is LivingSpriteObject)
                {
                    if (!passengerDictionary.ContainsKey(passenger.fixture))
                    {
                        passengerDictionary.Add(passenger.fixture, ((LivingSpriteObject)passenger.fixture.Body.Tag).controller);
                    }

                    if (passenger.moveBeforePlatform == beforeMovePlatform)
                    {
                        passengerDictionary[passenger.fixture].Move(ConvertUnits.ToDisplayUnits(passenger.velocity), passenger.standingOnPlatform);
                    }
                }
            }
        }

        void CalculatePassengerMovement(Vector2 moveAmount)
        {
            var movedPassengers = new HashSet<Fixture>();
            passengerMovement = new List<PassengerMovement>();

            float directionX = Math.Sign(moveAmount.X);
            float directionY = Math.Sign(moveAmount.Y);
            if (directionY == 0.0)
                directionY = -1.0f;

                    // Vertically moving platform
            if (moveAmount.Y != 0)
            {
                float rayLength = Math.Abs(moveAmount.Y) + skinWidth;
                for (int i = 0; i < verticalRayCount; i++)
                {
                    var rayOrigin = (directionY == 1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                    rayOrigin += RightVector * (verticalRaySpacing * i);
                    castList.Add(new Ray() { from = ConvertUnits.ToDisplayUnits(rayOrigin), to = ConvertUnits.ToDisplayUnits(rayOrigin + directionY * new Vector2(0, rayLength)), source = 3 });
                    var hit = caster.RayCast(rayOrigin, -UpVector * directionY, rayLength, out var hitInfo, passengerMask);
                    if (hit && hitInfo.distance != 0)
                    {
                           
                        if (!movedPassengers.Contains(hitInfo.fixture))
                        {
                            movedPassengers.Add(hitInfo.fixture);
                            float pushX = (directionY == -1) ? moveAmount.X : 0;
                            float pushY = moveAmount.Y - (hitInfo.distance - skinWidth) * directionY;

                            passengerMovement.Add(new PassengerMovement(hitInfo.fixture, new Vector2(pushX, pushY), directionY == -1, true));
                        }
                    }
                }
            }

            // Horizontally moving platform
            if (moveAmount.X != 0)
            {
                float rayLength = Math.Abs(moveAmount.X) + skinWidth;

                for (int i = 0; i < horizontalRayCount; i++)
                {
                    var rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                    rayOrigin += UpVector * (horizontalRaySpacing * i);

                    castList.Add(new Ray() { from = rayOrigin, to = rayOrigin + (UpVector * directionY) * rayLength,source= 3 });
                    castList.Add(new Ray() { from = ConvertUnits.ToDisplayUnits(rayOrigin), to = ConvertUnits.ToDisplayUnits(rayOrigin + directionX * new Vector2(0, rayLength)), source = 3 });
                    var hit = caster.RayCast(rayOrigin, RightVector * directionX, rayLength, out var hitInfo, passengerMask);                    

                    if (hit && hitInfo.distance != 0)
                    {
                        if (!movedPassengers.Contains(hitInfo.fixture))
                        {
                            movedPassengers.Add(hitInfo.fixture);
                            float pushX = moveAmount.X - (hitInfo.distance - skinWidth) * directionX;
                            float pushY = skinWidth;

                            passengerMovement.Add(new PassengerMovement(hitInfo.fixture, new Vector2(pushX, pushY), false, true));
                        }
                    }
                }
            }

            // Passenger on top of a horizontally or downward moving platform
            if (directionY == 1 || moveAmount.Y == 0 && moveAmount.X != 0)
            {
                float rayLength = skinWidth * 2;

                for (int i = 0; i < verticalRayCount; i++)
                {
                    Vector2 rayOrigin = raycastOrigins.topLeft + RightVector * (verticalRaySpacing * i);
                    var hit = caster.RayCast(rayOrigin, UpVector, rayLength, out var hitInfo, passengerMask);

                    if (hit && hitInfo.distance != 0)
                    {
                        if (!movedPassengers.Contains(hitInfo.fixture))
                        {
                            movedPassengers.Add(hitInfo.fixture);
                            float pushX = moveAmount.X;
                            float pushY = moveAmount.Y;

                            passengerMovement.Add(new PassengerMovement(hitInfo.fixture, new Vector2(pushX, pushY), true, false));
                        }
                    }
                }
            }
        }

        struct PassengerMovement
        {
            public Fixture fixture;
            public Vector2 velocity;
            public bool standingOnPlatform;
            public bool moveBeforePlatform;

            public PassengerMovement(Fixture _fixture, Vector2 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
            {
                fixture = _fixture;
                velocity = _velocity;
                standingOnPlatform = _standingOnPlatform;
                moveBeforePlatform = _moveBeforePlatform;
            }
        }     
    }

}