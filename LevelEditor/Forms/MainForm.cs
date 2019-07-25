using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using LevelEditor.Properties;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using AuxLib.Logging;
using System.Threading.Tasks;

namespace LevelEditor
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;
        String levelfilename;

        public Dictionary<string,SpriteSheet> spriteSheets;


        bool dirtyflag;
        public bool DirtyFlag
        {
            get { return dirtyflag; }
            set { dirtyflag = value; updatetitlebar(); }
        }

        Cursor dragcursor;
        LinkItemsForm linkItemsForm;

        [DllImport("User32.dll")]
        private static extern int SendMessage(int Handle, int wMsg, int wParam, int lParam);

        public static void SetListViewSpacing(ListView lst, int x, int y)
        {
            SendMessage((int)lst.Handle, 0x1000 + 53, 0, y * 65536 + x);
        }


        public MainForm()
        {
            Instance = this;
            InitializeComponent();
            
        }
        
        public void updatetitlebar()
        {
            Text = "LevelEditor - " + levelfilename + (DirtyFlag ? "*" : String.Empty);
        }

        public static Image getThumbNail(Bitmap bmp, int imgWidth, int imgHeight)
        {
            var retBmp = new Bitmap(imgWidth, imgHeight, System.Drawing.Imaging.PixelFormat.Format64bppPArgb);
            var grp = Graphics.FromImage(retBmp);
            int tnWidth = imgWidth, tnHeight = imgHeight;
            if (bmp.Width > bmp.Height)
                tnHeight = (int)(((float)bmp.Height / (float)bmp.Width) * tnWidth);
            else if (bmp.Width < bmp.Height)
                tnWidth = (int)(((float)bmp.Width / (float)bmp.Height) * tnHeight);
            int iLeft = (imgWidth / 2) - (tnWidth / 2);
            int iTop = (imgHeight / 2) - (tnHeight / 2);
            grp.DrawImage(bmp, iLeft, iTop, tnWidth, tnHeight);
            retBmp.Tag = bmp;
            return retBmp;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            linkItemsForm = new LinkItemsForm();

            //fill zoom combobox
            for (var i = 25; i <= 200; i += 25)
            {
                zoomcombo.Items.Add(i.ToString() + "%");
            }
            zoomcombo.SelectedIndex = 3;
            SetListViewSpacing(listView2, 128 + 8, 128 + 32);

            picturebox.AllowDrop = true;

        }
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Constants.Instance.export("settings.xml");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel)
                e.Cancel = true;
        }

        //TREEVIEW
        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.N) ActionNewLayer(sender, e);
            if (e.KeyCode == Keys.Delete) ActionDelete(sender, e);
            if (e.KeyCode == Keys.F7) ActionMoveUp(sender, e);
            if (e.KeyCode == Keys.F8) ActionMoveDown(sender, e);
            if (e.KeyCode == Keys.F4) ActionCenterView(sender, e);
            if (e.KeyCode == Keys.F2) treeView1.SelectedNode.BeginEdit();
            if (e.KeyCode == Keys.D && e.Control) ActionDuplicate(sender, e);
        }
        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null) return;

            TreeNode[] nodes = treeView1.Nodes.Find(e.Label, true);
            if (nodes.Length > 0)
            {
                MessageBox.Show("A layer or item with the name \"" + e.Label + "\" already exists in the level. Please use another name!");
                e.CancelEdit = true;
                return;
            }
            if (e.Node.Tag is Level)
            {
                var l = (Level)e.Node.Tag;
                MainForm.Instance.picturebox.beginCommand("Rename Level (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                MainForm.Instance.picturebox.endCommand();
            }
            if (e.Node.Tag is Layer)
            {
                var l = (Layer)e.Node.Tag;
                MainForm.Instance.picturebox.beginCommand("Rename Layer (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                MainForm.Instance.picturebox.endCommand();
            }
            if (e.Node.Tag is Item)
            {
                var i = (Item)e.Node.Tag;
                MainForm.Instance.picturebox.beginCommand("Rename Item (\"" + i.Name + "\" -> \"" + e.Label + "\")");
                i.Name = e.Label;
                e.Node.Name = e.Label;
                MainForm.Instance.picturebox.endCommand();
            }
            propertyGrid1.Refresh();
            picturebox.Select();
        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                picturebox.level.Visible = e.Node.Checked;
            }
            if (e.Node.Tag is Layer)
            {
                var l = (Layer)e.Node.Tag;
                l.Visible = e.Node.Checked;
            }
            if (e.Node.Tag is Item)
            {
                var i = (Item)e.Node.Tag;
                i.Visible = e.Node.Checked;
            }
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Level)
            {
                picturebox.selectlevel();
            }
            if (e.Node.Tag is Layer)
            {
                var l = (Layer)e.Node.Tag;
                picturebox.selectlayer(l);
            }
            if (e.Node.Tag is Item)
            {
                var i = (Item)e.Node.Tag;
                picturebox.selectitem(i);
            }
        }
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = treeView1.GetNodeAt(e.X, e.Y);
            }
        }
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (((TreeNode)e.Item).Tag is Layer) return;
            if (((TreeNode)e.Item).Tag is Level) return;
            MainForm.Instance.picturebox.beginCommand("Drag Item");
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            //get source node
            var sourcenode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (sourcenode == null)
            {
                e.Effect = DragDropEffects.None;
                return;
            }
            else e.Effect = DragDropEffects.Move;

            //get destination node and select it
            var p = treeView1.PointToClient(new Point(e.X, e.Y));
            var destnode = treeView1.GetNodeAt(p);
            if (destnode.Tag is Level) return;
            treeView1.SelectedNode = destnode;

            if (destnode != sourcenode)
            {
                var i1 = (Item)sourcenode.Tag;
                if (destnode.Tag is Item)
                {
                    var i2 = (Item)destnode.Tag;
                    MainForm.Instance.picturebox.moveItemToLayer(i1, i2.layer, i2);
                    int delta = 0;
                    if (destnode.Index > sourcenode.Index && i1.layer == i2.layer) delta = 1;
                    sourcenode.Remove();
                    destnode.Parent.Nodes.Insert(destnode.Index + delta, sourcenode);
                }
                if (destnode.Tag is Layer)
                {
                    var l2 = (Layer)destnode.Tag;
                    MainForm.Instance.picturebox.moveItemToLayer(i1, l2, null);
                    sourcenode.Remove();
                    destnode.Nodes.Insert(0, sourcenode);
                }
                MainForm.Instance.picturebox.selectitem(i1);
                //MainForm.Instance.picturebox.Draw();
                MainForm.Instance.picturebox.GraphicsDevice.Present();
                Application.DoEvents();
            }
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            MainForm.Instance.picturebox.endCommand();
        }



        //PICTURE BOX
        private void pictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var kea = new KeyEventArgs(e.KeyData);
            treeView1_KeyDown(sender, kea);
        }
        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            Logger.Instance.log("pictureBox1_Resize().");
            
            //if (MainForm.Instance.picturebox != null) Game1.Instance.resizebackbuffer(picturebox.Width, picturebox.Height);
            if (MainForm.Instance.picturebox != null) MainForm.Instance.picturebox.camera.updateviewport(picturebox.Width, picturebox.Height);
        }
        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            picturebox.Select();
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            menuStrip1.Select();

        }
        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            var lvi = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

            var spriteSheet = (SpriteSheet)lvi.ListView.Tag;
            var tex = GetBrushData(spriteSheet.SpriteDef[lvi.Name].SrcRectangle, spriteSheet.Texture);
            MainForm.Instance.picturebox.createTextureBrush(tex,spriteSheet,lvi.Name);

        }
        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            var p = picturebox.PointToClient(new Point(e.X, e.Y));
            MainForm.Instance.picturebox.setmousepos(p.X, p.Y);
            //MainForm.Instance.picturebox.Draw();
            MainForm.Instance.picturebox.GraphicsDevice.Present();
        }
        private void pictureBox1_DragLeave(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.destroyTextureBrush();
            //MainForm.Instance.picturebox.Draw();
            MainForm.Instance.picturebox.GraphicsDevice.Present();
        }





        // ACTIONS
        private void ActionDuplicate(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                var l = (Layer)treeView1.SelectedNode.Tag;
                var layercopy = l.clone();
                layercopy.Name = getUniqueNameBasedOn(layercopy.Name);
                for (int i = 0; i < layercopy.Items.Count; i++)
                {
                    layercopy.Items[i].Name = getUniqueNameBasedOn(layercopy.Items[i].Name);
                }
                MainForm.Instance.picturebox.beginCommand("Duplicate Layer \"" + l.Name + "\"");
                MainForm.Instance.picturebox.addLayer(layercopy);
                MainForm.Instance.picturebox.endCommand();
                MainForm.Instance.picturebox.updatetreeview();
            }
        }
        private void ActionCenterView(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Level)
            {
                MainForm.Instance.picturebox.camera.Position = Microsoft.Xna.Framework.Vector2.Zero;
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                var i = (Item)treeView1.SelectedNode.Tag;
                MainForm.Instance.picturebox.camera.Position = i.pPosition;
            }
        }
        private void ActionRename(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }
        private void ActionNewLayer(object sender, EventArgs e)
        {
            var f = new AddLayer(this);
            f.ShowDialog();
        }
        private void ActionDelete(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null) return;
            if (treeView1.SelectedNode.Tag is Layer)
            {
                var l = (Layer)treeView1.SelectedNode.Tag;
                MainForm.Instance.picturebox.deleteLayer(l);
            }
            else if (treeView1.SelectedNode.Tag is Item)
            {
                MainForm.Instance.picturebox.deleteSelectedItems();
            }
        }
        private void ActionMoveUp(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                var l = (Layer)treeView1.SelectedNode.Tag;
                if (l.level.Layers.IndexOf(l) > 0)
                {
                    MainForm.Instance.picturebox.beginCommand("Move Up Layer \"" + l.Name + "\"");
                    MainForm.Instance.picturebox.moveLayerUp(l);
                    MainForm.Instance.picturebox.endCommand();
                    MainForm.Instance.picturebox.updatetreeview();
                }
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                var i = (Item)treeView1.SelectedNode.Tag;
                if (i.layer.Items.IndexOf(i) > 0)
                {
                    MainForm.Instance.picturebox.beginCommand("Move Up Item \"" + i.Name + "\"");
                    MainForm.Instance.picturebox.moveItemUp(i);
                    MainForm.Instance.picturebox.endCommand();
                    MainForm.Instance.picturebox.updatetreeview();
                }
            }
        }
        private void ActionMoveDown(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Layer)
            {
                var l = (Layer)treeView1.SelectedNode.Tag;
                if (l.level.Layers.IndexOf(l) < l.level.Layers.Count - 1)
                {
                    MainForm.Instance.picturebox.beginCommand("Move Down Layer \"" + l.Name + "\"");
                    MainForm.Instance.picturebox.moveLayerDown(l);
                    MainForm.Instance.picturebox.endCommand();
                    MainForm.Instance.picturebox.updatetreeview();
                }
            }
            if (treeView1.SelectedNode.Tag is Item)
            {
                var i = (Item)treeView1.SelectedNode.Tag;
                if (i.layer.Items.IndexOf(i) < i.layer.Items.Count - 1)
                {
                    MainForm.Instance.picturebox.beginCommand("Move Down Item \"" + i.Name + "\"");
                    MainForm.Instance.picturebox.moveItemDown(i);
                    MainForm.Instance.picturebox.endCommand();
                    MainForm.Instance.picturebox.updatetreeview();
                }
            }
        }
        private void ActionAddCustomProperty(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is Item)
            {
                var i = (Item)treeView1.SelectedNode.Tag;
                var form = new AddCustomProperty(i.CustomProperties);
                form.ShowDialog();
            }
            if (treeView1.SelectedNode.Tag is Level)
            {
                var l = (Level)treeView1.SelectedNode.Tag;
                var form = new AddCustomProperty(l.CustomProperties);
                form.ShowDialog();
            }
            if (treeView1.SelectedNode.Tag is Layer)
            {
                var l = (Layer)treeView1.SelectedNode.Tag;
                var form = new AddCustomProperty(l.CustomProperties);
                form.ShowDialog();
            }
            propertyGrid1.Refresh();
        }







        //MENU
        public void newLevel()
        {
            Application.DoEvents();
            var newlevel = new Level();

            
            newlevel.EditorRelated.Version = picturebox.Version;
            picturebox.loadLevel(newlevel);

            newlevel.onContentDirectorySelected += LoadFolderContent;
            levelfilename = "untitled";
            DirtyFlag = false;
        }
        public void saveLevel(String filename)
        {
            MainForm.Instance.picturebox.saveLevel(filename);
            levelfilename = filename;
            DirtyFlag = false;

            if (Constants.Instance.SaveLevelStartApplication)
            {
                if (!File.Exists(Constants.Instance.SaveLevelApplicationToStart))
                {
                    MessageBox.Show("The file \"" + Constants.Instance.SaveLevelApplicationToStart + "\" doesn't exist!\nPlease provide a valid application executable in Tools -> Settings -> Save Level!\nLevel was saved.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Constants.Instance.SaveLevelAppendLevelFilename)
                {
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart, "\"" + levelfilename + "\"");
                }
                else
                {
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart);
                }
            }

        }
        public void loadLevel(String filename)
        {
            var level = Level.FromFile(filename, Instance.picturebox.Editor.Content);          
            MainForm.Instance.picturebox.loadLevel(level);
            levelfilename = filename;
            DirtyFlag = false;
        }
        public DialogResult checkCurrentLevelAndSaveEventually()
        {
            if (DirtyFlag)
            {
                var dr = MessageBox.Show("The current level has not been saved. Do you want to save now?", "Save?",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    if (levelfilename == "untitled")
                    {
                        var dialog = new SaveFileDialog();
                        dialog.Filter = "XML Files (*.xml)|*.xml";
                        if (dialog.ShowDialog() == DialogResult.OK) saveLevel(dialog.FileName);
                        else return DialogResult.Cancel;
                    }
                    else
                    {
                        saveLevel(levelfilename);
                    }
                }
                if (dr == DialogResult.Cancel) return DialogResult.Cancel;
            }
            return DialogResult.OK;
        }

        private void FileNew(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) return;
            newLevel();
        }
        private void FileOpen(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) return;
            var opendialog = new OpenFileDialog();
            opendialog.Filter = "XML Files (*.xml)|*.xml";
            if (opendialog.ShowDialog() == DialogResult.OK) loadLevel(opendialog.FileName);
        }
        private void FileSave(object sender, EventArgs e)
        {
            if (levelfilename == "untitled") FileSaveAs(sender, e);
            else saveLevel(levelfilename);

        }
        private void FileSaveAs(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "XML Files (*.xml)|*.xml";
            if (dialog.ShowDialog() == DialogResult.OK) saveLevel(dialog.FileName);
        }
        private void FileExit(object sender, EventArgs e)
        {
            Close();
        }



        private void EditUndo(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.undo();
        }

        private void EditRedo(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.redo();
        }

        private void EditSelectAll(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.selectAll();
        }

        private void zoomcombo_TextChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = zoomcombo.SelectedText;
            if (zoomcombo.Text.Length > 0 && MainForm.Instance.picturebox != null)
            {
                //float zoom = float.Parse(zoomcombo.Text.Substring(0, zoomcombo.Text.Length - 1));
                MainForm.Instance.picturebox.camera.Scale = 100 / 100;
            }
        }

        private void HelpQuickGuide(object sender, EventArgs e)
        {
            new QuickGuide().Show();
        }

        private void ToolsMenu_MouseEnter(object sender, EventArgs e)
        {
            moveSelectedItemsToLayerToolStripMenuItem.Enabled =
            copySelectedItemsToLayerToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count > 0;
            alignHorizontallyToolStripMenuItem.Enabled =
            alignVerticallyToolStripMenuItem.Enabled =
            alignRotationToolStripMenuItem.Enabled =
            alignScaleToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count > 1;

            linkItemsByACustomPropertyToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count == 2;

        }
        private void ToolsMenu_Click(object sender, EventArgs e)
        {
        }
        private void ToolsMoveToLayer(object sender, EventArgs e)
        {
            var f = new LayerSelectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                var chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
                Instance.picturebox.moveSelectedItemsToLayer(chosenlayer);
            }

        }
        private void ToolsCopyToLayer(object sender, EventArgs e)
        {
            var f = new LayerSelectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                var chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
                MainForm.Instance.picturebox.copySelectedItemsToLayer(chosenlayer);
            }
        }
        private void ToolsLinkItems(object sender, EventArgs e)
        {
            linkItemsForm.ShowDialog();
        }
        private void ToolsAlignHorizontally(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.alignHorizontally();
        }
        private void ToolsAlignVertically(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.alignVertically();
        }
        private void ToolsAlignRotation(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.alignRotation();
        }
        private void ToolsAlignScale(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.alignScale();
        }
        private void ToolsSettings(object sender, EventArgs e)
        {
            var f = new SettingsForm();
            f.ShowDialog();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //MainForm.Instance.picturebox.Draw();
            Instance.picturebox.GraphicsDevice.Present();
            Application.DoEvents();
        }


        private void propertyGrid1_Enter(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.beginCommand("Edit in PropertyGrid");
        }
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            MainForm.Instance.picturebox.endCommand();
            MainForm.Instance.picturebox.beginCommand("Edit in PropertyGrid");
        }

        public void UndoManyCommands(object sender, ToolStripItemClickedEventArgs e)
        {
            var c = (Command)e.ClickedItem.Tag;
            MainForm.Instance.picturebox.undoMany(c);
        }

        private void RedoManyCommands(object sender, ToolStripItemClickedEventArgs e)
        {
            var c = (Command)e.ClickedItem.Tag;
            MainForm.Instance.picturebox.redoMany(c);
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var spriteSheet = (SpriteSheet)(sender as ListView).Tag;
            var spriteSheetDef = (string)(sender as ListView).FocusedItem.Tag;
            var rect = spriteSheet.SpriteDef[spriteSheetDef].SrcRectangle;
            var tex = GetBrushData(rect, spriteSheet.Texture);
            
            Instance.picturebox.createTextureBrush(tex, spriteSheet, spriteSheetDef);
        }

        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
       
        public string getUniqueNameBasedOn(string name)
        {
            int i=0;
            string newname = "Copy of " + name;
            while (treeView1.Nodes.Find(newname, true).Length>0) 
            {
                newname = "Copy(" + i++.ToString() + ") of " + name;
            }
            return newname;
        }

        private Texture2D GetBrushData(Microsoft.Xna.Framework.Rectangle tile, Texture2D tileTex)
        {
            int[] data = new int[tile.Width * tile.Height];
            tileTex.GetData<int>(0, tile, data, 0, tile.Width * tile.Height);

            var texBrush = new Texture2D(MainForm.Instance.picturebox.GraphicsDevice, tile.Width, tile.Height);
            texBrush.SetData<int>(data);

            return texBrush;
        }


        private Bitmap GetButtonImage(Microsoft.Xna.Framework.Rectangle tile,Texture2D tileTex)
        {
            int[] data = new int[tile.Width * tile.Height];
            tileTex.GetData<int>(0, tile, data, 0, tile.Width * tile.Height);
            Bitmap bitmap = new Bitmap(tile.Width, tile.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int x = 0; x < tile.Width; ++x)
            {
                for (int y = 0; y < tile.Height; ++y)
                {
                    Color bitmapColor =
                        Color.FromArgb(data[y * tile.Width + x]);

                    bitmap.SetPixel(x, y, bitmapColor);
                }
            }
            return bitmap;
        }

        public void LoadFolderContent(string path)
        {
            // Remove old tabpages
            for (var idx=tabControl1.TabPages.Count-1; idx > 0; idx--)
            {
                tabControl1.TabPages.RemoveAt(idx);
            }

            spriteSheets = new Dictionary<string, SpriteSheet>();
            Image img = Resources.folder;
            var di = new DirectoryInfo(path);

            string filters = "*.jpg;*.png;*.bmp;";
            var fileList = new List<FileInfo>();
            string[] extensions = filters.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
            foreach (string filter in extensions) fileList.AddRange(di.GetFiles(filter));
            FileInfo[] files = fileList.ToArray();

            foreach (var file in files)
            {
                var tp = new TabPage(Path.GetFileNameWithoutExtension(file.FullName));
                var imgList = new ImageList() { TransparentColor = Color.Transparent, ColorDepth = ColorDepth.Depth32Bit };
                imgList.ImageSize = new Size(64, 64);
                var lv = new ListView();
                lv.LargeImageList = imgList;
                lv.Dock = DockStyle.Fill;
                lv.View = View.LargeIcon;
                
                tp.Controls.Add(lv);

                
                tabControl1.TabPages.Add(tp);

                Task.Run(() => { LoadSpriteSheet(file, lv); });
            }
        }

        private void LoadSpriteSheet(FileInfo file,ListView lv)
        {
            var textureFile = file.FullName;
            var defFile = Path.ChangeExtension(textureFile, ".xml");

            if (File.Exists(defFile))
            {
                try
                {
                    FileStream fileStream = new FileStream(textureFile, FileMode.Open);
                    Texture2D spriteAtlas = Texture2D.FromStream(Instance.picturebox.GraphicsDevice, fileStream);
                    fileStream.Dispose();
                    var spriteSheet = new SpriteSheet(spriteAtlas, defFile);
                    spriteSheets.Add(spriteSheet.Name, spriteSheet);
                    foreach (var spriteDef in spriteSheet.SpriteDef)
                    {
                        var rect = spriteDef.Value.SrcRectangle;
                        var item = GetButtonImage(rect, spriteAtlas);


                        var lvi = new ListViewItem();
                        lvi.Name = file.FullName;
                        lvi.Text = spriteDef.Key;
                        lvi.ImageKey = spriteDef.Key;
                        lvi.Tag = spriteDef.Key;
                        lvi.ToolTipText = file.Name + " (" + spriteDef.Value.SrcRectangle.Width.ToString() + " x " + spriteDef.Value.SrcRectangle.Height.ToString() + ")";

                        lv.Invoke((MethodInvoker)delegate {
                            lv.Items.Add(lvi);
                            lv.LargeImageList.Images.Add(spriteDef.Key, getThumbNail(item, 64, 64));
                        });


                    }

                    lv.Tag = spriteSheet;
                    lv.MouseDoubleClick += listView1_MouseDoubleClick;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
        }


        private void listView2_MouseDoubleClick(object sender, EventArgs e)
        {
            if (listView2.FocusedItem.Text == "Rectangle")
            {
                MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Rectangle);
            }
            if (listView2.FocusedItem.Text == "Circle")
            {
                MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Circle);
            }
            if (listView2.FocusedItem.Text == "Path")
            {
                MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Path);
            }
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
           

        }



        private void RunLevel(object sender, EventArgs e)
        {
            if (Constants.Instance.RunLevelStartApplication)
            {
                if (!System.IO.File.Exists(Constants.Instance.RunLevelApplicationToStart))
                {
                    MessageBox.Show("The file \"" + Constants.Instance.RunLevelApplicationToStart + "\" doesn't exist!\nPlease provide a valid application executable in Tools -> Settings -> Run Level!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                FileSave(sender, e);

                var processInfo = new ProcessStartInfo();
                processInfo.FileName = Constants.Instance.RunLevelApplicationToStart;
                processInfo.ErrorDialog = true;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;
                processInfo.RedirectStandardError = true;
                processInfo.WorkingDirectory = Path.GetDirectoryName(Constants.Instance.RunLevelApplicationToStart);
                if (Constants.Instance.RunLevelAppendLevelFilename)
                    processInfo.Arguments = levelfilename;
                Process.Start(processInfo);
            }
        }

        private void CustomPropertyContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (propertyGrid1.SelectedGridItem.Parent.Label != "Custom Properties") e.Cancel = true;
        }

        private void deleteCustomProperty(object sender, EventArgs e)
        {
            MainForm.Instance.picturebox.beginCommand("Delete Custom Property");
            var dpd = (DictionaryPropertyDescriptor)propertyGrid1.SelectedGridItem.PropertyDescriptor;
            dpd.sdict.Remove(dpd.Name);
            propertyGrid1.Refresh();
            MainForm.Instance.picturebox.endCommand();
        }

        private void ViewGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ShowGridButton.Checked = ViewGrid.Checked;
        }
        
        private void ViewWorldOrigin_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowWorldOrigin = ShowWorldOriginButton.Checked = ViewWorldOrigin.Checked;
        }
        
        private void ShowGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ViewGrid.Checked = ShowGridButton.Checked;
        }

        private void ShowWorldOriginButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowWorldOrigin = ViewWorldOrigin.Checked = ShowWorldOriginButton.Checked;
        }

        private void SnapToGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid =  ViewSnapToGrid.Checked = SnapToGridButton.Checked;
        }

        private void ViewSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid = SnapToGridButton.Checked = ViewSnapToGrid.Checked;
        }   
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


    }
}
