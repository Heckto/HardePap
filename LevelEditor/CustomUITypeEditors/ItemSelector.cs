﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Game1.GameObjects;
using LevelEditor;

namespace CustomUITypeEditors
{
    public partial class ItemSelector : UserControl
    {
        public GameObject Value;

        public ItemSelector(GameObject item)
        {
            InitializeComponent();
            Value = item;
        }

        private void ItemSelector_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Add((TreeNode)MainForm.Instance.treeView1.Nodes[0].Clone());
            treeView1.ImageList = MainForm.Instance.treeView1.ImageList;
            treeView1.ImageIndex = treeView1.SelectedImageIndex = 5;
            treeView1.Nodes[0].Expand();
            if (Value != null)
            {
                var nodes = treeView1.Nodes.Find(Value.Name, true);
                if (nodes.Length > 0)
                {
                    treeView1.SelectedNode = nodes[0];
                }
                else
                {
                    Value = null;
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is GameObject)
                Value = (GameObject)e.Node.Tag;
        }

    }


    public class ItemUITypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var wfes =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (wfes != null)
            {
                var uc1 = new ItemSelector((GameObject)value);
                wfes.DropDownControl(uc1);
                value = uc1.Value;
            }
            return value;
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }


    }


}
