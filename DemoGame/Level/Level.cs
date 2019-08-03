﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using Game1.CollisionDetection;
using AuxLib;
using AuxLib.Debug;
using AuxLib.Rand;
using AuxLib.Sound;
using AuxLib.Camera;
using Microsoft.Xna.Framework.Media;

namespace Game1
{
    public partial class Level
    {
        public delegate void HandlePlayerDrawDelegate(SpriteBatch sb, BoundedCamera camera);
        public event HandlePlayerDrawDelegate HandlePlayerDraw;


        /// <summary>
        /// The name of the level.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// A Level contains several Layers. Each Layer contains several Items.
        /// </summary>
        public List<Layer> Layers;

        [XmlIgnore]
        public World CollisionWorld;

        [XmlIgnore]
        public Dictionary<string, Texture2D> spritesheets;

        [XmlIgnore]
        public Rectangle Bounds;

        /// <summary>
        /// A Dictionary containing any user-defined Properties.
        /// </summary>
        public SerializableDictionary CustomProperties;

        public String ContentPath { get; set; } = String.Empty;

        public Level()
        {
            Visible = true;
            Layers = new List<Layer>();
            CustomProperties = new SerializableDictionary();

        }

        public static Level FromFile(string filename)
        {
            var stream = File.Open(filename, FileMode.Open);
            var serializer = new XmlSerializer(typeof(Level));
            var level = (Level)serializer.Deserialize(stream);
            stream.Close();

            foreach (var layer in level.Layers)
            {
                foreach (var item in layer.Items)
                {
                    item.CustomProperties.RestoreItemAssociations(level);
                }
            }
            return level;
        }

        public void LoadContent(ContentManager cm)
        {
            spritesheets = new Dictionary<string, Texture2D>();
            foreach (var layer in Layers)
            {
                foreach (var item in layer.Items)
                {
                    if (item is TextureItem)
                    {
                        var asset = (item as TextureItem).asset_name;
                        var assetName = ContentPath + "/" + asset;
                        if (!spritesheets.ContainsKey(asset))
                        {
                            var texture = cm.Load<Texture2D>(assetName);
                            spritesheets.Add(asset, texture);
                        }
                    }

                }
            }

            Bounds = (Rectangle)CustomProperties["bounds"].value;

            var song = "level" + Rand.GetRandomInt(1, 3);
            AudioManager.PlaySoundTrack(song, true, false);
            AudioManager.MusicVolume = 0.1f;

        }

        public void GenerateCollision()
        {
            CollisionWorld = new World(Bounds);

            var l = Layers.FirstOrDefault(elem => elem.Name == "collision");
            foreach (var elem in l.Items)
            {
                if (elem is RectangleItem)
                {
                    var rec = elem as RectangleItem;
                    var box = CollisionWorld.CreateRectangle(rec.Position.X, rec.Position.Y, rec.Width, rec.Height).AddTags(rec.ItemType);
                    if (rec.ItemType == ItemTypes.Transition)
                    {
                        box.Data = rec;
                    }
                }
                else if (elem is PathItem)
                {
                    var path = elem as PathItem;
                    var newList = path.WorldPoints.Select(v => new Vector2f(v.X, v.Y));
                    CollisionWorld.CreatePolyLine(newList.ToArray()).AddTags(ItemTypes.PolyLine);
                }
            }
        }

        public Item getItemByName(string name)
        {
            foreach (var layer in Layers)
            {
                foreach (var item in layer.Items)
                {
                    if (item.Name == name)
                        return item;
                }
            }
            return null;
        }

