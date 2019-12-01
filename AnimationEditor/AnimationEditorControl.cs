using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using AnimationEditor.Properties;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Forms.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Forms = System.Windows.Forms;
using AuxLib.Camera;
using AuxLib.Logging;

namespace AnimationEditor
{
    public class AnimationEditorControl : UpdateWindow
    {
        private EditorState _state;
        public EditorState state
        {
            get { return _state; }
            set
            {
                _state = value;               
                MainForm.Instance.toolStripStatusLabel4.Text = "Status: " + value.ToString();
            }
        }
        //Brush currentbrush;
        PrimitiveType currentprimitive;
        bool primitivestarted;
        List<Vector2> clickedPoints = new List<Vector2>();

        public Dictionary<string, SpriteSheet> spriteSheets = new Dictionary<string, SpriteSheet>();

        KeyFrame selectedKeyFrame;
        public KeyFrame SelectedKeyFrame
        {
            get { return selectedKeyFrame; }
            set
            {
                selectedKeyFrame = value;
                if (value == null) MainForm.Instance.toolStripStatusLabel3.Text = "Keyframe: (none)";
                else MainForm.Instance.toolStripStatusLabel3.Text = "Keyframe: " + selectedAnimation.Name;
            }
        }


        Animation selectedAnimation;
        public Animation SelectedAnimation
        {
            get { return selectedAnimation; }
            set
            {
                selectedAnimation = value;
                if (value == null) MainForm.Instance.toolStripStatusLabel2.Text = "Animation: (none)";
                else MainForm.Instance.toolStripStatusLabel2.Text = "Animation: " + selectedAnimation.Name;
            }
        }

        Vector2 mouseworldpos, grabbedpoint, newPosition;
        Vector2 initialpos;                   //position before user interaction
        float initialrot;                     //rotation before user interaction
        Vector2 initialscale;                 //scale before user interaction

        public CharacterDefinition characterDefinition;
        public Camera camera;
        KeyboardState kstate, oldkstate;
        MouseState mstate, oldmstate;
        Forms.Cursor cursorRot, cursorScale, cursorDup;
        Stack<Command> undoBuffer = new Stack<Command>();
        Stack<Command> redoBuffer = new Stack<Command>();
        bool commandInProgress = false;
        public Texture2D dummytexture;
        public string Version;

        protected override void Initialize()
        {
            Logger.Instance.log("Editor creation started.");
            state = EditorState.idle;
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Logger.Instance.log("Loading Resources.");
            Stream resStream;
            resStream = new MemoryStream(Resources.dragcopy);
            cursorDup = new Forms.Cursor(resStream);
            resStream = new MemoryStream(Resources.rotate);
            cursorRot = new Forms.Cursor(resStream);
            resStream = new MemoryStream(Resources.scale);
            cursorScale = new Forms.Cursor(resStream);

            var bmp = Resources.circle;
            resStream = new MemoryStream();
            bmp.Save(resStream, System.Drawing.Imaging.ImageFormat.Png);
            dummytexture = Texture2D.FromStream(GraphicsDevice, resStream);
            Logger.Instance.log("Resources loaded.");

            Logger.Instance.log("Loading Settings.");
            Constants.Instance.import("settings.xml");
            Logger.Instance.log("Settings loaded.");
            MainForm.Instance.ViewGrid.Checked = Constants.Instance.ShowGrid;
            MainForm.Instance.ViewSnapToGrid.Checked = Constants.Instance.SnapToGrid;            

            base.Initialize();
            Logger.Instance.log("Editor creation ended.");
            MainForm.Instance.newCharacterDefinition();

            
        }

        public AnimationEditorControl() {}

        protected override void Draw()
        {
            if (Editor == null)
                return;
            base.Draw();

            var sb = Editor.spriteBatch;
            GraphicsDevice.Clear(Constants.Instance.ColorBackground);
            if (characterDefinition == null)
                return;
            if (selectedAnimation != null)
            {
                foreach (var f in selectedAnimation.keyFrames)
                {
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.getViewMatrix());
                    //f.frame.AssetName

                    var spriteDef = spriteSheets[f.frame.AssetName].SpriteDef[f.frame.AssetIndex];
                    var destRect = new Rectangle((int)f.frame.Location.X,(int)f.frame.Location.Y, spriteDef.SrcRectangle.Width, spriteDef.SrcRectangle.Height);
                    sb.Draw(spriteSheets[f.frame.AssetName].Texture, destRect, spriteDef.SrcRectangle, Color.White,f.frame.Rotation, f.frame.Origin,SpriteEffects.None,0.0f);
                    sb.End();
                }
            }


