using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Forms;


namespace LevelEditor
{
    //[DebuggerDisplay("Name: {Name}, Address: {&Name}")]
    //public partial class Level : IUndoable
    //{

    //    public void add()
    //    {
    //    }

    //    public void remove()
    //    {
    //    }

    //    public IUndoable cloneforundo()
    //    {
    //        selecteditems = "";
    //        foreach (var i in MainForm.Instance.picturebox.SelectedItems) selecteditems += i.Name + ";";
    //        if (MainForm.Instance.picturebox.SelectedLayer != null) selectedlayers = MainForm.Instance.picturebox.SelectedLayer.Name;


    //        var result = (Level)this.MemberwiseClone();
    //        result.Layers = new List<Layer>(Layers);
    //        for (var i = 0; i < result.Layers.Count; i++)
    //        {
    //            result.Layers[i] = result.Layers[i].clone();
    //            result.Layers[i].level = result;
    //        }
    //        return (IUndoable)result;
    //    }

    //    public void makelike(IUndoable other)
    //    {
    //        /*Layer l2 = (Layer)other;
    //        Items = l2.Items;
    //        treenode.Nodes.Clear();
    //        foreach (Item i in Items)
    //        {
    //            Editor.Instance.addItem(i);
    //        }*/


    //        var l2 = (Level)other;
    //        Layers = l2.Layers;
    //        treenode.Nodes.Clear();
    //        foreach (var l in Layers)
    //        {
    //            MainForm.Instance.picturebox.addLayer(l);
    //            //TODO add all items
    //        }
    //    }

    //    public string getName()
    //    {
    //        return Name;
    //    }

    //    public void setName(string name)
    //    {
    //        Name = name;
    //        treenode.Text = name;
    //    }

    //}
}
