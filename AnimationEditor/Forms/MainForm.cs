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
using AnimationEditor.Properties;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using AuxLib;
using AuxLib.Logging;

namespace AnimationEditor
{
    public partial class MainForm : Form
    {
        public static MainForm Instance;
        String animationfilename;

        public Dictionary<string,SpriteSheet> spriteSheets = new Dictionary<string,SpriteSheet>();

        bool dirtyflag;
        public bool DirtyFlag
        {
            get { return dirtyflag; }
            set { dirtyflag = value; updatetitlebar(); }
        }

        Cursor dragcursor;

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
            Text = "AnimationEditor - " + animationfilename + (DirtyFlag ? "*" : String.Empty);
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
            //fill zoom combobox
            for (var i = 25; i <= 200; i += 25)
            {
                zoomcombo.Items.Add(i.ToString() + "%");
            }
            zoomcombo.SelectedIndex = 3;

            comboBox1.Items.Add("48x48");
            comboBox1.Items.Add("64x64");
            comboBox1.Items.Add("96x96");
            comboBox1.Items.Add("128x128");
            comboBox1.Items.Add("256x256");
            comboBox1.SelectedIndex = 1;

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
            if (e.KeyCode == Keys.N) ActionNewAnimation(sender, e);
            if (e.KeyCode == Keys.Delete) ActionDelete(sender, e);
            if (e.KeyCode == Keys.F7) ActionMoveUp(sender, e);
            if (e.KeyCode == Keys.F8) ActionMoveDown(sender, e);            
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
            if (e.Node.Tag is CharacterDefinition)
            {
                var l = (CharacterDefinition)e.Node.Tag;
                Instance.picturebox.beginCommand("Rename CharDef (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                Instance.picturebox.endCommand();
            }
            if (e.Node.Tag is Animation)
            {
                var l = (Animation)e.Node.Tag;
                Instance.picturebox.beginCommand("Rename Animation (\"" + l.Name + "\" -> \"" + e.Label + "\")");
                l.Name = e.Label;
                e.Node.Name = e.Label;
                Instance.picturebox.endCommand();
            }
            if (e.Node.Tag is Frame)
            {
                var i = (Frame)e.Node.Tag;
                Instance.picturebox.beginCommand("Rename Frame (\"" + i.Name + "\" -> \"" + e.Label + "\")");
                i.Name = e.Label;
                e.Node.Name = e.Label;
                Instance.picturebox.endCommand();
            }
            propertyGrid1.Refresh();
            picturebox.Select();
        }
        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //if (e.Node.Tag is Level)
            //{
            //    picturebox.level.Visible = e.Node.Checked;
            //}
            //if (e.Node.Tag is Layer)
            //{
            //    var l = (Layer)e.Node.Tag;
            //    l.Visible = e.Node.Checked;
            //}
            //if (e.Node.Tag is Item)
            //{
            //    var i = (Item)e.Node.Tag;
            //    i.Visible = e.Node.Checked;
            //}
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is CharacterDefinition)
            {
                propertyGrid1.SelectedObject = e.Node.Tag;
            }
            if (e.Node.Tag is Animation)
            {
                propertyGrid1.SelectedObject = e.Node.Tag;
                MainForm.Instance.picturebox.SelectedAnimation = (Animation)e.Node.Tag;
            }
            if (e.Node.Tag is KeyFrame)
            {
                propertyGrid1.SelectedObject = e.Node.Tag;
                MainForm.Instance.picturebox.SelectedKeyFrame = (KeyFrame)e.Node.Tag;
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
            //if (((TreeNode)e.Item).Tag is Layer) return;
            //if (((TreeNode)e.Item).Tag is Level) return;
            //MainForm.Instance.picturebox.beginCommand("Drag Item");
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            //get source node
            //var sourcenode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            //if (sourcenode == null)
            //{
            //    e.Effect = DragDropEffects.None;
            //    return;
            //}
            //else e.Effect = DragDropEffects.Move;

            ////get destination node and select it
            //var p = treeView1.PointToClient(new Point(e.X, e.Y));
            //var destnode = treeView1.GetNodeAt(p);
            //if (destnode.Tag is Level) return;
            //treeView1.SelectedNode = destnode;

            //if (destnode != sourcenode)
            //{
            //    var i1 = (Item)sourcenode.Tag;
            //    if (destnode.Tag is Item)
            //    {
            //        var i2 = (Item)destnode.Tag;
            //        MainForm.Instance.picturebox.moveItemToLayer(i1, i2.layer, i2);
            //        int delta = 0;
            //        if (destnode.Index > sourcenode.Index && i1.layer == i2.layer) delta = 1;
            //        sourcenode.Remove();
            //        destnode.Parent.Nodes.Insert(destnode.Index + delta, sourcenode);
            //    }
            //    if (destnode.Tag is Layer)
            //    {
            //        var l2 = (Layer)destnode.Tag;
            //        MainForm.Instance.picturebox.moveItemToLayer(i1, l2, null);
            //        sourcenode.Remove();
            //        destnode.Nodes.Insert(0, sourcenode);
            //    }
            //    MainForm.Instance.picturebox.selectitem(i1);
            //    //MainForm.Instance.picturebox.Draw();
            //    MainForm.Instance.picturebox.GraphicsDevice.Present();
            //    Application.DoEvents();
            //}
        }
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            //MainForm.Instance.picturebox.endCommand();
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
            