        public Layer getLayerByName(string name)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == name)
                    return layer;
            }
            return null;
        }

        public void Update(GameTime gameTime,BoundedCamera cam)
        {
            foreach (var layer in Layers)
            {
                layer.Update(gameTime,this,cam);
            }
        }

        public void Draw(SpriteBatch sb, BoundedCamera camera)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == "collision")
                {
                    HandlePlayerDraw?.Invoke(sb, camera);
                }
                else
                {
                    if (layer.Visible)
                    {
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix(layer.ScrollSpeed));
                        {
                            foreach (var item in layer.Items)
                            {
                                if (item.Visible && item is TextureItem)
                                {
                                    var texItem = item as TextureItem;
                                    var effects = SpriteEffects.None;
                                    if (texItem.FlipHorizontally) effects |= SpriteEffects.FlipHorizontally;
                                    if (texItem.FlipVertically) effects |= SpriteEffects.FlipVertically;
                                    sb.Draw(spritesheets[texItem.asset_name], item.Position, texItem.srcRectangle, texItem.TintColor, texItem.Rotation, texItem.Origin, texItem.Scale, effects, 0);
                                }

                            }
                        }
                        sb.End();
                    }
                }
            }

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());
            sb.DrawStroke(Bounds,Color.Black);
            sb.End();
        }

        public void DrawDebug(SpriteBatch sb, SpriteFont font, BoundedCamera camera)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());
            CollisionWorld.DrawDebug(sb, font, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);
            sb.End();
        }
    }

    [XmlInclude(typeof(MovingLayer))]
    public partial class Layer
    {
        /// <summary>
        /// The name of the layer.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        /// <summary>
        /// Should this layer be visible?
        /// </summary>
        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// The list of the items in this layer.
        /// </summary>
        public List<Item> Items;

        /// <summary>
        /// The Scroll Speed relative to the main camera. The X and Y components are 
        /// interpreted as factors, so (1;1) means the same scrolling speed as the main camera.
        /// Enables parallax scrolling.
        /// </summary>
        public Vector2 ScrollSpeed;

        
        /// <summary>
        /// A Dictionary containing any user-defined Properties.
        /// </summary>
        public SerializableDictionary CustomProperties;


        public Layer()
        {
            Items = new List<Item>();
            ScrollSpeed = Vector2.One;
            CustomProperties = new SerializableDictionary();
        }

        public virtual void Update(GameTime gameTime,Level lvl,BoundedCamera cam)
        {
            foreach (var item in Items)
            {
                item.Update(gameTime);
            }

        }
    }

    public class MovingLayer : Layer
    {
        public Vector2 ScrollVector { get; set; }

        public override void Update(GameTime gameTime, Level lvl,BoundedCamera cam)
        {
            var mat = Matrix.Invert(cam.GetViewMatrix(ScrollSpeed));
            var mapSize = new Vector2(lvl.Bounds.Width, lvl.Bounds.Height);
            foreach (var Item in Items)
            {
                var scrollVector = new Vector2(-20,0);
                
                var scrollAss = scrollVector;
                scrollAss.Normalize();                
                Item.Position += scrollVector;
                var itemBox = Item.getBoundingBox();

                var pos = Vector2.Transform(Item.Position, mat);
                var rect = new Rectangle((int)pos.X, (int)pos.Y, (int)(itemBox.Width), (int)(itemBox.Height));
                //var pos = Item.Position
                //itemBox.X = (int)(itemBox.X * ScrollSpeed.X);
                //itemBox.Y = (int)(itemBox.Y * ScrollSpeed.Y);
                if (!rect.Intersects(lvl.Bounds))
                {                    
                    var posVector = Vector2.Min(scrollAss * mapSize, mapSize);
                    var u = Vector2.Transform(posVector, mat);
                    
                    Item.Position -= (u);
                }
            }
        }
    }


    [XmlInclude(typeof(TextureItem))]
    [XmlInclude(typeof(RectangleItem))]
    [XmlInclude(typeof(CircleItem))]
    [XmlInclude(typeof(PathItem))]
    public abstract partial class Item
    {
        /// <summary>
        /// The name of this item.
        /// </summary>
        [XmlAttribute()]
        public String Name;

        /// <summary>
        /// Should this item be visible?
        /// </summary>
        [XmlAttribute()]
        public bool Visible;

        /// <summary>
        /// The item's position in world space.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// A Dictionary containing any user-defined Properties.
        /// </summary>
        public SerializableDictionary CustomProperties;


        public Item()
        {
            CustomProperties = new SerializableDictionary();
        }

        public abstract Rectangle getBoundingBox();
        public abstract void Update(GameTime gameTime);
    }


    public partial class TextureItem : Item
    {
        /// <summary>
        /// The item's rotation in radians.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The item's scale vector.
        /// </summary>
        public Vector2 Scale;

        /// <summary>
        /// The color to tint the item's texture with (use white for no tint).
        /// </summary>
        public Color TintColor;

        /// <summary>
        /// If true, the texture is flipped horizontally when drawn.
        /// </summary>
        public bool FlipHorizontally;

        /// <summary>
        /// If true, the texture is flipped vertically when drawn.
        /// </summary>
        public bool FlipVertically;

        /// <summary>
        /// The path to the texture's filename (including the extension) relative to ContentRootFolder.
        /// </summary>
        public String texture_filename;

        /// <summary>
        /// The texture_filename without extension. For using in Content.Load<Texture2D>().
        /// </summary>
        public String asset_name;

        /// <summary>
        /// The XNA texture to be drawn. Can be loaded either from file (using "texture_filename") 
        /// or via the Content Pipeline (using "asset_name") - then you must ensure that the texture
        /// exists as an asset in your project.
        /// Loading is done in the Item's load() method.
        /// </summary>
        public Rectangle srcRectangle;

        /// <summary>
        /// The item's origin relative to the upper left corner of the texture. Usually the middle of the texture.
        /// Used for placing and rotating the texture when drawn.
        /// </summary>
        public Vector2 Origin;


        public TextureItem() { }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)(srcRectangle.Width * Scale.X), (int)(srcRectangle.Height * Scale.Y));
        }
        
            

        public override void Update(GameTime gameTime)
        {
            
        }
    }


    public partial class RectangleItem : Item
    {
        public float Width;
        public float Height;
        public Color FillColor;
        public ItemTypes ItemType { get; set; }

        public RectangleItem() {}

        public override void Update(GameTime gameTime) {}

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, (int)Width, (int)Height);
        }
    }


    public partial class CircleItem : Item
    {
        public float Radius;
        public Color FillColor;
        public ItemTypes ItemType { get; set; }

        public CircleItem() {}

        public override void Update(GameTime gameTime)
        {            
        }

        public override Rectangle getBoundingBox()
        {
            return new Rectangle((int)(Position.X - 0.5f * Radius), (int)(Position.X - 0.5f * Radius), (int)(2 * Radius), (int)(2 * Radius));
        }
    }


    public partial class PathItem : Item
    {
        public Vector2[] LocalPoints;
        public Vector2[] WorldPoints;
        public bool IsPolygon;
        public int LineWidth;
        public Color LineColor;
        public ItemTypes ItemType { get; set; }

        public PathItem() {}

        public override void Update(GameTime gameTime)
        {
            
        }

        public override Rectangle getBoundingBox()
        {
            var minX = WorldPoints.Min(min => min.X);
            var maxX = WorldPoints.Max(max => max.X);
            var minY = WorldPoints.Min(min => min.Y);
            var maxY = WorldPoints.Max(max => max.Y);

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }
    }


    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////
    //
    //    NEEDED FOR SERIALIZATION. YOU SHOULDN'T CHANGE ANYTHING BELOW!
    //
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////


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

        public SerializableDictionary() : base() {}

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
                var cp = new CustomProperty();
                cp.name = reader.GetAttribute("Name");
                cp.description = reader.GetAttribute("Description");

                var type = reader.GetAttribute("Type");
                if (type == "string") cp.type = typeof(string);
                if (type == "bool") cp.type = typeof(bool);
                if (type == "Vector2") cp.type = typeof(Vector2);
                if (type == "Color") cp.type = typeof(Color);
                if (type == "Item") cp.type = typeof(Item);
                if (type == "Rectangle") cp.type = typeof(Rectangle);
                if (cp.type == typeof(Item))
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
                if (this[key].type == typeof(Item)) writer.WriteAttributeString("Type", "Item");
                writer.WriteAttributeString("Description", this[key].description);

                if (this[key].type == typeof(Item))
                {
                    var item = (Item)this[key].value;
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
                if (cp.type == typeof(Item)) cp.value = level.getItemByName((string)cp.value);
            }
        }


    }






}