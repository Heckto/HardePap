﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using LevelEditor.Properties;
using Microsoft.Xna.Framework;
using MonoGame.Forms.Controls;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Forms = System.Windows.Forms;
using AuxLib.Logging;
using AuxLib.Camera;
using Game1.GameObjects.Levels;
using Game1.GameObjects;
using AuxLib;

namespace LevelEditor
{


    public class EditorControl : MonoGameControl
    {        
        public bool canDraw = false;

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
        Brush currentbrush;
        PrimitiveType currentprimitive;
        bool primitivestarted;
        List<Vector2> clickedPoints = new List<Vector2>();
        Layer selectedlayer;
        public Layer SelectedLayer
        {
            get { return selectedlayer; }
            set
            {
                selectedlayer = value;
                if (value == null) MainForm.Instance.toolStripStatusLabel2.Text = "Layer: (none)";
                else MainForm.Instance.toolStripStatusLabel2.Text = "Layer: " + selectedlayer.Name;
            }
        }
        GameObject lastitem;
        public List<GameObject> SelectedItems;
        Rectangle selectionrectangle = new Rectangle();
        Vector2 mouseworldpos, grabbedpoint, initialcampos, newPosition;
        List<Vector2> initialpos;                   //position before user interaction
        List<float> initialrot;                     //rotation before user interaction
        List<Vector2> initialscale;                 //scale before user interaction
        public Level level;
        public BoundedCamera camera;
        KeyboardState kstate, oldkstate;
        MouseState mstate, oldmstate;
        Forms.Cursor cursorRot, cursorScale, cursorDup;
        Stack<Command> undoBuffer = new Stack<Command>();
        Stack<Command> redoBuffer = new Stack<Command>();
        bool commandInProgress = false;
        public Texture2D dummytexture;
        bool drawSnappedPoint = false;
        Vector2 posSnappedPoint = Vector2.Zero;
        public string Version;

        protected override void Initialize()
        {
            Logger.Instance.log("Editor creation started.");
            state = EditorState.idle;

            SelectedItems = new List<GameObject>();
            initialpos = new List<Vector2>();
            initialrot = new List<float>();
            initialscale = new List<Vector2>();

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
            MainForm.Instance.ViewGrid.Checked = Constants.Instance.ShowGrid;
            MainForm.Instance.ViewSnapToGrid.Checked = Constants.Instance.SnapToGrid;
            MainForm.Instance.ViewWorldOrigin.Checked = Constants.Instance.ShowWorldOrigin;
            Logger.Instance.log("Settings loaded.");

            Logger.Instance.log("Creating new level.");
            
            Logger.Instance.log("New level created.");

            Logger.Instance.log("Editor creation ended.");
            base.Initialize();

            MainForm.Instance.picturebox.Editor.Content.RootDirectory = @"C:\Users\martijn.kirsten\Desktop\HardePap-master\Game1\Content\bin\Windows\Content";
            MainForm.Instance.newLevel();

            
        }

        public EditorControl()
        {
            
        }

        protected override void Draw()
        {
            if (Editor == null && !canDraw)
                return;
            base.Draw();

            var sb = Editor.spriteBatch;
            GraphicsDevice.Clear(Constants.Instance.ColorBackground);
            if (level == null) return;
            foreach (var l in level.Layers)
            {
                var maincameraposition = camera.Position;
                camera.Position *= l.ScrollSpeed;

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.getViewMatrix());

                l.drawInEditor(sb);
                if (l == SelectedLayer && state == EditorState.selecting)
                {
                    Primitives.Instance.drawBoxFilled(sb, selectionrectangle, Constants.Instance.ColorSelectionBox);
                }
                if (l == SelectedLayer && state == EditorState.brush)
                {
                    currentbrush.Draw(sb, mouseworldpos);
                    //sb.Draw(currentbrush.texture, new Vector2(mouseworldpos.X, mouseworldpos.Y), null, new Color(1f, 1f, 1f, 0.7f),
                    //    0, new Vector2(currentbrush.texture.Width / 2, currentbrush.texture.Height / 2), 1, SpriteEffects.None, 0);
                }
                if (l == SelectedLayer && state == EditorState.brush_primitive && primitivestarted)
                {
                    switch (currentprimitive)
                    {
                        case PrimitiveType.Rectangle:
                            var rect = Extensions.RectangleFromVectors(clickedPoints[0], mouseworldpos);
                            Primitives.Instance.drawBoxFilled(sb, rect, Constants.Instance.ColorPrimitives);
                            break;
                        case PrimitiveType.Circle:
                            Primitives.Instance.drawCircleFilled(sb, clickedPoints[0], (mouseworldpos - clickedPoints[0]).Length(), Constants.Instance.ColorPrimitives);
                            break;
                        case PrimitiveType.Path:
                            Primitives.Instance.drawPath(sb, clickedPoints.ToArray(), Constants.Instance.ColorPrimitives, Constants.Instance.DefaultPathItemLineWidth);
                            Primitives.Instance.drawLine(sb, clickedPoints.Last(), mouseworldpos, Constants.Instance.ColorPrimitives, Constants.Instance.DefaultPathItemLineWidth);
                            break;

                    }
                }
                sb.End();
                //restore main camera position
                camera.Position = maincameraposition;
            }

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.getViewMatrix());
            Primitives.Instance.drawBox(sb, level.LevelBounds, Color.Green, 10);          
            sb.End();


