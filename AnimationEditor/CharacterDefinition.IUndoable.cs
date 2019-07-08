using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Forms;


namespace AnimationEditor
{
    [DebuggerDisplay("Name: {Name}, Address: {&Name}")]
    public partial class CharacterDefinition : IUndoable
    {

        public void add()
        {
        }

        public void remove()
        {
        }

        public IUndoable cloneforundo()
        {
            var result = (CharacterDefinition)this.MemberwiseClone();
            return (IUndoable)result;
        }

        public void makelike(IUndoable other)
        {
            /*Layer l2 = (Layer)other;
            Items = l2.Items;
            treenode.Nodes.Clear();
            foreach (Item i in Items)
            {
                Editor.Instance.addItem(i);
            }*/


            //Level l2 = (Level)other;
            //Layers = l2.Layers;
            //treenode.Nodes.Clear();
            //foreach (Layer l in Layers)
            //{
            //    MainForm.Instance.picturebox.addLayer(l);
            //    //TODO add all items
            //}
        }

        public string getName()
        {
            return String.Empty;
            //return Name;
        }

        public void setName(string name)
        {
            //Name = name;
            //treenode.Text = name;
        }

    }
}
