using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AuxLib.Debug
{
    public static class Debug
    {
        private static Texture2D pixel;

        //Draw rectangle
        public static void DrawRectangle(this SpriteBatch spriteBatch, RectangleF rect, Color color)
        {
            spriteBatch.DrawRectangle(rect.ToRectangle(), color);
        }

        //Draw rectangle
        public static void DrawRectangle(this SpriteBatch spriteBatch, RectangleF rect, Color color, float fillOpacity)
        {
            spriteBatch.DrawRectangle(rect.ToRectangle(), color, fillOpacity);
        }

        //Draw rectangle
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            spriteBatch.Draw(pixel, destinationRectangle: rect, color: color);
        }

        //Draw rectangle
        public static void DrawRectangle(this SpriteBatch spriteBatch, Rectangle rect, Color stroke, float fillOpacity)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            var fill = new Color(stroke, fillOpacity);
            spriteBatch.DrawFill(rect, fill);
            spriteBatch.DrawStroke(rect, stroke);
        }

        //Draw rectangle
        public static void DrawFill(this SpriteBatch spriteBatch, Rectangle rect, Color fill)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            spriteBatch.Draw(pixel, destinationRectangle: rect, color: fill);
        }

        //Draw rectangle
        public static void DrawStroke(this SpriteBatch spriteBatch, Rectangle rect, Color stroke)
        {
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }

            var left = new Rectangle(rect.Left, rect.Top, 1, rect.Height);
            var right = new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height);
            var top = new Rectangle(rect.Left, rect.Top, rect.Width, 1);
            var bottom = new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1);

            spriteBatch.Draw(pixel, destinationRectangle: left, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: right, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: top, color: stroke);
            spriteBatch.Draw(pixel, destinationRectangle: bottom, color: stroke);
        }

        static public void DrawLine(this SpriteBatch spriteBatch, Color color, Vector2 point1, Vector2 point2)
        {
            DrawLine(spriteBatch, color, point1, point2, 1.0f);
        }

        static public void DrawPolyline(this SpriteBatch spriteBatch, Color color, Vector2[] points)
        {
            for (var idx = 0; idx < points.Length - 1; idx++)
            {
                spriteBatch.DrawLine(color, points[idx], points[idx + 1], 0.3f, 5);
            }
        }

        static public void DrawPoint(SpriteBatch spriteBatch, Color color, Vector2 point1, int radius)
        {
            var rec = new Rectangle((int)(point1.X - radius), (int)(point1.Y - radius), 2 * radius, 2 * radius);
            spriteBatch.DrawRectangle(rec, color);
        }

        /// Draw a line into a SpriteBatch
        static public void DrawLine(this SpriteBatch spriteBatch, Color color, Vector2 point1, Vector2 point2, float Layer, float thickness = 1f)
        {
            //Check if data has been set for texture
            //Do this only once otherwise
            if (pixel == null)
            {
                pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                pixel.SetData(new Color[] { Color.White });
            }


            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = (point2 - point1).Length();
            var scale = new Vector2(length, thickness);
            spriteBatch.Draw(pixel, point1, null, color,
                       angle, Vector2.Zero, scale,
                       SpriteEffects.None, Layer);
        }

        static public void DrawString(this SpriteBatch spriteBatch, SpriteFont font, string message, int x, int y, Color color, float alpha)
        {
            spriteBatch.DrawString(font, message, new Vector2(x, y), new Color(color, alpha));
        }

        static public Vector2 MeasureString(this SpriteBatch spriteBatch, SpriteFont font,string message)
        {
            return font.MeasureString(message);
        }
    }
}