            //draw selection frames around selected items
            if (SelectedItems.Count > 0)
            {
                var maincameraposition = camera.Position;
                camera.Position *= SelectedItems[0].layer.ScrollSpeed;
                sb.Begin();
                var i = 0;
                foreach (var item in SelectedItems)
                {
                    if (item.Visible && item.layer.Visible && kstate.IsKeyUp(Keys.Space))
                    {
                        var color = i == 0 ? Constants.Instance.ColorSelectionFirst : Constants.Instance.ColorSelectionRest;
                        item.drawSelectionFrame(sb, camera.getViewMatrix(), color);
                        if (i == 0 && (state == EditorState.rotating || state == EditorState.scaling))
                        {
                            var center = Vector2.Transform(item.Transform.Position, camera.getViewMatrix());
                            var mouse = Vector2.Transform(mouseworldpos, camera.getViewMatrix());
                            Primitives.Instance.drawLine(sb, center, mouse, Constants.Instance.ColorSelectionFirst, 1);
                        }
                    }
                    i++;
                }
                sb.End();
                //restore main camera position
                camera.Position = maincameraposition;

            }

            if (Constants.Instance.ShowGrid)
            {
                sb.Begin();
                var max = Constants.Instance.GridNumberOfGridLines / 2;
                for (var x = 0; x <= max; x++)
                {
                    var start = Vector2.Transform(new Vector2(x, -max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    var end = Vector2.Transform(new Vector2(x, max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                    start = Vector2.Transform(new Vector2(-x, -max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    end = Vector2.Transform(new Vector2(-x, max) * Constants.Instance.GridSpacing.X, camera.getViewMatrix());
                    Primitives.Instance.drawLine(sb, start, end, Constants.Instance.GridColor, Constants.Instance.GridLineThickness);
                }
                for (var y = 0; y <= max; y++)
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
            if (Constants.Instance.ShowWorldOrigin)
            {
                sb.Begin();
                var worldOrigin = Vector2.Transform(Vector2.Zero, camera.getViewMatrix());
                Primitives.Instance.drawLine(sb, worldOrigin + new Vector2(-20, 0), worldOrigin + new Vector2(+20, 0), Constants.Instance.WorldOriginColor, Constants.Instance.WorldOriginLineThickness);
                Primitives.Instance.drawLine(sb, worldOrigin + new Vector2(0, -20), worldOrigin + new Vector2(0, 20), Constants.Instance.WorldOriginColor, Constants.Instance.WorldOriginLineThickness);
                sb.End();
            }

            if (drawSnappedPoint)
            {
                sb.Begin();
                posSnappedPoint = Vector2.Transform(posSnappedPoint, camera.getViewMatrix());
                Primitives.Instance.drawBoxFilled(sb, posSnappedPoint.X - 5, posSnappedPoint.Y - 5, 10, 10, Constants.Instance.ColorSelectionFirst);
                sb.End();

            }

            drawSnappedPoint = false;


            //Editor.spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)        
        {
            if (!this.Focused)
                return;
            if (level == null) return;

            oldkstate = kstate;
            oldmstate = mstate;
            kstate = Keyboard.GetState();




            mstate = Mouse.GetState();
            var mwheeldelta = mstate.ScrollWheelValue - oldmstate.ScrollWheelValue;
            if (mwheeldelta > 0 /* && kstate.IsKeyDown(Keys.LeftControl)*/)
            {
                var zoom = (float)Math.Round(camera.Zoom * 10) * 10.0f + 10.0f;
                MainForm.Instance.zoomcombo.Text = zoom.ToString() + "%";
                camera.Zoom = zoom / 100.0f;
            }
            if (mwheeldelta < 0 /* && kstate.IsKeyDown(Keys.LeftControl)*/)
            {
                var zoom = (float)Math.Round(camera.Zoom * 10) * 10.0f - 10.0f;
                if (zoom <= 0.0f) return;
                MainForm.Instance.zoomcombo.Text = zoom.ToString() + "%";
                camera.Zoom = zoom / 100.0f;
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

            if (kstate.IsKeyDown(Keys.Delete))
                deleteSelectedItems();

            if (kstate.IsKeyDown(Keys.Subtract))
            {
                var zoom = (float)(camera.Zoom * 0.995);
                MainForm.Instance.zoomcombo.Text = (zoom * 100).ToString("###0.0") + "%";
                camera.Zoom = zoom;
            }
            if (kstate.IsKeyDown(Keys.Add))
            {
                var zoom = (float)(camera.Zoom * 1.005);
                MainForm.Instance.zoomcombo.Text = (zoom * 100).ToString("###0.0") + "%";
                camera.Zoom = zoom;
            }

            //get mouse world position considering the ScrollSpeed of the current layer
            var maincameraposition = camera.Position;
            if (SelectedLayer != null)
                camera.Position *= SelectedLayer.ScrollSpeed;
            mouseworldpos = Vector2.Transform(new Vector2(mstate.X, mstate.Y), Matrix.Invert(camera.getViewMatrix()));
            //mouseworldpos = mouseworldpos.Round();
            mouseworldpos = new Vector2((float)Math.Round(mouseworldpos.X), (float)Math.Round(mouseworldpos.Y));
            MainForm.Instance.toolStripStatusLabel3.Text = "Mouse: (" + mouseworldpos.X + ", " + mouseworldpos.Y + ")";
            camera.Position = maincameraposition;


            if (state == EditorState.idle)
            {
                //get item under mouse cursor
                var item = getItemAtPos(mouseworldpos);
                if (item != null)
                {

                    MainForm.Instance.toolStripStatusLabel1.Text = item.Name;
                    var isOver = item.onMouseOver(mouseworldpos, out var msg);
                    if (isOver)
                    {
                        MainForm.Instance.picturebox.Cursor = Forms.Cursors.Hand;
                        MainForm.Instance.toolStripStatusLabel1.Text = msg;
                    }
                    else
                    {
                        MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    }
                    if (kstate.IsKeyDown(Keys.LeftControl)) MainForm.Instance.picturebox.Cursor = cursorDup;                    
                }
                else
                {
                    MainForm.Instance.toolStripStatusLabel1.Text = "";
                }
                if (item != lastitem && lastitem != null)
                {
                    lastitem.onMouseOut();
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                }
                lastitem = item;

                //LEFT MOUSE BUTTON CLICK
                if ((mstate.LeftButton == ButtonState.Pressed && oldmstate.LeftButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D1) && oldkstate.IsKeyUp(Keys.D1)))
                {
                    if (item != null)
                    {                        
                        item.onMouseButtonDown(mouseworldpos);
                        MainForm.Instance.picturebox.Cursor = Forms.Cursors.SizeAll;
                    }
                    if (kstate.IsKeyDown(Keys.LeftControl) && item != null)
                    {
                        if (!SelectedItems.Contains(item)) selectitem(item);

                        beginCommand("Add Item(s)");

                        var selecteditemscopy = new List<GameObject>();
                        foreach (var selitem in SelectedItems)
                        {
                            var i2 = selitem.clone();
                            selecteditemscopy.Add(i2);
                        }
                        foreach (var selitem in selecteditemscopy)
                        {
                            selitem.Name = selitem.getNamePrefix() + level.getNextItemNumber();
                            addItem(selitem);
                        }
                        selectitem(selecteditemscopy[0]);
                        updatetreeview();
                        for (var i = 1; i < selecteditemscopy.Count; i++) SelectedItems.Add(selecteditemscopy[i]);
                        startMoving();
                    }
                    else if (kstate.IsKeyDown(Keys.LeftShift) && item != null)
                    {
                        if (SelectedItems.Contains(item)) SelectedItems.Remove(item);
                        else SelectedItems.Add(item);
                    }
                    else if (SelectedItems.Contains(item))
                    {
                        beginCommand("Change Item(s)");
                        startMoving();
                    }
                    else if (!SelectedItems.Contains(item))
                    {
                        selectitem(item);
                        if (item != null)
                        {
                            beginCommand("Change Item(s)");
                            startMoving();
                        }
                        else
                        {
                            grabbedpoint = mouseworldpos;
                            selectionrectangle = Rectangle.Empty;
                            state = EditorState.selecting;
                        }

                    }
                }

                //MIDDLE MOUSE BUTTON CLICK
                if ((mstate.MiddleButton == ButtonState.Pressed && oldmstate.MiddleButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D2) && oldkstate.IsKeyUp(Keys.D2)))
                {
                    if (item != null) item.onMouseOut();
                    if (kstate.IsKeyDown(Keys.LeftControl))
                    {
                        grabbedpoint = new Vector2(mstate.X, mstate.Y);
                        initialcampos = camera.Position;
                        state = EditorState.cameramoving;
                        MainForm.Instance.picturebox.Cursor = Forms.Cursors.SizeAll;
                    }
                    else
                    {
                        if (SelectedItems.Count > 0)
                        {
                            grabbedpoint = mouseworldpos - SelectedItems[0].Transform.Position;

                            //save the initial rotation for each item
                            initialrot.Clear();
                            foreach (var selitem in SelectedItems)
                            {                                    
                                initialrot.Add(selitem.Transform.Rotation);
                            }

                            state = EditorState.rotating;
                            MainForm.Instance.picturebox.Cursor = cursorRot;

                            beginCommand("Rotate Item(s)");
                        }
                    }
                }

                //RIGHT MOUSE BUTTON CLICK
                if ((mstate.RightButton == ButtonState.Pressed && oldmstate.RightButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D3) && oldkstate.IsKeyUp(Keys.D3)))

                {
                    if (item != null) item.onMouseOut();
                    if (SelectedItems.Count > 0)
                    {
                        grabbedpoint = mouseworldpos - SelectedItems[0].Transform.Position;

                        //save the initial scale for each item
                        initialscale.Clear();
                        foreach (var selitem in SelectedItems)
                        {
                            initialscale.Add(selitem.Transform.Scale);
                        }

                        state = EditorState.scaling;
                        MainForm.Instance.picturebox.Cursor = cursorScale;

                        beginCommand("Scale Item(s)");
                    }
                }

                if (kstate.IsKeyDown(Keys.H) && oldkstate.GetPressedKeys().Length == 0 && SelectedItems.Count > 0)
                {
                    beginCommand("Flip Item(s) Horizontally");
                    foreach (var selitem in SelectedItems)
                    {
                        if (selitem is TextureItem)
                        {
                            var ti = (TextureItem)selitem;
                            ti.FlipHorizontally = !ti.FlipHorizontally;
                        }
                    }
                    MainForm.Instance.propertyGrid1.Refresh();
                    endCommand();
                }
                if (kstate.IsKeyDown(Keys.V) && oldkstate.GetPressedKeys().Length == 0 && SelectedItems.Count > 0)
                {
                    beginCommand("Flip Item(s) Vertically");
                    foreach (var selitem in SelectedItems)
                    {
                        if (selitem is TextureItem)
                        {
                            var ti = (TextureItem)selitem;
                            ti.FlipVertically = !ti.FlipVertically;
                        }
                    }
                    MainForm.Instance.propertyGrid1.Refresh();
                    endCommand();
                }
            }

            if (state == EditorState.moving)
            {
                var i = 0;
                foreach (var selitem in SelectedItems)
                {
                    newPosition = initialpos[i] + mouseworldpos - grabbedpoint;
                    if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G)) newPosition = snapToGrid(newPosition);
                    drawSnappedPoint = false;
                    selitem.Transform.Position = newPosition;
                    i++;
                }
                MainForm.Instance.propertyGrid1.Refresh();
                if ((mstate.LeftButton == ButtonState.Released && oldmstate.LeftButton == ButtonState.Pressed) ||
                    (kstate.IsKeyUp(Keys.D1) && oldkstate.IsKeyDown(Keys.D1)))
                {

                    foreach (var selitem in SelectedItems)
                        selitem.onMouseButtonUp(mouseworldpos);

                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    if (mouseworldpos != grabbedpoint) endCommand(); else abortCommand();
                }
            }

            if (state == EditorState.rotating)
            {
                var newpos = mouseworldpos - SelectedItems[0].Transform.Position;
                var deltatheta = (float)Math.Atan2(grabbedpoint.Y, grabbedpoint.X) - (float)Math.Atan2(newpos.Y, newpos.X);
                var i = 0;
                foreach (var selitem in SelectedItems)
                {
                    
                    selitem.Transform.Rotation = (initialrot[i] - deltatheta);
                    if (kstate.IsKeyDown(Keys.LeftControl))
                    {
                        selitem.Transform.Rotation = ((float)Math.Round(selitem.Transform.Rotation / MathHelper.PiOver4) * MathHelper.PiOver4);
                    }
                    i++;                    
                }
                MainForm.Instance.propertyGrid1.Refresh();
                if ((mstate.MiddleButton == ButtonState.Released && oldmstate.MiddleButton == ButtonState.Pressed) ||
                    (kstate.IsKeyUp(Keys.D2) && oldkstate.IsKeyDown(Keys.D2)))
                {
                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    endCommand();
                }
            }

            if (state == EditorState.scaling)
            {
                var newdistance = mouseworldpos - SelectedItems[0].Transform.Position;
                var factor = newdistance.Length() / grabbedpoint.Length();
                var i = 0;
                foreach (var selitem in SelectedItems)
                {
                    
                    if (selitem is TextureItem)
                    {
                        MainForm.Instance.toolStripStatusLabel1.Text = "Hold down [X] or [Y] to limit scaling to the according dimension.";
                    }

                    var newscale = initialscale[i];
                    if (!kstate.IsKeyDown(Keys.Y)) newscale.X = initialscale[i].X * (((factor - 1.0f) * 0.5f) + 1.0f);
                    if (!kstate.IsKeyDown(Keys.X)) newscale.Y = initialscale[i].Y * (((factor - 1.0f) * 0.5f) + 1.0f);
                    selitem.Transform.Scale = newscale;

                    if (kstate.IsKeyDown(Keys.LeftControl))
                    {
                        Vector2 scale;
                        scale.X = (float)Math.Round(selitem.Transform.Scale.X * 10) / 10;
                        scale.Y = (float)Math.Round(selitem.Transform.Scale.Y * 10) / 10;
                        selitem.Transform.Scale = scale;
                    }
                    i++;                                     
                }
                MainForm.Instance.propertyGrid1.Refresh();
                if ((mstate.RightButton == ButtonState.Released && oldmstate.RightButton == ButtonState.Pressed) ||
                    (kstate.IsKeyUp(Keys.D3) && oldkstate.IsKeyDown(Keys.D3)))
                {
                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                    endCommand();
                }
            }

            if (state == EditorState.cameramoving)
            {
                var newpos = new Vector2(mstate.X, mstate.Y);
                var distance = (newpos - grabbedpoint) / camera.Zoom;
                if (distance.Length() > 0)
                {
                    camera.Position = initialcampos - distance;
                }
                if (mstate.MiddleButton == ButtonState.Released)
                {
                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                }
            }

            if (state == EditorState.selecting)
            {
                if (SelectedLayer == null) return;
                var distance = mouseworldpos - grabbedpoint;
                if (distance.Length() > 0)
                {
                    SelectedItems.Clear();
                    selectionrectangle = Extensions.RectangleFromVectors(grabbedpoint, mouseworldpos);
                    foreach (var i in SelectedLayer.Items)
                    {
                        if (i.Visible && selectionrectangle.Contains((int)i.Transform.Position.X, (int)i.Transform.Position.Y)) SelectedItems.Add(i);
                    }
                    updatetreeviewselection();
                }
                if (mstate.LeftButton == ButtonState.Released)
                {
                    state = EditorState.idle;
                    MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
                }
            }

            if (state == EditorState.brush)
            {
                if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G))
                {
                    mouseworldpos = snapToGrid(mouseworldpos);
                }
                if (mstate.RightButton == ButtonState.Pressed && oldmstate.RightButton == ButtonState.Released) state = EditorState.idle;
                if (mstate.LeftButton == ButtonState.Pressed && oldmstate.LeftButton == ButtonState.Released) paintTextureBrush(true);
            }


            if (state == EditorState.brush_primitive)
            {

                if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G)) mouseworldpos = snapToGrid(mouseworldpos);

                if (kstate.IsKeyDown(Keys.LeftControl) && primitivestarted && currentprimitive == PrimitiveType.Rectangle)
                {
                    var distance = mouseworldpos - clickedPoints[0];
                    var squareside = Math.Max(distance.X, distance.Y);
                    mouseworldpos = clickedPoints[0] + new Vector2(squareside, squareside);
                }
                if ((mstate.LeftButton == ButtonState.Pressed && oldmstate.LeftButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D1) && oldkstate.IsKeyUp(Keys.D1)))
                {
                    clickedPoints.Add(mouseworldpos);
                    if (primitivestarted == false)
                    {
                        primitivestarted = true;
                        switch (currentprimitive)
                        {
                            case PrimitiveType.Rectangle:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Started;
                                break;
                            case PrimitiveType.Circle:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Started;
                                break;
                            case PrimitiveType.Path:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Started;
                                break;
                        }
                    }
                    else
                    {
                        if (currentprimitive != PrimitiveType.Path)
                        {
                            paintPrimitiveBrush();
                            clickedPoints.Clear();
                            primitivestarted = false;
                        }
                    }
                }
                if (kstate.IsKeyDown(Keys.Back) && oldkstate.IsKeyUp(Keys.Back))
                {
                    if (currentprimitive == PrimitiveType.Path && clickedPoints.Count > 1)
                    {
                        clickedPoints.RemoveAt(clickedPoints.Count - 1);
                    }
                }

