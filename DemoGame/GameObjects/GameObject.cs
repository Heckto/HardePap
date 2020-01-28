using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Xml.Serialization;
using Game1.GameObjects.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;

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
            Transform = new Transform2();

            Transform.TranformUpdated += OnTransformed;
            Transform.TransformBecameDirty += OnTransformed;
            CustomProperties = new SerializableDictionary();
        }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Visible;


        public Transform2 Transform { get; set; }

        [XmlIgnore()]
        public Layer layer;

        [XmlIgnore]
        protected Rectangle boundingrectangle;    //bounding rectangle in world space, for collision broadphase

        public SerializableDictionary CustomProperties;

        public abstract Rectangle getBoundingBox();

        #region Editing

        [XmlIgnore]
        protected bool hovering;
        public abstract GameObject clone();
        public virtual string getNamePrefix()
        {
            return this.GetType().Name + "_";
        }
        public abstract void OnTransformed();
        public abstract bool contains(Vector2 worldpos);
        
        public abstract void drawInEditor(SpriteBatch sb);
        public virtual void loadIntoEditor(ContentManager content) { }
        public abstract void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color);
        
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
