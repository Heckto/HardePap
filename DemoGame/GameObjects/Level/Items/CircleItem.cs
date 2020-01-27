using AuxLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.GameObjects.Levels
{
    public partial class CircleItem : GameObject
    {
        public float Radius;
        public Color FillColor;
        public ItemTypes ItemType { get; set; } = ItemTypes.None;

        public CircleItem() { }

        public CircleItem(Vector2 startpos, float radius)
            : base()
        {
            this.Position = startpos;
            this.Radius = radius;
            this.FillColor = new Color(192, 0, 192, 145);
            
        }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)(Position.X - 0.5f * Radius), (int)(Position.X - 0.5f * Radius), (int)(2 * Radius), (int)(2 * Radius));
        }


        #region Editable
        public override GameObject clone()
        {
            var result = (CircleItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.hovering = false;
            return result;
        }

        public override string getNamePrefix()
        {
            return "Circle_";
        }

        public override bool contains(Vector2 worldpos)
        {
            return (worldpos - Position).Length() <= Radius;
        }


        public override void OnTransformed()
        {
        }


        public override void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;            
            base.onMouseButtonDown(mouseworldpos);
        }


        public override bool CanScale()
        {
            return true;
        }

        public override Vector2 getScale()
        {
            return new Vector2(Radius, Radius);
        }

        public override void setScale(Vector2 scale)
        {
            Radius = (float)Math.Round(scale.X);
            base.setScale(scale);
        }

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            var c = FillColor;
            //if (hovering && Constants.Instance.EnableHighlightOnMouseOver) c = Constants.Instance.ColorHighlight;
            if (hovering)
                //c = Constants.Instance.ColorHighlight;
                c = new Color(255, 0, 0, 228);
            Primitives.Instance.drawCircleFilled(sb, Position, Radius, c);
        }


        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
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

            var origin = Vector2.Transform(Position, matrix);
            Primitives.Instance.drawBoxFilled(sb, origin.X - 5, origin.Y - 5, 10, 10, color);

        }

        #endregion
    }
}
