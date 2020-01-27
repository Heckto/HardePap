using AuxLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Xml.Serialization;
using AuxLib.Extensions;

namespace Game1.GameObjects.Levels
{
    public partial class RectangleItem : GameObject
    {
        public float Width { get; set; }
        public float Height { get; set; }

        //[Editor(typeof(XNAColorUITypeEditor), typeof(UITypeEditor))]
        public Color FillColor { get; set; } = new Color(192, 0, 192, 145);
        public ItemTypes ItemType { get; set; } = ItemTypes.None;

        public RectangleItem() { }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }

        #region Editable

        enum EdgeEnum
        {
            none, left, right, top, bottom
        }

        [XmlIgnore]
        EdgeEnum edgeundermouse, edgegrabbed;

        [XmlIgnore]
        Rectangle Rectangle { get { return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height); }  }

        [XmlIgnore]
        int initialwidth, initialheight;

        public RectangleItem(Rectangle rect) : base()
        {
            Position = rect.Location.ToVector2();
            Width = rect.Width;
            Height = rect.Height;
            OnTransformed();
            FillColor = new Color(192, 0, 192, 145);

        }

        public override GameObject clone()
        {
            var result = (RectangleItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.hovering = false;
            return result;
        }

        public override string getNamePrefix()
        {
            return "Rectangle_";
        }

        public override bool contains(Vector2 worldpos)
        {
            return Rectangle.Contains(new Point((int)worldpos.X, (int)worldpos.Y));
        }


        public override void OnTransformed()
        {
            /*Rectangle.Location = Position.ToPoint();
            Rectangle.Width = (int)Width;
            Rectangle.Height = (int)Height;*/
        }

        public override bool onMouseOver(Vector2 mouseworldpos, out string msg)
        {
            //System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + "RectangleItem.onMouseOver()");
            msg = String.Empty;
            var edgewidth = 10;
            if (Math.Abs(mouseworldpos.X - Rectangle.Left) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeWE;
                edgeundermouse = EdgeEnum.left;
                return true;
            }
            else if (Math.Abs(mouseworldpos.X - Rectangle.Right) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeWE;
                edgeundermouse = EdgeEnum.right;
                return true;
            }
            else if (Math.Abs(mouseworldpos.Y - Rectangle.Top) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeNS;
                edgeundermouse = EdgeEnum.top;
                return true;
            }
            else if (Math.Abs(mouseworldpos.Y - Rectangle.Bottom) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeNS;
                edgeundermouse = EdgeEnum.bottom;
                return true;
            }
            else
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.Default;
                edgeundermouse = EdgeEnum.none;
                return false;
            }
            //return false;
            //base.onMouseOver(mouseworldpos);
        }

        public override void onMouseOut()
        {
            //System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + "RectangleItem.onMouseOut()");
            base.onMouseOut();
        }

        public override void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;
            if (edgeundermouse != EdgeEnum.none)
            {
                edgegrabbed = edgeundermouse;
                initialwidth = Rectangle.Width;
                initialheight = Rectangle.Height;
            }
            //else MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }

        public override void onMouseButtonUp(Vector2 mouseworldpos)
        {
            edgegrabbed = EdgeEnum.none;
            base.onMouseButtonUp(mouseworldpos);
        }

        public override void setPosition(Vector2 pos)
        {
            var delta = pos - Position;
            if (pos == Position) return;
            switch (edgegrabbed)
            {
                case EdgeEnum.left:                    
                    Position = new Vector2(pos.X,Position.Y);
                    Width -= (int)delta.X;
                    OnTransformed();
                    break;
                case EdgeEnum.right:
                    Width = initialwidth + (int)delta.X;
                    OnTransformed();
                    break;
                case EdgeEnum.top:
                    Position = new Vector2(Position.X,pos.Y);
                    Height -= (int)delta.Y;
                    OnTransformed();
                    break;
                case EdgeEnum.bottom:
                    Height = initialheight + (int)delta.Y;
                    OnTransformed();
                    break;
                case EdgeEnum.none:
                    base.setPosition(pos);
                    break;
            }
        }

        public override bool CanScale()
        {
            return true;
        }

        public override Vector2 getScale()
        {
            return new Vector2(Width, Width);
        }

        public override void setScale(Vector2 scale)
        {
            var factor = scale.X / Width;
            Width = (float)Math.Round(scale.X);
            Height = (float)Math.Round(Height * factor);
            base.setScale(scale);
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;

            var c = FillColor;
            //if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            if (hovering)
                c = new Color(255, 0, 0, 228); 
            Primitives.Instance.drawBoxFilled(sb, Rectangle, c);
        }


        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {

            Primitives.Instance.drawBox(sb, this.Rectangle.Transform(matrix), color, 2);

            var poly = Rectangle.Transform(matrix).ToPolygon();

            foreach (var p in poly)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }

            Primitives.Instance.drawBoxFilled(sb, poly[0].X - 5, poly[0].Y - 5, 10, 10, color);



        }

        #endregion
    }
}
