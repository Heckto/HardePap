using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LevelEditor
{
    public partial class AddLayer : Form
    {

        public AddLayer(MainForm main)
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            TreeNode[] nodes = MainForm.Instance.treeView1.Nodes.Find(textBox1.Text, true);
            if (nodes.Length > 0)
            {
                MessageBox.Show("A layer or item with the name \"" + textBox1.Text + "\" already exists in the level. Please use another name!");
                return;
            }

            Layer l = null;
            if (radioButton1.Checked)
                l = new Layer(textBox1.Text);
            else
                l = new MovingLayer(textBox1.Text);
            MainForm.Instance.picturebox.beginCommand("Add Layer \"" + l.Name + "\"");
            MainForm.Instance.picturebox.addLayer(l);
            MainForm.Instance.picturebox.endCommand();
            MainForm.Instance.picturebox.SelectedLayer = l;
            MainForm.Instance.picturebox.updatetreeview();
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
