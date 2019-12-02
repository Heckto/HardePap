using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CustomUITypeEditors;
using System.Windows.Forms;
using System.IO;

namespace LevelEditor
{

    public partial class TextureItem
    {
        //for per-pixel-collision
        Color[] coldata;
        Matrix transform;
        Rectangle boundingrectangle;    //bounding rectangle in world space, for collision broadphase
        
        Vector2[] polygon;              //selection box: drawn when selected

        [XmlIgnore()]
        [DisplayName("Origin"), Category(" General")]
        [Description("The item's origin in texture space ([0,0] is upper left corner).")]
        public Vector2 pOrigin
        {
            get
            {
                return Origin;
            }
            set
            {
                Origin = value;
                OnTransformed();
            }
        }

        [XmlIgnore()]
        [DisplayName("Rotation"), Category(" General")]
        [Description("The item's rotation in radians.")]
        public float pRotation
        {
            get
            {
                return Rotation;
            }
            set
            {
                Rotation = value;
                OnTransformed();
            }
        }

        [XmlIgnore()]
        [DisplayName("Scale"), Category(" General")]
        [Description("The item's scale vector.")]
        public Vector2 pScale
        {
            get
            {
                return Scale;
            }
            set
            {
                Scale = value;
                OnTransformed();
            }
        }
        
        [XmlIgnore()]
        [DisplayName("TintColor"), Category(" General")]
        [Editor(typeof(XNAColorUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Description("The Color to tint the texture with. Use white for no tint.")]
        public Color pTintColor
        {
            get
            {
                return TintColor;
            }
            set
            {
                TintColor = value;
            }
        }
        
        [XmlIgnore()]
        [DisplayName("FlipHorizontally"), Category(" General")]
        [Description("If true, the texture is flipped horizontally when drawn.")]
        public bool pFlipHorizontally {
            get { return FlipHorizontally; }
            set { FlipHorizontally = value; }
        }

        [XmlIgnore()]
        [DisplayName("FlipVertically"), Category(" General")]
        [Description("If true, the texture is flipped vertically when drawn.")]
        public bool pFlipVertically {
            get { return FlipVertically; }
            set { FlipVertically = value; }
        }

        public TextureItem(String fullpath, Vector2 position, Rectangle srcRect) : base()
        {
            this.texture_filename = fullpath;
            this.asset_name = Path.GetFileNameWithoutExtension(fullpath);
            this.Position = position;
            this.Rotation = 0;
            this.Scale = Vector2.One;
            this.TintColor = Color.White;
            FlipHorizontally = FlipVertically = false;
            this.srcRectangle = srcRect;
            this.Origin = getTextureOrigin(srcRect);

            //compensate for origins that are not at the center of the texture
            var center = new Vector2(srcRect.Width / 2, srcRect.Height / 2);
            this.Position -= (center - Origin);

            OnTransformed();
        }

        public override Item clone()
        {
            var result = (TextureItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.polygon = (Vector2[])polygon.Clone();
            result.hovering = false;
            return result;
        }

        public override string getNamePrefix()
        {
            return "Texture_";
        }

        public override void OnTransformed()
        {
            
            coldata = new Color[srcRectangle.Width * srcRectangle.Height];
            MainForm.Instance.spriteSheets[texture_filename].Texture.GetData<Color>(0, srcRectangle, coldata, 0, srcRectangle.Width * srcRectangle.Height);

            polygon = new Vector2[4];


            transform =
                Matrix.CreateTranslation(new Vector3(-Origin.X, -Origin.Y, 0.0f)) *
                Matrix.CreateScale(Scale.X, Scale.Y, 1) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateTranslation(new Vector3(Position, 0.0f));

            var leftTop = new Vector2(0, 0);
            var rightTop = new Vector2(srcRectangle.Width, 0);
            var leftBottom = new Vector2(0, srcRectangle.Height);
            var rightBottom = new Vector2(srcRectangle.Width, srcRectangle.Height);

            // Transform all four corners into work space
            Vector2.Transform(ref leftTop, ref transform, out leftTop);
            Vector2.Transform(ref rightTop, ref transform, out rightTop);
            Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
            Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

            //if (polygon == null)
            //    polygon = new Vector2[4];

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
            MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }


        public override bool CanRotate()
        {
            return true;
        }

        public override float getRotation()
        {
            return pRotation;
        }

        public override void setRotation(float rotation)
        {
            pRotation = rotation;
        }


        public override bool CanScale()
        {
            return true;
        }

        public override Vector2 getScale()
        {
            return pScale;
        }

        public override void setScale(Vector2 scale)
        {
            pScale = scale;
        }


        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;

            var se = SpriteEffects.None;
            if (pFlipHorizontally) se |= SpriteEffects.FlipHorizontally;
            if (pFlipVertically) se |= SpriteEffects.FlipVertically;
            var c = TintColor;
            if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            sb.Draw(MainForm.Instance.spriteSheets[texture_filename].Texture, Position, srcRectangle, c, Rotation, Origin, Scale, se, 0);
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
            var origin = Vector2.Transform(pPosition, matrix);
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
            var positionInB = Vector2.Transform(worldpos, Matrix.Invert(transform));
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
            switch (Constants.Instance.DefaultTextureOriginMethod)
            {
                case TextureOriginMethodEnum.TextureCenter:
                    return new Vector2(srcRect.Width / 2, srcRect.Height / 2);
                case TextureOriginMethodEnum.TopLeft:
                    return new Vector2(0, 0);
                case TextureOriginMethodEnum.TopRight:
                    return new Vector2(srcRect.Width, 0);
                case TextureOriginMethodEnum.BottomLeft:
                    return new Vector2(0, srcRect.Height);
                case TextureOriginMethodEnum.BottomRight:
                    return new Vector2(srcRect.Width, srcRect.Height);
            }
            return Vector2.Zero;
        }


        
        


    }










}