            ////draw selection frames around selected items
            //if (SelectedItems.Count > 0)
            //{
            //    var maincameraposition = camera.Position;
            //    camera.Position *= SelectedItems[0].layer.ScrollSpeed;
            //    sb.Begin();
            //    int i = 0;
            //    foreach (var item in SelectedItems)
            //    {
            //        if (item.Visible && item.layer.Visible && kstate.IsKeyUp(Keys.Space))
            //        {
            //            var color = i == 0 ? Constants.Instance.ColorSelectionFirst : Constants.Instance.ColorSelectionRest;
            //            item.drawSelectionFrame(sb, camera.matrix, color);
            //            if (i == 0 && (state == EditorState.rotating || state == EditorState.scaling))
            //            {
            //                var center = Vector2.Transform(item.Position, camera.matrix);
            //                var mouse = Vector2.Transform(mouseworldpos, camera.matrix);
            //                Primitives.Instance.drawLine(sb, center, mouse, Constants.Instance.ColorSelectionFirst, 1);
            //            }
            //        }
            //        i++;
            //    }
            //    sb.End();
            //    //restore main camera position
            //    camera.Position = maincameraposition;

            //}

            if (Constants.Instance.ShowGrid)
            {
                sb.Begin();
                int max = Constants.Instance.GridNumberOfGridLines / 2;
                for (int x = 0; x <= max; x++)
                {
                    var start = Vector2.Transform(new Vector2(x, -max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    var end = Vector2.Transform(new Vector2(x, max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                    start = Vector2.Transform(new Vector2(-x, -max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    end = Vector2.Transform(new Vector2(-x, max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                }
                for (int y = 0; y <= max; y++)
                {
                    var start = Vector2.Transform(new Vector2(-max, y) * Constants.Instance.GridSpacing.Y, camera.getViewMatrix());
                    var end = Vector2.Transform(new Vector2(max, y) * Constants.Instance.GridSpacing.Y, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                    start = Vector2.Transform(new Vector2(-max, -y) * Constants.Instance.GridSpacing.Y, camera.getViewMatrix());
                    end = Vector2.Transform(new Vector2(max, -y) * Constants.Instance.GridSpacing.Y, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                }
                sb.End();
            }
        }

        protected override void Update(GameTime gameTime)        
        {
            if (!this.Focused)
                return;
            if (characterDefinition == null)
                return;

            oldkstate = kstate;
            oldmstate = mstate;
            kstate = Keyboard.GetState();

            mstate = Mouse.GetState();
            int mwheeldelta = mstate.ScrollWheelValue - oldmstate.ScrollWheelValue;
            if (mwheeldelta > 0)
            {
                float zoom = (float)Math.Round(camera.Scale * 10) * 10.0f + 10.0f;
                MainForm.Instance.zoomcombo.Text = zoom.ToString() + "%";
                camera.Scale = zoom / 100.0f;
            }
            if (mwheeldelta < 0)
            {
                float zoom = (float)Math.Round(camera.Scale * 10) * 10.0f - 10.0f;
                if (zoom <= 0.0f) return;
                MainForm.Instance.zoomcombo.Text = zoom.ToString() + "%";
                camera.Scale = zoom / 100.0f;
            }

            //Camera movement
            float delta;
            if (kstate.IsKeyDown(Keys.LeftShift)) delta = Constants.Instance.CameraFastSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else delta = Constants.Instance.CameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (kstate.IsKeyDown(Keys.W) && kstate.IsKeyUp(Keys.LeftControl))
                camera.Position += (new Vector2(0, -delta));
            if (kstate.IsKeyDown(Keys.S) && kstate.IsKeyUp(Keys.LeftControl)) camera.Position += (new Vector2(0, +delta));
            if (kstate.IsKeyDown(Keys.A) && kstate.IsKeyUp(Keys.LeftControl)) camera.Position += (new Vector2(-delta, 0));
            if (kstate.IsKeyDown(Keys.D) && kstate.IsKeyUp(Keys.LeftControl)) camera.Position += (new Vector2(+delta, 0));

            //if (kstate.IsKeyDown(Keys.Delete))
            //    deleteSelectedItems();

            if (kstate.IsKeyDown(Keys.Subtract))
            {
                float zoom = (float)(camera.Scale * 0.995);
                MainForm.Instance.zoomcombo.Text = (zoom * 100).ToString("###0.0") + "%";
                camera.Scale = zoom;
            }
            if (kstate.IsKeyDown(Keys.Add))
            {
                float zoom = (float)(camera.Scale * 1.005);
                MainForm.Instance.zoomcombo.Text = (zoom * 100).ToString("###0.0") + "%";
                camera.Scale = zoom;
            }

            mouseworldpos = Vector2.Transform(new Vector2(mstate.X, mstate.Y), Matrix.Invert(camera.matrix));
            mouseworldpos = mouseworldpos.Round();
            MainForm.Instance.toolStripStatusLabel3.Text = "Mouse: (" + mouseworldpos.X + ", " + mouseworldpos.Y + ")";

            if (state == EditorState.idle)
            {
                //get item under mouse cursor
                var item = getItemAtPos(mouseworldpos);
                if (item != null)
                {
                    MainForm.Instance.toolStripStatusLabel1.Text = item.Name;
                    //item.onMouseOver(mouseworldpos);
                    if (kstate.IsKeyDown(Keys.LeftControl)) MainForm.Instance.picturebox.Cursor = cursorDup;
                }
                else
                {
                    MainForm.Instance.toolStripStatusLabel1.Text = "";
                }
                //if (item != lastitem && lastitem != null) lastitem.onMouseOut();
                //lastitem = item;

                //LEFT MOUSE BUTTON CLICK
                if ((mstate.LeftButton == ButtonState.Pressed && oldmstate.LeftButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D1) && oldkstate.IsKeyUp(Keys.D1)))
                {

                    if (item != null)
                    {                        
                        startMoving();
                    }                   
                }

                //MIDDLE MOUSE BUTTON CLICK
                if ((mstate.MiddleButton == ButtonState.Pressed && oldmstate.MiddleButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D2) && oldkstate.IsKeyUp(Keys.D2)))
                {
                    grabbedpoint = mouseworldpos;

                    if (item != null)
                    {
                        //    //save the initial rotation for each item
                        initialrot = selectedKeyFrame.frame.Rotation;
                        state = EditorState.rotating;
                        MainForm.Instance.picturebox.Cursor = cursorRot;
                    }
                }

                //RIGHT MOUSE BUTTON CLICK
                if ((mstate.RightButton == ButtonState.Pressed && oldmstate.RightButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D3) && oldkstate.IsKeyUp(Keys.D3)))

                {
                    //if (item != null) item.onMouseOut();
                    if (SelectedKeyFrame != null)
                    {
                        //grabbedpoint = mouseworldpos - SelectedItems[0].pPosition;

                        ////save the initial scale for each item
                        //initialscale.Clear();
                        //foreach (var selitem in SelectedItems)
                        //{
                        //    if (selitem.CanScale())
                        //    {
                        //        initialscale.Add(selitem.getScale());
                        //    }
                        //}

                        //state = EditorState.scaling;
                        //MainForm.Instance.picturebox.Cursor = cursorScale;

                        //beginCommand("Scale Item(s)");
                    }
                }

                if (kstate.IsKeyDown(Keys.H) && oldkstate.GetPressedKeys().Length == 0 && SelectedKeyFrame != null)
                {
                    beginCommand("Flip Item(s) Horizontally");
                    SelectedKeyFrame.frame.Flip = 1- SelectedKeyFrame.frame.Flip;
                    //foreach (var selitem in SelectedItems)
                    //{
                    //    if (selitem is TextureItem)
                    //    {
                    //        var ti = (TextureItem)selitem;
                    //        ti.FlipHorizontally = !ti.FlipHorizontally;
                    //    }
                    //}
                    MainForm.Instance.propertyGrid1.Refresh();
                    endCommand();
                }
                if (kstate.IsKeyDown(Keys.V) && oldkstate.GetPressedKeys().Length == 0 && SelectedKeyFrame != null)
                {
                    beginCommand("Flip Item(s) Vertically");
                    SelectedKeyFrame.frame.Flip = 1 - SelectedKeyFrame.frame.Flip;
                    //foreach (var selitem in SelectedItems)
                    //{
                    //    if (selitem is TextureItem)
                    //    {
                    //        var ti = (TextureItem)selitem;
                    //        ti.FlipVertically = !ti.FlipVertically;
                    //    }
                    //}
                    MainForm.Instance.propertyGrid1.Refresh();
                    endCommand();
                }
            }

            if (state == EditorState.moving)
            {
                    newPosition = initialpos + mouseworldpos - grabbedpoint;
                    //if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G)) newPosition = snapToGrid(newPosition);
                    //drawSnappedPoint = false;
                selectedKeyFrame.frame.Location = newPosition;
                //newPosition.setPosition(newPosition);
                    
                
                MainForm.Instance.propertyGrid1.Refresh();
                if ((mstate.LeftButton == ButtonState.Released && oldmstate.LeftButton == ButtonState.Pressed) ||
                    (kstate.IsKeyUp(Keys.D1) && oldkstate.IsKeyDown(Keys.D1)))
                {

                    //foreach (var selitem in SelectedItems) selitem.onMouseButtonUp(mouseworldpos);

                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    if (mouseworldpos != grabbedpoint) endCommand(); else abortCommand();
                }
            }

            if (state == EditorState.rotating)
            {
                var newpos = mouseworldpos - selectedKeyFrame.frame.Location;
                float deltatheta = (float)Math.Atan2(grabbedpoint.Y, grabbedpoint.X) - (float)Math.Atan2(newpos.Y, newpos.X);
                int i = 0;

                selectedKeyFrame.frame.Rotation = initialrot - deltatheta;
               
                MainForm.Instance.propertyGrid1.Refresh();
                if ((mstate.MiddleButton == ButtonState.Released && oldmstate.MiddleButton == ButtonState.Pressed) ||
                    (kstate.IsKeyUp(Keys.D2) && oldkstate.IsKeyDown(Keys.D2)))
                {
                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    endCommand();
                }
            }

            //if (state == EditorState.scaling)
            //{
            //    var newdistance = mouseworldpos - SelectedItems[0].pPosition;
            //    float factor = newdistance.Length() / grabbedpoint.Length();
            //    int i = 0;
            //    foreach (var selitem in SelectedItems)
            //    {
            //        if (selitem.CanScale())
            //        {
            //            if (selitem is TextureItem)
            //            {
            //                MainForm.Instance.toolStripStatusLabel1.Text = "Hold down [X] or [Y] to limit scaling to the according dimension.";
            //            }

            //            var newscale = initialscale[i];
            //            if (!kstate.IsKeyDown(Keys.Y)) newscale.X = initialscale[i].X * (((factor - 1.0f) * 0.5f) + 1.0f);
            //            if (!kstate.IsKeyDown(Keys.X)) newscale.Y = initialscale[i].Y * (((factor - 1.0f) * 0.5f) + 1.0f);
            //            selitem.setScale(newscale);

            //            if (kstate.IsKeyDown(Keys.LeftControl))
            //            {
            //                Vector2 scale;
            //                scale.X = (float)Math.Round(selitem.getScale().X * 10) / 10;
            //                scale.Y = (float)Math.Round(selitem.getScale().Y * 10) / 10;
            //                selitem.setScale(scale);
            //            }
            //            i++;
            //        }
            //    }
            //    MainForm.Instance.propertyGrid1.Refresh();
            //    if ((mstate.RightButton == ButtonState.Released && oldmstate.RightButton == ButtonState.Pressed) ||
            //        (kstate.IsKeyUp(Keys.D3) && oldkstate.IsKeyDown(Keys.D3)))
            //    {
            //        state = EditorState.idle;
            //        MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
            //        endCommand();
            //    }
            //}         

            //if (state == EditorState.brush_primitive)
            //{

            //    if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G)) mouseworldpos = snapToGrid(mouseworldpos);

            //    if (kstate.IsKeyDown(Keys.LeftControl) && primitivestarted && currentprimitive == PrimitiveType.Rectangle)
            //    {
            //        var distance = mouseworldpos - clickedPoints[0];
            //        float squareside = Math.Max(distance.X, distance.Y);
            //        mouseworldpos = clickedPoints[0] + new Vector2(squareside, squareside);
            //    }
            //    if ((mstate.LeftButton == ButtonState.Pressed && oldmstate.LeftButton == ButtonState.Released) ||
            //        (kstate.IsKeyDown(Keys.D1) && oldkstate.IsKeyUp(Keys.D1)))
            //    {
            //        clickedPoints.Add(mouseworldpos);
            //        if (primitivestarted == false)
            //        {
            //            primitivestarted = true;
            //            switch (currentprimitive)
            //            {
            //                case PrimitiveType.Rectangle:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Started;
            //                    break;
            //                case PrimitiveType.Circle:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Started;
            //                    break;
            //                case PrimitiveType.Path:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Started;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            if (currentprimitive != PrimitiveType.Path)
            //            {
            //                paintPrimitiveBrush();
            //                clickedPoints.Clear();
            //                primitivestarted = false;
            //            }
            //        }
            //    }
            //    if (kstate.IsKeyDown(Keys.Back) && oldkstate.IsKeyUp(Keys.Back))
            //    {
            //        if (currentprimitive == PrimitiveType.Path && clickedPoints.Count > 1)
            //        {
            //            clickedPoints.RemoveAt(clickedPoints.Count - 1);
            //        }
            //    }

            //    if ((mstate.MiddleButton == ButtonState.Pressed && oldmstate.MiddleButton == ButtonState.Released) ||
            //        (kstate.IsKeyDown(Keys.D2) && oldkstate.IsKeyUp(Keys.D2)))
            //    {
            //        if (currentprimitive == PrimitiveType.Path && primitivestarted)
            //        {
            //            paintPrimitiveBrush();
            //            clickedPoints.Clear();
            //            primitivestarted = false;
            //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
            //        }
            //    }
            //    if ((mstate.RightButton == ButtonState.Pressed && oldmstate.RightButton == ButtonState.Released) ||
            //        (kstate.IsKeyDown(Keys.D3) && oldkstate.IsKeyUp(Keys.D3)))
            //    {
            //        if (primitivestarted)
            //        {
            //            clickedPoints.Clear();
            //            primitivestarted = false;
            //            switch (currentprimitive)
            //            {
            //                case PrimitiveType.Rectangle:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
            //                    break;
            //                case PrimitiveType.Circle:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
            //                    break;
            //                case PrimitiveType.Path:
            //                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
            //                    break;
            //            }
            //        }
            //        else
            //        {
            //            destroyPrimitiveBrush();
            //            clickedPoints.Clear();
            //            primitivestarted = false;
            //        }
            //    }
            //}
        }

        //public void getSelectionFromLevel()
        //{
        //    SelectedItems.Clear();
        //    SelectedLayer = null;
        //    string[] itemnames = level.selecteditems.Split(';');
        //    foreach (string itemname in itemnames)
        //    {
        //        if (itemname.Length > 0) SelectedItems.Add(level.getItemByName(itemname));
        //    }
        //    SelectedLayer = level.getLayerByName(level.selectedlayers);
        //}

        //public void selectlevel()
        //{
        //    MainForm.Instance.propertyGrid1.SelectedObject = level;
        //}

        public void addAnimation(Animation a)
        {
            characterDefinition.animations.Add(a);
        }

        public void deleteLayer(Animation a)
        {
            MainForm.Instance.picturebox.beginCommand("Delete Layer \"" + a.Name + "\"");
            characterDefinition.animations.Remove(a);
            MainForm.Instance.picturebox.endCommand();           
            updatetreeview();
        }

        //public void moveLayerUp(Layer l)
        //{
        //    int index = level.Layers.IndexOf(l);
        //    level.Layers[index] = level.Layers[index - 1];
        //    level.Layers[index - 1] = l;
        //    selectlayer(l);
        //}

        //public void moveLayerDown(Layer l)
        //{
        //    int index = level.Layers.IndexOf(l);
        //    level.Layers[index] = level.Layers[index + 1];
        //    level.Layers[index + 1] = l;
        //    selectlayer(l);
        //}

        //public void selectlayer(Layer l)
        //{
        //    if (SelectedItems.Count > 0) selectitem(null);
        //    SelectedLayer = l;
        //    updatetreeviewselection();
        //    MainForm.Instance.propertyGrid1.SelectedObject = l;
        //}

        public void addItem(KeyFrame f)
        {
            selectedAnimation.keyFrames.Add(f);
        }

        //public void deleteSelectedItems()
        //{
        //    beginCommand("Delete Item(s)");
        //    var selecteditemscopy = new List<Item>(SelectedItems);

        //    var itemsaffected = new List<Item>();

        //    foreach (var selitem in selecteditemscopy)
        //    {

        //        foreach (var l in level.Layers)
        //            foreach (var i in l.Items)
        //                foreach (var cp in i.CustomProperties.Values)
        //                {
        //                    if (cp.type == typeof(Item) && cp.value == selitem)
        //                    {
        //                        cp.value = null;
        //                        itemsaffected.Add(i);
        //                    }
        //                }

        //        selitem.layer.Items.Remove(selitem);
        //    }
        //    endCommand();
        //    selectitem(null);
        //    updatetreeview();

        //    if (itemsaffected.Count > 0)
        //    {
        //        string message = "";
        //        foreach (var item in itemsaffected) message += item.Name + " (Layer: " + item.layer.Name + ")\n";
        //        Forms.MessageBox.Show("The following Items have Custom Properties of Type \"Item\" that refered to items that have just been deleted:\n\n"
        //            + message + "\nThe corresponding Custom Properties have been set to NULL, since the Item referred to doesn't exist anymore.");
        //    }

        //}

        //public void moveItemUp(Item i)
        //{
        //    int index = i.layer.Items.IndexOf(i);
        //    i.layer.Items[index] = i.layer.Items[index - 1];
        //    i.layer.Items[index - 1] = i;
        //    //updatetreeview();
        //}

        //public void moveItemDown(Item i)
        //{
        //    int index = i.layer.Items.IndexOf(i);
        //    i.layer.Items[index] = i.layer.Items[index + 1];
        //    i.layer.Items[index + 1] = i;
        //    selectitem(i);
        //    //updatetreeview();
        //}

        //public void selectitem(Item i)
        //{
        //    SelectedItems.Clear();
        //    if (i != null)
        //    {
        //        SelectedItems.Add(i);
        //        SelectedLayer = i.layer;
        //        updatetreeviewselection();
        //        MainForm.Instance.propertyGrid1.SelectedObject = i;
        //    }
        //    else
        //    {
        //        selectlayer(SelectedLayer);
        //    }
        //}

        //public void selectAll()
        //{
        //    if (SelectedLayer == null) return;
        //    //if (SelectedLayer.Items.Count == 0) return;
        //    SelectedItems.Clear();
        //    foreach (var i in SelectedLayer.Items)
        //    {
        //        SelectedItems.Add(i);
        //    }
        //    updatetreeviewselection();
        //}

        //public void moveItemToLayer(Item i1, Layer l2, Item i2)
        //{
        //    int index2 = i2 == null ? 0 : l2.Items.IndexOf(i2);
        //    i1.layer.Items.Remove(i1);
        //    l2.Items.Insert(index2, i1);
        //    i1.layer = l2;
        //}
        //public void moveSelectedItemsToLayer(Layer chosenlayer)
        //{
        //    if (chosenlayer == SelectedLayer) return;
        //    beginCommand("Move Item(s) To Layer \"" + chosenlayer.Name + "\"");
        //    var selecteditemscopy = new List<Item>(SelectedItems);
        //    foreach (var i in selecteditemscopy)
        //    {
        //        moveItemToLayer(i, chosenlayer, null);
        //    }
        //    endCommand();
        //    SelectedItems.Clear();
        //    updatetreeview();
        //}
        //public void copySelectedItemsToLayer(Layer chosenlayer)
        //{
        //    //if (chosenlayer == SelectedLayer) return;
        //    beginCommand("Copy Item(s) To Layer \"" + chosenlayer.Name + "\"");
        //    var selecteditemscopy = new List<Item>(SelectedItems);
        //    foreach (var i in selecteditemscopy)
        //    {
        //        var copy = i.clone();
        //        copy.layer = chosenlayer;
        //        copy.Name = MainForm.Instance.getUniqueNameBasedOn(copy.Name);
        //        addItem(copy);
        //    }
        //    endCommand();
        //    SelectedItems.Clear();
        //    updatetreeview();
        //}

        //public void createTextureBrush(Texture2D tex, SpriteSheet ss, string name)
        //{
        //    state = EditorState.brush;
        //    currentbrush = new Brush(tex, ss, name);
        //}

        //public void destroyTextureBrush()
        //{
        //    state = EditorState.idle;
        //    currentbrush = null;
        //}

        //public void paintTextureBrush(bool continueAfterPaint)
        //{
        //    if (SelectedLayer == null)
        //    {
        //        System.Windows.Forms.MessageBox.Show(Resources.No_Layer);
        //        destroyTextureBrush();
        //        return;
        //    }
        //    Item i = new TextureItem(currentbrush.spriteName, new Vector2((int)mouseworldpos.X, (int)mouseworldpos.Y));
        //    i.Name = i.getNamePrefix() + level.getNextItemNumber();
        //    i.layer = SelectedLayer;
        //    beginCommand("Add Item \"" + i.Name + "\"");
        //    addItem(i);
        //    endCommand();
        //    updatetreeview();
        //    if (!continueAfterPaint) destroyTextureBrush();
        //}

        //public void createPrimitiveBrush(PrimitiveType primitiveType)
        //{
        //    if (SelectedLayer == null)
        //    {
        //        System.Windows.Forms.MessageBox.Show(Resources.No_Layer);
        //        return;
        //    }

        //    state = EditorState.brush_primitive;
        //    primitivestarted = false;
        //    clickedPoints.Clear();
        //    currentprimitive = primitiveType;
        //    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Cross;
        //    MainForm.Instance.listView2.Cursor = Forms.Cursors.Cross;
        //    switch (primitiveType)
        //    {
        //        case PrimitiveType.Rectangle:
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
        //            break;
        //        case PrimitiveType.Circle:
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
        //            break;
        //        case PrimitiveType.Path:
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
        //            break;

        //    }
        //}

        //public void destroyPrimitiveBrush()
        //{
        //    state = EditorState.idle;
        //    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
        //    MainForm.Instance.listView2.Cursor = Forms.Cursors.Default;
        //}

        //public void paintPrimitiveBrush()
        //{
        //    switch (currentprimitive)
        //    {
        //        case PrimitiveType.Rectangle:
        //            Item ri = new RectangleItem(Extensions.RectangleFromVectors(clickedPoints[0], clickedPoints[1]));
        //            ri.Name = ri.getNamePrefix() + level.getNextItemNumber();
        //            ri.layer = SelectedLayer;
        //            beginCommand("Add Item \"" + ri.Name + "\"");
        //            addItem(ri);
        //            endCommand();
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
        //            break;
        //        case PrimitiveType.Circle:
        //            Item ci = new CircleItem(clickedPoints[0], (mouseworldpos - clickedPoints[0]).Length());
        //            ci.Name = ci.getNamePrefix() + level.getNextItemNumber();
        //            ci.layer = SelectedLayer;
        //            beginCommand("Add Item \"" + ci.Name + "\"");
        //            addItem(ci);
        //            endCommand();
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
        //            break;
        //        case PrimitiveType.Path:
        //            Item pi = new PathItem(clickedPoints.ToArray());
        //            pi.Name = pi.getNamePrefix() + level.getNextItemNumber();
        //            pi.layer = SelectedLayer;
        //            beginCommand("Add Item \"" + pi.Name + "\"");
        //            addItem(pi);
        //            endCommand();
        //            MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
        //            break;
        //    }
        //    updatetreeview();
        //}


        public void startMoving()
        {
            grabbedpoint = mouseworldpos;
            initialpos = (selectedKeyFrame.frame.Location);            

            state = EditorState.moving;

            MainForm.Instance.picturebox.Cursor = Forms.Cursors.SizeAll;
        }

        //public void setmousepos(int screenx, int screeny)
        //{
        //    var maincameraposition = camera.Position;
        //    if (SelectedLayer != null) camera.Position *= SelectedLayer.ScrollSpeed;
        //    mouseworldpos = Vector2.Transform(new Vector2(screenx, screeny), Matrix.Invert(camera.matrix));
        //    if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G))
        //    {
        //        mouseworldpos = snapToGrid(mouseworldpos);
        //    }
        //    camera.Position = maincameraposition;
        //}

        public KeyFrame getItemAtPos(Vector2 mouseworldpos)
        {
            if (selectedAnimation == null) return null;
            if (selectedKeyFrame == null) return null;
            return SelectedKeyFrame;
        }

        public void loadAnimation(CharacterDefinition def)
        {            
           
            characterDefinition = def;
            MainForm.Instance.loadfolder(Constants.Instance.DefaultContentRootFolder);
            MainForm.Instance.treeView1.Nodes.Clear();          

            if (characterDefinition.Name == null)
                characterDefinition.Name = "New_char";

            var CharDefNode = new Forms.TreeNode(characterDefinition.Name);
            CharDefNode.Tag = characterDefinition;
            MainForm.Instance.treeView1.Nodes.Add(CharDefNode);

            SelectedKeyFrame = null;



            camera = new Camera(MainForm.Instance.picturebox.Width, MainForm.Instance.picturebox.Height);
            camera.Position = characterDefinition.EditorRelated.CameraPosition;
            MainForm.Instance.zoomcombo.Text = "100%";
            undoBuffer.Clear();
            redoBuffer.Clear();
            MainForm.Instance.undoButton.DropDownItems.Clear();
            MainForm.Instance.redoButton.DropDownItems.Clear();
            MainForm.Instance.undoButton.Enabled = MainForm.Instance.undoMenuItem.Enabled = undoBuffer.Count > 0;
            MainForm.Instance.redoButton.Enabled = MainForm.Instance.redoMenuItem.Enabled = redoBuffer.Count > 0;
            commandInProgress = false;

            updatetreeview();
        }

        public void updatetreeview()
        {
            MainForm.Instance.treeView1.Nodes.Clear();

            var charDefNode = new Forms.TreeNode(characterDefinition.Name);
            MainForm.Instance.treeView1.Nodes.Add(charDefNode);
            charDefNode.Tag = characterDefinition;
            charDefNode.ContextMenuStrip = MainForm.Instance.LevelContextMenu;

            foreach (var animation in characterDefinition.animations)
            {
                var animNode = new Forms.TreeNode(animation.Name);
                charDefNode.Nodes.Add(animNode);
                animNode.Tag = animation;

                animNode.ContextMenuStrip = MainForm.Instance.LayerContextMenu;                

                foreach (var item in animation.keyFrames)
                {
                    var framenode = animNode.Nodes.Add(item.frame.Name, item.frame.Name);
                    framenode.Tag = item;
                    framenode.ContextMenuStrip = MainForm.Instance.ItemContextMenu;
                }
                animNode.Expand();
            }
            charDefNode.Expand();

            updatetreeviewselection();
        }

        public void updatetreeviewselection()
        {
            MainForm.Instance.propertyGrid1.SelectedObject = null;
            if (SelectedKeyFrame != null)
            {
                Forms.TreeNode[] nodes = MainForm.Instance.treeView1.Nodes.Find(SelectedKeyFrame.Name, true);
                if (nodes.Length > 0)
                {
                    //var selecteditemscopy = new List<Item>(SelectedItems);
                    //MainForm.Instance.propertyGrid1.SelectedObject = SelectedItems[0];
                    //MainForm.Instance.treeView1.SelectedNode = nodes[0];
                    //MainForm.Instance.treeView1.SelectedNode.EnsureVisible();
                    //SelectedItems = selecteditemscopy;
                }
            }
            //else if (SelectedLayer != null)
            //{
            //    Forms.TreeNode[] nodes = MainForm.Instance.treeView1.Nodes[0].Nodes.Find(SelectedLayer.Name, false);
            //    if (nodes.Length > 0)
            //    {
            //        MainForm.Instance.treeView1.SelectedNode = nodes[0];
            //        MainForm.Instance.treeView1.SelectedNode.EnsureVisible();
            //    }
            //}
        }

        public void saveAnimation(string filename)
        {
            characterDefinition.EditorRelated.CameraPosition = camera.Position;
            characterDefinition.EditorRelated.Version = Version;
            characterDefinition.export(filename);
        }

        //public void alignHorizontally()
        //{
        //    beginCommand("Align Horizontally");
        //    foreach (var i in SelectedItems)
        //    {
        //        i.pPosition = new Vector2(i.pPosition.X, SelectedItems[0].pPosition.Y);
        //    }
        //    endCommand();
        //}

        //public void alignVertically()
        //{
        //    beginCommand("Align Vertically");
        //    foreach (var i in SelectedItems)
        //    {
        //        i.pPosition = new Vector2(SelectedItems[0].pPosition.X, i.pPosition.Y);
        //    }
        //    endCommand();
        //}

        //public void alignRotation()
        //{
        //    beginCommand("Align Rotation");
        //    foreach (TextureItem i in SelectedItems)
        //    {
        //        i.pRotation = ((TextureItem)SelectedItems[0]).pRotation;
        //    }
        //    endCommand();
        //}

        //public void alignScale()
        //{
        //    beginCommand("Align Scale");
        //    foreach (TextureItem i in SelectedItems)
        //    {
        //        i.pScale = ((TextureItem)SelectedItems[0]).pScale;
        //    }
        //    endCommand();
        //}






        public void beginCommand(string description)
        {
            if (commandInProgress)
            {
                undoBuffer.Pop();
            }
            undoBuffer.Push(new Command(description));
            commandInProgress = true;
        }
        public void endCommand()
        {
            if (!commandInProgress) return;
            //System.Diagnostics.Debug.WriteLine(System.DateTime.Now.ToString() + ": endCommand()");
            undoBuffer.Peek().saveAfterState();
            redoBuffer.Clear();
            MainForm.Instance.redoButton.DropDownItems.Clear();
            MainForm.Instance.DirtyFlag = true;
            MainForm.Instance.undoButton.Enabled = MainForm.Instance.undoMenuItem.Enabled = undoBuffer.Count > 0;
            MainForm.Instance.redoButton.Enabled = MainForm.Instance.redoMenuItem.Enabled = redoBuffer.Count > 0;

            var item = new Forms.ToolStripMenuItem(undoBuffer.Peek().Description);
            item.Tag = undoBuffer.Peek();
            MainForm.Instance.undoButton.DropDownItems.Insert(0, item);
            commandInProgress = false;
        }
        public void abortCommand()
        {
            if (!commandInProgress) return;
            undoBuffer.Pop();
            commandInProgress = false;
        }
        public void undo()
        {
            if (commandInProgress)
            {
                undoBuffer.Pop();
                commandInProgress = false;
            }
            if (undoBuffer.Count == 0) return;
            undoBuffer.Peek().Undo();
            redoBuffer.Push(undoBuffer.Pop());
            MainForm.Instance.propertyGrid1.Refresh();
            MainForm.Instance.DirtyFlag = true;
            MainForm.Instance.undoButton.Enabled = MainForm.Instance.undoMenuItem.Enabled = undoBuffer.Count > 0;
            MainForm.Instance.redoButton.Enabled = MainForm.Instance.redoMenuItem.Enabled = redoBuffer.Count > 0;
            MainForm.Instance.redoButton.DropDownItems.Insert(0, MainForm.Instance.undoButton.DropDownItems[0]);
        }
        public void undoMany(Command c)
        {
            while (redoBuffer.Count == 0 || redoBuffer.Peek() != c) undo();
        }
        public void redo()
        {
            if (commandInProgress)
            {
                undoBuffer.Pop();
                commandInProgress = false;
            }
            if (redoBuffer.Count == 0) return;
            redoBuffer.Peek().Redo();
            undoBuffer.Push(redoBuffer.Pop());
            MainForm.Instance.propertyGrid1.Refresh();
            MainForm.Instance.DirtyFlag = true;
            MainForm.Instance.undoButton.Enabled = MainForm.Instance.undoMenuItem.Enabled = undoBuffer.Count > 0;
            MainForm.Instance.redoButton.Enabled = MainForm.Instance.redoMenuItem.Enabled = redoBuffer.Count > 0;
            MainForm.Instance.undoButton.DropDownItems.Insert(0, MainForm.Instance.redoButton.DropDownItems[0]);
        }
        public void redoMany(Command c)
        {
            while (undoBuffer.Count == 0 || undoBuffer.Peek() != c) redo();
        }
    }

    public enum EditorState
    {
        idle,
        moving,         //user is moving an item
        rotating,       //user is rotating an item
        scaling,        //user is scaling an item
        brush_primitive //use is adding a primitive item
    }

    public enum PrimitiveType
    {
        Rectangle, Circle, Path
    }
}
