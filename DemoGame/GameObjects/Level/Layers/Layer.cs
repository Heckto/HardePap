using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using Game1.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing.Design;
using System.ComponentModel;
using static Game1.GameObjects.Levels.Level;

namespace Game1.GameObjects.Levels
{
    [XmlInclude(typeof(MovingLayer))]
    public partial class Layer : ICustomTypeDescriptor
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Visible { get; set; }

        public List<GameObject> Items;

        public Vector2 ScrollSpeed { get; set; }

        public SerializableDictionary CustomProperties;
        public Layer() : base()
        {
            Items = new List<GameObject>();
            ScrollSpeed = Vector2.One;
            CustomProperties = new SerializableDictionary();
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
