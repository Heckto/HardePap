using System;
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
using AuxLib.Rand;
using AuxLib.Sound;
using AuxLib.Camera;
using Game1.Sprite;
using Game1.DataContext;

namespace Game1.Levels
{
    public partial class Level
    {
        [XmlIgnore]
        public readonly Dictionary<string, SpriteObject> Sprites = new Dictionary<string, SpriteObject>();

        [XmlAttribute()]
        public String Name;

        [XmlAttribute()]
        public bool Visible;
        public List<Layer> Layers;

        [XmlIgnore]
        public World CollisionWorld;

        [XmlIgnore]
        public Dictionary<string, Texture2D> spritesheets;

        [XmlIgnore]
        public Rectangle CamBounds;

        [XmlIgnore]
        public Rectangle LevelBounds;

        public SerializableDictionary CustomProperties;

        public String ContentPath { get; set; } = String.Empty;

        [XmlIgnore]
        public List<IUpdateableItem> updateList = new List<IUpdateableItem>();

        [XmlIgnore]
        private ContentManager Content;

        [XmlIgnore]
        public GameContext context;

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

                    if (item is IUpdateableItem)
                        level.updateList.Add(item as IUpdateableItem);
                }

                if (layer is IUpdateableItem)
                    level.updateList.Add(layer as IUpdateableItem);
            }
            return level;
        }

        public void LoadContent(ContentManager cm)
        {
            Content = cm;
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

            CamBounds = (Rectangle)CustomProperties["bounds"].value;

            LevelBounds = (Rectangle)CustomProperties["bounds"].value;
            LevelBounds.Inflate((int)(0.05 * CamBounds.Width), (int)(0.05 * CamBounds.Height));
            var song = "level" + Rand.GetRandomInt(1, 4);
            AudioManager.PlaySoundTrack(song, true, false);
            AudioManager.MusicVolume = 0.1f;           
        }

        public void GenerateCollision()
        {
            CollisionWorld = new World(LevelBounds);

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

        public Item GetItemByName(string name)
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

        public Layer GetLayerByName(string name)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == name)
                    return layer;
            }
            return null;
        }

        public void Update(GameTime gameTime)
        {
            foreach (var updateItem in updateList)
            {
                updateItem.Update(gameTime, this);
            }
            foreach (var sprite in Sprites.ToList())
                sprite.Value.Update(gameTime);            
        }

        public void Draw(SpriteBatch sb, BoundedCamera camera)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == "collision")
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());
                    foreach (var sprite in Sprites)
                        sprite.Value.Draw(sb);
                    sb.End();
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

        }

        public void DrawDebug(SpriteBatch sb, SpriteFont font, BoundedCamera camera)
        {
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.GetViewMatrix());
            var b = CollisionWorld.Bounds;
            CollisionWorld.DrawDebug(sb, font, (int)b.X, (int)b.Y, (int)b.Width, (int)b.Height);
            sb.End();
        }

        public void AddSprite(string spriteName, SpriteObject sprite)
        {
            Sprites.Add(spriteName, sprite);
        }

        public void RemoveSprite(string spriteName)
        {
            if (Sprites.ContainsKey(spriteName))
            {
                CollisionWorld.Remove(Sprites[spriteName].CollisionBox);
                Sprites.Remove(spriteName);
            }
        }

        public void RemoveSprite(SpriteObject sprite)
        {
            foreach (var item in Sprites.Where(kvp => kvp.Value == sprite))
            {
                RemoveSprite(item.Key);
            }
        }

        public Rectangle GetLevelBounds()
        {
            var worldBounds = Rectangle.Empty;
            foreach (var l in Layers)
            {
                if (l.Name != "collision" && l.Name != "background")
                {
                    foreach (var item in l.Items)
                    {
                        worldBounds = Rectangle.Union(worldBounds, item.getBoundingBox());
                    }
                }
            }
            return worldBounds;
        }

        #region NIGGA FIX THIS SHIT !!!

        [XmlIgnore]
        public Player player;

 

        public void SpawnPlayer()
        {
            if (player != null)
            {
                CollisionWorld.Remove(player.CollisionBox);
            }

            var spawnLocation = (Vector2)CustomProperties["spawnVector"].value;
            RemoveSprite("Player");
            player = new Player(spawnLocation, context, Content);
//            player.onTransition += Player_onTransition;
            AddSprite("Player", player);
        }

        public LivingSpriteObject SpawnEnemy(string name, Vector2 location)
        {
            RemoveSprite(name);
            var enemy = new Enemies.Enemy1(location, context, player, Content);
            AddSprite(name, enemy);
            return enemy;
        }

        #endregion
    }
}
