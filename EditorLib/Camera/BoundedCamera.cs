using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib.Camera
{
    public class BoundedCamera
    {
        public bool focussed = true;

        public BoundedCamera(Viewport viewport)
        {
            Origin = new Vector2(viewport.Width / 2.0f, viewport.Height / 2.0f);
            Zoom = 1.0f;
            this.viewport = viewport;
        }

        public Viewport viewport;
        public Vector2 Origin { get; set; }
        public float Zoom { get; set; }
        public float Rotation { get; set; }

        public Matrix getViewMatrix(Vector2 parallax)
        {
            // To add parallax, simply multiply it by the position
            return Matrix.CreateTranslation(new Vector3(-Position * parallax, 0.0f)) *
                // The next line has a catch. See note below.
                Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(Zoom, Zoom, 1) *
                Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }

        public Matrix getViewMatrix()
        {
            return getViewMatrix(Vector2.One);
        }

        public void LookAt(Vector2 position)
        {
            Position = position - Origin;
        }

        public Rectangle? Limits
        {
            get { return _limits; }
            set
            {
                if (value != null)
                {
                    // Assign limit but make sure it's always bigger than the viewport
                    _limits = new Rectangle
                    {
                        X = value.Value.X,
                        Y = value.Value.Y,
                        Width = Math.Max(viewport.Width, value.Value.Width),
                        Height = Math.Max(viewport.Height, value.Value.Height)
                    };

                    // Validate camera position with new limit
                    Position = Position;
                }
                else
                {
                    _limits = null;
                }
            }
        }

        private Rectangle? _limits;


        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;

                // If there's a limit set and the camera is not transformed clamp position to limits
                if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
                {
                    _position.X = MathHelper.Clamp(_position.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - viewport.Width);
                    _position.Y = MathHelper.Clamp(_position.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - viewport.Height);
                }
            }
        }

        public bool SetPosition(Vector2 newPos)
        {
            var oldPos = _position;
            if (Limits != null && Zoom == 1.0f && Rotation == 0.0f)
            {
                
                
                _position.X = MathHelper.Clamp(newPos.X, Limits.Value.X, Limits.Value.X + Limits.Value.Width - viewport.Width);
                _position.Y = MathHelper.Clamp(newPos.Y, Limits.Value.Y, Limits.Value.Y + Limits.Value.Height - viewport.Height);
            }
            return !_position.Equals(oldPos);
        }
    }
}