            if (MainForm.Instance.picturebox != null)
                Instance.picturebox.camera.updateviewport(picturebox.Width, picturebox.Height);
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
            //e.Effect = DragDropEffects.Move;
            //var lvi = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

            //var spriteSheet = (SpriteSheet)lvi.ListView.Tag;
            //var tex = GetBrushData(spriteSheet.SpriteDef[lvi.Name].SrcRectangle, spriteSheet.Texture);
            //MainForm.Instance.picturebox.createTextureBrush(tex,spriteSheet,lvi.Name);

        }
        private void pictureBox1_DragOver(object sender, DragEventArgs e)
        {
            //e.Effect = DragDropEffects.Move;
            //var p = picturebox.PointToClient(new Point(e.X, e.Y));
            //MainForm.Instance.picturebox.setmousepos(p.X, p.Y);
            ////MainForm.Instance.picturebox.Draw();
            //MainForm.Instance.picturebox.GraphicsDevice.Present();
        }
        private void pictureBox1_DragLeave(object sender, EventArgs e)
        {
            //MainForm.Instance.picturebox.destroyTextureBrush();
            ////MainForm.Instance.picturebox.Draw();
            //MainForm.Instance.picturebox.GraphicsDevice.Present();
        }
        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            //MainForm.Instance.picturebox.paintTextureBrush(false);
            //listView1.Cursor = Cursors.Default;
            //picturebox.Cursor = Cursors.Default;
        }




        // ACTIONS
        private void ActionDuplicate(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode.Tag is Layer)
            //{
            //    var l = (Layer)treeView1.SelectedNode.Tag;
            //    var layercopy = l.clone();
            //    layercopy.Name = getUniqueNameBasedOn(layercopy.Name);
            //    for (int i = 0; i < layercopy.Items.Count; i++)
            //    {
            //        layercopy.Items[i].Name = getUniqueNameBasedOn(layercopy.Items[i].Name);
            //    }
            //    MainForm.Instance.picturebox.beginCommand("Duplicate Layer \"" + l.Name + "\"");
            //    MainForm.Instance.picturebox.addLayer(layercopy);
            //    MainForm.Instance.picturebox.endCommand();
            //    MainForm.Instance.picturebox.updatetreeview();
            //}
        }
        private void ActionRename(object sender, EventArgs e)
        {
            treeView1.SelectedNode.BeginEdit();
        }
        private void ActionNewAnimation(object sender, EventArgs e)
        {
            var anim = new Animation("Test");
            TreeNode[] nodes = Instance.treeView1.Nodes.Find(textBox1.Text, true);
            if (nodes.Length > 0)
            {
                MessageBox.Show("A layer or item with the name \"" + textBox1.Text + "\" already exists in the level. Please use another name!");
                return;
            }


            
            MainForm.Instance.picturebox.beginCommand("Add Layer \"" + anim.Name + "\"");
            MainForm.Instance.picturebox.addAnimation(anim);
            MainForm.Instance.picturebox.endCommand();
            MainForm.Instance.picturebox.updatetreeview();
        }
        private void ActionDelete(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode == null) return;
            //if (treeView1.SelectedNode.Tag is Layer)
            //{
            //    var l = (Layer)treeView1.SelectedNode.Tag;
            //    MainForm.Instance.picturebox.deleteLayer(l);
            //}
            //else if (treeView1.SelectedNode.Tag is Item)
            //{
            //    MainForm.Instance.picturebox.deleteSelectedItems();
            //}
        }
        private void ActionMoveUp(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode.Tag is Layer)
            //{
            //    var l = (Layer)treeView1.SelectedNode.Tag;
            //    if (l.level.Layers.IndexOf(l) > 0)
            //    {
            //        MainForm.Instance.picturebox.beginCommand("Move Up Layer \"" + l.Name + "\"");
            //        MainForm.Instance.picturebox.moveLayerUp(l);
            //        MainForm.Instance.picturebox.endCommand();
            //        MainForm.Instance.picturebox.updatetreeview();
            //    }
            //}
            //if (treeView1.SelectedNode.Tag is Item)
            //{
            //    var i = (Item)treeView1.SelectedNode.Tag;
            //    if (i.layer.Items.IndexOf(i) > 0)
            //    {
            //        MainForm.Instance.picturebox.beginCommand("Move Up Item \"" + i.Name + "\"");
            //        MainForm.Instance.picturebox.moveItemUp(i);
            //        MainForm.Instance.picturebox.endCommand();
            //        MainForm.Instance.picturebox.updatetreeview();
            //    }
            //}
        }
        private void ActionMoveDown(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode.Tag is Layer)
            //{
            //    var l = (Layer)treeView1.SelectedNode.Tag;
            //    if (l.level.Layers.IndexOf(l) < l.level.Layers.Count - 1)
            //    {
            //        MainForm.Instance.picturebox.beginCommand("Move Down Layer \"" + l.Name + "\"");
            //        MainForm.Instance.picturebox.moveLayerDown(l);
            //        MainForm.Instance.picturebox.endCommand();
            //        MainForm.Instance.picturebox.updatetreeview();
            //    }
            //}
            //if (treeView1.SelectedNode.Tag is Item)
            //{
            //    var i = (Item)treeView1.SelectedNode.Tag;
            //    if (i.layer.Items.IndexOf(i) < i.layer.Items.Count - 1)
            //    {
            //        MainForm.Instance.picturebox.beginCommand("Move Down Item \"" + i.Name + "\"");
            //        MainForm.Instance.picturebox.moveItemDown(i);
            //        MainForm.Instance.picturebox.endCommand();
            //        MainForm.Instance.picturebox.updatetreeview();
            //    }
            //}
        }
        private void ActionAddCustomProperty(object sender, EventArgs e)
        {
            //if (treeView1.SelectedNode.Tag is Item)
            //{
            //    var i = (Item)treeView1.SelectedNode.Tag;
            //    var form = new AddCustomProperty(i.CustomProperties);
            //    form.ShowDialog();
            //}
            //if (treeView1.SelectedNode.Tag is Level)
            //{
            //    var l = (Level)treeView1.SelectedNode.Tag;
            //    var form = new AddCustomProperty(l.CustomProperties);
            //    form.ShowDialog();
            //}
            //if (treeView1.SelectedNode.Tag is Layer)
            //{
            //    var l = (Layer)treeView1.SelectedNode.Tag;
            //    var form = new AddCustomProperty(l.CustomProperties);
            //    form.ShowDialog();
            //}
            propertyGrid1.Refresh();
        }







        //MENU
        public void newCharacterDefinition()
        {
            Application.DoEvents();
            var new_chardef = new CharacterDefinition();
            //new_animation.EditorRelated.Version = picturebox.Version;
            picturebox.loadAnimation(new_chardef);
            animationfilename = "untitled";
            DirtyFlag = false;

            
        }
        public void saveCharacterDefinition(String filename)
        {
            MainForm.Instance.picturebox.saveAnimation(filename);
            animationfilename = filename;
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
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart, "\"" + animationfilename + "\"");
                }
                else
                {
                    Process.Start(Constants.Instance.SaveLevelApplicationToStart);
                }
            }

        }
        public void loadCharacterDefinition(String filename)
        {
            var animation = CharacterDefinition.FromFile(filename, Instance.picturebox.Editor.Content);          
            Instance.picturebox.loadAnimation(animation);
            animationfilename = filename;
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
                    if (animationfilename == "untitled")
                    {
                        var dialog = new SaveFileDialog();
                        dialog.Filter = "XML Files (*.xml)|*.xml";
                        if (dialog.ShowDialog() == DialogResult.OK) saveCharacterDefinition(dialog.FileName);
                        else return DialogResult.Cancel;
                    }
                    else
                    {
                        saveCharacterDefinition(animationfilename);
                    }
                }
                if (dr == DialogResult.Cancel) return DialogResult.Cancel;
            }
            return DialogResult.OK;
        }

        private void FileNew(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel)
                return;
            newCharacterDefinition();
        }
        private void FileOpen(object sender, EventArgs e)
        {
            if (checkCurrentLevelAndSaveEventually() == DialogResult.Cancel) return;
            var opendialog = new OpenFileDialog();
            opendialog.Filter = "XML Files (*.xml)|*.xml";
            if (opendialog.ShowDialog() == DialogResult.OK) loadCharacterDefinition(opendialog.FileName);
        }
        private void FileSave(object sender, EventArgs e)
        {
            if (animationfilename == "untitled") FileSaveAs(sender, e);
            else saveCharacterDefinition(animationfilename);

        }
        private void FileSaveAs(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "XML Files (*.xml)|*.xml";
            if (dialog.ShowDialog() == DialogResult.OK) saveCharacterDefinition(dialog.FileName);
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
           //MainForm.Instance.picturebox.selectAll();
        }

        private void zoomcombo_TextChanged(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = zoomcombo.SelectedText;
            if (zoomcombo.Text.Length > 0 && MainForm.Instance.picturebox != null)
            {
                float zoom = float.Parse(zoomcombo.Text.Substring(0, zoomcombo.Text.Length - 1));
                Instance.picturebox.camera.Scale = 100 / 100;
            }
        }

        private void HelpQuickGuide(object sender, EventArgs e)
        {
           // new QuickGuide().Show();
        }

        private void ToolsMenu_MouseEnter(object sender, EventArgs e)
        {
            //moveSelectedItemsToLayerToolStripMenuItem.Enabled =
            //copySelectedItemsToLayerToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count > 0;
            //alignHorizontallyToolStripMenuItem.Enabled =
            //alignVerticallyToolStripMenuItem.Enabled =
            //alignRotationToolStripMenuItem.Enabled =
            //alignScaleToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count > 1;

            //linkItemsByACustomPropertyToolStripMenuItem.Enabled = MainForm.Instance.picturebox.SelectedItems.Count == 2;

        }
        private void ToolsMenu_Click(object sender, EventArgs e)
        {
        }
        private void ToolsMoveToLayer(object sender, EventArgs e)
        {
            //var f = new LayerSelectForm();
            //if (f.ShowDialog() == DialogResult.OK)
            //{
            //    var chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
            //    MainForm.Instance.picturebox.moveSelectedItemsToLayer(chosenlayer);
            //}

        }
        private void ToolsCopyToLayer(object sender, EventArgs e)
        {
            //var f = new LayerSelectForm();
            //if (f.ShowDialog() == DialogResult.OK)
            //{
            //    var chosenlayer = (Layer)f.treeView1.SelectedNode.Tag;
            //    MainForm.Instance.picturebox.copySelectedItemsToLayer(chosenlayer);
            //}
        }
        private void ToolsLinkItems(object sender, EventArgs e)
        {
            //linkItemsForm.ShowDialog();
        }
        private void ToolsAlignHorizontally(object sender, EventArgs e)
        {
           // MainForm.Instance.picturebox.alignHorizontally();
        }
        private void ToolsAlignVertically(object sender, EventArgs e)
        {
            //MainForm.Instance.picturebox.alignVertically();
        }
        private void ToolsAlignRotation(object sender, EventArgs e)
        {
           // MainForm.Instance.picturebox.alignRotation();
        }
        private void ToolsAlignScale(object sender, EventArgs e)
        {
            //MainForm.Instance.picturebox.alignScale();
        }
        private void ToolsSettings(object sender, EventArgs e)
        {
            var f = new SettingsForm();
            f.ShowDialog();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
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

        private void comboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    listView1.LargeImageList = imageList48;
                    SetListViewSpacing(listView1, 48 + 8, 48 + 32);
                    break;
                case 1:
                    listView1.LargeImageList = imageList64;
                    SetListViewSpacing(listView1, 64 + 8, 64 + 32);
                    break;
                case 2:
                    listView1.LargeImageList = imageList96;
                    SetListViewSpacing(listView1, 96 + 8, 96 + 32);
                    break;
                case 3:
                    listView1.LargeImageList = imageList128;
                    SetListViewSpacing(listView1, 128 + 8, 128 + 32);
                    break;
                case 4:
                    listView1.LargeImageList = imageList256;
                    SetListViewSpacing(listView1, 256 + 8, 256 + 32);
                    break;
            }
        }

        private void buttonFolderUp_Click(object sender, EventArgs e)
        {
            var di = Directory.GetParent(textBox1.Text);
            if (di == null) return;
            loadfolder(di.FullName);
        }
        private void chooseFolder_Click(object sender, EventArgs e)
        {
            var d = new FolderBrowserDialog();
            d.SelectedPath = textBox1.Text;
            if (d.ShowDialog() == DialogResult.OK) loadfolder(d.SelectedPath);
        }
        private void listView1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = listView1.FocusedItem.ToolTipText;
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewItem item = listView1.GetItemAt(e.X, e.Y);
            var spriteSheet = (SpriteSheetIndex)item.Tag;
            var f = new Frame(spriteSheet.texture, spriteSheet.index);
            var kf = new KeyFrame(f);
            Instance.picturebox.addItem(kf);
            Instance.picturebox.updatetreeview();
        }
        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var lvi = (ListViewItem)e.Item;
            toolStripStatusLabel1.Text = lvi.ToolTipText;
            var bmp = new Bitmap(listView1.LargeImageList.Images[lvi.ImageKey]);
            dragcursor = new Cursor(bmp.GetHicon());
            listView1.DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void listView1_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private void listView1_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effect == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                listView1.Cursor = dragcursor;
                picturebox.Cursor = Cursors.Default;
            }
            else
            {
                e.UseDefaultCursors = true;
                listView1.Cursor = Cursors.Default;
                picturebox.Cursor = Cursors.Default;
            }
        }
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            listView1.Cursor = Cursors.Default;
            picturebox.Cursor = Cursors.Default;
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




        public void loadfolder(string path)
        {
            loadfolder_foreground(path);
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
            Bitmap bitmap = new Bitmap(tile.Width, tile.Height);

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

        public void loadfolder_foreground(string path)
        {
            imageList48.Images.Clear();
            imageList64.Images.Clear();
            imageList96.Images.Clear();
            imageList128.Images.Clear();
            imageList256.Images.Clear();

            Image img = Resources.folder;
            imageList48.Images.Add(img);
            imageList64.Images.Add(img);
            imageList96.Images.Add(img);
            imageList128.Images.Add(img);
            imageList256.Images.Add(img);

            listView1.Clear();

            var di = new DirectoryInfo(path);
            textBox1.Text = di.FullName;
            DirectoryInfo[] folders = di.GetDirectories();
            foreach (var folder in folders)
            {
                var lvi = new ListViewItem();
                lvi.Text = folder.Name;
                lvi.ToolTipText = folder.Name;
                lvi.ImageIndex = 0;
                lvi.Tag = "folder";
                lvi.Name = folder.FullName;
                listView1.Items.Add(lvi);
            }

            string filters = "*.png";
            var fileList = new List<FileInfo>();
            string[] extensions = filters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string filter in extensions)
                fileList.AddRange(di.GetFiles(filter, SearchOption.AllDirectories));
            FileInfo[] files = fileList.ToArray();

            foreach (var spriteSheetFile in files)
            {
                var fileStream = new FileStream(spriteSheetFile.FullName, FileMode.Open);
                Texture2D texture = Texture2D.FromStream(Instance.picturebox.GraphicsDevice, fileStream);
                fileStream.Dispose();

                

                SpriteSheet ass = null;
                var defFile = Path.ChangeExtension(spriteSheetFile.FullName, ".xml");
                if (File.Exists(defFile))
                    ass = new SpriteSheet(texture, defFile);
                else
                    ass = new SpriteSheet(texture);

                var asset = Path.GetFileNameWithoutExtension(spriteSheetFile.FullName);
                spriteSheets.Add(asset, ass);

                foreach (var sprite in ass.SpriteDef)
                {
                    var bmp = GetButtonImage(sprite.Value.SrcRectangle, ass.Texture);

                    imageList48.Images.Add(sprite.Key, getThumbNail(bmp, 48, 48));
                    imageList64.Images.Add(sprite.Key, getThumbNail(bmp, 64, 64));
                    imageList96.Images.Add(sprite.Key, getThumbNail(bmp, 96, 96));
                    imageList128.Images.Add(sprite.Key, getThumbNail(bmp, 128, 128));
                    imageList256.Images.Add(sprite.Key, getThumbNail(bmp, 256, 256));

                    
                    var lviTag = new SpriteSheetIndex(asset, sprite.Key);

                    var lvi = new ListViewItem();
                    lvi.Name = sprite.Key;
                    lvi.Text = sprite.Key;
                    lvi.ImageKey = sprite.Key;
                    lvi.Tag = lviTag;
                    lvi.ToolTipText = sprite.Key + " (" + bmp.Width.ToString() + " x " + bmp.Height.ToString() + ")";

                    listView1.Items.Add(lvi);

                    bmp.Dispose();
                }

                Instance.picturebox.spriteSheets = spriteSheets;
            }
        }






        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //if (listView2.FocusedItem.Text == "Rectangle")
            //{
            //    MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Rectangle);
            //}
            //if (listView2.FocusedItem.Text == "Circle")
            //{
            //    MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Circle);
            //}
            //if (listView2.FocusedItem.Text == "Path")
            //{
            //    MainForm.Instance.picturebox.createPrimitiveBrush(PrimitiveType.Path);
            //}

        }
       

        private void CustomPropertyContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (propertyGrid1.SelectedGridItem.Parent.Label != "Custom Properties") e.Cancel = true;
        }

        private void deleteCustomProperty(object sender, EventArgs e)
        {
            //MainForm.Instance.picturebox.beginCommand("Delete Custom Property");
            //var dpd = (DictionaryPropertyDescriptor)propertyGrid1.SelectedGridItem.PropertyDescriptor;
            //dpd.sdict.Remove(dpd.Name);
            //propertyGrid1.Refresh();
            //MainForm.Instance.picturebox.endCommand();
        }

        private void ViewGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ShowGridButton.Checked = ViewGrid.Checked;
        }
        
        
        private void ShowGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.ShowGrid = ViewGrid.Checked = ShowGridButton.Checked;
        }


        private void SnapToGridButton_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid =  ViewSnapToGrid.Checked = SnapToGridButton.Checked;
        }

        private void ViewSnapToGrid_CheckedChanged(object sender, EventArgs e)
        {
            Constants.Instance.SnapToGrid = SnapToGridButton.Checked = ViewSnapToGrid.Checked;
        }

    }

   
}
