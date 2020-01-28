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
            return boundingrectangle;
        }

        #region Editable

        enum EdgeEnum
        {
            none, left, right, top, bottom
        }

        [XmlIgnore]
        EdgeEnum edgeundermouse, edgegrabbed;

        [XmlIgnore]
        int initialwidth, initialheight;

        public RectangleItem(Rectangle rect) : base()
        {
            Width = rect.Width;
            Height = rect.Height;
            Transform.Position = rect.Location.ToVector2();            
            FillColor = new Color(192, 0, 192, 145);
        }

        public override GameObject clone()
        {
            var result = (RectangleItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.hovering = false;
            return result;
        }

        public override bool contains(Vector2 worldpos)
        {
            return boundingrectangle.Contains((int)worldpos.X, (int)worldpos.Y);
        }

        [XmlIgnore]
        Vector2[] polygon;

        public override void OnTransformed()
        {
            polygon = new Vector2[4];
            
            Transform.GetLocalMatrix(out var mat);

            var leftTop = new Vector2(0, 0);
            var rightTop = new Vector2(Width, 0);
            var leftBottom = new Vector2(0, Height);
            var rightBottom = new Vector2(Width, Height);

            //var Origin = new Vector2(Width / 2, Height / 2);

            // Transform all four corners into work space
            leftTop = Vector2.Transform(leftTop, mat);
            rightTop = Vector2.Transform(rightTop, mat);
            leftBottom = Vector2.Transform(leftBottom, mat);
            rightBottom = Vector2.Transform(rightBottom, mat);

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

        public override bool onMouseOver(Vector2 mouseworldpos, out string msg)
        {
            //System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + "RectangleItem.onMouseOver()");
            msg = String.Empty;
            var edgewidth = 10;
            if (Math.Abs(mouseworldpos.X - boundingrectangle.Left) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeWE;
                edgeundermouse = EdgeEnum.left;
                return true;
            }
            else if (Math.Abs(mouseworldpos.X - boundingrectangle.Right) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeWE;
                edgeundermouse = EdgeEnum.right;
                return true;
            }
            else if (Math.Abs(mouseworldpos.Y - boundingrectangle.Top) <= edgewidth)
            {
                //MainForm.Instance.picturebox.Cursor = Cursors.SizeNS;
                edgeundermouse = EdgeEnum.top;
                return true;
            }
            else if (Math.Abs(mouseworldpos.Y - boundingrectangle.Bottom) <= edgewidth)
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
                initialwidth = boundingrectangle.Width;
                initialheight = boundingrectangle.Height;
            }
            //else MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }

        public override void onMouseButtonUp(Vector2 mouseworldpos)
        {
            edgegrabbed = EdgeEnum.none;
            base.onMouseButtonUp(mouseworldpos);
        }

        //public override void setPosition(Vector2 pos)
        //{
        //    var delta = pos - Position;
        //    if (pos == Position) return;
        //    switch (edgegrabbed)
        //    {
        //        case EdgeEnum.left:                    
        //            Position = new Vector2(pos.X,Position.Y);
        //            Width -= (int)delta.X;
        //            OnTransformed();
        //            break;
        //        case EdgeEnum.right:
        //            Width = initialwidth + (int)delta.X;
        //            OnTransformed();
        //            break;
        //        case EdgeEnum.top:
        //            Position = new Vector2(Position.X,pos.Y);
        //            Height -= (int)delta.Y;
        //            OnTransformed();
        //            break;
        //        case EdgeEnum.bottom:
        //            Height = initialheight + (int)delta.Y;
        //            OnTransformed();
        //            break;
        //        case EdgeEnum.none:
        //            base.setPosition(pos);
        //            break;
        //    }
        //}

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            var c = hovering ? new Color(255, 0, 0, 228) : FillColor;
            Primitives.Instance.drawBoxFilled(sb, boundingrectangle, c);
        }


        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            Primitives.Instance.drawBox(sb, this.boundingrectangle.Transform(matrix), color, 2);
            var poly = boundingrectangle.Transform(matrix).ToPolygon();
            foreach (var p in poly)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }
            Primitives.Instance.drawBoxFilled(sb, poly[0].X - 5, poly[0].Y - 5, 10, 10, color);
        }

        #endregion
    }
}
