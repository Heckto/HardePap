using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Xml.Serialization;
using Game1.GameObjects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing.Design;
using System.ComponentModel;

namespace Game1.GameObjects.Levels
{
    [XmlInclude(typeof(MovingLayer))]
    public partial class Layer
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
    }
}
