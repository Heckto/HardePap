using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1.Helper
{
    /// <summary>
    /// Line Batch
    /// For drawing lines in a spritebatch
    /// </summary>
    static public class LineBatch
    {
        static private Texture2D _empty_texture;
        static private bool _set_data = false;
        static int alphaFactor = 64;

        static public void Init(GraphicsDevice device)
        {
            _empty_texture = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
        }

        static public void DrawEmptyRectangle(SpriteBatch batch, Color color, Rectangle rec)
        {
            Vector2 LeftTop = new Vector2(rec.Left, rec.Top);
            Vector2 LeftBottom = new Vector2(rec.Left, rec.Top + rec.Height);
            Vector2 RightTop = new Vector2(rec.Left + rec.Width, rec.Top);
            Vector2 RightBottom = new Vector2(rec.Left + rec.Width, rec.Top + rec.Height);


            DrawLine(batch, color, LeftTop, RightTop);
            DrawLine(batch, color, RightTop, RightBottom);
            DrawLine(batch, color, RightBottom, LeftBottom);
            DrawLine(batch, color, LeftBottom, LeftTop);
        }

        static public void DrawRectangle(SpriteBatch batch, Color color, Rectangle rec)
        {
            batch.Draw(_empty_texture, rec, new Color(color, alphaFactor));
        }

        static public void DrawLine(SpriteBatch batch, Color color,
                                    Vector2 point1, Vector2 point2)
        {

            DrawLine(batch, color, point1, point2, 1.0f);
        }

        static public void DrawPoint(SpriteBatch batch, Color color,
                                    Vector2 point1)
        {
            var rec = new Rectangle((int)(point1.X - 2), (int)(point1.Y - 2), 4, 4);
            DrawRectangle(batch, color, rec);
        }
        /// <summary>
        /// Draw a line into a SpriteBatch
        /// </summary>
        /// <param name="batch">SpriteBatch to draw line</param>
        /// <param name="color">The line color</param>
        /// <param name="point1">Start Point</param>
        /// <param name="point2">End Point</param>
        /// <param name="Layer">Layer or Z position</param>
        static public void DrawLine(SpriteBatch batch, Color color, Vector2 point1,
                                    Vector2 point2, float Layer)
        {
            //Check if data has been set for texture
            //Do this only once otherwise
            if (!_set_data)
            {
                _empty_texture.SetData(new[] { Color.White });
                _set_data = true;
            }


            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();

            batch.Draw(_empty_texture, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, 1),
                       SpriteEffects.None, Layer);
        }
    }
}