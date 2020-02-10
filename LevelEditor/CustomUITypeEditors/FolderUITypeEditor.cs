using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;


namespace CustomUITypeEditors
{

    /// <summary>
    /// A FolderEditor that always starts at the currently selected folder. For use on a property of type: string.
    /// </summary>
    public class FolderUITypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var path = Convert.ToString(value);
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = path;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    path = dlg.SelectedPath;
                }
            }
            return path;
        }
    }


}
