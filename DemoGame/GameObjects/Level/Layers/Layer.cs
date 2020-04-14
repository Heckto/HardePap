using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.ComponentModel;
using Game1.GameObjects.Graphics.PostProcessing;
using static Game1.GameObjects.Levels.Level;
using Game1.Rendering;
using AuxLib.Camera;
using Microsoft.Xna.Framework.Content;

namespace Game1.GameObjects.Levels
{
    [XmlInclude(typeof(MovingLayer))]
    public partial class Layer : ICustomTypeDescriptor
    {
        [XmlIgnore]
        public readonly List<PostProcessor> _postProcessors = new List<PostProcessor>();

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Visible { get; set; }

        public List<GameObject> Items;

        public Vector2 ScrollSpeed { get; set; }

        [XmlIgnore]
        public Dictionary<string, Renderer> renderList;

        public SerializableDictionary CustomProperties;

        public void LoadContent(ContentManager contentManager)
        {
            renderList = new Dictionary<string, Renderer>() {
                { RenderMaterial.DefaultMaterial.ToString() ,new Renderer(RenderMaterial.DefaultMaterial) }
            };
        }

        public Layer() : base()
        {
            Items = new List<GameObject>();
            ScrollSpeed = Vector2.One;
            CustomProperties = new SerializableDictionary();
        }

        public virtual void Update(GameTime gameTime,Level lvl)
        {
            for (var idx = 0; idx < Items.Count; idx++)
            {
                if (Items[idx] is IUpdateableItem updateItem)
                    updateItem.Update(gameTime, lvl);
            }

            for (var idx = 0; idx < _postProcessors.Count; idx++)
                _postProcessors[idx].Update(gameTime);
        }

        public void Draw(SpriteBatch sb, FocusCamera camera,RenderTarget2D renderTarget)
        {
            foreach (var entry in renderList)
            {
                if (entry.Value.getRenderCount() > 0)
                    entry.Value.Render(sb, camera.getViewMatrix(ScrollSpeed));
            }

            if (_postProcessors.Count > 0)
            {
                for(var i=0; i < _postProcessors.Count; i++)
                {
                    _postProcessors[i].Process(sb, renderTarget);
                }
            }
        }

        public void AddObject(GameObject obj)
        {
            obj.layer = this;
            Items.Add(obj);
        }

        #region EDITOR

        [XmlIgnore]
        public Level level;

        public Layer(String name) : this()
        {
            this.Name = name;
            this.Visible = true;
        }

        public Layer clone()
        {
            var result = (Layer)this.MemberwiseClone();
            result.Items = new List<GameObject>(Items);
            for (var i = 0; i < result.Items.Count; i++)
            {
                result.Items[i] = result.Items[i].clone();
                result.Items[i].layer = result;
            }
            return result;
        }



        public GameObject getItemAtPos(Vector2 mouseworldpos)
        {
            for (var i = Items.Count - 1; i >= 0; i--)
            {                    
                if (Items[i].contains(mouseworldpos) && Items[i].Visible) return Items[i];
            }
            return null;
        }

        public void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            foreach (var item in Items)
            {
                    item.drawInEditor(sb);
            }


        }

        #endregion

        #region Typedescriptor

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
