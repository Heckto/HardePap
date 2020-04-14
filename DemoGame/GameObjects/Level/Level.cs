using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Diagnostics;
using AuxLib;
using AuxLib.RandomGeneration;
using AuxLib.Sound;
using AuxLib.Camera;
using Game1.GameObjects.Sprite;
using Game1.DataContext;
using Game1.GameObjects.Characters;
using System.ComponentModel;
using System.Text;
using Game1.GameObjects.Graphics.PostProcessing;
using Game1.GameObjects.ParticleEffects;
using Microsoft.Xna.Framework.Content;
using Game1.Rendering;
using Game1.GameObjects.Graphics.PostProcessing.PostProcessors;

namespace Game1.GameObjects.Levels
{
    public class Level : ICustomTypeDescriptor
    {
        [XmlIgnore]
        public readonly List<PostProcessor> _postProcessors = new List<PostProcessor>();

        [XmlAttribute()]
        public string Name;

        public List<Layer> Layers;

        [XmlIgnore]
        public World CollisionWorld;

        [XmlIgnore]
        public DebugView debugView;

        public Rectangle LevelBounds { get; set; }

        public string ContentPath { get; set; } = string.Empty;

        [XmlIgnore]
        public GameContext context;

        public SerializableDictionary CustomProperties;

        [XmlIgnore]
        private RenderTarget2D render_target;

        [XmlIgnore]
        public List<GameObject>  removeList = new List<GameObject>();

        public Level() : base()
        {
            CustomProperties = new SerializableDictionary();
            Layers = new List<Layer>();
        }

        public static Level FromFile(string filename)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }
        public void LoadContent(ContentManager contentManager)
        {
            CollisionWorld = new World(new Vector2(0, 10));
            debugView = new DebugView(CollisionWorld);
            debugView.LoadContent(DemoGame.graphics.GraphicsDevice, contentManager);

            foreach (var layer in Layers)
            {
                layer.level = this;
                layer.LoadContent(contentManager);
                
                if (layer.Name == "Mountains")
                    layer._postProcessors.Add(new WaterReflectionPostProcessor(context));

                foreach (var item in layer.Items)
                {                    
                    if (item is SpriteObject sprite)
                    {                        
                        sprite.context = context;
                    }
                    item.layer = layer;
                    item.LoadContent(contentManager);
                    item.Initialize();
                    
                }
            }

            render_target = new RenderTarget2D(context.graphics, context.graphics.Viewport.Width, context.graphics.Viewport.Height,false, SurfaceFormat.Color,DepthFormat.None,0, RenderTargetUsage.PreserveContents);

            this._postProcessors.Add(new FogPostProcessor(context));

            context.camera.Bounds = LevelBounds;

            LevelBounds.Inflate((int)(0.05 * LevelBounds.Width), (int)(0.05 * LevelBounds.Height));

            var song = "level" + Rand.GetRandomInt(1, 4);
            AudioManager.PlaySoundTrack(song, true, false);            
            AudioManager.MusicVolume = 0.1f;           
        }

        public void GenerateCollision()
        {
            var l = Layers.FirstOrDefault(elem => elem.Name == "Collision");
            if (l != null)
            {
                foreach (var elem in l.Items)
                {
                    if (elem is RectangleItem)
                    {
                        var rec = elem as RectangleItem;
                        var origin = new Vector2(rec.Width / 2, rec.Height / 2);
                        var colBody = CollisionWorld.CreateRectangle(ConvertUnits.ToSimUnits(rec.Width), ConvertUnits.ToSimUnits(rec.Height), 10, ConvertUnits.ToSimUnits(rec.Transform.Position + origin));
                        if (rec.ItemType == ItemTypes.Transition || rec.ItemType == ItemTypes.ScriptTrigger)
                        {
                            colBody.Tag = rec;
                            colBody.SetCollisionCategories(Category.Cat9);
                        }
                        else
                            colBody.SetCollisionCategories(Category.Cat2);
                    }
                    else if (elem is PathItem)
                    {
                        var path = elem as PathItem;
                        for (var idx = 0; idx < path.WorldPoints.Length - 1; idx++)
                        {
                            var colBody = CollisionWorld.CreateEdge(ConvertUnits.ToSimUnits(path.WorldPoints[idx]), ConvertUnits.ToSimUnits(path.WorldPoints[idx + 1]));
                            colBody.SetCollisionCategories(Category.Cat2);
                        }
                    }
                }
            }
        }

