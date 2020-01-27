using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using CustomUITypeEditors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Windows.Forms;
using AuxLib;
using Game1.GameObjects.Levels;
using LevelEditor.Items;
using Game1.GameObjects;

namespace LevelEditor
{
    public partial class EditableCircleItem : CircleItem
    {
        CircleItem circleItem;

        [DisplayName("Radius"), Category(" General")]
        [XmlIgnore()]
        public float pRadius { get { return Radius; } set { Radius = value; } }

        [DisplayName("FillColor"), Category(" General")]
        [Editor(typeof(XNAColorUITypeEditor), typeof(UITypeEditor))]
        [XmlIgnore()]
        public Color pFillColor { get { return FillColor; } set { FillColor = value; } }

        public EditableCircleItem(Vector2 startpos, float radius)
            : base()
        {
            this.Position = startpos;
            this.Radius = radius;
            this.FillColor = Constants.Instance.ColorPrimitives;
        }

        public Item clone()
        {
            var result = (CircleItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.hovering = false;
            return result;
        }

        public string getNamePrefix()
        {
            return "Circle_";
        }

        public bool contains(Vector2 worldpos)
        {
            return (worldpos - Position).Length() <= Radius;
        }


        public void OnTransformed()
        {
        }


        public void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;
            MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }


        public bool CanScale()
        {
            return true;
        }

        public Vector2 getScale()
        {
            return new Vector2(pRadius, pRadius);
        }

        public void setScale(Vector2 scale)
        {
            pRadius = (float)Math.Round(scale.X);
        }

        public void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            var c = FillColor;
            if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            Primitives.Instance.drawCircleFilled(sb, Position, Radius, c);
        }


        public void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {

            var transformedPosition = Vector2.Transform(Position, matrix);
            var transformedRadius = Vector2.TransformNormal(Vector2.UnitX * Radius, matrix);
            Primitives.Instance.drawCircle(sb, transformedPosition, transformedRadius.Length(), color, 2);

            var extents = new Vector2[4];
            extents[0] = transformedPosition + Vector2.UnitX * transformedRadius.Length();
            extents[1] = transformedPosition + Vector2.UnitY * transformedRadius.Length();
            extents[2] = transformedPosition - Vector2.UnitX * transformedRadius.Length();
            extents[3] = transformedPosition - Vector2.UnitY * transformedRadius.Length();

            foreach (var p in extents)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }

            var origin = Vector2.Transform(pPosition, matrix);
            Primitives.Instance.drawBoxFilled(sb, origin.X - 5, origin.Y - 5, 10, 10, color);

        }

    }
}
