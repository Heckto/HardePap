using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Game1.GameObjects.Levels
{
    public partial class TextureItem : GameObject, IDrawableItem
    {
        [XmlIgnore]
        public Texture2D texture;

        public Color TintColor { get; set; }

        public bool FlipHorizontally { get; set; }

        public bool FlipVertically { get; set; }

        public String texture_filename;

        [ReadOnly(true)]
        public String asset_name { get; set; }

        [ReadOnly(true)]
        public Rectangle srcRectangle { get; set; }

        [ReadOnly(true)]
        public Vector2 Origin { get; set; }

        public TextureItem() : base() { }

        public void Draw(SpriteBatch sb)
        {
            var effects = SpriteEffects.None;
            if (FlipHorizontally) effects |= SpriteEffects.FlipHorizontally;
            if (FlipVertically) effects |= SpriteEffects.FlipVertically;            
            sb.Draw(texture, Transform.Position, srcRectangle, TintColor, Transform.Rotation, Origin, Transform.Scale, effects, 0);
        }

        public override Rectangle getBoundingBox()
        {
            return boundingrectangle;
        }

        #region Editable

        [XmlIgnore]
        Color[] coldata;


        

        [XmlIgnore]
        Vector2[] polygon;

        public TextureItem(String fullpath, Vector2 position, Rectangle srcRect,Texture2D tex)
        {            
            this.texture = tex;
            this.texture_filename = fullpath;
            this.asset_name = Path.GetFileNameWithoutExtension(fullpath);
            this.TintColor = Color.White;
            FlipHorizontally = FlipVertically = false;
            this.srcRectangle = srcRect;
            this.Origin = getTextureOrigin(srcRect);
            
            //compensate for origins that are not at the center of the texture
            //var center = new Vector2(srcRect.Width / 2, srcRect.Height / 2);
            //this.Transform.Position = position - (center - Origin);
            this.Transform.Position = position;
        }

        public override GameObject clone()
        {
            var result = (TextureItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.polygon = (Vector2[])polygon.Clone();
            result.hovering = false;
            return result;
        }


        public override void OnTransformed()
        {
            
            coldata = new Color[srcRectangle.Width * srcRectangle.Height];
            texture.GetData<Color>(0, srcRectangle, coldata, 0, srcRectangle.Width * srcRectangle.Height);
            polygon = new Vector2[4];

            Transform.GetLocalMatrix(out var mat);

            var leftTop = new Vector2(0, 0);
            var rightTop = new Vector2(srcRectangle.Width, 0);
            var leftBottom = new Vector2(0, srcRectangle.Height);
            var rightBottom = new Vector2(srcRectangle.Width, srcRectangle.Height);

            // Transform all four corners into work space
            leftTop = Vector2.Transform(leftTop - Origin, mat);
            rightTop = Vector2.Transform(rightTop - Origin, mat);
            leftBottom = Vector2.Transform(leftBottom - Origin, mat);
            rightBottom = Vector2.Transform(rightBottom - Origin, mat);

            polygon[0] = leftTop;
            polygon[1] = rightTop;
            polygon[3] = leftBottom;
            polygon[2] = rightBottom;

            // Find the minimum and maximum extents of the rectangle in world space
            var min = Vector2.Min(Vector2.Min(leftTop, rightTop),
                                      Vector2.Min(leftBottom, rightBottom));
            var max = Vector2.Max(Vector2.Max(leftTop, rightTop),
                                      Vector2.Max(leftBottom, rightBottom));

            // Return as a rectangle
            boundingrectangle = new Rectangle((int)min.X, (int)min.Y,
                                 (int)(max.X - min.X), (int)(max.Y - min.Y));
        }


        public override void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;
            //MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            var se = SpriteEffects.None;
            if (FlipHorizontally) se |= SpriteEffects.FlipHorizontally;
            if (FlipVertically) se |= SpriteEffects.FlipVertically;
            var c = hovering ? new Color(255, 0, 0, 228) : TintColor;
            sb.Draw(texture, Transform.Position, srcRectangle, c, Transform.Rotation, Origin, Transform.Scale, se, 0);
        }

        public override void loadIntoEditor(ContentManager content)
        {
            texture = content.Load<Texture2D>(asset_name);            
        }

        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            var poly = new Vector2[4];
            Vector2.Transform(polygon, ref matrix, poly);

            Primitives.Instance.drawPolygon(sb, poly, color, 2);
            foreach (var p in poly)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }
            var origin = Vector2.Transform(Transform.Position, matrix);
            Primitives.Instance.drawBoxFilled(sb, origin.X - 5, origin.Y - 5, 10, 10, color);
        }

        public override bool contains(Vector2 worldpos)
        {
            if (boundingrectangle.Contains((int)worldpos.X, (int)worldpos.Y))
            {
                return intersectpixels(worldpos);
            }
            return false;
        }

        public bool intersectpixels(Vector2 worldpos)
        {
            
            Transform.GetLocalMatrix(out var mat);
            var positionInB = Vector2.Transform(worldpos + Origin, Matrix.Invert(mat));
            var xB = (int)Math.Round(positionInB.X);
            var yB = (int)Math.Round(positionInB.Y);

            if (FlipHorizontally) xB = srcRectangle.Width - xB;
            if (FlipVertically) yB = srcRectangle.Height - yB;

            // If the pixel lies within the bounds of B
            if (0 <= xB && xB < srcRectangle.Width && 0 <= yB && yB < srcRectangle.Height)
            {
                var colorB = coldata[xB + yB * srcRectangle.Width];
                if (colorB.A != 0)
                {
                    return true;
                }
            }
            return false;
        }



        public Vector2 getTextureOrigin(Rectangle srcRect)
        {
            return new Vector2(srcRect.Width / 2, srcRect.Height / 2);
        }

        #endregion
    }
}
