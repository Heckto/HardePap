using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using Game1.GameObjects.Levels;

namespace Game1.GameObjects
{
    public class CustomProperty
    {
        public string name;
        public object value;
        public Type type;
        public string description;

        public CustomProperty()
        {
        }

        public CustomProperty(string n, object v, Type t, string d)
        {
            name = n;
            value = v;
            type = t;
            description = d;
        }

        public CustomProperty clone()
        {
            var result = new CustomProperty(name, value, type, description);
            return result;
        }
    }


    public class SerializableDictionary : Dictionary<String, CustomProperty>, IXmlSerializable
    {

        public SerializableDictionary() : base() { }

        public SerializableDictionary(SerializableDictionary copyfrom) : base(copyfrom)
        {
            var keyscopy = new string[Keys.Count];
            Keys.CopyTo(keyscopy, 0);
            foreach (var key in keyscopy)
            {
                this[key] = this[key].clone();
            }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {

            var wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty) return;

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                var cp = new CustomProperty
                {
                    name = reader.GetAttribute("Name"),
                    description = reader.GetAttribute("Description")
                };

                var type = reader.GetAttribute("Type");
                if (type == "string") cp.type = typeof(string);
                if (type == "bool") cp.type = typeof(bool);
                if (type == "Vector2") cp.type = typeof(Vector2);
                if (type == "Color") cp.type = typeof(Color);
                if (type == "Item") cp.type = typeof(GameObject);
                if (type == "Rectangle") cp.type = typeof(Rectangle);
                if (cp.type == typeof(GameObject))
                {
                    cp.value = reader.ReadInnerXml();
                    Add(cp.name, cp);
                }
                else
                {
                    reader.ReadStartElement("Property");
                    var valueSerializer = new XmlSerializer(cp.type);
                    var obj = valueSerializer.Deserialize(reader);
                    cp.value = Convert.ChangeType(obj, cp.type);
                    Add(cp.name, cp);
                    reader.ReadEndElement();
                }

                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var key in Keys)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("Name", this[key].name);
                if (this[key].type == typeof(string)) writer.WriteAttributeString("Type", "string");
                if (this[key].type == typeof(bool)) writer.WriteAttributeString("Type", "bool");
                if (this[key].type == typeof(Vector2)) writer.WriteAttributeString("Type", "Vector2");
                if (this[key].type == typeof(Color)) writer.WriteAttributeString("Type", "Color");
                if (this[key].type == typeof(GameObject)) writer.WriteAttributeString("Type", "Item");
                writer.WriteAttributeString("Description", this[key].description);

                if (this[key].type == typeof(GameObject))
                {
                    var item = (GameObject)this[key].value;
                    if (item != null) writer.WriteString(item.Name);
                    else writer.WriteString("$null$");
                }
                else
                {
                    var valueSerializer = new XmlSerializer(this[key].type);
                    valueSerializer.Serialize(writer, this[key].value);
                }
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Must be called after all Items have been deserialized. 
        /// Restores the Item references in CustomProperties of type Item.
        /// </summary>
        public void RestoreItemAssociations(Level level)
        {
            foreach (var cp in Values)
            {
                if (cp.type == typeof(GameObject)) cp.value = level.GetItemByName((string)cp.value);
            }
        }
    }
}