        public GameObject GetItemByName(string name)
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
            removeList.Clear();

            foreach (var layer in Layers)
            {
                layer.Update(gameTime, this);
            }

            // Remove dead objects
            for (var idx = 0; idx < removeList.Count; idx++)
            {
                RemoveSprite(removeList[idx]);
            }

            for (var idx = 0; idx < _postProcessors.Count; idx++)
                _postProcessors[idx].Update(gameTime);

            context.camera.UpdateCamera(gameTime,player.controller.latestVelocity);
        }

        public void Draw(SpriteBatch sb, FocusCamera camera)
        {
            context.graphics.SetRenderTarget(render_target);
            foreach (var layer in Layers)
            {
                if (layer.Visible)
                {
                    layer.Draw(sb, camera,render_target);
                }
            }


            if (_postProcessors.Count > 0)
            {

                _postProcessors[0].Process(sb, render_target);
            }


            context.graphics.SetRenderTarget(null);

            sb.Begin();
            sb.Draw(render_target, new Rectangle(0, 0, this.render_target.Width, this.render_target.Height), Color.White);
            sb.End();



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
            var playLayer = Layers.FirstOrDefault(l => l.Name == "Collision");
            
            if (playLayer != null)
            {
                sprite.layer = playLayer;
                sprite.Name = spriteName;
                playLayer.AddObject(sprite);
            }
        }

        public void RemoveSprite(string spriteName)
        {
            var playLayer = Layers.FirstOrDefault(l => l.Name == "Collision");
            if (playLayer != null)
            {
                var spriteItem = playLayer.Items.FirstOrDefault(elem => elem.Name.Equals(spriteName));
                RemoveSprite(spriteItem);
            }
        }

        public void RemoveSprite(GameObject spriteItem)
        {
            var playLayer = Layers.First(l => l.Name == "Collision");
            if (playLayer != null)
            {
                if (spriteItem != null)
                {
                    if (spriteItem is SpriteObject obj)
                    {
                        CollisionWorld.Remove(obj.CollisionBox);
                    }
                    playLayer.Items.Remove(spriteItem);
                }
            }
        }
        #region NIGGA FIX THIS SHIT !!!

        [XmlIgnore]
        public LivingSpriteObject player;

        public void SpawnPlayer(Vector2? loc)
        {
            if (CustomProperties != null && CustomProperties.ContainsKey("spawnVector"))
            {
                var spawnLocation = !loc.HasValue ? (Vector2)CustomProperties["spawnVector"].value : loc.Value;
            }
            RemoveSprite("Player");
            var l = new Vector2(100, 0);
            player = new Ninja1(l, context);


            player.LoadContent(context.content);
            AddSprite("Player", player);

            player.Initialize();
            
        }

        public LivingSpriteObject SpawnEnemy(string name, Vector2 location)
        {
            RemoveSprite(name);
            var enemy = new Zombie1(location, context);
            AddSprite(name, enemy);
            return enemy;
        }

        #endregion


        #region Editor

        public GameObject getItemByName(string name)
        {
            foreach (var layer in Layers)
            {
                foreach (var item in layer.Items)
                {
                    if (item.Name == name) return item;
                }
            }
            return null;
        }

        public Layer getLayerByName(string name)
        {
            foreach (var layer in Layers)
            {
                if (layer.Name == name) return layer;
            }
            return null;
        }

        // public IUndoable cloneforundo()
        //{
        //    selecteditems = "";
        //    foreach (var i in MainForm.Instance.picturebox.SelectedItems) selecteditems += i.Name + ";";
        //    if (MainForm.Instance.picturebox.SelectedLayer != null) selectedlayers = MainForm.Instance.picturebox.SelectedLayer.Name;


        //    var result = (Level)this.MemberwiseClone();
        //    result.Layers = new List<Layer>(Layers);
        //    for (var i = 0; i < result.Layers.Count; i++)
        //    {
        //        result.Layers[i] = result.Layers[i].clone();
        //        result.Layers[i].level = result;
        //    }
        //    return (IUndoable)result;
        //}

        //public void makelike(IUndoable other)
        //{
        //    /*Layer l2 = (Layer)other;
        //    Items = l2.Items;
        //    treenode.Nodes.Clear();
        //    foreach (Item i in Items)
        //    {
        //        Editor.Instance.addItem(i);
        //    }*/


        //    var l2 = (Level)other;
        //    Layers = l2.Layers;
        //    treenode.Nodes.Clear();
        //    foreach (var l in Layers)
        //    {
        //        MainForm.Instance.picturebox.addLayer(l);
        //        //TODO add all items
        //    }
        //}

        //public string getName()
        //{
        //    return Name;
        //}

        //public void setName(string name)
        //{
        //    Name = name;
        //    treenode.Text = name;
        //}

        [XmlIgnore()]
        public string selectedlayers;
        [XmlIgnore()]
        public string selecteditems;

        public delegate void onContentDirectorySelect(string folder);
        public event onContentDirectorySelect onContentDirectorySelected;

        public class EditorVars
        {
            public int NextItemNumber;
            public string ContentRootFolder;
            public Vector2 CameraPosition;
            public string Version;
        }

        [XmlIgnore()]

        [Category(" General")]
        [Description("When the level is saved, each texture is saved with a path relative to this folder."
                     + "You should set this to the \"Content.RootDirectory\" of your game project.")]
        //[EditorAttribute(typeof(FolderUITypeEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public String ContentRootFolder
        {
            get
            {
                return EditorRelated.ContentRootFolder;
            }
            set
            {
                EditorRelated.ContentRootFolder = value;
                onContentDirectorySelected?.Invoke(value);
            }
        }



        EditorVars editorrelated = new EditorVars();
        [Browsable(false)]
        public EditorVars EditorRelated
        {
            get
            {
                return editorrelated;
            }
            set
            {
                editorrelated = value;
            }
        }

        [XmlIgnore()]
        public object treenode;


        public string getNextItemNumber()
        {
            return (++EditorRelated.NextItemNumber).ToString("0000");
        }


        public void export(string filename)
        {
            try
            {
                foreach (var l in Layers)
                {
                    foreach (var i in l.Items)
                    {
                        if (i is TextureItem)
                        {
                            var ti = (TextureItem)i;
                            ti.texture_filename = ti.texture.Name;
                            //ti.asset_name = ti.texture_filename.Substring(0, ti.texture_filename.LastIndexOf('.'));
                        }
                    }
                }


                var writer = new XmlTextWriter(filename, null)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4
                };

                var serializer = new XmlSerializer(typeof(Level));
                serializer.Serialize(writer, this);

                writer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //MessageBox.Show(ex.Message);
            }
        }



        public string RelativePath(string relativeTo, string pathToTranslate)
        {
            var absoluteDirectories = relativeTo.Split('\\');
            var relativeDirectories = pathToTranslate.Split('\\');

            //Get the shortest of the two paths
            var length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            var lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                // throw new ArgumentException("Paths do not have a common base");
                return pathToTranslate;

            //Build up the relative path
            var relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0) relativePath.Append("..\\");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + "\\");

            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
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

        #region Ass

        public class DictionaryPropertyDescriptor : PropertyDescriptor
        {
            String key;
            public SerializableDictionary sdict;


            public DictionaryPropertyDescriptor(SerializableDictionary dict, String key, Attribute[] attrs)
                : base(key, attrs)
            {
                this.key = key;
                this.sdict = dict;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return null; }
            }

            public override object GetValue(object component)
            {
                return sdict[key].value;
            }

            public override string Description
            {
                get { return sdict[key].description; }
            }

            public override string Category
            {
                get { return "Custom Properties"; }
            }

            public override string DisplayName
            {
                get { return key; }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override void ResetValue(object component)
            {
                //Have to implement
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override void SetValue(object component, object value)
            {
                sdict[key].value = value;
            }

            public override Type PropertyType
            {
                get { return sdict[key].type; }
            }

            //public override object GetEditor(Type editorBaseType)
            //{
            //    if (sdict[key].type == typeof(Vector2)) return new Vector2UITypeEditor();
            //    if (sdict[key].type == typeof(Rectangle)) return new RectangleUITypeEditor();
            //    if (sdict[key].type == typeof(Color)) return new XNAColorUITypeEditor();
            //    if (sdict[key].type == typeof(GameObject)) return new ItemUITypeEditor();

            //    return base.GetEditor(editorBaseType);
            //}

        }

        #endregion
    }
}