                if ((mstate.MiddleButton == ButtonState.Pressed && oldmstate.MiddleButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D2) && oldkstate.IsKeyUp(Keys.D2)))
                {
                    if (currentprimitive == PrimitiveType.Path && primitivestarted)
                    {
                        paintPrimitiveBrush();
                        clickedPoints.Clear();
                        primitivestarted = false;
                        MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
                    }
                }
                if ((mstate.RightButton == ButtonState.Pressed && oldmstate.RightButton == ButtonState.Released) ||
                    (kstate.IsKeyDown(Keys.D3) && oldkstate.IsKeyUp(Keys.D3)))
                {
                    if (primitivestarted)
                    {
                        clickedPoints.Clear();
                        primitivestarted = false;
                        switch (currentprimitive)
                        {
                            case PrimitiveType.Rectangle:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
                                break;
                            case PrimitiveType.Circle:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
                                break;
                            case PrimitiveType.Path:
                                MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
                                break;
                        }
                    }
                    else
                    {
                        destroyPrimitiveBrush();
                        clickedPoints.Clear();
                        primitivestarted = false;
                    }
                }
            }
        }

        public void getSelectionFromLevel()
        {
            SelectedItems.Clear();
            SelectedLayer = null;
            var itemnames = level.selecteditems.Split(';');
            foreach (var itemname in itemnames)
            {
                if (itemname.Length > 0) SelectedItems.Add(level.getItemByName(itemname));
            }
            SelectedLayer = level.getLayerByName(level.selectedlayers);
        }

        public void selectlevel()
        {
            MainForm.Instance.propertyGrid1.SelectedObject = level;
        }

        public void addLayer(Layer l)
        {
            l.level = level;
            if (!l.level.Layers.Contains(l)) l.level.Layers.Add(l);
        }

        public void deleteLayer(Layer l)
        {
            if (level.Layers.Count > 0)
            {
                MainForm.Instance.picturebox.beginCommand("Delete Layer \"" + l.Name + "\"");
                level.Layers.Remove(l);
                MainForm.Instance.picturebox.endCommand();
            }
            if (level.Layers.Count > 0) SelectedLayer = level.Layers.Last();
            else SelectedLayer = null;
            selectitem(null);
            updatetreeview();
        }

        public void moveLayerUp(Layer l)
        {
            var index = level.Layers.IndexOf(l);
            level.Layers[index] = level.Layers[index - 1];
            level.Layers[index - 1] = l;
            selectlayer(l);
        }

        public void moveLayerDown(Layer l)
        {
            var index = level.Layers.IndexOf(l);
            level.Layers[index] = level.Layers[index + 1];
            level.Layers[index + 1] = l;
            selectlayer(l);
        }

        public void selectlayer(Layer l)
        {
            if (SelectedItems.Count > 0) selectitem(null);
            SelectedLayer = l;
            updatetreeviewselection();
            MainForm.Instance.propertyGrid1.SelectedObject = l;
        }

        public void addItem(GameObject i)
        {
            if (!i.layer.Items.Contains(i)) i.layer.Items.Add(i);
        }

        public void deleteSelectedItems()
        {
            beginCommand("Delete Item(s)");
            var selecteditemscopy = new List<GameObject>(SelectedItems);

            var itemsaffected = new List<GameObject>();

            foreach (var selitem in selecteditemscopy)
            {

                foreach (var l in level.Layers)
                    foreach (var i in l.Items)
                        foreach (var cp in i.CustomProperties.Values)
                        {
                            if (cp.type == typeof(GameObject) && cp.value == selitem)
                            {
                                cp.value = null;
                                itemsaffected.Add(i);
                            }
                        }

                selitem.layer.Items.Remove(selitem);
            }
            endCommand();
            selectitem(null);
            updatetreeview();

            if (itemsaffected.Count > 0)
            {
                var message = "";
                foreach (var item in itemsaffected) message += item.Name + " (Layer: " + item.layer.Name + ")\n";
                Forms.MessageBox.Show("The following Items have Custom Properties of Type \"Item\" that refered to items that have just been deleted:\n\n"
                    + message + "\nThe corresponding Custom Properties have been set to NULL, since the Item referred to doesn't exist anymore.");
            }

        }

        public void moveItemUp(GameObject i)
        {
            var index = i.layer.Items.IndexOf(i);
            i.layer.Items[index] = i.layer.Items[index - 1];
            i.layer.Items[index - 1] = i;
            //updatetreeview();
        }

        public void moveItemDown(GameObject i)
        {
            var index = i.layer.Items.IndexOf(i);
            i.layer.Items[index] = i.layer.Items[index + 1];
            i.layer.Items[index + 1] = i;
            selectitem(i);
            //updatetreeview();
        }

        public void selectitem(GameObject i)
        {
            SelectedItems.Clear();
            if (i != null)
            {
                SelectedItems.Add(i);
                SelectedLayer = i.layer;
                updatetreeviewselection();
                MainForm.Instance.propertyGrid1.SelectedObject = i;
            }
            else
            {
                selectlayer(SelectedLayer);
            }
        }

        public void selectAll()
        {
            if (SelectedLayer == null) return;
            //if (SelectedLayer.Items.Count == 0) return;
            SelectedItems.Clear();
            foreach (var i in SelectedLayer.Items)
            {
                SelectedItems.Add(i);
            }
            updatetreeviewselection();
        }

        public void moveItemToLayer(GameObject i1, Layer l2, GameObject i2)
        {
            var index2 = i2 == null ? 0 : l2.Items.IndexOf(i2);
            i1.layer.Items.Remove(i1);
            l2.Items.Insert(index2, i1);
            i1.layer = l2;
        }
        public void moveSelectedItemsToLayer(Layer chosenlayer)
        {
            if (chosenlayer == SelectedLayer) return;
            beginCommand("Move Item(s) To Layer \"" + chosenlayer.Name + "\"");
            var selecteditemscopy = new List<GameObject>(SelectedItems);
            foreach (var i in selecteditemscopy)
            {
                moveItemToLayer(i, chosenlayer, null);
            }
            endCommand();
            SelectedItems.Clear();
            updatetreeview();
        }
        public void copySelectedItemsToLayer(Layer chosenlayer)
        {
            //if (chosenlayer == SelectedLayer) return;
            beginCommand("Copy Item(s) To Layer \"" + chosenlayer.Name + "\"");
            var selecteditemscopy = new List<GameObject>(SelectedItems);
            foreach (var i in selecteditemscopy)
            {
                var copy = i.clone();
                copy.layer = chosenlayer;
                copy.Name = MainForm.Instance.getUniqueNameBasedOn(copy.Name);
                addItem(copy);
                
            }
            endCommand();
            SelectedItems.Clear();
            updatetreeview();
        }

        public void createTextureBrush(Texture2D tex, Rectangle srcRect,string asset_name, string sprite_name)
        {
            if (SelectedLayer == null)
            {
                Forms.MessageBox.Show(Resources.No_Layer);
                destroyTextureBrush();
                return;
            }
            state = EditorState.brush;
            currentbrush = new TextureBrush(tex, srcRect, asset_name, sprite_name);
        }

        public void createEntityBrush(GameObject o,string name)
        {
            if (SelectedLayer == null)
            {
                Forms.MessageBox.Show(Resources.No_Layer);
                destroyTextureBrush();
                return;
            }
            state = EditorState.brush;
            currentbrush = new EntityBrush(o, name);
        }

        public void destroyTextureBrush()
        {
            state = EditorState.idle;
            currentbrush = null;
        }

        public void paintTextureBrush(bool continueAfterPaint)
        {
            GameObject i = null;
            if (currentbrush is TextureBrush br)
            {
                
                i = new TextureItem(br.spriteSheet, new Vector2((int)mouseworldpos.X, (int)mouseworldpos.Y), MainForm.Instance.spriteSheets[(currentbrush as TextureBrush).spriteSheet].SpriteDef[(currentbrush as TextureBrush).spriteName].SrcRectangle, MainForm.Instance.spriteSheets[(currentbrush as TextureBrush).spriteSheet].Texture);
            }
            else if (currentbrush is EntityBrush ebr)
            {

                i = (GameObject)Activator.CreateInstance(ebr.entity.GetType());
                i.Transform.Position = mouseworldpos;
                i.OnTransformed();
            }
            i.Name = i.getNamePrefix() + level.getNextItemNumber();
            i.layer = SelectedLayer;
            beginCommand("Add Item \"" + i.Name + "\"");
            addItem(i);
            endCommand();
            updatetreeview();
            if (!continueAfterPaint) destroyTextureBrush();
        }

        public void createPrimitiveBrush(PrimitiveType primitiveType)
        {
            if (SelectedLayer == null)
            {
                Forms.MessageBox.Show(Resources.No_Layer);
                return;
            }

            state = EditorState.brush_primitive;
            primitivestarted = false;
            clickedPoints.Clear();
            currentprimitive = primitiveType;
            MainForm.Instance.picturebox.Cursor = Forms.Cursors.Cross;
            MainForm.Instance.listView2.Cursor = Forms.Cursors.Cross;
            switch (primitiveType)
            {
                case PrimitiveType.Rectangle:
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
                    break;
                case PrimitiveType.Circle:
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
                    break;
                case PrimitiveType.Path:
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
                    break;

            }
        }

        public void destroyPrimitiveBrush()
        {
            state = EditorState.idle;
            MainForm.Instance.picturebox.Cursor = Forms.Cursors.Default;
            MainForm.Instance.listView2.Cursor = Forms.Cursors.Default;
        }

        public void paintPrimitiveBrush()
        {
            switch (currentprimitive)
            {
                case PrimitiveType.Rectangle:
                    GameObject ri = new RectangleItem(Extensions.RectangleFromVectors(clickedPoints[0], clickedPoints[1]));
                    ri.Name = ri.getNamePrefix() + level.getNextItemNumber();
                    ri.layer = SelectedLayer;
                    beginCommand("Add Item \"" + ri.Name + "\"");
                    addItem(ri);
                    endCommand();
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Rectangle_Entered;
                    break;
                case PrimitiveType.Circle:
                    GameObject ci = new CircleItem(clickedPoints[0], (mouseworldpos - clickedPoints[0]).Length());
                    ci.Name = ci.getNamePrefix() + level.getNextItemNumber();
                    ci.layer = SelectedLayer;
                    beginCommand("Add Item \"" + ci.Name + "\"");
                    addItem(ci);
                    endCommand();
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Circle_Entered;
                    break;
                case PrimitiveType.Path:
                    GameObject pi = new PathItem(clickedPoints.ToArray());
                    pi.Name = pi.getNamePrefix() + level.getNextItemNumber();
                    pi.layer = SelectedLayer;
                    beginCommand("Add Item \"" + pi.Name + "\"");
                    addItem(pi);
                    endCommand();
                    MainForm.Instance.toolStripStatusLabel1.Text = Resources.Path_Entered;
                    break;
            }
            updatetreeview();
        }


        public void startMoving()
        {
            grabbedpoint = mouseworldpos;

            //save the distance to mouse for each item
            initialpos.Clear();
            foreach (var selitem in SelectedItems)
            {
                initialpos.Add(selitem.Transform.Position);
            }

            state = EditorState.moving;
            
            MainForm.Instance.picturebox.Cursor = Forms.Cursors.SizeAll;
        }

        public void setmousepos(int screenx, int screeny)
        {
            var maincameraposition = camera.Position;
            if (SelectedLayer != null)
                camera.Position *= SelectedLayer.ScrollSpeed;
            mouseworldpos = Vector2.Transform(new Vector2(screenx, screeny), Matrix.Invert(camera.getViewMatrix()));
            if (Constants.Instance.SnapToGrid || kstate.IsKeyDown(Keys.G))
            {
                mouseworldpos = snapToGrid(mouseworldpos);
            }
            camera.Position = maincameraposition;
        }

        public GameObject getItemAtPos(Vector2 mouseworldpos)
        {
            if (SelectedLayer == null) return null;
            return SelectedLayer.getItemAtPos(mouseworldpos);
            /*if (level.Layers.Count == 0) return null;
            for (int i = level.Layers.Count - 1; i >= 0; i--)
            {
                Item item = level.Layers[i].getItemAtPos(mouseworldpos);
                if (item != null) return item;
            }
            return null;*/
        }

        public async void loadLevel(Level l)
        {
            if (l.ContentRootFolder == null)
            {
                l.ContentRootFolder = Constants.Instance.DefaultContentRootFolder;
                if (!Directory.Exists(l.ContentRootFolder))
                {
                    var dr = Forms.MessageBox.Show(
                        "The DefaultContentRootFolder \"" + l.ContentRootFolder + "\" (as set in the Settings Dialog) doesn't exist!\n"
                        + "The ContentRootFolder of the new level will be set to the Editor's work directory (" + Forms.Application.StartupPath + ").\n"
                        + "Please adjust the DefaultContentRootFolder in the Settings Dialog.\n"
                        + "Do you want to open the Settings Dialog now?", "Error",
                        Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Exclamation);
                    if (dr == Forms.DialogResult.Yes) new SettingsForm().ShowDialog();
                    l.ContentRootFolder = Forms.Application.StartupPath;
                }
            }
            else
            {
                if (!Directory.Exists(l.ContentRootFolder))
                {
                    l.ContentRootFolder = Constants.Instance.DefaultContentRootFolder;
                    if (!Directory.Exists(l.ContentRootFolder))
                    {
                        var dr = Forms.MessageBox.Show(
                            "The DefaultContentRootFolder \"" + l.ContentRootFolder + "\" (as set in the Settings Dialog) doesn't exist!\n"
                            + "The ContentRootFolder of the new level will be set to the Editor's work directory (" + Forms.Application.StartupPath + ").\n"
                            + "Please adjust the DefaultContentRootFolder in the Settings Dialog.\n"
                            + "Do you want to open the Settings Dialog now?", "Error",
                            Forms.MessageBoxButtons.YesNo, Forms.MessageBoxIcon.Exclamation);
                        if (dr == Forms.DialogResult.Yes) new SettingsForm().ShowDialog();
                        l.ContentRootFolder = Forms.Application.StartupPath;
                    }
                    //Forms.MessageBox.Show("The directory \"" + l.ContentRootFolder + "\" doesn't exist! "
                    //    + "Please adjust the XML file before trying again.");
                    //return;
                }
            }

            foreach (var layer in l.Layers)
            {
                layer.level = l;
                foreach (var item in layer.Items)
                {
                    item.layer = layer;
                    item.LoadContent(Editor.Content);
                    //item.Initialize();

                    item.Transform.TranformUpdated += item.OnTransformed;
                    item.Transform.TransformBecameDirty += item.OnTransformed;

                    item.OnTransformed();
                }
            }

            MainForm.Instance.LoadFolderContent(l.ContentRootFolder);

            if (l.Name == null) l.Name = "Level_01";


            SelectedLayer = null;
            if (l.Layers.Count > 0) SelectedLayer = l.Layers[0];
            SelectedItems.Clear();



            camera = new BoundedCamera(new Viewport(0, 0, MainForm.Instance.picturebox.Width, MainForm.Instance.picturebox.Height))
            {
                Position = l.EditorRelated.CameraPosition
            };
            MainForm.Instance.zoomcombo.Text = "100%";
            undoBuffer.Clear();
            redoBuffer.Clear();
            MainForm.Instance.undoButton.DropDownItems.Clear();
            MainForm.Instance.redoButton.DropDownItems.Clear();
            MainForm.Instance.undoButton.Enabled = MainForm.Instance.undoMenuItem.Enabled = undoBuffer.Count > 0;
            MainForm.Instance.redoButton.Enabled = MainForm.Instance.redoMenuItem.Enabled = redoBuffer.Count > 0;
            commandInProgress = false;

            level = l;

            updatetreeview();

            
        }

        public void updatetreeview()
        {
            MainForm.Instance.treeView1.Nodes.Clear();
            level.treenode = MainForm.Instance.treeView1.Nodes.Add(level.Name);
            ((Forms.TreeNode)level.treenode).Tag = level;            
            ((Forms.TreeNode)level.treenode).ContextMenuStrip = MainForm.Instance.LevelContextMenu;

            foreach (var layer in level.Layers)
            {
                var layernode = ((Forms.TreeNode)level.treenode).Nodes.Add(layer.Name, layer.Name);
                layernode.Tag = layer;
                layernode.Checked = layer.Visible;
                layernode.ContextMenuStrip = MainForm.Instance.LayerContextMenu;
                layernode.ImageIndex = layernode.SelectedImageIndex = 0;

                foreach (var item in layer.Items)
                {
                    var itemnode = layernode.Nodes.Add(item.Name, item.Name);
                    itemnode.Tag = item;
                    itemnode.Checked = true;
                    itemnode.ContextMenuStrip = MainForm.Instance.ItemContextMenu;
                    var imageindex = 0;
                    if (item is TextureItem) imageindex = 1;
                    if (item is RectangleItem) imageindex = 2;
                    if (item is CircleItem) imageindex = 3;
                    if (item is PathItem) imageindex = 4;
                    itemnode.ImageIndex = itemnode.SelectedImageIndex = imageindex;
                }
                layernode.Expand();
            }
            ((Forms.TreeNode)level.treenode).Expand();

            updatetreeviewselection();
        }

        public void updatetreeviewselection()
        {
            MainForm.Instance.propertyGrid1.SelectedObject = null;
            if (SelectedItems.Count > 0)
            {
                var nodes = MainForm.Instance.treeView1.Nodes.Find(SelectedItems[0].Name, true);
                if (nodes.Length > 0)
                {
                    var selecteditemscopy = new List<GameObject>(SelectedItems);
                    MainForm.Instance.propertyGrid1.SelectedObject = SelectedItems[0];
                    MainForm.Instance.treeView1.SelectedNode = nodes[0];
                    MainForm.Instance.treeView1.SelectedNode.EnsureVisible();
                    SelectedItems = selecteditemscopy;
                }
            }
            else if (SelectedLayer != null)
            {
                var nodes = MainForm.Instance.treeView1.Nodes[0].Nodes.Find(SelectedLayer.Name, false);
                if (nodes.Length > 0)
                {
                    MainForm.Instance.treeView1.SelectedNode = nodes[0];
                    MainForm.Instance.treeView1.SelectedNode.EnsureVisible();
                }
            }
        }

        public void saveLevel(string filename)
        {
            level.EditorRelated.CameraPosition = camera.Position;
            level.EditorRelated.Version = Version;
            level.export(filename);
        }

        public void alignHorizontally()
        {
            beginCommand("Align Horizontally");
            foreach (var i in SelectedItems)
            {
                i.Transform.Position = new Vector2(i.Transform.Position.X, SelectedItems[0].Transform.Position.Y);
            }
            endCommand();
        }

        public void alignVertically()
        {
            beginCommand("Align Vertically");
            foreach (var i in SelectedItems)
            {
                i.Transform.Position = new Vector2(SelectedItems[0].Transform.Position.X, i.Transform.Position.Y);
            }
            endCommand();
        }

        public void alignRotation()
        {
            beginCommand("Align Rotation");
            foreach (TextureItem i in SelectedItems)
            {
                i.Transform.Rotation = ((TextureItem)SelectedItems[0]).Transform.Rotation;
            }
            endCommand();
        }

        public void alignScale()
        {
            beginCommand("Align Scale");
            foreach (TextureItem i in SelectedItems)
            {
                i.Transform.Scale = ((TextureItem)SelectedItems[0]).Transform.Scale;
            }
            endCommand();
        }






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

            var item = new Forms.ToolStripMenuItem(undoBuffer.Peek().Description)
            {
                Tag = undoBuffer.Peek()
            };
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


        public Vector2 snapToGrid(Vector2 input)
        {

            var result = input;
            result.X = Constants.Instance.GridSpacing.X * (int)Math.Round(result.X / Constants.Instance.GridSpacing.X);
            result.Y = Constants.Instance.GridSpacing.Y * (int)Math.Round(result.Y / Constants.Instance.GridSpacing.Y);
            posSnappedPoint = result;
            drawSnappedPoint = true;
            return result;
        }
    }

    public enum EditorState
    {
        idle,
        brush,          //"stamp mode": user double clicked on an item to add multiple instances of it
        cameramoving,   //user is moving the camera
        moving,         //user is moving an item
        rotating,       //user is rotating an item
        scaling,        //user is scaling an item
        selecting,      //user has opened a select box by dragging the mouse (windows style)
        brush_primitive //use is adding a primitive item
    }

    public enum PrimitiveType
    {
        Rectangle, Circle, Path
    }
}
