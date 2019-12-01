using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using AuxLib;

namespace Game666
{
    public class Camera
    {
        public Controller2D target;

        public float verticalOffset = 1;
        public float lookAheadDstX = 3;
        public float lookSmoothTimeX = .2f;
        public float verticalSmoothTime= .2f;
        public Vector2 focusAreaSize = new Vector2(1, 3);

        public FocusArea focusArea;
        public Vector2 focusPosition = Vector2.Zero;

        float currentLookAheadX;
        float targetLookAheadX;
        float lookAheadDirX;
        float smoothLookVelocityX;
        float smoothVelocityY;
        bool lookAheadStopped;

        public bool focussedOnPlayer = true;
     

        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (focussedOnPlayer)
                    _position = value - Origin;
                else
                    _position = value;

                // If there's a limit set and the camera is not transformed clamp position to limits
                if (Bounds != null)
                {
                    _position.X = MathHelper.Clamp(_position.X, Bounds.X, Bounds.X + Bounds.Width - 1200);
                    _position.Y = MathHelper.Clamp(_position.Y, Bounds.Y, Bounds.Y + Bounds.Height - 800);
                }
            }
        }

        public void FocusUpdate(GameTime gameTime)
        {
            var size = ConvertUnits.ToSimUnits(((Player)target.collider.Tag).colliderSize);
            var bb = new RectangleF(target.collider.Position.X - size.X / 2, target.collider.Position.Y - size.Y / 2, size.X, size.Y);
            focusArea.Update(bb);

            focusPosition = focusArea.center + new Vector2(0,1) * verticalOffset;

            if (focusArea.velocity.X != 0)
            {
                lookAheadDirX = Math.Sign(focusArea.velocity.X);
                if (Math.Sign(target.latestVelocity.X) == Math.Sign(focusArea.velocity.X) && target.latestVelocity.X != 0)
                {
                    lookAheadStopped = false;
                    targetLookAheadX = lookAheadDirX * lookAheadDstX;
                }
                else
                {
                    if (!lookAheadStopped)
                    {
                        lookAheadStopped = true;
                        targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
                    }
                }
            }


            currentLookAheadX = Player.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX,100,(float)gameTime.ElapsedGameTime.TotalSeconds);


            // Y lookout uit for now !!!!
            //focusPosition.Y = (Player.SmoothDamp(ConvertUnits.ToSimUnits(Position.Y), focusPosition.Y, ref smoothVelocityY, verticalSmoothTime,100, (float)gameTime.ElapsedGameTime.TotalSeconds));
            focusPosition += new Vector2(1,0) * currentLookAheadX;

            if (focussedOnPlayer)
                Position = ConvertUnits.ToDisplayUnits(focusPosition);
        }

        



        public float Zoom { get; set; }
        public Rectangle Bounds { get; set; }
        //public Rectangle VisibleArea { get; protected set; }


        private float currentMouseWheelValue, previousMouseWheelValue, zoom, previousZoom;

        private Vector2 Origin;
        private float Rotation = 0.0f;

        public Camera(Viewport viewport,Rectangle? bounds)
        {
            if (bounds.HasValue)
                Bounds = bounds.Value;
            else
                Bounds = viewport.Bounds;
            Zoom = 1f;

            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);

            Position = Vector2.Zero;
            
        }       

        public void SetViewTarget(Controller2D _target)
        {
            target = _target;
            var size = ConvertUnits.ToSimUnits(((Player)target.collider.Tag).colliderSize);
            var bb = new RectangleF(target.collider.Position.X - size.X / 2, target.collider.Position.Y - size.Y / 2, size.X, size.Y);
            focusArea = new FocusArea(bb, focusAreaSize);
        }


        //private void UpdateVisibleArea()
        //{
        //    var inverseViewMatrix = Matrix.Invert(Transform);

        //    var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
        //    var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
        //    var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
        //    var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

        //    var min = new Vector2(
        //        MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
        //        MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
        //    var max = new Vector2(
        //        MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
        //        MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
        //    VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        //}

        public Matrix getViewMatrix()
        {
            return getViewMatrix(Vector2.One);
        }

        public Matrix getViewMatrix(Vector2 parallax)
        {
                return
                Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                Matrix.CreateTranslation(-new Vector3(Bounds.Width* 0.5f, Bounds.Height * 0.5f, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom) *
                Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0.0f));
        }

        public Matrix getScaledViewMatrix()
        {
            return 
                Matrix.CreateTranslation(ConvertUnits.ToSimUnits(new Vector3(-Position , 0))) *

                Matrix.CreateTranslation(ConvertUnits.ToSimUnits(-new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0))) *
                Matrix.CreateRotationZ(Rotation) * 
                Matrix.CreateScale(Zoom) *
                Matrix.CreateTranslation(ConvertUnits.ToSimUnits(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0)));
        }

        public void MoveCamera(Vector2 movePosition)
        {
            var newPosition = Position + movePosition;
            Position = newPosition;
        }

        public void AdjustZoom(float zoomAmount)
        {
            Zoom += 3 * zoomAmount;
            if (Zoom < .35f)
            {
                Zoom = .35f;
            }
            if (Zoom > 10f)
            {
                Zoom = 10f;
            }
        }

        public void UpdateCamera(GameTime gameTime)
        {
            var cameraMovement = Vector2.Zero;
            int moveSpeed;

            if (Zoom > .8f)
            {
                moveSpeed = 15;
            }
            else if (Zoom < .8f && Zoom >= .6f)
            {
                moveSpeed = 20;
            }
            else if (Zoom < .6f && Zoom > .35f)
            {
                moveSpeed = 25;
            }
            else if (Zoom <= .35f)
            {
                moveSpeed = 30;
            }
            else
            {
                moveSpeed = 10;
            }


            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                cameraMovement.Y = -moveSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                cameraMovement.Y = moveSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                cameraMovement.X = -moveSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                cameraMovement.X = moveSpeed;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                focussedOnPlayer = !focussedOnPlayer;
                if (focussedOnPlayer)
                {
                    SetViewTarget(target);
                }
            }

            previousMouseWheelValue = currentMouseWheelValue;
            currentMouseWheelValue = Mouse.GetState().ScrollWheelValue;

            if (currentMouseWheelValue > previousMouseWheelValue)
            {
                AdjustZoom(.05f);
            }

            if (currentMouseWheelValue < previousMouseWheelValue)
            {
                AdjustZoom(-.05f);
            }

            previousZoom = zoom;
            zoom = Zoom;
            if (cameraMovement != Vector2.Zero && !focussedOnPlayer)
                MoveCamera(cameraMovement);

            
            FocusUpdate(gameTime);
        }
    }

    public struct FocusArea
    {
        public Vector2 velocity;
        public Vector2 center;
        public float left, right;
        public float top, bottom;

        public FocusArea(RectangleF targetBounds, Vector2 size)
        {
            
            left = targetBounds.Left - size.X / 2;
            right = targetBounds.Right + size.X / 2;
            bottom = targetBounds.Bottom;
            top = targetBounds.Top - size.Y;
            velocity = Vector2.Zero;
            center = new Vector2(left + right / 2, top + bottom / 2);
        }

        public void Update(RectangleF targetBounds)
        {
            float shiftX = 0;
            if (targetBounds.Left < left)
            {
                shiftX = targetBounds.Left - left;
            }
            else if (targetBounds.Right > right)
            {
                shiftX = targetBounds.Right - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.Top < top)
            {
                shiftY = targetBounds.Top - top;
            }
            else if (targetBounds.Bottom > bottom)
            {
                shiftY = targetBounds.Bottom - bottom;
            }
            top += shiftY;
            bottom += shiftY;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
