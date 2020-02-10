using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework.Input;
using AuxLib.Input;
using Game1.GameObjects.Sprite;

namespace AuxLib.Camera
{
    public class FocusCamera
    {
        private Rectangle viewPort;

        public LivingSpriteObject target;

        public float verticalOffset = 1;
        public float lookAheadDstX = 3;
        public float lookSmoothTimeX = .5f;
        public float verticalSmoothTime= .2f;
        public Vector2 focusAreaSize = new Vector2(2, 3);

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
                    _position.X = MathHelper.Clamp(_position.X, Bounds.X, Bounds.X + Bounds.Width - viewPort.Width);
                    _position.Y = MathHelper.Clamp(_position.Y, Bounds.Y, Bounds.Y + Bounds.Height - viewPort.Height);
                }
            }
        }

        public void FocusUpdate(GameTime gameTime,Vector2 latestVelocity)
        {
            if (target != null)
            {

                var bb = new RectangleF(
                   ConvertUnits.ToSimUnits(target.Transform.Position.X - target.colBodySize.X / 2),
                   ConvertUnits.ToSimUnits(target.Transform.Position.Y - target.colBodySize.Y / 2),
                   ConvertUnits.ToSimUnits(target.colBodySize.X),
                   ConvertUnits.ToSimUnits(target.colBodySize.Y));

                focusArea.Update(bb);

                focusPosition = focusArea.center + new Vector2(0, 1) * verticalOffset;

                if (focusArea.velocity.X != 0)
                {
                    lookAheadDirX = Math.Sign(focusArea.velocity.X);
                    if (Math.Sign(latestVelocity.X) == Math.Sign(focusArea.velocity.X) && latestVelocity.X != 0)
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


                currentLookAheadX = Maths.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX, 100, (float)gameTime.ElapsedGameTime.TotalSeconds);


                // Y lookout uit for now !!!!
                //focusPosition.Y = (Player.SmoothDamp(ConvertUnits.ToSimUnits(Position.Y), focusPosition.Y, ref smoothVelocityY, verticalSmoothTime,100, (float)gameTime.ElapsedGameTime.TotalSeconds));
                focusPosition += new Vector2(1, 0) * currentLookAheadX;

                if (focussedOnPlayer)
                    Position = ConvertUnits.ToDisplayUnits(focusPosition);
            }
        }

        



        public float Zoom { get; set; }
        public Rectangle Bounds { get; set; }
        //public Rectangle VisibleArea { get; protected set; }


        private float currentMouseWheelValue, previousMouseWheelValue, zoom;

        public Vector2 Origin;
        private float Rotation = 0.0f;

        public FocusCamera(Viewport _viewport,Rectangle? bounds)
        {
            if (bounds.HasValue)
                Bounds = bounds.Value;
            else
                Bounds = _viewport.Bounds;
            Zoom = 1f;

            viewPort = _viewport.Bounds;
            Origin = new Vector2(_viewport.Width / 2.0f, _viewport.Height / 2.0f);

            Position = Vector2.Zero;
            
        }       

        public void SetViewTarget(LivingSpriteObject _target)
        {
            target = _target;
            
            var bb = new RectangleF(
                ConvertUnits.ToSimUnits(target.Transform.Position.X - target.colBodySize.X / 2),
                ConvertUnits.ToSimUnits(target.Transform.Position.Y - target.colBodySize.Y / 2),
                ConvertUnits.ToSimUnits(target.colBodySize.X),
                ConvertUnits.ToSimUnits(target.colBodySize.Y));
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
            if (parallax == Vector2.Zero)
                return Matrix.Identity;
            else
                return
                Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                Matrix.CreateTranslation(-new Vector3(Bounds.Width* 0.5f, Bounds.Height * 0.5f, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom,Zoom, 1) *
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

        public void UpdateCamera(GameTime gameTime, Vector2 latestVelocity)
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


            if (!focussedOnPlayer)
            {
                if (InputHandler.Instance.IsPressed(0, Buttons.RightThumbstickUp, Keys.Up))
                {
                    cameraMovement.Y = -moveSpeed;
                }

                if (InputHandler.Instance.IsPressed(0, Buttons.RightThumbstickDown, Keys.Down))
                {
                    cameraMovement.Y = moveSpeed;
                }

                if (InputHandler.Instance.IsPressed(0, Buttons.RightThumbstickLeft, Keys.Left))
                {
                    cameraMovement.X = -moveSpeed;
                }

                if (InputHandler.Instance.IsPressed(0, Buttons.RightThumbstickRight, Keys.Right))
                {
                    cameraMovement.X = moveSpeed;
                }
            }

            
            if (InputHandler.Instance.WasPressed(0, Buttons.Back, Keys.F))
            {
                focussedOnPlayer = !focussedOnPlayer;
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

            zoom = Zoom;
            if (cameraMovement != Vector2.Zero && !focussedOnPlayer)
                MoveCamera(cameraMovement);

            
            FocusUpdate(gameTime, latestVelocity);
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
