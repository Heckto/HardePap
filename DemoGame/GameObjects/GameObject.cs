using Game1.GameObjects.Levels;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Xml.Serialization;
using Game1.GameObjects.Characters;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;
using Game1.GameObjects.ParticleEffects;
using Game1.GameObjects.Obstacles;
using System.ComponentModel;
using static Game1.GameObjects.Levels.Level;
using Game1.Rendering;
using Game1.GameObjects.Graphics.Effects;

namespace Game1.GameObjects
{
    [XmlInclude(typeof(TextureItem))]
    [XmlInclude(typeof(RectangleItem))]
    [XmlInclude(typeof(CircleItem))]
    [XmlInclude(typeof(PathItem))]
    [XmlInclude(typeof(Ninja1))]
    [XmlInclude(typeof(Zombie1))]
    [XmlInclude(typeof(Zombie2))]
    [XmlInclude(typeof(FireEffect))]
    [XmlInclude(typeof(MovingPlatform))]

    public abstract class GameObject : IEditableGameObject, ICustomTypeDescriptor
    {

        public GameObject()
        {
            Visible = true;
            Transform = new Transform2();
            Transform.TranformUpdated += OnTransformed;
            Transform.TransformBecameDirty += OnTransformed;
            CustomProperties = new SerializableDictionary();
            
        }

        private bool _IsAlive = true;

        [XmlIgnore]
        public bool IsAlive
        {
            get
            {
                return _IsAlive;
            }
            set
            {
                if (!value)
                {
                    layer.level.removeList.Add(this);
                    if (this is IDrawableItem item)
                        layer.renderList[Material.ToString()].Remove(item);
                }
                _IsAlive = value;
            }
        }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Visible;

        public Transform2 Transform { get; set; }

        [XmlIgnore()]
        public Layer layer;


        protected RenderMaterial _Material;

        [XmlIgnore()]
        public RenderMaterial Material 
        {
            get
            {
                return _Material;
            }
            set
            {
                if (this is IDrawableItem)
                {
                    if (_Material != null && layer.renderList.ContainsKey(_Material.ToString()))
                        layer.renderList[_Material.ToString()].Remove(this as IDrawableItem);
                    if (!layer.renderList.ContainsKey(value.ToString()))
                        
                        layer.renderList.Add(value.ToString(), new Renderer(value));
                    if (layer.renderList.ContainsKey(value.ToString()))
                    {
                        layer.renderList[value.ToString()].Add(this as IDrawableItem);
                        _Material = value;
                    }
                }
            }

        } 

        public virtual void LoadContent(ContentManager contentManager) { }

        public virtual void Initialize() {

            Material = RenderMaterial.DefaultMaterial;
            OnTransformed();
        }

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
            return GetType().Name + "_";
        }
        public abstract void OnTransformed();
        public abstract bool contains(Vector2 worldpos);
        
        public abstract void drawInEditor(SpriteBatch sb);
        //public virtual void loadIntoEditor(ContentManager content) { }
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

        #region TypeDescriptor

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var pdc = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(this))
            {
                pdc.Add(pd);
            }

            //put Position property on top
            var posd = pdc["pPosition"];
            pdc.Remove(posd);
            pdc.Insert(0, posd);

            foreach (var key in CustomProperties.Keys)
            {
                pdc.Add(new DictionaryPropertyDescriptor(CustomProperties, key, attributes));
            }
            return pdc;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
