using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Xml.Serialization;
using Game1.GameObjects.Characters;
using Microsoft.Xna.Framework.Content;

namespace Game1.GameObjects
{
    [XmlInclude(typeof(TextureItem))]
    [XmlInclude(typeof(RectangleItem))]
    [XmlInclude(typeof(CircleItem))]
    [XmlInclude(typeof(PathItem))]
    [XmlInclude(typeof(Ninja1))]
    [XmlInclude(typeof(Zombie1))]
    [XmlInclude(typeof(Zombie2))]

    public abstract class GameObject : IEditableGameObject
    {

        public GameObject()
        {
            CustomProperties = new SerializableDictionary();
        }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Visible;

        public Vector2 Position { get; set; }

        [XmlIgnore()]
        public Layer layer;

        public SerializableDictionary CustomProperties;

        public abstract Rectangle getBoundingBox();

        #region Editing

        [XmlIgnore]
        protected bool hovering;
        public abstract GameObject clone();
        public abstract string getNamePrefix();
        public abstract void OnTransformed();
        public abstract bool contains(Vector2 worldpos);
        public virtual bool CanRotate()
        {
            return false;
        }
        public virtual float getRotation()
        {
            return 0;
        }
        public virtual void setRotation(float rotation)
        {
            OnTransformed();
        }
        public abstract bool CanScale();
        public virtual Vector2 getScale()
        {
            return Vector2.One;
        }
        public virtual void setScale(Vector2 scale)
        {
            OnTransformed();
        }
        public abstract void drawInEditor(SpriteBatch sb);
        public virtual void loadIntoEditor(ContentManager content) { }
        public abstract void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color);
        public virtual void setPosition(Vector2 pos)
        {
            Position = pos;
            OnTransformed();
        }

        public virtual bool onMouseOver(Vector2 mouseworldpos, out string msg)
        {
            msg = String.Empty;
            hovering = true;
            return true;
        }

        public virtual void onMouseOut()
        {
            hovering = false;
        }

        public virtual void onMouseButtonDown(Vector2 mouseworldpos)
        {
        }

        public virtual void onMouseButtonUp(Vector2 mouseworldpos)
        {
        }

        #endregion
    }
}
