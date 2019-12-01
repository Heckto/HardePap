using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Linq;
//using AuxLib.CollisionDetection;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using AuxLib;
using AuxLib.RandomGeneration;
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
        public DebugView debugView;

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

        public void LoadContent()
        {

            CollisionWorld = new World(new Vector2(0, 10));

            debugView = new DebugView(CollisionWorld);
            debugView.LoadContent(DemoGame.graphics.GraphicsDevice, DemoGame.ContentManager);
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
                            var texture = DemoGame.ContentManager.Load<Texture2D>(assetName);
                            spritesheets.Add(asset, texture);
                        }
                    }

                }
            }

            CamBounds = (Rectangle)CustomProperties["bounds"].value;

            LevelBounds = (Rectangle)CustomProperties["bounds"].value;
            LevelBounds.Inflate((int)(0.05 * CamBounds.Width), (int)(0.05 * CamBounds.Height));
            context.camera.Bounds = LevelBounds;
            var song = "level" + Rand.GetRandomInt(1, 4);
            AudioManager.PlaySoundTrack(song, true, false);
            
            AudioManager.MusicVolume = 0.0f;           
        }

        public void GenerateCollision()
        {
            var l = Layers.FirstOrDefault(elem => elem.Name == "collision");
            Body colBody;
            foreach (var elem in l.Items)
            {
                if (elem is RectangleItem)
                {
                    var rec = elem as RectangleItem;
                    var origin = new Vector2(rec.Width / 2, rec.Height / 2);
                    colBody = CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(rec.Width), ConvertUnits.ToSimUnits(rec.Height), 10, ConvertUnits.ToSimUnits(rec.Position + origin));
                    if (rec.ItemType == ItemTypes.Transition || rec.ItemType == ItemTypes.ScriptTrigger)
                    {
                        colBody.Tag = rec;
                    }
                    colBody.SetCollisionCategories(Category.Cat2);
                }
                else if (elem is PathItem)
                {
                    var path = elem as PathItem;
                    for(var idx=0;idx < path.WorldPoints.Length-1; idx++)
                    {
                        colBody = CollisionWorld.CreateEdge(ConvertUnits.ToSimUnits(path.WorldPoints[idx]), ConvertUnits.ToSimUnits(path.WorldPoints[idx + 1]));
                        colBody.SetCollisionCategories(Category.Cat2);
                    }


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

            
            context.camera.UpdateCamera(gameTime,player.controller.latestVelocity);
        }

        public void Draw(SpriteBatch sb, FocusCamera camera)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == "collision")
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.getViewMatrix());
                    foreach (var sprite in Sprites)
                        sprite.Value.Draw(sb);
                    sb.End();
                }
                else
                {
                    if (layer.Visible)
                    {
                        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.getViewMatrix(layer.ScrollSpeed));
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

        public void DrawDebug(SpriteBatch sb, SpriteFont font, FocusCamera camera)
        {
            var projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Width), ConvertUnits.ToSimUnits(sb.GraphicsDevice.Viewport.Height), 0f, 0f, 1f);
            debugView.RenderDebugData(projection, camera.getScaledViewMatrix());


            var projection2 = Matrix.CreateOrthographicOffCenter(0f, sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height, 0f, 0f, 1f);
            debugView.BeginCustomDraw(projection2, camera.getViewMatrix());

            foreach (var ray in player.controller.castList)
                debugView.DrawSegment(ray.from, ray.to, Color.Blue);

            var areaPoints = new Vector2[] {
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.left,camera.focusArea.top)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.right, camera.focusArea.top)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.right, camera.focusArea.bottom)),
                ConvertUnits.ToDisplayUnits(new Vector2(camera.focusArea.left, camera.focusArea.bottom))
            };
            debugView.DrawSolidPolygon(areaPoints, 4, Color.Red);

            debugView.DrawPoint(ConvertUnits.ToDisplayUnits(camera.focusPosition), 3, Color.White);

            debugView.DrawPoint(camera.Position, 3, Color.Pink);

            var cameraBounds = new Vector2[] {
                new Vector2(camera.Bounds.Left,camera.Bounds.Top),
                new Vector2(camera.Bounds.Right, camera.Bounds.Top),
                new Vector2(camera.Bounds.Right, camera.Bounds.Bottom),
                new Vector2(camera.Bounds.Left, camera.Bounds.Bottom)
            };

            debugView.DrawPolygon(cameraBounds, 4, Color.Green);

            debugView.EndCustomDraw();
        }

        public void AddSprite(string spriteName, SpriteObject sprite)
        {
            Sprites.Add(spriteName, sprite);            
        }

        public void RemoveSprite(string spriteName)
        {
            if (Sprites.ContainsKey(spriteName))
            {
                //CollisionWorld.Remove(Sprites[spriteName].CollisionBox);
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
        #region NIGGA FIX THIS SHIT !!!

        [XmlIgnore]
        public LivingSpriteObject player;



        public void SpawnPlayer(Vector2? loc)
        {
            if (player != null)
            {
                CollisionWorld.Remove(player.CollisionBox);
            }

            Vector2 spawnLocation;
            if (!loc.HasValue)
                spawnLocation = (Vector2)CustomProperties["spawnVector"].value;
            else
                spawnLocation = loc.Value;

            
            RemoveSprite("Player");
            var l = new Vector2(3500, 3500);
            player = new Ninja1(l, context);
            AddSprite("Player", player);
        }

        public LivingSpriteObject SpawnEnemy(string name, Vector2 location)
        {
            RemoveSprite(name);
            var enemy = new Zombie1(location, context, ItemTypes.Enemy);
            AddSprite(name, enemy);
            return enemy;
        }

        #endregion
    }
}
